<script setup>
import { ref, defineProps, defineEmits } from "vue";

const props = defineProps({
    formData: {
        type: Object,
        required: true,
    },
    formErrors: {
        type: Object,
        required: true,
    },
    isEditing: {
        type: Boolean,
        default: false,
    },
});

const emit = defineEmits(["update:formData", "update:formErrors"]);

// State for new color input
const newColor = ref({
    name: "",
    quantity: 1,
    images: [],
    imagePreviews: [],
    mainImageIndex: 0,
});

// Format image URL
const formatImageUrl = (imagePath) => {
    if (!imagePath) return null;

    // If image path is already a full URL or base64 data
    if (imagePath.startsWith("http") || imagePath.startsWith("data:"))
        return imagePath;

    // Get base URL from API config
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Normalize the path (convert \ to /)
    const normalizedPath = imagePath.replace(/\\/g, "/");

    // Check if path starts with /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Handle color image upload
const handleColorImageUpload = (event) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    // Clear any previous errors
    const updatedFormErrors = { ...props.formErrors };
    updatedFormErrors.colors = "";
    emit("update:formErrors", updatedFormErrors);

    // Process each file
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Validate file type
        if (!["image/jpeg", "image/png", "image/jpg"].includes(file.type)) {
            updatedFormErrors.colors =
                "Chỉ chấp nhận các định dạng: jpg, jpeg, png";
            emit("update:formErrors", updatedFormErrors);
            continue;
        }

        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            updatedFormErrors.colors = "Kích thước tệp tối đa là 5MB";
            emit("update:formErrors", updatedFormErrors);
            continue;
        }

        // Add file to the list
        newColor.value.images.push(file);

        // Create preview
        const reader = new FileReader();
        reader.onload = (e) => {
            newColor.value.imagePreviews.push(e.target.result);
        };
        reader.readAsDataURL(file);
    }

    // Clear the file input for future uploads
    if (event.target) {
        event.target.value = "";
    }
};

// Set main image for a color
const setColorMainImage = (index, colorIndex) => {
    if (colorIndex !== undefined) {
        // Setting main image for an existing color
        const updatedFormData = { ...props.formData };
        updatedFormData.colorData[colorIndex].mainImageIndex = index;
        emit("update:formData", updatedFormData);
    } else {
        // Setting main image for a new color
        newColor.value.mainImageIndex = index;
    }
};

// Remove an image from a color
const removeColorImage = (index) => {
    // If removing the main image, select a new main image
    if (index === newColor.value.mainImageIndex) {
        if (newColor.value.imagePreviews.length > 1) {
            const newMainIndex = index === 0 ? 1 : 0;
            newColor.value.mainImageIndex = newMainIndex;
        } else {
            newColor.value.mainImageIndex = 0;
        }
    } else if (index < newColor.value.mainImageIndex) {
        // If removing an image before the main image, adjust the main image index
        newColor.value.mainImageIndex--;
    }

    // Remove from arrays
    newColor.value.images.splice(index, 1);
    newColor.value.imagePreviews.splice(index, 1);
};

// Add a color with its images to the product
const addColorWithImages = () => {
    // Validate color name
    if (!newColor.value.name.trim()) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors = "Vui lòng nhập tên màu";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    // Validate images
    if (newColor.value.images.length === 0) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors =
            "Vui lòng thêm ít nhất một hình ảnh cho màu này";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    // Validate quantity
    if (!newColor.value.quantity || newColor.value.quantity < 1) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors = "Số lượng phải lớn hơn 0";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    // Check if the color already exists
    const colorExists = props.formData.colorData.some(
        (color) =>
            color.name.toLowerCase() === newColor.value.name.toLowerCase()
    );

    if (colorExists) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors = "Màu này đã được thêm";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    // Add the color data to the form
    const updatedFormData = { ...props.formData };
    updatedFormData.colorData.push({
        name: newColor.value.name,
        quantity: newColor.value.quantity,
        images: [...newColor.value.images],
        imagePreviews: [...newColor.value.imagePreviews],
        mainImageIndex: newColor.value.mainImageIndex,
    });

    // For backward compatibility, also add to simple colors array
    updatedFormData.colors.push(newColor.value.name);
    emit("update:formData", updatedFormData);

    // Reset new color form
    resetNewColor();
};

