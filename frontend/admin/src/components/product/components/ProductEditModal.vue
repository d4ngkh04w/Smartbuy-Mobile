<script setup>
import { ref, defineEmits, watch } from "vue";
import TechnicalSpecsForm from "./TechnicalSpecsForm.vue";
import ColorManagerSection from "./ColorManagerSection.vue";
import productService from "../../../services/productService.js";
import emitter from "../../../utils/evenBus.js";

const props = defineProps({
    showModal: {
        type: Boolean,
        required: true,
    },
    product: {
        type: Object,
        required: false,
        default: () => ({
            id: null,
            name: "",
            importPrice: "",
            salePrice: "",
            description: "",
            productLineId: null,
            detail: {},
            colors: [],
        }),
    },
    brands: {
        type: Array,
        default: () => [],
    },
    productLines: {
        type: Array,
        default: () => [],
    },
});

const emit = defineEmits(["close", "brand-change", "product-updated"]);

// Utility function for error handling
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

// Form states
const basicInfoForm = ref({
    name: "",
    brandId: null,
    productLineId: null,
    importPrice: "",
    salePrice: "",
    description: "",
});

const technicalSpecsForm = ref({
    warranty: "12",
    ram: "",
    storage: "",
    screenSize: "",
    screenResolution: "",
    battery: "",
    processor: "",
    os: "",
    simSlots: 1,
});

// Watch for changes in props.brands to set a default brand if needed
watch(
    () => props.brands,
    (newBrands) => {
        // Only set default brand if form is empty and product is not loaded yet
        if (
            newBrands &&
            newBrands.length > 0 &&
            !basicInfoForm.value.brandId &&
            !props.product?.id
        ) {
            basicInfoForm.value.brandId = newBrands[0].id;
            emit("brand-change", basicInfoForm.value.brandId);
        }
    },
    { immediate: true }
);

// Add this watch to trigger product line loading when the component mounts
watch(
    () => props.showModal,
    (isVisible) => {
        if (isVisible && basicInfoForm.value.brandId) {
            // When the modal becomes visible and we have a brand ID, emit to load product lines
            emit("brand-change", basicInfoForm.value.brandId);
        }
    }
);

// Watch for changes in productLines to select the first one by default if needed
watch(
    () => props.productLines,
    (newProductLines) => {      
        // Only auto-select if we don't have a specific productLineId set
        if (
            newProductLines &&
            newProductLines.length > 0 &&
            !basicInfoForm.value.productLineId &&
            !props.product?.productLineId
        ) {
            console.log(
                "Auto-selecting first product line:",
                newProductLines[0].id
            );
            basicInfoForm.value.productLineId = newProductLines[0].id;
        } else if (
            props.product?.productLineId &&
            newProductLines &&
            newProductLines.length > 0
        ) {
            // Make sure the selected product line exists in the new list
            const existingProductLine = newProductLines.find(
                (pl) => pl.id === props.product.productLineId
            );
            if (existingProductLine) {
                console.log(
                    "Product line exists in new list, keeping selection:",
                    props.product.productLineId
                );
                basicInfoForm.value.productLineId = props.product.productLineId;
            } else {
                console.log(
                    "Product line not found in new list, clearing selection"
                );
                basicInfoForm.value.productLineId = null;
            }
        }
    },
    { immediate: true }
);

// Form errors
const formErrors = ref({
    name: "",
    brandId: "",
    productLineId: "",
    importPrice: "",
    salePrice: "",
});

// Loading states
const savingBasicInfo = ref(false);
const savingTechSpecs = ref(false);

// New function to find brand ID by product line ID - moved up before use
const findBrandIdByProductLine = (productLineId) => {
    console.log("Finding brand ID for product line:", productLineId);

    if (!productLineId) {
        console.log("No product line ID provided");
        return null;
    }

    if (!props.brands || !props.brands.length) {
        console.log("No brands available");
        return null;
    }

    console.log("Available brands:", props.brands);

    // Find the product line in all the brands' product lines
    for (const brand of props.brands) {
        if (brand.productLines && Array.isArray(brand.productLines)) {
            const foundProductLine = brand.productLines.find(
                (pl) => pl.id === productLineId
            );
            if (foundProductLine) {
                return brand.id;
            }
        }
    }

    // If not found in nested productLines, check if the current productLines prop
    // contains the productLineId and match it with brand data
    if (props.productLines && Array.isArray(props.productLines)) {
        const foundProductLine = props.productLines.find(
            (pl) => pl.id === productLineId
        );
        if (foundProductLine && foundProductLine.brandId) {
            console.log(
                "Found product line in productLines prop, brand ID:",
                foundProductLine.brandId
            );
            return foundProductLine.brandId;
        }
    }

    console.log("Brand ID not found for product line:", productLineId);
    return null;
};

