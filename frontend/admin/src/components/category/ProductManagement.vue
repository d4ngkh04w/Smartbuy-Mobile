<script setup>
import { ref, onMounted, computed } from "vue";
import {
    getProducts,
    deleteProduct as deleteProductApi,
} from "@/services/productService";

// State
const products = ref([]);
const loading = ref(false);
const showDeleteModal = ref(false);
const productToDelete = ref(null);
const searchQuery = ref("");
const statusFilter = ref("all"); // "all", "active", "inactive"

// API base URL
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api/v1";

// Function to format image URL
const formatImageUrl = (imagePath) => {
    if (!imagePath) return null;

    // If image is already a full URL (starts with http or https)
    if (imagePath.startsWith("http")) {
        return imagePath;
    }

    // Get base URL from API config
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Normalize file path (convert \ to /)
    const normalizedPath = imagePath.replace(/\\/g, "/");

    // Check if path starts with /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Format price with currency symbol
const formatPrice = (price) => {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(price);
};

// Format RAM and Storage
const formatRAM = (ram) => {
    return `${ram} GB`;
};

const formatStorage = (storage) => {
    return storage >= 1024 ? `${storage / 1024} TB` : `${storage} GB`;
};

// Get main image of product
const getMainImage = (product) => {
    if (!product.images || product.images.length === 0) return null;
    const mainImage = product.images.find((img) => img.isMain);
    return mainImage
        ? formatImageUrl(mainImage.imagePath)
        : formatImageUrl(product.images[0].imagePath);
};

// Computed properties
const filteredProducts = computed(() => {
    let result = products.value;

    // Filter by status
    if (statusFilter.value !== "all") {
        const isActive = statusFilter.value === "active";
        result = result.filter((product) => product.isActive === isActive);
    }

    // Filter by search query
    if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase().trim();
        result = result.filter(
            (product) =>
                product.name.toLowerCase().includes(query) ||
                (product.description &&
                    product.description.toLowerCase().includes(query))
        );
    }

    return result;
});

const activeCount = computed(() => {
    return products.value.filter((p) => p.isActive).length;
});

const totalValue = computed(() => {
    return products.value.reduce(
        (sum, product) => sum + product.salePrice * product.quantity,
        0
    );
});

// Fetch all products
const fetchProducts = async () => {
    loading.value = true;
    try {
        const response = await getProducts();
        products.value = response.data.products;
        console.log("Fetched products:", products.value);
    } catch (error) {
        console.error("Error fetching products:", error);
    } finally {
        loading.value = false;
    }
};

// Confirm delete
const confirmDelete = (product) => {
    productToDelete.value = product;
    showDeleteModal.value = true;
};

// Cancel delete
const cancelDelete = () => {
    showDeleteModal.value = false;
    productToDelete.value = null;
};

// Delete product
const confirmDeleteProduct = async () => {
    if (!productToDelete.value) return;

    loading.value = true;
    try {
        await deleteProductApi(productToDelete.value.id);
        fetchProducts();
        showDeleteModal.value = false;
        productToDelete.value = null;
    } catch (error) {
        console.error("Error deleting product:", error);
        // Handle error (show notification, etc.)
    } finally {
        loading.value = false;
    }
};

// Load data when component mounts
onMounted(() => {
    fetchProducts();
});
</script>

