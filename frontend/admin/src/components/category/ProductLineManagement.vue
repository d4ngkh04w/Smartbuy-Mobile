<template>
    <div class="product-line-management">
        <div class="section-header">
            <div class="left-section">
                <h2><i class="fas fa-mobile-alt"></i> Quản lý Dòng sản phẩm</h2>
                <p>Quản lý các dòng sản phẩm theo thương hiệu</p>
            </div>

            <div class="right-section">
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm dòng sản phẩm..."
                    />
                </div>
                <button @click="openAddProductLineModal" class="add-button">
                    <i class="fas fa-plus"></i> Thêm dòng sản phẩm
                </button>
            </div>
        </div>

        <!-- Status Cards -->
        <div class="status-cards">
            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-mobile-alt"></i>
                </div>
                <div class="status-content">
                    <h3>{{ productLines.length }}</h3>
                    <p>Tổng số dòng sản phẩm</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-trademark"></i>
                </div>
                <div class="status-content">
                    <h3>{{ uniqueBrandCount }}</h3>
                    <p>Thương hiệu</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-content">
                    <h3>{{ activeCount }}</h3>
                    <p>Đang kích hoạt</p>
                </div>
            </div>
        </div>

        <!-- Brand filter pills -->
        <div class="brand-filter">
            <h3 class="filter-title">Lọc theo thương hiệu:</h3>
            <div class="filter-pills">
                <button
                    @click="selectedBrandId = null"
                    :class="['pill', !selectedBrandId ? 'active' : '']"
                >
                    Tất cả
                </button>
                <button
                    v-for="brand in brands"
                    :key="brand.id"
                    @click="selectedBrandId = brand.id"
                    :class="[
                        'pill',
                        selectedBrandId === brand.id ? 'active' : '',
                    ]"
                >
                    <span v-if="brand.logo" class="brand-logo">
                        <img :src="brand.logo" :alt="brand.name" />
                    </span>
                    {{ brand.name }}
                </button>
            </div>
        </div>

        <!-- Product Lines hierarchical view -->
        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
            <div
                v-else-if="filteredProductLines.length === 0"
                class="empty-state"
            >
                <i class="fas fa-th-list"></i>
                <p v-if="searchQuery">
                    Không tìm thấy dòng sản phẩm phù hợp với tìm kiếm
                </p>
                <p v-else-if="selectedBrandId">
                    Không có dòng sản phẩm nào cho thương hiệu này
                </p>
                <p v-else>Không có dòng sản phẩm nào</p>
                <button @click="openAddProductLineModal" class="action-button">
                    <i class="fas fa-plus"></i> Thêm dòng sản phẩm mới
                </button>
            </div>

            <div v-else class="product-line-list">
                <div
                    v-for="brandGroup in organizedProductLines"
                    :key="brandGroup.id"
                    class="brand-group"
                >
                    <div class="brand-header">
                        <div class="brand-info">
                            <div class="brand-logo-wrapper">
                                <img
                                    v-if="brandGroup.logo"
                                    :src="brandGroup.logo"
                                    :alt="brandGroup.name"
                                    class="brand-logo"
                                />
                                <span v-else class="no-logo">
                                    <i class="fas fa-trademark"></i>
                                </span>
                            </div>
                            <h3 class="brand-name">{{ brandGroup.name }}</h3>
                        </div>
                        <div class="brand-count">
                            <span class="count-badge"
                                >{{ brandGroup.productLines.length }} dòng sản
                                phẩm</span
                            >
                        </div>
                    </div>

                    <div class="product-lines">
                        <div
                            v-for="productLine in brandGroup.productLines"
                            :key="productLine.id"
                            class="product-line-item"
                        >
                            <div class="product-line-content">
                                <div class="product-line-image">
                                    <img
                                        v-if="productLine.image"
                                        :src="productLine.image"
                                        :alt="productLine.name"
                                    />
                                    <div v-else class="no-image">
                                        <i class="fas fa-mobile-alt"></i>
                                    </div>
                                </div>

                                <div class="product-line-details">
                                    <h4 class="product-line-name">
                                        {{ productLine.name }}
                                    </h4>
                                    <p class="product-line-description">
                                        {{
                                            productLine.description ||
                                            "Không có mô tả"
                                        }}
                                    </p>
                                    <div class="product-line-meta">
                                        <span
                                            :class="[
                                                'status-badge',
                                                productLine.isActive
                                                    ? 'active'
                                                    : 'inactive',
                                            ]"
                                        >
                                            {{
                                                productLine.isActive
                                                    ? "Đang kích hoạt"
                                                    : "Không kích hoạt"
                                            }}
                                        </span>
                                        <span class="product-count">
                                            <i class="fas fa-cube"></i>
                                            {{
                                                getProductLineProductsCount(
                                                    productLine.id
                                                )
                                            }}
                                            sản phẩm
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <div class="product-line-actions">
                                <button
                                    @click="editProductLine(productLine)"
                                    class="edit-button"
                                    title="Chỉnh sửa"
                                >
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button
                                    @click="confirmDelete(productLine)"
                                    class="delete-button"
                                    title="Xóa"
                                >
                                    <i class="fas fa-trash-alt"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Add/Edit Product Line Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>
                        {{
                            isEditing
                                ? "Chỉnh sửa dòng sản phẩm"
                                : "Thêm dòng sản phẩm mới"
                        }}
                    </h3>
                    <button @click="closeModal" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <form @submit.prevent="submitForm" class="modal-form">
                    <div class="form-group">
                        <label
                            >Thương hiệu <span class="required">*</span></label
                        >
                        <div class="select-wrapper">
                            <select
                                v-model="formData.brandId"
                                class="form-select"
                                required
                            >
                                <option :value="null" disabled>
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
                            <i class="fas fa-chevron-down select-icon"></i>
                        </div>
                    </div>

                    <div class="form-group">
                        <label
                            >Tên dòng sản phẩm
                            <span class="required">*</span></label
                        >
                        <input
                            v-model="formData.name"
                            type="text"
                            placeholder="Nhập tên dòng sản phẩm"
                            required
                        />
                    </div>

                    <div class="form-group">
                        <label>Ảnh minh họa</label>
                        <div
                            class="upload-area"
                            @click="$refs.fileInput.click()"
                        >
                            <img
                                v-if="imagePreview"
                                :src="imagePreview"
                                alt="Preview"
                                class="image-preview"
                            />
                            <div v-else class="upload-placeholder">
                                <i class="fas fa-cloud-upload-alt"></i>
                                <span>Nhấp để tải lên ảnh (SVG, PNG, JPG)</span>
                            </div>
                        </div>
                        <input
                            ref="fileInput"
                            type="file"
                            @change="handleFileChange"
                            accept="image/*"
                            class="hidden-input"
                        />
                        <div v-if="imagePreview" class="preview-actions">
                            <button
                                type="button"
                                @click="clearImage"
                                class="remove-button"
                            >
                                <i class="fas fa-trash-alt"></i> Xóa ảnh
                            </button>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Mô tả</label>
                        <textarea
                            v-model="formData.description"
                            placeholder="Nhập mô tả dòng sản phẩm"
                            rows="3"
                        ></textarea>
                    </div>

                    <div class="form-group">
                        <label class="checkbox-label">
                            <input
                                type="checkbox"
                                v-model="formData.isActive"
                            />
                            <span>Kích hoạt dòng sản phẩm</span>
                        </label>
                    </div>

                    <div class="form-actions">
                        <button
                            type="button"
                            @click="closeModal"
                            class="cancel-button"
                        >
                            <i class="fas fa-times"></i> Hủy
                        </button>
                        <button
                            type="submit"
                            class="submit-button"
                            :disabled="loading"
                        >
                            <span v-if="loading" class="spinner small"></span>
                            <i
                                v-else
                                class="fas"
                                :class="isEditing ? 'fa-save' : 'fa-plus'"
                            ></i>
                            {{ isEditing ? "Cập nhật" : "Thêm mới" }}
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <!-- Delete Confirmation Modal -->
        <div v-if="showDeleteModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div class="modal-header warning">
                    <h3>Xác nhận xóa dòng sản phẩm</h3>
                    <button @click="cancelDelete" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div class="warning-icon">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn xóa dòng sản phẩm
                        <strong>"{{ productLineToDelete?.name }}"</strong>?
                    </p>
                    <div v-if="hasProducts" class="warning-details">
                        <i class="fas fa-info-circle"></i>
                        <span
                            >Dòng sản phẩm này đang có
                            {{
                                getProductLineProductsCount(
                                    productLineToDelete?.id
                                )
                            }}
                            sản phẩm. Việc xóa có thể ảnh hưởng đến dữ liệu liên
                            quan.</span
                        >
                    </div>
                </div>

                <div class="modal-actions">
                    <button @click="cancelDelete" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="deleteProductLine"
                        class="delete-confirm-button"
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner small"></span>
                        <i v-else class="fas fa-trash-alt"></i> Xóa
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import axios from "axios";