// Initialize forms when product changes
watch(
    () => props.product,
    (newProduct) => {       

        if (!newProduct || !newProduct.id) {
            console.log("Resetting form - no product or no product ID");
            // Reset the form when product is null or has no ID
            basicInfoForm.value = {
                name: "",
                brandId: null,
                productLineId: null,
                importPrice: "",
                salePrice: "",
                description: "",
            };
            technicalSpecsForm.value = {
                warranty: "12",
                ram: "",
                storage: "",
                screenSize: "",
                screenResolution: "",
                battery: "",
                processor: "",
                os: "",
                simSlots: 1,
            };
        } else {
            console.log("Initializing form with product data");
            console.log("Product brandId:", newProduct.brandId);
            console.log("Product productLineId:", newProduct.productLineId);

            // Initialize form with product data
            let brandId = newProduct.brandId;

            // If brandId is not directly available, try to find it from product line
            if (!brandId && newProduct.productLineId) {
                console.log("Brand ID not found, searching by product line...");
                brandId = findBrandIdByProductLine(newProduct.productLineId);
            }

            console.log("Final brandId to use:", brandId);

            basicInfoForm.value = {
                name: newProduct.name || "",
                brandId: brandId || null,
                productLineId: newProduct.productLineId || null,
                importPrice: newProduct.importPrice || "",
                salePrice: newProduct.salePrice || "",
                description: newProduct.description || "",
            };

            technicalSpecsForm.value = {
                warranty: newProduct.detail?.warranty || "12",
                ram: newProduct.detail?.ram || "",
                storage: newProduct.detail?.storage || "",
                screenSize: newProduct.detail?.screenSize || "",
                screenResolution: newProduct.detail?.screenResolution || "",
                battery: newProduct.detail?.battery || "",
                processor: newProduct.detail?.processor || "",
                os: newProduct.detail?.operatingSystem || "",
                simSlots: newProduct.detail?.simSlots || 1,
            };

            console.log("Form initialized:", basicInfoForm.value);

            // If we have brandId, emit to load correct product lines
            if (brandId) {
                console.log("Emitting brand-change with brandId:", brandId);
                emit("brand-change", brandId);
            }
        }

        // Reset errors
        Object.keys(formErrors.value).forEach((key) => {
            formErrors.value[key] = "";
        });
    },
    { immediate: true }
);

const handleBrandChange = () => {
    // Reset product line when brand changes
    basicInfoForm.value.productLineId = null;
    emit("brand-change", basicInfoForm.value.brandId);
};

// Xử lý sự kiện khi màu sản phẩm được cập nhật
const handleColorUpdated = () => {
    emit("product-updated");
};

const validateBasicInfo = () => {
    let isValid = true;

    // Reset errors
    Object.keys(formErrors.value).forEach((key) => {
        formErrors.value[key] = "";
    });

    // Validate required fields
    if (!basicInfoForm.value.name.trim()) {
        formErrors.value.name = "Tên sản phẩm không được để trống";
        isValid = false;
    }

    if (!basicInfoForm.value.brandId) {
        formErrors.value.brandId = "Vui lòng chọn thương hiệu";
        isValid = false;
    }

    if (!basicInfoForm.value.productLineId) {
        formErrors.value.productLineId = "Vui lòng chọn dòng sản phẩm";
        isValid = false;
    }

    if (!basicInfoForm.value.importPrice) {
        formErrors.value.importPrice = "Giá nhập không được để trống";
        isValid = false;
    } else if (
        isNaN(basicInfoForm.value.importPrice) ||
        Number(basicInfoForm.value.importPrice) <= 0
    ) {
        formErrors.value.importPrice = "Giá nhập phải là số dương";
        isValid = false;
    }

    if (!basicInfoForm.value.salePrice) {
        formErrors.value.salePrice = "Giá bán không được để trống";
        isValid = false;
    } else if (
        isNaN(basicInfoForm.value.salePrice) ||
        Number(basicInfoForm.value.salePrice) <= 0
    ) {
        formErrors.value.salePrice = "Giá bán phải là số dương";
        isValid = false;
    }

    return isValid;
};