// Remove a color from the product
const removeColorData = (index) => {
    const updatedFormData = { ...props.formData };
    // Also remove from the simple colors array for backward compatibility
    const colorName = updatedFormData.colorData[index].name;
    const simpleColorIndex = updatedFormData.colors.findIndex(
        (c) => c === colorName
    );
    if (simpleColorIndex !== -1) {
        updatedFormData.colors.splice(simpleColorIndex, 1);
    }

    // Remove the color data
    updatedFormData.colorData.splice(index, 1);
    emit("update:formData", updatedFormData);
};

// Reset the new color form
const resetNewColor = () => {
    newColor.value = {
        name: "",
        quantity: 1, // Default quantity
        images: [],
        imagePreviews: [],
        mainImageIndex: 0,
    };

    const updatedFormErrors = { ...props.formErrors };
    updatedFormErrors.colors = "";
    emit("update:formErrors", updatedFormErrors);
};

// Handle color-specific image operations for existing colors during editing
const handleColorImageUploadForExisting = (event, colorIndex) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    const updatedFormData = { ...props.formData };
    const colorData = updatedFormData.colorData[colorIndex];

    // Process each file
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Validate file type
        if (!["image/jpeg", "image/png", "image/jpg"].includes(file.type)) {
            const updatedFormErrors = { ...props.formErrors };
            updatedFormErrors.colors =
                "Chỉ chấp nhận các định dạng: jpg, jpeg, png";
            emit("update:formErrors", updatedFormErrors);
            continue;
        }

        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            const updatedFormErrors = { ...props.formErrors };
            updatedFormErrors.colors = "Kích thước tệp tối đa là 5MB";
            emit("update:formErrors", updatedFormErrors);
            continue;
        }

        // Initialize arrays if they don't exist
        if (!colorData.newImages) colorData.newImages = [];
        if (!colorData.newImagePreviews) colorData.newImagePreviews = [];

        // Add file to the list
        colorData.newImages.push(file);

        // Create preview
        const reader = new FileReader();
        reader.onload = (e) => {
            colorData.newImagePreviews.push(e.target.result);
            emit("update:formData", updatedFormData);
        };
        reader.readAsDataURL(file);
    }

    // Clear the file input for future uploads
    if (event.target) {
        event.target.value = "";
    }
};

// Set main image for existing color (either from existing images or new ones)
const setExistingColorMainImage = (
    colorIndex,
    imageIndex,
    isNewImage = false
) => {
    const updatedFormData = { ...props.formData };
    const colorData = updatedFormData.colorData[colorIndex];

    // Reset the main image ID since we're setting a new one
    colorData.mainImageId = null;

    if (isNewImage) {
        // Set main image from newly uploaded images
        colorData.mainImageIndex = imageIndex;

        // Reset any selected main image from existing images
        for (let i = 0; i < colorData.existingImages?.length || 0; i++) {
            if (colorData.existingImages[i]) {
                colorData.existingImages[i].isMain = false;
            }
        }
    } else {
        // Set main image from existing images
        const image = colorData.existingImages[imageIndex];
        if (image) {
            colorData.mainImageId = image.id;

            // Update isMain flag on all existing images for UI update
            for (let i = 0; i < colorData.existingImages.length; i++) {
                colorData.existingImages[i].isMain = i === imageIndex;
            }

            // Reset any main image from new images
            colorData.mainImageIndex = undefined;
        }
    }
    emit("update:formData", updatedFormData);
};