// State
const productLines = ref([]);
const brands = ref([]);
const products = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showDeleteModal = ref(false);
const isEditing = ref(false);
const formData = ref({
    name: "",
    brandId: null,
    description: "",
    imageFile: null,
    isActive: true,
});
const productLineToDelete = ref(null);
const hasProducts = ref(false);
const imagePreview = ref("");
const fileInput = ref(null);
const searchQuery = ref("");
const selectedBrandId = ref(null);

// API base URL
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api/v1";

// Computed properties
const filteredProductLines = computed(() => {
    let filtered = productLines.value;

    // Filter by brand if selected
    if (selectedBrandId.value !== null) {
        filtered = filtered.filter(
            (pl) => pl.brandId === selectedBrandId.value
        );
    }

    // Filter by search query
    if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase().trim();
        filtered = filtered.filter(
            (pl) =>
                pl.name.toLowerCase().includes(query) ||
                (pl.description && pl.description.toLowerCase().includes(query))
        );
    }

    return filtered;
});

// Organize product lines by brand
const organizedProductLines = computed(() => {
    const brandMap = {};

    // Create brand map for organizing
    brands.value.forEach((brand) => {
        brandMap[brand.id] = {
            id: brand.id,
            name: brand.name,
            logo: brand.logo,
            productLines: [],
        };
    });

    // Assign product lines to brands
    filteredProductLines.value.forEach((pl) => {
        if (brandMap[pl.brandId]) {
            brandMap[pl.brandId].productLines.push(pl);
        }
    });

    // Convert to array and filter out empty brands
    return Object.values(brandMap).filter((b) => b.productLines.length > 0);
});