const saveBasicInfo = async () => {
    if (!validateBasicInfo()) {
        return;
    }

    savingBasicInfo.value = true;

    try {
        const formData = new FormData();
        formData.append("Name", basicInfoForm.value.name);
        formData.append(
            "ImportPrice",
            basicInfoForm.value.importPrice.toString()
        );
        formData.append("SalePrice", basicInfoForm.value.salePrice.toString());
        formData.append("ProductLineId", basicInfoForm.value.productLineId);
        formData.append("Description", basicInfoForm.value.description || "");

        // Save the product data
        await productService.updateProduct(props.product.id, formData);

        // After successful save, update the product object
        props.product.name = basicInfoForm.value.name;
        props.product.importPrice = basicInfoForm.value.importPrice;
        props.product.salePrice = basicInfoForm.value.salePrice;
        props.product.productLineId = basicInfoForm.value.productLineId;
        props.product.description = basicInfoForm.value.description;

        showSuccessNotification(
            "Thông tin cơ bản đã được cập nhật thành công!"
        );

        emit("product-updated");
    } catch (error) {
        console.error("Error updating basic info:", error);

        let errorMessage = "Có lỗi xảy ra khi cập nhật thông tin cơ bản.";
        if (error.response?.data?.message) {
            errorMessage = error.response.data.message;
        }

        showErrorNotification(errorMessage);
    } finally {
        savingBasicInfo.value = false;
    }
};

const saveTechnicalSpecs = async () => {
    savingTechSpecs.value = true;

    try {
        const formData = new FormData();

        // Add technical specs fields
        formData.append("Warranty", technicalSpecsForm.value.warranty || "12");
        formData.append("RAM", technicalSpecsForm.value.ram || "");
        formData.append("Storage", technicalSpecsForm.value.storage || "");
        formData.append(
            "ScreenSize",
            technicalSpecsForm.value.screenSize || ""
        );
        formData.append(
            "ScreenResolution",
            technicalSpecsForm.value.screenResolution || ""
        );
        formData.append("Battery", technicalSpecsForm.value.battery || "");
        formData.append("OS", technicalSpecsForm.value.os || "");
        formData.append("Processor", technicalSpecsForm.value.processor || "");
        formData.append("SimSlots", technicalSpecsForm.value.simSlots || "1");

        await productService.updateProduct(props.product.id, formData);

        showSuccessNotification(
            "Thông số kỹ thuật đã được cập nhật thành công!"
        );

        emit("product-updated");
    } catch (error) {
        console.error("Error updating technical specs:", error);

        let errorMessage = "Có lỗi xảy ra khi cập nhật thông số kỹ thuật.";
        if (error.response?.data?.message) {
            errorMessage = error.response.data.message;
        }

        showErrorNotification(errorMessage);
    } finally {
        savingTechSpecs.value = false;
    }
};
</script>