// Remove image from existing color
const removeExistingColorImage = (
    colorIndex,
    imageIndex,
    isNewImage = false
) => {
    const updatedFormData = { ...props.formData };
    const colorData = updatedFormData.colorData[colorIndex];

    if (isNewImage) {
        // Remove from new images
        colorData.newImages.splice(imageIndex, 1);
        colorData.newImagePreviews.splice(imageIndex, 1);

        // If we removed the main image from new images, reset main image index
        if (
            colorData.mainImageIndex !== undefined &&
            imageIndex === colorData.mainImageIndex
        ) {
            colorData.mainImageIndex =
                colorData.newImages.length > 0 ? 0 : undefined;
        } else if (
            colorData.mainImageIndex !== undefined &&
            imageIndex < colorData.mainImageIndex
        ) {
            // If removing an image before the main image, adjust the index
            colorData.mainImageIndex--;
        }
    } else {
        // Mark existing image for removal
        const image = colorData.existingImages[imageIndex];
        if (image) {
            // Initialize removedImageIds if it doesn't exist
            if (!colorData.removedImageIds) colorData.removedImageIds = [];
            colorData.removedImageIds.push(image.id);

            // If we removed the main image, unset mainImageId
            if (colorData.mainImageId === image.id) {
                colorData.mainImageId = null;

                // Try to set another existing image as main
                const remainingImages = colorData.existingImages.filter(
                    (img) => !colorData.removedImageIds.includes(img.id)
                );

                if (remainingImages.length > 0) {
                    colorData.mainImageId = remainingImages[0].id;
                } else if (
                    colorData.newImages &&
                    colorData.newImages.length > 0
                ) {
                    // If no existing images left, set first new image as main
                    colorData.mainImageIndex = 0;
                }
            }
        }
    }

    emit("update:formData", updatedFormData);
};

// Add new color during edit
const addNewColorDuringEdit = () => {
    // Use the same validation logic as addColorWithImages
    if (!newColor.value.name.trim()) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors = "Vui lòng nhập tên màu";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    if (newColor.value.images.length === 0) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors =
            "Vui lòng thêm ít nhất một hình ảnh cho màu này";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    const colorExists = props.formData.colorData.some(
        (color) =>
            color.name.toLowerCase() === newColor.value.name.toLowerCase()
    );

    if (colorExists) {
        const updatedFormErrors = { ...props.formErrors };
        updatedFormErrors.colors = "Màu này đã được thêm";
        emit("update:formErrors", updatedFormErrors);
        return;
    }

    // Add the color data to the form with isNewColor flag
    const updatedFormData = { ...props.formData };
    updatedFormData.colorData.push({
        name: newColor.value.name,
        quantity: newColor.value.quantity,
        images: [...newColor.value.images],
        imagePreviews: [...newColor.value.imagePreviews],
        mainImageIndex: newColor.value.mainImageIndex,
        isNewColor: true,
    });

    // For backward compatibility
    updatedFormData.colors.push(newColor.value.name);
    emit("update:formData", updatedFormData);

    // Reset new color form
    resetNewColor();
};

defineExpose({
    resetNewColor,
});
</script>