const uniqueBrandCount = computed(() => {
    return new Set(productLines.value.map((pl) => pl.brandId)).size;
});

const activeCount = computed(() => {
    return productLines.value.filter((pl) => pl.isActive).length;
});

// Methods
const getProductLineProductsCount = (productLineId) => {
    return products.value.filter((p) => p.productLineId === productLineId)
        .length;
};

// Fetch all brands
const fetchBrands = async () => {
    try {
        const response = await axios.get(`${API_URL}/brand`);
        brands.value = response.data.brands;
    } catch (error) {
        console.error("Error fetching brands:", error);
    }
};

// Fetch all product lines
const fetchProductLines = async () => {
    loading.value = true;
    try {
        const response = await axios.get(`${API_URL}/product-line`);
        productLines.value = response.data.productLines;
    } catch (error) {
        console.error("Error fetching product lines:", error);
    } finally {
        loading.value = false;
    }
};

// Fetch products for counts
const fetchProducts = async () => {
    try {
        const response = await axios.get(`${API_URL}/product?limit=1000`);
        products.value = response.data.products;
    } catch (error) {
        console.error("Error fetching products:", error);
    }
};

// Open modal to add new product line
const openAddProductLineModal = () => {
    isEditing.value = false;
    formData.value = {
        name: "",
        brandId: selectedBrandId.value || null,
        description: "",
        imageFile: null,
        isActive: true,
    };
    imagePreview.value = "";
    showModal.value = true;
};

