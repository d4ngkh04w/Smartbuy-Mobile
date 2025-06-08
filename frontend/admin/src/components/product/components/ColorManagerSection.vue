<script setup>
import { ref, defineEmits, onMounted, watch } from "vue";
import productService from "../../../services/productService.js";
import emitter from "../../../utils/evenBus.js";

const props = defineProps({
    productId: {
        type: Number,
        required: true,
    },
    existingColors: {
        type: Array,
        default: () => [],
    },
});

// Utility functions for notifications
const showErrorNotification = (message) => {
    emitter.emit("show-notification", {
        status: "error",
        message,
    });
};

const showSuccessNotification = (message) => {
    emitter.emit("show-notification", {
        status: "success",
        message,
    });
};

// Local state to keep track of colors
const colors = ref([]);

// Watch for changes in existingColors prop
const refreshColorData = async () => {
    try {
        // Fetch the latest color data directly from API
        if (props.productId) {
            const response = await productService.getProductById(
                props.productId
            );
            if (
                response.data &&
                response.data.data &&
                response.data.data.colors
            ) {
                colors.value = response.data.data.colors;
            }
        }
    } catch (error) {
        console.error("Error refreshing color data:", error);
    }
};

// Watch for changes in existingColors prop
watch(
    () => props.existingColors,
    (newColors) => {
        if (
            newColors &&
            JSON.stringify(newColors) !== JSON.stringify(colors.value)
        ) {
            // Update local state with new colors
            colors.value = [...newColors];
            existingColorImageInputs.value = Array(colors.value.length).fill(
                null
            );
        }
    },
    { deep: true }
);

// Initialize colors from props
onMounted(() => {
    colors.value = [...props.existingColors];
    // Initialize refs array for file inputs
    existingColorImageInputs.value = Array(colors.value.length).fill(null);
});

const emit = defineEmits(["color-updated"]);

// Loading states
const addingColor = ref(false);
const updatingColors = ref({});

// Delete confirmation state
const colorPendingDelete = ref(null);
const deletingColor = ref(null);

// Form state for new color
const newColor = ref({
    name: "",
    quantity: 0, // Allow zero
    images: [],
    imagePreviews: [],
    mainImageIndex: 0,
});

// State for editing existing colors
const editingColorId = ref(null);
const editingColor = ref({
    name: "",
    quantity: 0,
    newImages: [],
    newImagePreviews: [],
    mainImageIndex: 0,
    newMainImageIndex: undefined,
    removeImageIds: [],
    mainImageId: null,
    originalName: "",
    existingImages: [],
});

// Refs for file inputs
const colorImageInput = ref(null);
const existingColorImageInputs = ref([]);

// Computed properties for canAddColor
const canAddColor = () => {
    // Only require name, quantity >= 0, images optional
    return newColor.value.name.trim() !== "" && newColor.value.quantity >= 0;
};

// Image handling methods
const formatImageUrl = (imagePath) => {
    return productService.getUrlImage(imagePath);
};

const triggerImageUpload = () => {
    colorImageInput.value.click();
};

const triggerEditImageUpload = (index) => {
    if (existingColorImageInputs.value[index]) {
        existingColorImageInputs.value[index].click();
    }
};

const handleImageUpload = (event) => {
    const files = event.target.files;
    if (!files || files.length === 0) return;

    // Validate files
    let allValid = true;
    for (const file of files) {
        const validation = productService.validateImageFile(file);
        if (!validation.valid) {
            showErrorNotification(validation.message);
            allValid = false;
            break;
        }
    }

    if (!allValid) {
        return;
    }

    // Add files to new color
    for (const file of files) {
        newColor.value.images.push(file);
        newColor.value.imagePreviews.push(URL.createObjectURL(file));
    }

    // Reset file input
    event.target.value = null;
};

const handleExistingColorImageUpload = (colorId, index) => {
    const files = existingColorImageInputs.value[index].files;
    if (!files || files.length === 0) return;

    // Validate files
    let allValid = true;
    for (const file of files) {
        const validation = productService.validateImageFile(file);
        if (!validation.valid) {
            showErrorNotification(validation.message);
            allValid = false;
            break;
        }
    }

    if (!allValid) {
        return;
    }

    // Initialize arrays if they don't exist
    if (!editingColor.value.newImages) {
        editingColor.value.newImages = [];
        editingColor.value.newImagePreviews = [];
    }

    // Add files to editing color
    for (const file of files) {
        editingColor.value.newImages.push(file);
        editingColor.value.newImagePreviews.push(URL.createObjectURL(file));
    }

    // Reset file input
    existingColorImageInputs.value[index].value = null;
};

const removeImage = (index) => {
    newColor.value.images.splice(index, 1);

    // Revoke object URL to prevent memory leaks
    URL.revokeObjectURL(newColor.value.imagePreviews[index]);
    newColor.value.imagePreviews.splice(index, 1);

    // Adjust main image index if needed
    if (newColor.value.mainImageIndex >= newColor.value.images.length) {
        newColor.value.mainImageIndex = newColor.value.images.length - 1;
    }
};