<template>
    <div v-if="showModal" class="modal-backdrop">
        <div class="modal-container product-modal">
            <div class="modal-header">
                <h3>Chỉnh sửa sản phẩm: {{ product.name }}</h3>
                <button @click="$emit('close')" class="close-button">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="modal-form-container">
                <div class="modal-form">
                    <!-- Basic Information Section -->
                    <div class="form-section">
                        <div class="section-header">
                            <h4 class="section-title">
                                <i class="fas fa-info-circle"></i> Thông tin cơ
                                bản
                            </h4>
                            <button
                                type="button"
                                class="save-section-button"
                                @click="saveBasicInfo"
                                :disabled="savingBasicInfo"
                            >
                                <span
                                    v-if="savingBasicInfo"
                                    class="spinner small"
                                ></span>
                                <span v-else>
                                    <i class="fas fa-save"></i> Lưu thông tin cơ
                                    bản
                                </span>
                            </button>
                        </div>

                        <!-- Category Selection Section -->
                        <div class="form-row">
                            <div class="form-group">
                                <label for="brandSelect">
                                    Thương hiệu <span class="required">*</span>
                                </label>
                                <div class="select-wrapper">
                                    <select
                                        id="brandSelect"
                                        v-model="basicInfoForm.brandId"
                                        @change="handleBrandChange"
                                    >
                                        <option
                                            v-for="brand in brands"
                                            :key="brand.id"
                                            :value="brand.id"
                                        >
                                            {{ brand.name }}
                                        </option>
                                    </select>
                                </div>
                                <span
                                    v-if="formErrors.brandId"
                                    class="error-message"
                                >
                                    {{ formErrors.brandId }}
                                </span>
                            </div>

                            <div class="form-group">
                                <label for="productLineSelect">
                                    Dòng sản phẩm
                                    <span class="required">*</span>
                                </label>
                                <div class="select-wrapper">
                                    <select
                                        id="productLineSelect"
                                        v-model="basicInfoForm.productLineId"
                                        :disabled="
                                            !basicInfoForm.brandId ||
                                            !productLines.length
                                        "
                                    >
                                        <option
                                            v-if="!productLines.length"
                                            :value="null"
                                        >
                                            {{
                                                !basicInfoForm.brandId
                                                    ? "Vui lòng chọn thương hiệu trước"
                                                    : "Không có dòng sản phẩm"
                                            }}
                                        </option>
                                        <option
                                            v-for="line in productLines"
                                            :key="line.id"
                                            :value="line.id"
                                        >
                                            {{ line.name }}
                                        </option>
                                    </select>
                                </div>
                                <span
                                    v-if="formErrors.productLineId"
                                    class="error-message"
                                >
                                    {{ formErrors.productLineId }}
                                </span>
                            </div>
                        </div>
                        <!-- Product Name -->
                        <div class="form-row">
                            <div class="form-group full-width">
                                <label for="productName">
                                    Tên sản phẩm <span class="required">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="productName"
                                    v-model="basicInfoForm.name"
                                    placeholder="Nhập tên sản phẩm (tối đa 100 ký tự)"
                                    maxlength="100"
                                />
                                <span
                                    v-if="formErrors.name"
                                    class="error-message"
                                >
                                    {{ formErrors.name }}
                                </span>
                            </div>
                        </div>

                        <!-- Price Information -->
                        <div class="form-row">
                            <div class="form-group">
                                <label for="importPrice">
                                    Giá nhập <span class="required">*</span>
                                </label>
                                <div class="currency-input">
                                    <input
                                        type="number"
                                        id="importPrice"
                                        v-model="basicInfoForm.importPrice"
                                        placeholder="0"
                                        min="0"
                                    />
                                    <span class="currency">VNĐ</span>
                                </div>
                                <span
                                    v-if="formErrors.importPrice"
                                    class="error-message"
                                >
                                    {{ formErrors.importPrice }}
                                </span>
                            </div>

                            <div class="form-group">
                                <label for="salePrice">
                                    Giá bán <span class="required">*</span>
                                </label>
                                <div class="currency-input">
                                    <input
                                        type="number"
                                        id="salePrice"
                                        v-model="basicInfoForm.salePrice"
                                        placeholder="0"
                                        min="0"
                                    />
                                    <span class="currency">VNĐ</span>
                                </div>
                                <span
                                    v-if="formErrors.salePrice"
                                    class="error-message"
                                >
                                    {{ formErrors.salePrice }}
                                </span>
                            </div>
                        </div>
                        <!-- Description -->
                        <div class="form-row">
                            <div class="form-group full-width">
                                <label for="description">Mô tả sản phẩm</label>
                                <textarea
                                    id="description"
                                    v-model="basicInfoForm.description"
                                    rows="4"
                                    placeholder="Mô tả chi tiết về sản phẩm (tối đa 2000 ký tự)"
                                    maxlength="2000"
                                ></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Technical Specifications Section -->
                    <div class="form-section">
                        <div class="section-header">
                            <h4 class="section-title">
                                <i class="fas fa-microchip"></i> Thông số kỹ
                                thuật
                            </h4>
                            <button
                                type="button"
                                class="save-section-button"
                                @click="saveTechnicalSpecs"
                                :disabled="savingTechSpecs"
                            >
                                <span
                                    v-if="savingTechSpecs"
                                    class="spinner small"
                                ></span>
                                <span v-else>
                                    <i class="fas fa-save"></i> Lưu thông số kỹ
                                    thuật
                                </span>
                            </button>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="ram">RAM (GB)</label>
                                <input
                                    type="number"
                                    id="ram"
                                    v-model="technicalSpecsForm.ram"
                                    placeholder="Ví dụ: 8"
                                    min="0"
                                />
                            </div>

                            <div class="form-group">
                                <label for="storage">Bộ nhớ trong (GB)</label>
                                <input
                                    type="number"
                                    id="storage"
                                    v-model="technicalSpecsForm.storage"
                                    placeholder="Ví dụ: 128"
                                    min="0"
                                />
                            </div>

                            <div class="form-group">
                                <label for="warranty">Bảo hành (tháng)</label>
                                <input
                                    type="number"
                                    id="warranty"
                                    v-model="technicalSpecsForm.warranty"
                                    placeholder="Ví dụ: 12"
                                    min="0"
                                />
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="screenSize"
                                    >Kích thước màn hình (inch)</label
                                >
                                <input
                                    type="text"
                                    id="screenSize"
                                    v-model="technicalSpecsForm.screenSize"
                                    placeholder="Ví dụ: 6.5"
                                />
                            </div>

                            <div class="form-group">
                                <label for="screenResolution"
                                    >Độ phân giải màn hình</label
                                >
                                <input
                                    type="text"
                                    id="screenResolution"
                                    v-model="
                                        technicalSpecsForm.screenResolution
                                    "
                                    placeholder="Ví dụ: 1920x1080"
                                />
                            </div>

                            <div class="form-group">
                                <label for="battery"
                                    >Dung lượng pin (mAh)</label
                                >
                                <input
                                    type="number"
                                    id="battery"
                                    v-model="technicalSpecsForm.battery"
                                    placeholder="Ví dụ: 5000"
                                    min="0"
                                />
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="processor">Vi xử lý (CPU)</label>
                                <input
                                    type="text"
                                    id="processor"
                                    v-model="technicalSpecsForm.processor"
                                    placeholder="Ví dụ: Snapdragon 8 Gen 2"
                                />
                            </div>

                            <div class="form-group">
                                <label for="os">Hệ điều hành</label>
                                <input
                                    type="text"
                                    id="os"
                                    v-model="technicalSpecsForm.os"
                                    placeholder="Ví dụ: Android 13"
                                />
                            </div>

                            <div class="form-group">
                                <label for="simSlots">Số khe SIM</label>
                                <select
                                    id="simSlots"
                                    v-model="technicalSpecsForm.simSlots"
                                >
                                    <option :value="1">1 SIM</option>
                                    <option :value="2">2 SIM</option>
                                    <option :value="3">3 SIM</option>
                                    <option :value="4">4 SIM</option>
                                </select>
                            </div>
                        </div>
                    </div>
                    <!-- Colors Section -->
                    <ColorManagerSection
                        v-if="product && product.id"
                        :product-id="product.id"
                        :existing-colors="product.colors || []"
                        @color-updated="handleColorUpdated"
                    />

                    <div class="form-actions">
                        <button
                            type="button"
                            class="cancel-button"
                            @click="$emit('close')"
                        >
                            <i class="fas fa-times"></i> Hủy
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Modal Styling */
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    backdrop-filter: blur(3px);
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    width: 90%;
    max-width: 1000px;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
    position: sticky;
    top: 0;
    background-color: white;
    z-index: 10;
    border-radius: 12px 12px 0 0;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: #333;
}