<template>
    <div class="form-section">
        <h4 class="section-title">Màu sắc và Hình ảnh</h4>

        <div class="color-section-layout">
            <div class="color-add-form-simple">
                <h5>Thêm màu sắc mới</h5>

                <div class="color-name-input">
                    <label for="colorNameInput">
                        Tên màu
                        <span class="required">*</span>
                    </label>
                    <input
                        type="text"
                        id="colorNameInput"
                        v-model="newColor.name"
                        placeholder="Ví dụ: Đen, Trắng, Xanh,..."
                    />
                </div>

                <!-- Thêm ô nhập số lượng cho màu sắc -->
                <div class="color-name-input">
                    <label for="colorQuantityInput">
                        Số lượng
                        <span class="required">*</span>
                    </label>
                    <input
                        type="number"
                        id="colorQuantityInput"
                        v-model="newColor.quantity"
                        min="1"
                        placeholder="Nhập số lượng cho màu này"
                    />
                </div>

                <div class="color-upload-wrapper-simple">
                    <label>
                        Hình ảnh màu sắc
                        <span class="required">*</span>
                    </label>
                    <div
                        class="color-upload-area-simple"
                        @click="$refs.colorImageInput.click()"
                    >
                        <input
                            type="file"
                            ref="colorImageInput"
                            class="hidden-input"
                            accept="image/png, image/jpeg, image/jpg"
                            multiple
                            @change="handleColorImageUpload"
                        />
                        <div class="upload-placeholder-simple">
                            <i class="fas fa-cloud-upload-alt"></i>
                            <p>Nhấn để tải hình ảnh lên</p>
                        </div>
                    </div>
                    <span v-if="formErrors.colors" class="error-message">
                        {{ formErrors.colors }}
                    </span>
                </div>

                <div
                    v-if="newColor.imagePreviews.length > 0"
                    class="color-preview-simple"
                >
                    <div class="preview-label">Xem trước hình ảnh:</div>
                    <div class="image-previews-grid">
                        <div
                            v-for="(preview, index) in newColor.imagePreviews"
                            :key="`new-preview-${index}`"
                            class="color-image-preview"
                            :class="{
                                'main-image': index === newColor.mainImageIndex,
                            }"
                        >
                            <img :src="preview" alt="Preview" />
                            <div class="image-actions">
                                <button
                                    type="button"
                                    @click="setColorMainImage(index)"
                                    title="Đặt làm ảnh đại diện"
                                >
                                    <i class="fas fa-star"></i>
                                </button>
                                <button
                                    type="button"
                                    @click="removeColorImage(index)"
                                    title="Xóa ảnh"
                                >
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="color-actions-simple">
                    <button
                        type="button"
                        class="add-color-btn"
                        @click="
                            isEditing
                                ? addNewColorDuringEdit()
                                : addColorWithImages()
                        "
                        :disabled="
                            !newColor.name ||
                            newColor.images.length === 0 ||
                            !newColor.quantity ||
                            newColor.quantity < 1
                        "
                    >
                        <i class="fas fa-plus"></i> Thêm màu sắc
                    </button>
                    <button
                        type="button"
                        class="reset-color-btn"
                        @click="resetNewColor"
                    >
                        <i class="fas fa-times"></i> Hủy
                    </button>
                </div>
            </div>

            <div class="colors-display-simple">
                <div class="colors-display-header">
                    <h5 class="colors-display-title">Màu sắc đã thêm</h5>
                </div>

                <div
                    class="colors-list-simple"
                    v-if="formData.colorData.length > 0"
                >
                    <div
                        v-for="(color, colorIndex) in formData.colorData"
                        :key="`color-${colorIndex}`"
                        class="color-item-simple"
                    >
                        <div class="color-item-header">
                            <div>
                                <h6>{{ color.name }}</h6>
                                <div class="color-quantity">
                                    <span class="quantity-label"
                                        >Số lượng:</span
                                    >
                                    <span class="quantity-value">{{
                                        color.quantity
                                    }}</span>
                                </div>
                            </div>
                            <button
                                type="button"
                                class="remove-color-btn-simple"
                                @click="removeColorData(colorIndex)"
                                title="Xóa màu sắc"
                            >
                                <i class="fas fa-times"></i>
                            </button>
                        </div>

                        <div v-if="isEditing && color.id">
                            <!-- Existing images display for editing -->
                            <div
                                v-if="
                                    color.existingImages &&
                                    color.existingImages.length > 0
                                "
                            >
                                <div class="color-thumbnails-simple">
                                    <div
                                        v-for="(
                                            image, imgIndex
                                        ) in color.existingImages"
                                        :key="`existing-img-${image.id}`"
                                        class="color-image-preview"
                                        :class="{
                                            'main-image':
                                                image.isMain ||
                                                image.id === color.mainImageId,
                                        }"
                                    >
                                        <img
                                            :src="
                                                formatImageUrl(image.imagePath)
                                            "
                                            alt="Preview"
                                        />
                                        <div class="image-actions">
                                            <button
                                                type="button"
                                                @click="
                                                    setExistingColorMainImage(
                                                        colorIndex,
                                                        imgIndex
                                                    )
                                                "
                                                title="Đặt làm ảnh đại diện"
                                            >
                                                <i class="fas fa-star"></i>
                                            </button>
                                            <button
                                                type="button"
                                                @click="
                                                    removeExistingColorImage(
                                                        colorIndex,
                                                        imgIndex
                                                    )
                                                "
                                                title="Xóa ảnh"
                                            >
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>

                                    <!-- Display for new images added to existing color -->
                                    <div
                                        v-for="(
                                            preview, newImgIndex
                                        ) in color.newImagePreviews"
                                        :key="`new-img-${colorIndex}-${newImgIndex}`"
                                        class="color-image-preview"
                                        :class="{
                                            'main-image':
                                                color.mainImageId === null &&
                                                newImgIndex ===
                                                    color.mainImageIndex,
                                        }"
                                    >
                                        <img :src="preview" alt="Preview" />
                                        <div class="image-actions">
                                            <button
                                                type="button"
                                                @click="
                                                    setExistingColorMainImage(
                                                        colorIndex,
                                                        newImgIndex,
                                                        true
                                                    )
                                                "
                                                title="Đặt làm ảnh đại diện"
                                            >
                                                <i class="fas fa-star"></i>
                                            </button>
                                            <button
                                                type="button"
                                                @click="
                                                    removeExistingColorImage(
                                                        colorIndex,
                                                        newImgIndex,
                                                        true
                                                    )
                                                "
                                                title="Xóa ảnh"
                                            >
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>

                                    <!-- Add more images button -->
                                    <div
                                        class="color-image-preview add-more"
                                        @click="
                                            $refs[
                                                `colorImageInput${colorIndex}`
                                            ][0].click()
                                        "
                                    >
                                        <input
                                            type="file"
                                            :ref="`colorImageInput${colorIndex}`"
                                            class="hidden-input"
                                            accept="image/png, image/jpeg, image/jpg"
                                            multiple
                                            @change="
                                                (e) =>
                                                    handleColorImageUploadForExisting(
                                                        e,
                                                        colorIndex
                                                    )
                                            "
                                        />
                                        <div class="add-icon">
                                            <i class="fas fa-plus"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Thêm input số lượng cho màu chỉnh sửa -->
                            <div class="color-quantity-edit">
                                <label for="colorQuantityEdit">Số lượng:</label>
                                <input
                                    type="number"
                                    id="colorQuantityEdit"
                                    v-model="color.quantity"
                                    min="1"
                                    placeholder="Số lượng"
                                    class="quantity-input-edit"
                                />
                            </div>
                        </div>

                        <!-- New color images display -->
                        <div
                            v-if="
                                !color.id ||
                                (!color.existingImages && !color.id)
                            "
                        >
                            <div class="color-thumbnails-simple">
                                <div
                                    v-for="(
                                        preview, imgIndex
                                    ) in color.imagePreviews || color.images"
                                    :key="`preview-${colorIndex}-${imgIndex}`"
                                    class="color-image-preview"
                                    :class="{
                                        'main-image':
                                            imgIndex === color.mainImageIndex,
                                    }"
                                >
                                    <img
                                        :src="
                                            typeof preview === 'string'
                                                ? preview
                                                : URL.createObjectURL(preview)
                                        "
                                        alt="Preview"
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div v-else class="no-colors-added">
                    Chưa có màu sắc nào được thêm
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.form-section {
    background-color: #fff;
    border-radius: 12px;
    padding: 1.5rem;
    margin-bottom: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.section-title {
    font-size: 1rem;
    margin: 0 0 1rem 0;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.required {
    color: #ef4444;
}

.color-section-layout {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1.5rem;
}

@media (max-width: 992px) {
    .color-section-layout {
        grid-template-columns: 1fr;
    }
}

.color-add-form-simple,
.colors-display-simple {
    background-color: #f9fafb;
    border-radius: 8px;
    padding: 1.25rem;
}

.color-add-form-simple h5,
.colors-display-simple h5 {
    margin-top: 0;
    margin-bottom: 1rem;
    color: #4b5563;
    font-size: 0.95rem;
}

.color-name-input {
    margin-bottom: 1rem;
}

.color-name-input label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    color: #4b5563;
}

.color-name-input input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
}