const setAsMainImage = (index) => {
    newColor.value.mainImageIndex = index;
};

// Quantity adjustment modal states
const showQuantityModal = ref(false);
const quantityModalType = ref(""); // 'add' or 'subtract'
const adjustmentQuantity = ref(0);
const currentEditingColorId = ref(null);

// Show quantity adjustment modal
const showQuantityAdjustmentModal = (type, colorId) => {
    quantityModalType.value = type;
    currentEditingColorId.value = colorId;
    adjustmentQuantity.value = 0;
    showQuantityModal.value = true;
};

// Apply quantity adjustment
const applyQuantityAdjustment = () => {
    if (adjustmentQuantity.value <= 0) {
        showErrorNotification("Số lượng phải lớn hơn 0");
        return;
    }

    if (quantityModalType.value === "add") {
        editingColor.value.quantity += adjustmentQuantity.value;
    } else if (quantityModalType.value === "subtract") {
        const newQuantity =
            editingColor.value.quantity - adjustmentQuantity.value;
        editingColor.value.quantity = Math.max(0, newQuantity);
    }

    closeQuantityModal();
};

// Close quantity modal
const closeQuantityModal = () => {
    showQuantityModal.value = false;
    quantityModalType.value = "";
    adjustmentQuantity.value = 0;
    currentEditingColorId.value = null;
};

// Color management methods
const addNewColor = async () => {
    if (!canAddColor()) return;

    addingColor.value = true;

    try {
        const colorData = {
            name: newColor.value.name.trim(),
            quantity: newColor.value.quantity,
            mainImageIndex: newColor.value.mainImageIndex,
            images: newColor.value.images,
        };
        await productService.createProductColor(props.productId, colorData);

        // Refresh color data immediately
        await refreshColorData();

        // Emit event to refresh product data
        emit("color-updated");

        // Reset form
        newColor.value = {
            name: "",
            quantity: 0,
            images: [],
            imagePreviews: [],
            mainImageIndex: 0,
        };

        // Show notification
        showSuccessNotification("Đã thêm màu mới thành công");
    } catch (error) {
        showErrorNotification("Có lỗi xảy ra khi thêm màu sản phẩm.");
    } finally {
        addingColor.value = false;
    }
};

const startEditColor = (color) => {
    editingColorId.value = color.id; // Store full color object for reference during editing
    const originalName = color.name ? color.name.trim() : "";

    editingColor.value = {
        id: color.id,
        name: color.name || "",
        quantity: color.quantity || 0,
        originalName: originalName, // Store trimmed original name for accurate comparison
        existingImages: color.images || [],
        newImages: [],
        newImagePreviews: [],
        removeImageIds: [],
        mainImageId: null,
    };

    // Find main image if exists
    const mainImage = color.images?.find((img) => img.isMain);
    if (mainImage) {
        editingColor.value.mainImageId = mainImage.id;
    }
};

const cancelEditColor = () => {
    // Revoke any object URLs to prevent memory leaks
    if (editingColor.value.newImagePreviews) {
        editingColor.value.newImagePreviews.forEach((url) =>
            URL.revokeObjectURL(url)
        );
    }

    editingColorId.value = null;
    editingColor.value = {
        name: "",
        quantity: 0,
        newImages: [],
        newImagePreviews: [],
        mainImageIndex: 0,
        newMainImageIndex: undefined,
        removeImageIds: [],
        mainImageId: null,
        originalName: "",
        existingImages: [],
    };
};