<template>
    <div class="product-management">
        <div class="section-header">
            <div class="left-section">
                <h2><i class="fas fa-mobile-alt"></i> Quản lý Sản phẩm</h2>
                <p>Quản lý các sản phẩm điện thoại di động</p>
            </div>

            <div class="right-section">
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm sản phẩm..."
                    />
                </div>

                <!-- Status Filter -->
                <div class="status-filter">
                    <i class="fas fa-filter"></i>
                    <select v-model="statusFilter">
                        <option value="all">Tất cả trạng thái</option>
                        <option value="active">Đang kích hoạt</option>
                        <option value="inactive">Chưa kích hoạt</option>
                    </select>
                </div>

                <button class="add-button">
                    <i class="fas fa-plus"></i> Thêm sản phẩm
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
                    <h3>{{ products.length }}</h3>
                    <p>Tổng số sản phẩm</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-content">
                    <h3>{{ activeCount }}</h3>
                    <p>Sản phẩm đang kích hoạt</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-money-bill-wave"></i>
                </div>
                <div class="status-content">
                    <h3>{{ formatPrice(totalValue) }}</h3>
                    <p>Tổng giá trị</p>
                </div>
            </div>
        </div>

        <!-- Products Data Card -->
        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
            <div v-else-if="filteredProducts.length === 0" class="empty-state">
                <i class="fas fa-box-open"></i>
                <p>Không có sản phẩm nào</p>
                <button class="action-button">
                    <i class="fas fa-plus"></i> Thêm sản phẩm
                </button>
            </div>
            <div v-else class="products-grid">
                <div
                    v-for="product in filteredProducts"
                    :key="product.id"
                    class="product-card"
                >
                    <div class="product-image">
                        <img
                            :src="getMainImage(product)"
                            :alt="product.name"
                            onerror="this.src='https://via.placeholder.com/150x150?text=No+Image'"
                        />
                        <div
                            :class="[
                                'status-badge',
                                product.isActive ? 'active' : 'inactive',
                            ]"
                        >
                            {{ product.isActive ? "Đang bán" : "Ngừng bán" }}
                        </div>
                    </div>
                    <div class="product-info">
                        <h3 class="product-name">{{ product.name }}</h3>
                        <div class="product-specs">
                            <span
                                >RAM: {{ formatRAM(product.detail.ram) }}</span
                            >
                            <span
                                >•
                                {{
                                    formatStorage(product.detail.storage)
                                }}</span
                            >
                        </div>
                        <div class="product-prices">
                            <div class="sale-price">
                                {{ formatPrice(product.salePrice) }}
                            </div>
                            <div class="quantity">
                                Kho: {{ product.quantity }}
                            </div>
                        </div>
                    </div>
                    <div class="product-actions">
                        <button class="edit-button" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button
                            @click="confirmDelete(product)"
                            class="delete-button"
                            title="Xóa"
                        >
                            <i class="fas fa-trash-alt"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Delete Confirmation Modal -->
        <div v-if="showDeleteModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div class="modal-header warning">
                    <h3>Xác nhận xóa sản phẩm</h3>
                    <button @click="cancelDelete" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div class="warning-icon">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn xóa sản phẩm
                        <strong>"{{ productToDelete?.name }}"</strong>?
                    </p>
                    <div class="warning-details">
                        <i class="fas fa-info-circle"></i>
                        <span>
                            Sản phẩm sẽ bị vô hiệu hóa và không hiển thị cho
                            khách hàng. Các đơn hàng liên quan đến sản phẩm này
                            sẽ vẫn được giữ nguyên.
                        </span>
                    </div>
                </div>

                <div class="modal-actions">
                    <button @click="cancelDelete" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="confirmDeleteProduct"
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

<style scoped>
.product-management {
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

.status-filter {
    position: relative;
    width: 200px;
}

.status-filter i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.status-filter select {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23666' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'%3E%3C/polyline%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 10px center;
    background-size: 16px;
    transition: all 0.3s;
}

.status-filter select:focus {
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

/* Products Grid */
.products-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1.25rem;
}

.product-card {
    background-color: white;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
    position: relative;
    display: flex;
    flex-direction: column;
    border: 1px solid #eee;
}

.product-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.1);
}

.product-image {
    height: 160px;
    overflow: hidden;
    position: relative;
    background-color: #f9f9f9;
}

.product-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    object-position: center;
}

.status-badge {
    position: absolute;
    top: 10px;
    right: 10px;
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
    z-index: 1;
}

.status-badge.active {
    background-color: #e6f7ea;
    color: #22c55e;
}

.status-badge.inactive {
    background-color: #fee2e2;
    color: #ef4444;
}

.product-info {
    padding: 0.75rem 1rem;
    flex-grow: 1;
}

.product-name {
    font-size: 1rem;
    font-weight: 600;
    margin: 0 0 0.5rem 0;
    color: #333;
    line-height: 1.4;
}

.product-specs {
    font-size: 0.85rem;
    color: #666;
    margin-bottom: 0.5rem;
    display: flex;
    gap: 0.25rem;
}

.product-prices {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 0.5rem;
}

.sale-price {
    font-weight: 700;
    color: var(--primary-color);
    font-size: 1.1rem;
}

.quantity {
    font-size: 0.85rem;
    color: #666;
}

.product-actions {
    display: flex;
    border-top: 1px solid #eee;
}

.edit-button,
.delete-button {
    flex: 1;
    padding: 0.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    background-color: white;
    cursor: pointer;
    transition: all 0.2s;
}

.edit-button {
    color: #22c55e;
    border-right: 1px solid #eee;
}

.edit-button:hover {
    background-color: #f0f9f4;
}

.delete-button {
    color: #ef4444;
}

.delete-button:hover {
    background-color: #fef2f2;
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

.cancel-button,
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

    .search-box,
    .status-filter {
        width: 100%;
    }

    .add-button {
        width: 100%;
        justify-content: center;
    }

    .status-cards {
        grid-template-columns: 1fr;
    }

    .products-grid {
        grid-template-columns: repeat(auto-fill, minmax(100%, 1fr));
    }
}
</style>