.color-upload-wrapper-simple {
    margin-bottom: 1rem;
}

.color-upload-wrapper-simple label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    color: #4b5563;
}

.color-upload-area-simple {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 1.5rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.3s;
}

.color-upload-area-simple:hover {
    border-color: var(--primary-color);
    background-color: #fff5fc;
}

.upload-placeholder-simple {
    display: flex;
    flex-direction: column;
    align-items: center;
    color: #6b7280;
}

.upload-placeholder-simple i {
    font-size: 1.5rem;
    margin-bottom: 0.5rem;
    color: var(--primary-color);
}

.upload-placeholder-simple p {
    margin: 0;
    font-size: 0.9rem;
}

.hidden-input {
    display: none;
}

.error-message {
    color: #ef4444;
    font-size: 0.85rem;
    margin-top: 0.5rem;
    display: block;
}

.color-preview-simple {
    margin-top: 1rem;
    margin-bottom: 1rem;
}

.preview-label {
    font-size: 0.9rem;
    color: #4b5563;
    margin-bottom: 0.5rem;
}

.image-previews-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
    gap: 0.5rem;
}

.color-image-preview {
    position: relative;
    width: 80px;
    height: 80px;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid #ddd;
}

.color-image-preview img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.main-image {
    border: 2px solid var(--primary-color);
}