// Open modal to edit product line
const editProductLine = (productLine) => {
    isEditing.value = true;
    formData.value = {
        id: productLine.id,
        name: productLine.name,
        brandId: productLine.brandId,
        description: productLine.description || "",
        imageFile: null,
        isActive:
            productLine.isActive !== undefined ? productLine.isActive : true,
    };
    imagePreview.value = productLine.image || "";
    showModal.value = true;
};

// Handle file upload
const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
        if (file.size > 1024 * 1024) {
            alert("Kích thước tệp quá lớn. Vui lòng chọn tệp dưới 1MB.");
            return;
        }

        formData.value.imageFile = file;
        // Create preview
        imagePreview.value = URL.createObjectURL(file);
    }
};

// Clear image
const clearImage = () => {
    formData.value.imageFile = null;
    imagePreview.value = "";
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Submit form (create or update)
const submitForm = async () => {
    loading.value = true;

    try {
        const data = new FormData();
        data.append("Name", formData.value.name);
        data.append("BrandId", formData.value.brandId);
        if (formData.value.imageFile) {
            data.append("Image", formData.value.imageFile);
        }
        if (formData.value.description) {
            data.append("Description", formData.value.description);
        }
        data.append("IsActive", formData.value.isActive);

        if (isEditing.value) {
            await axios.put(
                `${API_URL}/product-line/${formData.value.id}`,
                data
            );
        } else {
            await axios.post(`${API_URL}/product-line`, data);
        }

        closeModal();
        fetchProductLines();
    } catch (error) {
        console.error("Error submitting form:", error);
        // Handle error (show notification, etc.)
    } finally {
        loading.value = false;
    }
};

// Close modal
const closeModal = () => {
    showModal.value = false;
    formData.value = {
        name: "",
        brandId: null,
        description: "",
        imageFile: null,
        isActive: true,
    };
    imagePreview.value = "";
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Confirm delete
const confirmDelete = async (productLine) => {
    productLineToDelete.value = productLine;
    hasProducts.value = getProductLineProductsCount(productLine.id) > 0;
    showDeleteModal.value = true;
};

// Cancel delete
const cancelDelete = () => {
    showDeleteModal.value = false;
    productLineToDelete.value = null;
    hasProducts.value = false;
};

// Delete product line
const deleteProductLine = async () => {
    if (!productLineToDelete.value) return;

    loading.value = true;
    try {
        await axios.delete(
            `${API_URL}/product-line/${productLineToDelete.value.id}`
        );
        fetchProductLines();
        showDeleteModal.value = false;
        productLineToDelete.value = null;
    } catch (error) {
        console.error("Error deleting product line:", error);
        // Handle error (show notification, etc.)
    } finally {
        loading.value = false;
    }
};

// Watch for selectedBrandId changes to keep UI in sync
watch(selectedBrandId, (newValue) => {
    // If a user is adding a new product line and has a brand selected, pre-select it in the form
    if (newValue && !showModal.value) {
        formData.value.brandId = newValue;
    }
});

// Load data when component mounts
onMounted(async () => {
    await fetchBrands();
    await fetchProductLines();
    await fetchProducts();
});
</script>

<style scoped>
.product-line-management {
    width: 100%;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.left-section h2 {
    font-size: 1.25rem;
    color: #333;
    margin: 0 0 0.25rem 0;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.left-section p {
    color: #666;
    margin: 0;
    font-size: 0.9rem;
}

.right-section {
    display: flex;
    gap: 1rem;
    align-items: center;
}

.search-box {
    position: relative;
    width: 250px;
}

.search-box i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
}

.search-box input {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    transition: all 0.3s;
}

.search-box input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.add-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.6rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s;
}

.add-button:hover {
    background-color: #e94e9c;
}

.status-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.status-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.25rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.status-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.icon-wrapper {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background-color: #fff5fc;
    color: var(--primary-color);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
}

.status-content h3 {
    font-size: 1.5rem;
    margin: 0 0 0.25rem 0;
    color: #333;
}

.status-content p {
    margin: 0;
    color: #666;
    font-size: 0.85rem;
}

.brand-filter {
    background-color: white;
    border-radius: 12px;
    padding: 1.25rem;
    margin-bottom: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.filter-title {
    font-size: 0.9rem;
    color: #666;
    margin: 0 0 0.75rem 0;
    font-weight: 500;
}

.filter-pills {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
}

.pill {
    padding: 0.5rem 1rem;
    border-radius: 20px;
    background-color: #f1f5f9;
    color: #64748b;
    border: none;
    font-size: 0.9rem;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.pill:hover {
    background-color: #e2e8f0;
}

.pill.active {
    background-color: var(--primary-color);
    color: white;
}

.brand-logo {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
}

.brand-logo img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.data-card {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    padding: 1.5rem;
    overflow: hidden;
}

.loading-state,
.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 0;
    color: #666;
}

.spinner {
    width: 40px;
    height: 40px;
    border: 3px solid rgba(248, 110, 211, 0.1);
    border-top: 3px solid var(--primary-color);
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 1rem;
}

.spinner.small {
    width: 16px;
    height: 16px;
    border-width: 2px;
    margin: 0;
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

.empty-state i {
    font-size: 3rem;
    color: #ddd;
    margin-bottom: 1rem;
}

.empty-state p {
    margin-bottom: 1rem;
}

.action-button {
    padding: 0.5rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.product-line-list {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.brand-group {
    border: 1px solid #eee;
    border-radius: 12px;
    overflow: hidden;
}

.brand-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 1.5rem;
    background-color: #f8f9fa;
    border-bottom: 1px solid #eee;
}

.brand-info {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.brand-logo-wrapper {
    width: 32px;
    height: 32px;
    border-radius: 8px;
    overflow: hidden;
    background-color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #eee;
}

.brand-logo {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.no-logo {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
}

.brand-name {
    margin: 0;
    font-size: 1.1rem;
    color: #333;
}

.count-badge {
    background-color: #f1f5f9;
    color: #64748b;
    padding: 0.25rem 0.5rem;
    border-radius: 20px;
    font-size: 0.8rem;
}

.product-lines {
    display: flex;
    flex-direction: column;
}

.product-line-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    transition: background-color 0.2s;
}

.product-line-item:last-child {
    border-bottom: none;
}

.product-line-item:hover {
    background-color: #f9f9f9;
}

.product-line-content {
    display: flex;
    align-items: center;
    gap: 1rem;
    flex: 1;
}

.product-line-image {
    width: 48px;
    height: 48px;
    border-radius: 8px;
    overflow: hidden;
    background-color: #f9f9f9;
    border: 1px solid #eee;
    display: flex;
    align-items: center;
    justify-content: center;
}

.product-line-image img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.no-image {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
}

.product-line-details {
    display: flex;
    flex-direction: column;
    flex: 1;
}

.product-line-name {
    margin: 0 0 0.25rem 0;
    font-size: 1rem;
    color: #333;
}

.product-line-description {
    margin: 0 0 0.5rem 0;
    font-size: 0.9rem;
    color: #666;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

.product-line-meta {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.status-badge {
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
}

.status-badge.active {
    background-color: #e6f7ea;
    color: #22c55e;
}

.status-badge.inactive {
    background-color: #fee2e2;
    color: #ef4444;
}

.product-count {
    font-size: 0.8rem;
    color: #64748b;
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.product-line-actions {
    display: flex;
    gap: 0.5rem;
}

.edit-button,
.delete-button {
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    cursor: pointer;
    transition: all 0.2s;
}

.edit-button {
    background-color: #e6f7ea;
    color: #22c55e;
}

.edit-button:hover {
    background-color: #22c55e;
    color: white;
}

.delete-button {
    background-color: #fee2e2;
    color: #ef4444;
}

.delete-button:hover {
    background-color: #ef4444;
    color: white;
}

/* Modal Styles */
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
    backdrop-filter: blur(3px);
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 500px;
    max-height: 90vh;
    overflow-y: auto;
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-header.warning {
    border-bottom-color: #fee2e2;
    color: #ef4444;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    color: #333;
}

.close-button {
    background: none;
    border: none;
    color: #666;
    font-size: 1rem;
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

.modal-form {
    padding: 1.5rem;
}

.form-group {
    margin-bottom: 1.25rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #333;
}

.form-group input[type="text"],
.form-group textarea,
.form-select {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.95rem;
    transition: all 0.3s;
}

.form-group input[type="text"]:focus,
.form-group textarea:focus,
.form-select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.select-wrapper {
    position: relative;
}

.select-icon {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.form-select {
    appearance: none;
    padding-right: 2.5rem;
}

.required {
    color: #ef4444;
}

.upload-area {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 2rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.3s;
}

.upload-area:hover {
    border-color: var(--primary-color);
    background-color: #fff5fc;
}

.upload-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    color: #666;
}

.upload-placeholder i {
    font-size: 2rem;
    color: #999;
}

.image-preview {
    max-width: 100%;
    max-height: 150px;
    margin: 0 auto;
    display: block;
}

.preview-actions {
    display: flex;
    justify-content: center;
    margin-top: 0.75rem;
}

.remove-button {
    background: none;
    border: none;
    color: #ef4444;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.hidden-input {
    display: none;
}

.checkbox-label {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
}

.checkbox-label input {
    width: 16px;
    height: 16px;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1.5rem;
}

.cancel-button,
.submit-button,
.delete-confirm-button {
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
}

.cancel-button {
    background-color: #f9f9f9;
    border: 1px solid #ddd;
    color: #666;
}

.cancel-button:hover {
    background-color: #eee;
}

.submit-button {
    background-color: var(--primary-color);
    border: none;
    color: white;
}

.submit-button:hover {
    background-color: #e94e9c;
}

.submit-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.modal-body {
    padding: 1.5rem;
}

.text-center {
    text-align: center;
}

.warning-icon {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background-color: #fee2e2;
    color: #ef4444;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 2rem;
    margin: 0 auto 1.5rem auto;
}

.warning-message {
    font-size: 1.1rem;
    color: #333;
    margin-bottom: 1.5rem;
}

.warning-details {
    display: flex;
    align-items: flex-start;
    gap: 0.5rem;
    padding: 1rem;
    background-color: #fee2e2;
    border-radius: 8px;
    color: #ef4444;
    text-align: left;
    font-size: 0.9rem;
}

.modal-actions {
    padding: 1.25rem 1.5rem;
    border-top: 1px solid #eee;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
}

.delete-confirm-button {
    background-color: #ef4444;
    border: none;
    color: white;
}

.delete-confirm-button:hover {
    background-color: #dc2626;
}

.delete-confirm-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

@media (max-width: 768px) {
    .section-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
    }

    .right-section {
        width: 100%;
        flex-direction: column;
    }

    .search-box {
        width: 100%;
    }

    .add-button {
        width: 100%;
        justify-content: center;
    }

    .status-cards {
        grid-template-columns: 1fr;
    }
}
</style>
