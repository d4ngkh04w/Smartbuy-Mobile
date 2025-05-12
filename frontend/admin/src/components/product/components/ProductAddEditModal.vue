<script setup>
import { ref, defineProps, defineEmits, watch, computed } from "vue";
import TechnicalSpecsForm from "./TechnicalSpecsForm.vue";
import ColorManager from "./ColorManager.vue";

const props = defineProps({
    showModal: {
        type: Boolean,
        required: true,
    },
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
    brands: {
        type: Array,
        default: () => [],
    },
    productLines: {
        type: Array,
        default: () => [],
    },
    submitLoading: {
        type: Boolean,
        default: false,
    },
});

const emit = defineEmits([
    "update:formData",
    "update:formErrors",
    "close",
    "submit",
    "brand-change",
]);

const filteredProductLines = computed(() => {
    if (!props.formData.brandId) return [];
    return props.productLines;
});

const handleBrandChange = () => {
    emit("brand-change");
};

const updateFormData = (key, value) => {
    const updatedFormData = { ...props.formData };
    updatedFormData[key] = value;
    emit("update:formData", updatedFormData);
};

const colorManagerRef = ref(null);
</script>

<template>
    <div v-if="showModal" class="modal-backdrop">
        <div class="modal-container product-modal">
            <div class="modal-header">
                <h3>
                    {{
                        isEditing
                            ? "Chỉnh sửa sản phẩm: " + formData.name
                            : "Thêm sản phẩm mới"
                    }}
                </h3>
                <button @click="$emit('close')" class="close-button">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="modal-form-container">
                <form @submit.prevent="$emit('submit')" class="modal-form">
                    <!-- Category Selection Section -->
                    <div class="form-section">
                        <h4 class="section-title">Chọn danh mục</h4>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="brandSelect">
                                    Thương hiệu
                                    <span class="required">*</span>
                                </label>
                                <div class="select-wrapper">
                                    <select
                                        id="brandSelect"
                                        v-model="formData.brandId"
                                        @change="handleBrandChange"
                                    >
                                        <option value="" disabled selected>
                                            Chọn thương hiệu
                                        </option>
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
                                        v-model="formData.productLineId"
                                        :disabled="!formData.brandId"
                                    >
                                        <option value="" disabled selected>
                                            Chọn dòng sản phẩm
                                        </option>
                                        <option
                                            v-for="line in filteredProductLines"
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
                    </div>

                    <!-- Basic Information -->
                    <div class="form-section">
                        <h4 class="section-title">Thông tin cơ bản</h4>

                        <!-- Tên sản phẩm - full width -->
                        <div class="form-group">
                            <label for="productName">
                                Tên sản phẩm <span class="required">*</span>
                            </label>
                            <input
                                type="text"
                                id="productName"
                                v-model="formData.name"
                                placeholder="Nhập tên sản phẩm"
                            />
                            <span v-if="formErrors.name" class="error-message">
                                {{ formErrors.name }}
                            </span>
                        </div>

                        <!-- Nhóm thông tin giá -->
                        <div class="form-row">
                            <div class="form-group">
                                <label for="importPrice">
                                    Giá nhập <span class="required">*</span>
                                </label>
                                <div class="currency-input">
                                    <input
                                        type="number"
                                        id="importPrice"
                                        v-model="formData.importPrice"
                                        min="0"
                                        step="1000"
                                    />
                                    <span class="currency">VND</span>
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
                                        v-model="formData.salePrice"
                                        min="0"
                                        step="1000"
                                    />
                                    <span class="currency">VND</span>
                                </div>
                                <span
                                    v-if="formErrors.salePrice"
                                    class="error-message"
                                >
                                    {{ formErrors.salePrice }}
                                </span>
                            </div>
                        </div>

                        <!-- Nhóm trạng thái -->
                        <div class="form-row">
                            <div class="form-group" v-if="isEditing">
                                <label>Trạng thái</label>
                                <div class="toggle-switch-wrapper">
                                    <div class="toggle-switch-container">
                                        <label class="toggle-switch">
                                            <input
                                                type="checkbox"
                                                v-model="formData.isActive"
                                            />
                                            <span class="toggle-slider"></span>
                                        </label>
                                        <span class="toggle-status">
                                            {{
                                                formData.isActive
                                                    ? "Đang bán"
                                                    : "Ngừng bán"
                                            }}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Mô tả sản phẩm - full width -->
                        <div class="form-group">
                            <label for="description">
                                Mô tả sản phẩm
                                <span class="optional-hint"
                                    >(không bắt buộc)</span
                                >
                            </label>
                            <textarea
                                id="description"
                                v-model="formData.description"
                                rows="4"
                                placeholder="Nhập mô tả sản phẩm"
                            ></textarea>
                        </div>
                    </div>
                    <!-- Technical Specifications -->
                    <TechnicalSpecsForm
                        :formData="formData"
                        @update:formData="
                            (updatedData) =>
                                emit('update:formData', updatedData)
                        "
                    />
                    <!-- Colors Section -->
                    <ColorManager
                        ref="colorManagerRef"
                        :formData="formData"
                        :formErrors="formErrors"
                        :isEditing="isEditing"
                        @update:formData="
                            (updatedData) =>
                                emit('update:formData', updatedData)
                        "
                        @update:formErrors="
                            (updatedErrors) =>
                                emit('update:formErrors', updatedErrors)
                        "
                    />

                    <div class="form-actions">
                        <button
                            type="button"
                            class="cancel-button"
                            @click="$emit('close')"
                        >
                            <i class="fas fa-times"></i> Hủy
                        </button>
                        <button
                            type="submit"
                            class="submit-button"
                            :disabled="submitLoading"
                        >
                            <span
                                v-if="submitLoading"
                                class="spinner small"
                            ></span>
                            <span v-else>
                                <i class="fas fa-save"></i>
                                {{
                                    isEditing ? "Lưu thay đổi" : "Thêm sản phẩm"
                                }}
                            </span>
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    width: 95%;
    max-width: 1200px;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.close-button {
    background: none;
    border: none;
    font-size: 1.25rem;
    color: #6b7280;
    cursor: pointer;
}