.image-actions {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.6);
    display: flex;
    justify-content: space-around;
    padding: 0.25rem;
}

.image-actions button {
    background: none;
    border: none;
    color: white;
    cursor: pointer;
    padding: 0.25rem;
    font-size: 0.8rem;
}

.color-actions-simple {
    display: flex;
    gap: 0.5rem;
    margin-top: 1rem;
}

.add-color-btn,
.reset-color-btn {
    padding: 0.75rem 1rem;
    border: none;
    border-radius: 8px;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.3s;
}

.add-color-btn {
    background-color: var(--primary-color);
    color: white;
}

.add-color-btn:hover {
    background-color: #e94e9c;
}

.add-color-btn:disabled {
    background-color: #d1d5db;
    cursor: not-allowed;
}

.reset-color-btn {
    background-color: #f3f4f6;
    color: #4b5563;
}

.reset-color-btn:hover {
    background-color: #e5e7eb;
}

.colors-list-simple {
    max-height: 400px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.color-item-simple {
    background-color: white;
    border-radius: 8px;
    padding: 1rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.color-item-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 0.75rem;
}

.color-item-header h6 {
    margin: 0;
    font-size: 0.95rem;
    color: #111827;
}

.color-quantity {
    font-size: 0.85rem;
    color: #6b7280;
    margin-top: 0.25rem;
}

.quantity-label {
    margin-right: 0.25rem;
}

.quantity-value {
    font-weight: 500;
    color: #4b5563;
}

.remove-color-btn-simple {
    background: none;
    border: none;
    color: #ef4444;
    cursor: pointer;
    font-size: 1rem;
}

.color-thumbnails-simple {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(70px, 1fr));
    gap: 0.5rem;
}

.no-colors-added {
    text-align: center;
    padding: 2rem;
    color: #6b7280;
    font-size: 0.9rem;
}

.color-quantity-edit {
    margin-top: 0.75rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.color-quantity-edit label {
    font-size: 0.85rem;
    color: #4b5563;
}

.quantity-input-edit {
    width: 80px;
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 6px;
}

.add-more {
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f3f4f6;
    cursor: pointer;
}

.add-icon {
    color: #6b7280;
}

.add-more:hover {
    background-color: #e5e7eb;
}

.add-more:hover .add-icon {
    color: var(--primary-color);
}
</style>