const saveColorChanges = async (colorId) => {
    if (!editingColor.value.name.trim() || editingColor.value.quantity < 0) {
        showErrorNotification(
            "Tên màu không được để trống và số lượng phải lớn hơn hoặc bằng 0"
        );
        return;
    }

    updatingColors[colorId] = true;
    try {
        const formData = new FormData();

        // Only append name if it has changed from the original
        const trimmedName = editingColor.value.name.trim();
        const isNameChanged = trimmedName !== editingColor.value.originalName;
        console.log(
            `Color name: ${trimmedName}, Original: ${editingColor.value.originalName}, Changed: ${isNameChanged}`
        );

        if (isNameChanged) {
            formData.append("Name", trimmedName);
            console.log("Sending updated color name to server");
        } else {
            console.log("Color name unchanged, not sending to server");
        }

        // Always append quantity
        formData.append("Quantity", editingColor.value.quantity.toString()); // Add main image ID if exists (for existing images)
        if (editingColor.value.mainImageId) {
            formData.append(
                "MainImageId",
                editingColor.value.mainImageId.toString()
            );
            console.log(
                "Setting main image from existing:",
                editingColor.value.mainImageId
            );
        }

        // Add images to remove if any
        if (
            editingColor.value.removeImageIds &&
            editingColor.value.removeImageIds.length > 0
        ) {
            for (const id of editingColor.value.removeImageIds) {
                formData.append("RemoveImageIds", id.toString());
            }
            console.log("Removing images:", editingColor.value.removeImageIds);
        }

        // Add new images if any
        if (
            editingColor.value.newImages &&
            editingColor.value.newImages.length > 0
        ) {
            for (const image of editingColor.value.newImages) {
                formData.append("AddImages", image);
            }

            // Set main image index for new images if selected
            if (editingColor.value.newMainImageIndex !== undefined) {
                formData.append(
                    "MainImageIndex",
                    editingColor.value.newMainImageIndex.toString()
                );
                console.log(
                    "Setting main image from new images at index:",
                    editingColor.value.newMainImageIndex
                );
            }
        }

        const formEntries = [...formData.entries()].map(([key, value]) => {
            return `${key}: ${value instanceof File ? value.name : value}`;
        });
        console.log("Form data being sent:", formEntries); // Send update request to API
        const response = await productService.updateProductColor(
            props.productId,
            colorId,
            formData
        );

        // Emit event to refresh product data and close modal
        emit("color-updated");

        // Reset edit state
        cancelEditColor();

        // Show notification
        showSuccessNotification("Đã cập nhật màu sản phẩm thành công");
    } catch (error) {
        showErrorNotification("Có lỗi xảy ra khi cập nhật màu sản phẩm.");
    } finally {
        updatingColors[colorId] = false;
    }
};

const toggleDeleteConfirm = (colorId) => {
    if (colorPendingDelete.value === colorId) {
        colorPendingDelete.value = null;
    } else {
        colorPendingDelete.value = colorId;
    }
};

const deleteColor = async (colorId) => {
    if (!colorId) return;

    deletingColor.value = colorId;

    try {
        await productService.deleteProductColor(props.productId, colorId);

        // Emit event to refresh product data
        emit("color-updated");

        // Show notification
        showSuccessNotification("Đã xóa màu sản phẩm thành công");
    } catch (error) {
        showErrorNotification("Có lỗi xảy ra khi xóa màu sản phẩm.");
    } finally {
        deletingColor.value = null;
        colorPendingDelete.value = null;
    }
};

const setMainImage = (imageId) => {
    editingColor.value.mainImageId = imageId;
    editingColor.value.newMainImageIndex = undefined; // Clear new image main selection
};

const setMainImageForNewImage = (index) => {
    editingColor.value.newMainImageIndex = index;
    editingColor.value.mainImageId = null; // Clear existing image main selection
};

const removeExistingImage = (imageId) => {
    if (!editingColor.value.removeImageIds) {
        editingColor.value.removeImageIds = [];
    }

    editingColor.value.removeImageIds.push(imageId);

    // If removing the current main image, clear the main selection
    if (editingColor.value.mainImageId === imageId) {
        editingColor.value.mainImageId = null;
    }
};

const removeNewImage = (index) => {
    editingColor.value.newImages.splice(index, 1);

    // Revoke object URL to prevent memory leaks
    URL.revokeObjectURL(editingColor.value.newImagePreviews[index]);
    editingColor.value.newImagePreviews.splice(index, 1);

    // Adjust main image index if needed
    if (editingColor.value.newMainImageIndex === index) {
        editingColor.value.newMainImageIndex = undefined;
    } else if (editingColor.value.newMainImageIndex > index) {
        editingColor.value.newMainImageIndex--;
    }
};
</script>