.modal-form-container {
    padding: 1.5rem;
    overflow-y: auto;
    max-height: calc(90vh - 70px);
}

.modal-form {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

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

.form-row {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 1rem;
    margin-bottom: 1rem;
}

.form-group {
    margin-bottom: 1rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    color: #4b5563;
}

.form-group input,
.form-group select,
.form-group textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    transition: all 0.3s;
}

.form-group textarea {
    resize: vertical;
    min-height: 100px;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.required {
    color: #ef4444;
}

.optional-hint {
    font-size: 0.8rem;
    color: #6b7280;
    font-weight: normal;
    margin-left: 0.5rem;
}

.error-message {
    color: #ef4444;
    font-size: 0.85rem;
    margin-top: 0.5rem;
    display: block;
}

.select-wrapper {
    position: relative;
}

.select-wrapper:after {
    content: "\f078";
    font-family: "Font Awesome 5 Free";
    font-weight: 900;
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    color: #6b7280;
    font-size: 0.8rem;
}

select {
    appearance: none;
}

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
    color: #6b7280;
    font-size: 0.85rem;
}

.toggle-switch-wrapper {
    display: flex;
    align-items: center;
}

.toggle-switch-container {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.toggle-switch {
    position: relative;
    display: inline-block;
    width: 50px;
    height: 26px;
    margin-top: 6px;
}

.toggle-switch input {
    opacity: 0;
    width: 0;
    height: 0;
}

.toggle-slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #e5e7eb;
    transition: 0.4s;
    border-radius: 34px;
}

.toggle-slider:before {
    position: absolute;
    content: "";
    height: 18px;
    width: 18px;
    left: 4px;
    bottom: 4px;
    background-color: white;
    transition: 0.4s;
    border-radius: 50%;
}

input:checked + .toggle-slider {
    background-color: #22c55e;
}

input:focus + .toggle-slider {
    box-shadow: 0 0 1px #22c55e;
}

input:checked + .toggle-slider:before {
    transform: translateX(24px);
}

.toggle-status {
    font-weight: 500;
    color: #333;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 2rem;
}

.cancel-button,
.submit-button {
    padding: 0.75rem 1.5rem;
    border-radius: 8px;
    font-size: 0.95rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    border: none;
    transition: all 0.3s;
}

.cancel-button {
    background-color: #f3f4f6;
    color: #4b5563;
}

.cancel-button:hover {
    background-color: #e5e7eb;
}

.submit-button {
    background-color: var(--primary-color);
    color: white;
}

.submit-button:hover {
    background-color: #e94e9c;
}

.submit-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.spinner.small {
    width: 20px;
    height: 20px;
    border: 3px solid rgba(255, 255, 255, 0.3);
    border-radius: 50%;
    border-top: 3px solid white;
    animation: spin 1s linear infinite;
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