.close-button {
    background: none;
    border: none;
    font-size: 1rem;
    color: #666;
    cursor: pointer;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.close-button:hover {
    background-color: #f9f9f9;
    color: #333;
}

.modal-form-container {
    padding: 1.5rem;
}

/* Form Section Styling */
.form-section {
    margin-bottom: 2rem;
    padding: 1.5rem;
    border-radius: 8px;
    background-color: #ffffff;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.form-section:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
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

.section-subtitle {
    font-size: 0.85rem;
    color: #666;
    margin-top: 0.25rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.section-subtitle i {
    color: var(--primary-color, #f86ed3);
    font-size: 0.9rem;
}

.save-section-button {
    padding: 0.5rem 1rem;
    background-color: var(--primary-color, #f86ed3);
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 0.85rem;
    cursor: pointer;
    display: flex;
    align-items: center;
}

.save-section-button:hover {
    background-color: #e94e9c;
}

.save-section-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.save-section-button i {
    margin-right: 6px;
}

/* Form Fields Styling */
.form-row {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-bottom: 1rem;
}

.form-group {
    flex: 1;
    min-width: 200px;
}

.form-group.full-width {
    flex-basis: 100%;
}

label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    font-weight: 500;
    color: #555;
}

input,
textarea,
select {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 0.95rem;
    transition: border-color 0.2s;
}

input:focus,
textarea:focus,
select:focus {
    border-color: var(--primary-color, #f86ed3);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.select-wrapper {
    position: relative;
}

.select-wrapper:after {
    content: "\f107";
    font-family: "Font Awesome 5 Free";
    font-weight: 900;
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

select {
    appearance: none;
    cursor: pointer;
}

textarea {
    resize: vertical;
    min-height: 100px;
}

.required {
    color: #ef4444;
}

.error-message {
    display: block;
    color: #ef4444;
    font-size: 0.8rem;
    margin-top: 0.3rem;
}

/* File Upload Styling */
.file-upload-container {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.file-input {
    display: none;
}

.file-upload-button {
    padding: 0.75rem 1.25rem;
    background-color: var(--primary-color, #f86ed3);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 0.9rem;
}

.file-upload-button:hover {
    background-color: #e94e9c;
}

.file-name {
    font-size: 0.9rem;
    color: #666;
}

/* Image Preview Styling */
.image-preview-container {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-top: 1rem;
}

.image-preview-item {
    position: relative;
    width: 100px;
    height: 100px;
    border-radius: 8px;
    overflow: hidden;
    border: 2px solid #eee;
}

.image-preview-item.main-image {
    border-color: var(--primary-color, #f86ed3);
}

.image-preview-item img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.image-preview-actions {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    display: flex;
    justify-content: space-between;
    padding: 5px;
    background-color: rgba(0, 0, 0, 0.5);
}

.image-preview-actions button {
    background: none;
    border: none;
    color: white;
    cursor: pointer;
    font-size: 0.8rem;
    padding: 3px;
    border-radius: 3px;
}

.image-preview-actions button:hover {
    background-color: rgba(255, 255, 255, 0.2);
}

.image-preview-actions button.active {
    color: var(--primary-color, #f86ed3);
}

/* Submit Button Styling */
.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 2rem;
}

.cancel-button,
.close-button-bottom {
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
    background-color: #ffffff;
    border: 1px solid #ddd;
    color: #666;
}

.cancel-button:hover,
.close-button-bottom:hover {
    background-color: #eee;
}

.submit-button,
.save-section-button {
    background-color: var(--primary-color, #f86ed3);
    border: none;
    color: white;
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
}

.submit-button:hover,
.save-section-button:hover {
    background-color: #e94e9c;
}

.submit-button:disabled,
.save-section-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

/* Loading Spinner */
.spinner {
    display: inline-block;
    width: 20px;
    height: 20px;
    border: 2px solid rgba(248, 110, 211, 0.1);
    border-radius: 50%;
    border-top-color: white;
    animation: spin 1s linear infinite;
}

.spinner.small {
    width: 16px;
    height: 16px;
    border-width: 2px;
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

/* Additional Styles */
.currency-input {
    position: relative;
}

.currency-input input {
    padding-right: 3rem;
}

.currency {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    font-size: 0.85rem;
}

.modal-form {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.optional-hint {
    font-size: 0.8rem;
    color: #666;
    font-weight: normal;
    margin-left: 0.5rem;
}
</style>