<template>
    <div class="form-section">
        <div class="section-header">
            <h4 class="section-title">
                <i class="fas fa-palette"></i> Quản lý màu sắc
            </h4>
        </div>

        <!-- Add new color form -->
        <div class="color-form">
            <h5>
                <i
                    class="fas fa-plus-circle"
                    style="color: var(--primary-color, #f86ed3)"
                ></i>
                Thêm màu mới
            </h5>

            <div class="form-row">
                <div class="form-group">
                    <label for="colorName"
                        >Tên màu <span class="required">*</span></label
                    >
                    <input
                        type="text"
                        id="colorName"
                        v-model="newColor.name"
                        placeholder="Ví dụ: Đen, Trắng, Xanh,..."
                    />
                </div>
                <div class="form-group">
                    <label for="colorQuantity">Số lượng</label>
                    <input
                        type="number"
                        id="colorQuantity"
                        v-model="newColor.quantity"
                        placeholder="Nhập số lượng (từ 0 trở lên)"
                        min="0"
                    />
                </div>
            </div>

            <div class="image-upload-container">
                <label>Hình ảnh</label>

                <input
                    type="file"
                    ref="colorImageInput"
                    @change="handleImageUpload"
                    accept="image/jpeg,image/png,image/jpg"
                    multiple
                    class="hidden-input"
                    style="display: none"
                />

                <div class="image-upload-area" @click="triggerImageUpload">
                    <div class="upload-placeholder">
                        <i class="fas fa-cloud-upload-alt"></i>
                        <p>Nhấp để tải lên hình ảnh</p>
                        <small>Định dạng: PNG, JPG. Tối đa 2MB mỗi ảnh.</small>
                    </div>
                </div>

                <div
                    v-if="newColor.imagePreviews.length > 0"
                    class="image-preview-grid"
                >
                    <div
                        v-for="(preview, index) in newColor.imagePreviews"
                        :key="index"
                        :class="[
                            'image-preview-item',
                            { 'main-image': index === newColor.mainImageIndex },
                        ]"
                    >
                        <img :src="preview" alt="Preview" />
                        <div class="image-actions">
                            <button
                                type="button"
                                @click.stop="setAsMainImage(index)"
                                :class="[
                                    'set-main-btn',
                                    {
                                        active:
                                            index === newColor.mainImageIndex,
                                    },
                                ]"
                                title="Đặt làm ảnh chính"
                            >
                                <i class="fas fa-star"></i>
                            </button>
                            <button
                                type="button"
                                @click.stop="removeImage(index)"
                                class="remove-btn"
                                title="Xóa ảnh"
                            >
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <div class="color-form-actions">
                <button
                    type="button"
                    class="add-color-btn"
                    @click="addNewColor"
                    :disabled="!canAddColor() || addingColor"
                >
                    <span v-if="addingColor" class="spinner small"></span>
                    <span v-else> <i class="fas fa-plus"></i> Thêm màu </span>
                </button>
            </div>
        </div>
        <!-- Existing colors list -->
        <div v-if="colors.length > 0" class="existing-colors">
            <h5>
                <i
                    class="fas fa-list"
                    style="color: var(--primary-color, #f86ed3)"
                ></i>
                Màu sắc hiện có
            </h5>

            <div class="color-list">
                <div
                    v-for="(color, colorIndex) in colors"
                    :key="color.id"
                    class="color-item"
                >
                    <div class="color-item-header">
                        <h6>{{ color.name }}</h6>
                        <div class="color-actions">
                            <button
                                v-if="
                                    editingColorId !== color.id &&
                                    colorPendingDelete !== color.id
                                "
                                type="button"
                                class="edit-btn"
                                @click="startEditColor(color)"
                                title="Chỉnh sửa"
                            >
                                <i class="fas fa-edit"></i>
                            </button>
                            <button
                                v-if="
                                    editingColorId !== color.id &&
                                    colorPendingDelete !== color.id
                                "
                                type="button"
                                class="delete-btn"
                                @click="toggleDeleteConfirm(color.id)"
                                title="Xóa"
                            >
                                <i class="fas fa-trash"></i>
                            </button>

                            <!-- Delete confirmation inline -->
                            <div
                                v-if="colorPendingDelete === color.id"
                                class="delete-confirm-actions"
                            >
                                <span class="delete-confirm-text"
                                    >Xác nhận xóa?</span
                                >
                                <button
                                    type="button"
                                    class="delete-cancel-btn"
                                    @click="toggleDeleteConfirm(null)"
                                    :disabled="deletingColor === color.id"
                                >
                                    <i class="fas fa-times"></i>
                                </button>
                                <button
                                    type="button"
                                    class="delete-confirm-btn"
                                    @click="deleteColor(color.id)"
                                    :disabled="deletingColor === color.id"
                                >
                                    <span
                                        v-if="deletingColor === color.id"
                                        class="spinner small"
                                    ></span>
                                    <i v-else class="fas fa-check"></i>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="color-item-body">
                        <template v-if="editingColorId !== color.id">
                            <div class="color-info">
                                <span>Số lượng: {{ color.quantity || 0 }}</span>
                            </div>

                            <div class="image-thumbnails">
                                <div
                                    v-for="image in color.images"
                                    :key="image.id"
                                    :class="[
                                        'thumbnail',
                                        { 'main-thumbnail': image.isMain },
                                    ]"
                                >
                                    <img
                                        :src="formatImageUrl(image.imagePath)"
                                        alt="Color image"
                                    />
                                </div>
                            </div>
                        </template>
                        <div v-else class="color-edit-form">
                            <div class="form-row">
                                <div class="form-group">
                                    <label :for="`editColorName${color.id}`"
                                        >Tên màu</label
                                    >
                                    <input
                                        :id="`editColorName${color.id}`"
                                        type="text"
                                        v-model="editingColor.name"
                                        placeholder="Nhập tên màu"
                                    />
                                </div>

                                <div class="form-group">
                                    <label :for="`editColorQuantity${color.id}`"
                                        >Số lượng hiện tại</label
                                    >
                                    <div class="quantity-display-wrapper">
                                        <span class="current-quantity">{{
                                            editingColor.quantity
                                        }}</span>
                                        <div class="quantity-actions">
                                            <button
                                                type="button"
                                                class="quantity-action-btn add-btn"
                                                @click="
                                                    showQuantityAdjustmentModal(
                                                        'add',
                                                        color.id
                                                    )
                                                "
                                                title="Thêm số lượng"
                                            >
                                                <i class="fas fa-plus"></i> Thêm
                                            </button>
                                            <button
                                                type="button"
                                                class="quantity-action-btn subtract-btn"
                                                @click="
                                                    showQuantityAdjustmentModal(
                                                        'subtract',
                                                        color.id
                                                    )
                                                "
                                                :disabled="
                                                    editingColor.quantity <= 0
                                                "
                                                title="Giảm số lượng"
                                            >
                                                <i class="fas fa-minus"></i>
                                                Giảm
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="existing-images">
                                <label>Hình ảnh hiện tại</label>
                                <div class="image-preview-grid">
                                    <div
                                        v-for="image in editingColor.existingImages.filter(
                                            (img) =>
                                                !editingColor.removeImageIds?.includes(
                                                    img.id
                                                )
                                        )"
                                        :key="image.id"
                                        :class="[
                                            'image-preview-item',
                                            {
                                                'main-image':
                                                    image.id ===
                                                    editingColor.mainImageId,
                                            },
                                        ]"
                                    >
                                        <img
                                            :src="
                                                formatImageUrl(image.imagePath)
                                            "
                                            alt="Color image"
                                        />
                                        <div class="image-actions">
                                            <button
                                                type="button"
                                                @click.stop="
                                                    setMainImage(image.id)
                                                "
                                                :class="[
                                                    'set-main-btn',
                                                    {
                                                        active:
                                                            image.id ===
                                                            editingColor.mainImageId,
                                                    },
                                                ]"
                                                title="Đặt làm ảnh chính"
                                            >
                                                <i class="fas fa-star"></i>
                                            </button>
                                            <button
                                                type="button"
                                                @click.stop="
                                                    removeExistingImage(
                                                        image.id
                                                    )
                                                "
                                                class="remove-btn"
                                                title="Xóa ảnh"
                                            >
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="image-upload-container">
                                <label>Thêm hình ảnh mới</label>

                                <input
                                    type="file"
                                    :ref="
                                        (el) => {
                                            existingColorImageInputs[
                                                colorIndex
                                            ] = el;
                                        }
                                    "
                                    @change="
                                        () =>
                                            handleExistingColorImageUpload(
                                                color.id,
                                                colorIndex
                                            )
                                    "
                                    accept="image/jpeg,image/png,image/jpg"
                                    multiple
                                    class="hidden-input"
                                    style="display: none"
                                />

                                <div
                                    class="image-upload-area"
                                    @click="
                                        () => triggerEditImageUpload(colorIndex)
                                    "
                                >
                                    <div class="upload-placeholder">
                                        <i class="fas fa-cloud-upload-alt"></i>
                                        <p>Nhấp để tải lên hình ảnh mới</p>
                                        <small
                                            >Định dạng: PNG, JPG. Tối đa 2MB mỗi
                                            ảnh.</small
                                        >
                                    </div>
                                </div>

                                <div
                                    v-if="
                                        editingColor.newImagePreviews?.length >
                                        0
                                    "
                                    class="image-preview-grid"
                                >
                                    <div
                                        v-for="(
                                            preview, index
                                        ) in editingColor.newImagePreviews"
                                        :key="'new-' + index"
                                        :class="[
                                            'image-preview-item',
                                            {
                                                'main-image':
                                                    index ===
                                                    editingColor.newMainImageIndex,
                                            },
                                        ]"
                                    >
                                        <img :src="preview" alt="Preview" />
                                        <div class="image-actions">
                                            <button
                                                type="button"
                                                @click.stop="
                                                    setMainImageForNewImage(
                                                        index
                                                    )
                                                "
                                                :class="[
                                                    'set-main-btn',
                                                    {
                                                        active:
                                                            index ===
                                                            editingColor.newMainImageIndex,
                                                    },
                                                ]"
                                                title="Đặt làm ảnh chính"
                                            >
                                                <i class="fas fa-star"></i>
                                            </button>
                                            <button
                                                type="button"
                                                @click.stop="
                                                    removeNewImage(index)
                                                "
                                                class="remove-btn"
                                                title="Xóa ảnh"
                                            >
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="color-form-actions">
                                <button
                                    type="button"
                                    class="cancel-edit-btn"
                                    @click="cancelEditColor"
                                >
                                    <i class="fas fa-times"></i> Hủy
                                </button>
                                <button
                                    type="button"
                                    class="save-color-btn"
                                    @click="saveColorChanges(color.id)"
                                    :disabled="updatingColors[color.id]"
                                >
                                    <span
                                        v-if="updatingColors[color.id]"
                                        class="spinner small"
                                    ></span>
                                    <span v-else>
                                        <i class="fas fa-save"></i> Lưu thay đổi
                                    </span>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Quantity Adjustment Modal -->
    <div
        v-if="showQuantityModal"
        class="modal-overlay"
        @click="closeQuantityModal"
    >
        <div class="quantity-modal" @click.stop>
            <div class="modal-header">
                <h3>
                    <i
                        :class="
                            quantityModalType === 'add'
                                ? 'fas fa-plus-circle'
                                : 'fas fa-minus-circle'
                        "
                    ></i>
                    {{
                        quantityModalType === "add"
                            ? "Thêm số lượng"
                            : "Giảm số lượng"
                    }}
                </h3>
                <button
                    type="button"
                    class="close-modal-btn"
                    @click="closeQuantityModal"
                >
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="modal-body">
                <div class="form-group">
                    <label for="adjustmentQuantity">
                        {{
                            quantityModalType === "add"
                                ? "Số lượng cần thêm"
                                : "Số lượng cần giảm"
                        }}
                    </label>
                    <input
                        id="adjustmentQuantity"
                        type="number"
                        v-model="adjustmentQuantity"
                        :placeholder="
                            quantityModalType === 'add'
                                ? 'Nhập số lượng cần thêm'
                                : 'Nhập số lượng cần giảm'
                        "
                        min="1"
                        max="9999"
                        @keyup.enter="applyQuantityAdjustment"
                        autofocus
                    />
                </div>

                <div
                    v-if="quantityModalType === 'subtract'"
                    class="quantity-info"
                >
                    <p>
                        <strong>Số lượng hiện tại:</strong>
                        {{ editingColor.quantity }}
                    </p>
                    <p
                        v-if="adjustmentQuantity > editingColor.quantity"
                        class="warning-text"
                    >
                        <i class="fas fa-exclamation-triangle"></i>
                        Số lượng giảm sẽ được điều chỉnh về 0 (không thể âm)
                    </p>
                </div>
            </div>

            <div class="modal-actions">
                <button
                    type="button"
                    class="cancel-btn"
                    @click="closeQuantityModal"
                >
                    Hủy
                </button>
                <button
                    type="button"
                    class="apply-btn"
                    @click="applyQuantityAdjustment"
                    :disabled="adjustmentQuantity <= 0"
                >
                    <i
                        :class="
                            quantityModalType === 'add'
                                ? 'fas fa-plus'
                                : 'fas fa-minus'
                        "
                    ></i>
                    {{ quantityModalType === "add" ? "Thêm" : "Giảm" }}
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.color-form {
    background-color: #ffffff;
    border-radius: 8px;
    padding: 1.25rem;
    margin-bottom: 1.5rem;
    transition: all 0.3s;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.color-form:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.color-form h5,
.existing-colors h5 {
    font-size: 1rem;
    margin: 0 0 1rem 0;
    color: #444;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.image-upload-container {
    margin-bottom: 1rem;
}

.image-upload-area {
    border: 2px dashed #d1d5db;
    border-radius: 8px;
    padding: 2rem 1rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.3s;
}

.image-upload-area:hover {
    border-color: var(--primary-color, #f86ed3);
    background-color: rgba(248, 110, 211, 0.05);
}

.upload-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
    color: #666;
}

.upload-placeholder i {
    font-size: 2rem;
    margin-bottom: 0.5rem;
    color: var(--primary-color, #f86ed3);
}

.upload-placeholder p {
    margin: 0.5rem 0;
}

.upload-placeholder small {
    font-size: 0.8rem;
}

.image-preview-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
    gap: 1rem;
    margin-top: 1rem;
}

.image-preview-item {
    position: relative;
    border-radius: 8px;
    overflow: hidden;
    aspect-ratio: 1;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    border: 2px solid #e5e7eb;
}

.image-preview-item.main-image {
    border-color: var(--primary-color, #f86ed3);
}

.image-preview-item img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.image-actions {
    position: absolute;
    top: 0;
    right: 0;
    display: flex;
    flex-direction: column;
    background-color: rgba(0, 0, 0, 0.4);
    border-bottom-left-radius: 8px;
}

.image-actions button {
    background: none;
    border: none;
    padding: 0.35rem;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    color: white;
    font-size: 0.8rem;
}

.set-main-btn.active {
    color: var(--primary-color, #f86ed3);
}

.color-form-actions {
    display: flex;
    justify-content: flex-end;
    margin-top: 1rem;
    gap: 0.5rem;
}

.add-color-btn,
.save-color-btn {
    background-color: var(--primary-color, #f86ed3);
    color: white;
    border: none;
    border-radius: 8px;
    padding: 0.75rem 1.25rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
    font-weight: 500;
}

.add-color-btn:hover,
.save-color-btn:hover {
    background-color: #e94e9c;
}

.add-color-btn:disabled,
.save-color-btn:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.cancel-edit-btn {
    background-color: #ffffff;
    border: 1px solid #ddd;
    color: #666;
    border-radius: 8px;
    padding: 0.75rem 1.25rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-weight: 500;
    transition: all 0.2s;
}

.cancel-edit-btn:hover {
    background-color: #eee;
}

.existing-colors {
    margin-top: 2rem;
}

.color-list {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.color-item {
    background-color: white;
    border-radius: 8px;
    border: 1px solid #e5e7eb;
    overflow: hidden;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.color-item:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.color-item-header {
    background-color: #ffffff;
    padding: 0.75rem 1rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.color-item-header h6 {
    margin: 0;
    font-size: 0.95rem;
    color: #333;
    font-weight: 600;
}

.color-actions {
    display: flex;
    gap: 0.5rem;
}

.delete-btn {
    background: none;
    border: none;
    color: #ef4444;
    cursor: pointer;
    padding: 5px;
    border-radius: 4px;
    transition: background-color 0.2s;
}

.delete-btn:hover {
    background-color: rgba(239, 68, 68, 0.1);
}

.edit-btn {
    background: none;
    border: none;
    color: var(--primary-color, #f86ed3);
    cursor: pointer;
    padding: 5px;
    border-radius: 4px;
    transition: background-color 0.2s;
}

.edit-btn:hover {
    background-color: rgba(248, 110, 211, 0.1);
}

/* Delete confirmation inline styles */
.delete-confirm-actions {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    animation: slideInRight 0.2s ease-out;
}

@keyframes slideInRight {
    from {
        transform: translateX(20px);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

.delete-confirm-text {
    font-size: 0.85rem;
    color: #ef4444;
    margin-right: 0.25rem;
    font-weight: 500;
}

.delete-cancel-btn,
.delete-confirm-btn {
    width: 28px;
    height: 28px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    cursor: pointer;
    transition: all 0.2s;
}

.delete-cancel-btn {
    background-color: #f3f4f6;
    color: #4b5563;
}

.delete-cancel-btn:hover {
    background-color: #d1d5db;
    transform: scale(1.05);
}

.delete-confirm-btn {
    background-color: #ef4444;
    color: white;
}

.delete-confirm-btn:hover {
    background-color: #dc2626;
    transform: scale(1.05);
    box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3);
}

.color-item-body {
    padding: 0.75rem 1rem;
}

.color-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.75rem;
}

.color-info span {
    font-size: 0.9rem;
    color: #555;
}

.image-thumbnails {
    display: flex;
    gap: 0.75rem;
    flex-wrap: wrap;
}

.thumbnail {
    width: 50px;
    height: 50px;
    border-radius: 4px;
    overflow: hidden;
    border: 1px solid #e5e7eb;
    transition: transform 0.2s;
}

.thumbnail:hover {
    transform: scale(1.05);
}

.thumbnail.main-thumbnail {
    border-color: var(--primary-color, #f86ed3);
}

.thumbnail img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.color-edit-form {
    padding: 1rem;
    background-color: #ffffff;
    border-top: 1px solid #e5e7eb;
}

.confirm-modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1100;
    backdrop-filter: blur(3px);
    animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
    from {
        opacity: 0;
    }
    to {
        opacity: 1;
    }
}

.confirm-dialog {
    background-color: white;
    border-radius: 12px;
    width: 400px;
    max-width: 95%;
    box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
    transform: translateY(0);
    animation: slideIn 0.3s ease;
    overflow: hidden;
}

@keyframes slideIn {
    from {
        transform: translateY(20px);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

.existing-images {
    margin-bottom: 1.5rem;
}

.existing-images label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    color: #555;
    font-weight: 500;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.section-title {
    margin: 0;
    font-weight: 600;
    font-size: 1.1rem;
    color: #444;
}

.section-title i {
    margin-right: 8px;
    color: var(--primary-color, #f86ed3);
}

/* Form fields styling */
.form-row {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-bottom: 1rem;
}

.form-group {
    flex: 1;
    min-width: 200px;
    margin-bottom: 1rem;
}

.form-group label,
.image-upload-container label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    font-weight: 500;
    color: #555;
}

.form-group input,
.form-group select,
.form-group textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 0.95rem;
    background-color: #fff;
    transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
    border-color: var(--primary-color, #f86ed3);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

/* Quantity input wrapper styles */
.quantity-input-wrapper {
    display: flex;
    align-items: center;
    border: 1px solid #ddd;
    border-radius: 6px;
    background-color: #fff;
    overflow: hidden;
    transition: border-color 0.2s;
}

.quantity-input-wrapper:focus-within {
    border-color: var(--primary-color, #f86ed3);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
}

.quantity-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f8f9fa;
    border: none;
    width: 40px;
    height: 40px;
    color: #495057;
    cursor: pointer;
    transition: all 0.2s;
    font-size: 0.9rem;
}

.quantity-btn:hover:not(:disabled) {
    background-color: var(--primary-color, #f86ed3);
    color: white;
}

.quantity-btn:disabled {
    background-color: #e9ecef;
    color: #adb5bd;
    cursor: not-allowed;
}

.quantity-btn.decrease-btn {
    border-right: 1px solid #ddd;
}

.quantity-btn.increase-btn {
    border-left: 1px solid #ddd;
}

.quantity-input-wrapper input {
    border: none;
    background: transparent;
    text-align: center;
    font-weight: 500;
    flex: 1;
    padding: 0.75rem 0.5rem;
    outline: none;
    min-width: 60px;
}

.quantity-input-wrapper input:focus {
    border: none;
    box-shadow: none;
}

/* Quantity display wrapper styles for editing colors */
.quantity-display-wrapper {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.current-quantity {
    font-size: 1.1rem;
    font-weight: 600;
    color: #333;
    background-color: #f8f9fa;
    padding: 0.5rem 1rem;
    border-radius: 6px;
    border: 1px solid #ddd;
    min-width: 60px;
    text-align: center;
}

.quantity-actions {
    display: flex;
    gap: 0.5rem;
}

.quantity-action-btn {
    display: flex;
    align-items: center;
    gap: 0.3rem;
    padding: 0.5rem 0.8rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.85rem;
    font-weight: 500;
    transition: all 0.2s;
}

.quantity-action-btn.add-btn {
    background-color: #d4edda;
    color: #155724;
    border: 1px solid #c3e6cb;
}

.quantity-action-btn.add-btn:hover {
    background-color: #c3e6cb;
    transform: translateY(-1px);
}

.quantity-action-btn.subtract-btn {
    background-color: #f8d7da;
    color: #721c24;
    border: 1px solid #f5c6cb;
}

.quantity-action-btn.subtract-btn:hover:not(:disabled) {
    background-color: #f5c6cb;
    transform: translateY(-1px);
}

.quantity-action-btn:disabled {
    background-color: #e9ecef;
    color: #6c757d;
    cursor: not-allowed;
    border-color: #dee2e6;
}

/* Quantity Modal Styles */
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1200;
    backdrop-filter: blur(3px);
    animation: fadeIn 0.2s ease;
}

.quantity-modal {
    background-color: white;
    border-radius: 12px;
    width: 450px;
    max-width: 95%;
    box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
    transform: translateY(0);
    animation: slideIn 0.3s ease;
    overflow: hidden;
}

.modal-header {
    padding: 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
    background-color: #fafafa;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.2rem;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.modal-header h3 i {
    color: var(--primary-color, #f86ed3);
}

.close-modal-btn {
    background: none;
    border: none;
    font-size: 1.2rem;
    color: #666;
    cursor: pointer;
    padding: 0.5rem;
    border-radius: 50%;
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.close-modal-btn:hover {
    background-color: #f0f0f0;
    color: #333;
}

.modal-body {
    padding: 1.5rem;
}

.modal-body .form-group {
    margin-bottom: 1rem;
}

.modal-body label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #555;
}

.modal-body input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
    transition: border-color 0.2s;
}

.modal-body input:focus {
    border-color: var(--primary-color, #f86ed3);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.quantity-info {
    margin-top: 1rem;
    padding: 1rem;
    background-color: #f8f9fa;
    border-radius: 6px;
    border-left: 4px solid #007bff;
}

.quantity-info p {
    margin: 0;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
}

.quantity-info p:last-child {
    margin-bottom: 0;
}

.warning-text {
    color: #856404;
    background-color: #fff3cd;
    border: 1px solid #ffeaa7;
    padding: 0.5rem;
    border-radius: 4px;
    font-size: 0.85rem;
}

.warning-text i {
    margin-right: 0.3rem;
}

.modal-actions {
    padding: 1rem 1.5rem;
    border-top: 1px solid #eee;
    display: flex;
    justify-content: flex-end;
    gap: 0.75rem;
    background-color: #fafafa;
}

.cancel-btn {
    padding: 0.6rem 1.2rem;
    background-color: #f8f9fa;
    border: 1px solid #dee2e6;
    color: #6c757d;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s;
}

.cancel-btn:hover {
    background-color: #e9ecef;
}

.apply-btn {
    padding: 0.6rem 1.2rem;
    background-color: var(--primary-color, #f86ed3);
    border: none;
    color: white;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 0.3rem;
    font-weight: 500;
}

.apply-btn:hover:not(:disabled) {
    background-color: #e94e9c;
    transform: translateY(-1px);
}

.apply-btn:disabled {
    background-color: #dee2e6;
    color: #6c757d;
    cursor: not-allowed;
}

.required {
    color: #ef4444;
}

.spinner.small {
    width: 16px;
    height: 16px;
    border: 2px solid rgba(248, 110, 211, 0.1);
    border-radius: 50%;
    border-top-color: white;
    animation: spin 1s linear infinite;
    margin-right: 0.5rem;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}
</style>
