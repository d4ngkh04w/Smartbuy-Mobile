<script setup>
import { defineEmits } from "vue";
import productService from "../../../services/productService.js";

const props = defineProps({
    showModal: {
        type: Boolean,
        required: true,
    },
    product: {
        type: Object,
        default: null,
    },
});

const emit = defineEmits(["close"]);

// Format currency function
const formatCurrency = (value) => {
    if (!value) return "0 ₫";
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(value);
};

// Format image URL
const formatImageUrl = (imagePath) => {
    return productService.getUrlImage(imagePath);
};
</script>

<template>
    <div v-if="showModal" class="modal-backdrop">
        <div class="modal-container detail-modal">
            <div class="modal-header">
                <h3>Chi tiết sản phẩm</h3>
                <button @click="$emit('close')" class="close-button">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="modal-body">
                <div v-if="product" class="product-detail-content">
                    <!-- Thông tin cơ bản -->
                    <div class="product-detail-header">
                        <div class="product-detail-image">
                            <img
                                v-if="
                                    product.colors &&
                                    product.colors.length > 0 &&
                                    product.colors[0].images &&
                                    product.colors[0].images.length > 0
                                "
                                :src="
                                    formatImageUrl(
                                        product.colors[0].images.find(
                                            (img) => img.isMain
                                        )?.imagePath ||
                                            product.colors[0].images[0]
                                                ?.imagePath
                                    )
                                "
                                :alt="product.name"
                            />
                            <div v-else class="no-image">
                                <i class="fas fa-mobile-alt"></i>
                            </div>
                        </div>
                        <div class="product-detail-basic">
                            <h4 class="product-detail-name">
                                {{ product.name }}
                            </h4>
                            <div class="product-detail-info-row">
                                <span class="detail-label">ID sản phẩm:</span>
                                <span class="detail-value">{{
                                    product.id
                                }}</span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Thương hiệu:</span>
                                <span class="detail-value">{{
                                    product.brandName
                                }}</span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Dòng sản phẩm:</span>
                                <span class="detail-value">{{
                                    product.productLineName
                                }}</span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Giá nhập:</span>
                                <span class="detail-value">{{
                                    formatCurrency(product.importPrice)
                                }}</span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Giá bán:</span>
                                <span class="detail-value highlight">{{
                                    formatCurrency(product.salePrice)
                                }}</span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Số lượng:</span>
                                <span
                                    class="detail-value"
                                    :class="{
                                        'low-stock': product.stock < 10,
                                        'out-of-stock': product.stock <= 0,
                                    }"
                                >
                                    {{ product.stock }}
                                </span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Đã bán:</span>
                                <span class="detail-value sold-count">
                                    {{ product.sold || 0 }}
                                </span>
                            </div>
                            <div class="product-detail-info-row">
                                <span class="detail-label">Trạng thái:</span>
                                <span
                                    class="status-badge"
                                    :class="
                                        product.isActive ? 'active' : 'inactive'
                                    "
                                >
                                    {{
                                        product.isActive
                                            ? "Đang bán"
                                            : "Ngừng bán"
                                    }}
                                </span>
                            </div>
                        </div>
                    </div>

                    <!-- Mô tả sản phẩm -->
                    <div
                        v-if="product.description"
                        class="product-detail-section"
                    >
                        <h5 class="detail-section-title">Mô tả sản phẩm</h5>
                        <p class="product-description">
                            {{ product.description }}
                        </p>
                    </div>

                    <!-- Thông số kỹ thuật -->
                    <div v-if="product.detail" class="product-detail-section">
                        <h5 class="detail-section-title">Thông số kỹ thuật</h5>
                        <div class="specs-grid">
                            <div v-if="product.detail.ram" class="spec-item">
                                <div class="spec-label">
                                    <i class="fas fa-memory"></i> RAM
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.ram }} GB
                                </div>
                            </div>
                            <div
                                v-if="product.detail.storage"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-hdd"></i> Bộ nhớ trong
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.storage }} GB
                                </div>
                            </div>
                            <div
                                v-if="product.detail.screenSize"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-mobile-alt"></i> Kích thước
                                    màn hình
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.screenSize }}
                                    inch
                                </div>
                            </div>
                            <div
                                v-if="product.detail.screenResolution"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-expand"></i> Độ phân giải
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.screenResolution }}
                                </div>
                            </div>
                            <div
                                v-if="product.detail.battery"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-battery-full"></i> Pin
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.battery }} mAh
                                </div>
                            </div>
                            <div
                                v-if="product.detail.processor"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-microchip"></i> Chip xử lý
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.processor }}
                                </div>
                            </div>
                            <div
                                v-if="product.detail.operatingSystem"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-cog"></i> Hệ điều hành
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.operatingSystem }}
                                </div>
                            </div>
                            <div
                                v-if="product.detail.warranty"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-shield-alt"></i> Bảo hành
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.warranty }}
                                    tháng
                                </div>
                            </div>
                            <div
                                v-if="product.detail.simSlots"
                                class="spec-item"
                            >
                                <div class="spec-label">
                                    <i class="fas fa-sim-card"></i> Số khe SIM
                                </div>
                                <div class="spec-value">
                                    {{ product.detail.simSlots }}
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Màu sắc và hình ảnh -->
                    <div
                        v-if="product.colors && product.colors.length > 0"
                        class="product-detail-section"
                    >
                        <h5 class="detail-section-title">
                            Màu sắc và hình ảnh
                        </h5>
                        <div class="colors-list">
                            <div
                                v-for="color in product.colors"
                                :key="color.id"
                                class="color-detail-item"
                            >
                                <div class="color-header-detail">
                                    <h6 class="color-name">
                                        {{ color.name }}
                                    </h6>
                                    <div class="color-quantity-detail">
                                        <span class="quantity-label-detail"
                                            >Số lượng:</span
                                        >
                                        <span
                                            class="quantity-value-detail"
                                            :class="{
                                                'low-stock':
                                                    color.quantity < 10,
                                                'out-of-stock':
                                                    color.quantity <= 0,
                                            }"
                                        >
                                            {{ color.quantity }}
                                        </span>
                                    </div>
                                </div>
                                <div class="color-images-grid">
                                    <div
                                        v-for="image in color.images"
                                        :key="image.id"
                                        class="color-image-container"
                                        :class="{ 'main-image': image.isMain }"
                                    >
                                        <img
                                            :src="
                                                formatImageUrl(image.imagePath)
                                            "
                                            :alt="`${product.name} - ${color.name}`"
                                        />
                                        <div
                                            v-if="image.isMain"
                                            class="main-image-badge"
                                        >
                                            <i class="fas fa-star"></i> Ảnh
                                            chính
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div v-else>
                    <p>Không có thông tin chi tiết sản phẩm.</p>
                </div>
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
    max-width: 800px;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
    overflow: hidden;
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

.modal-body {
    padding: 1.5rem;
    overflow-y: auto;
    max-height: calc(90vh - 70px);
}

.product-detail-content {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.product-detail-header {
    display: grid;
    grid-template-columns: 200px 1fr;
    gap: 2rem;
}

@media (max-width: 768px) {
    .product-detail-header {
        grid-template-columns: 1fr;
    }
}

.product-detail-image {
    width: 200px;
    height: 200px;
    border-radius: 12px;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #ffffff;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.product-detail-image img {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
}

.no-image {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ddd;
    font-size: 3rem;
}

.product-detail-basic {
    display: flex;
    flex-direction: column;
}

.product-detail-name {
    font-size: 1.5rem;
    margin: 0 0 1rem 0;
    color: #333;
    font-weight: 600;
    line-height: 1.3;
    border-left: 3px solid var(--primary-color);
    padding-left: 0.75rem;
}

.product-detail-info-row {
    display: flex;
    gap: 1rem;
    margin-bottom: 0.5rem;
    align-items: center;
}

.detail-label {
    min-width: 120px;
    color: #6b7280;
    font-size: 0.9rem;
}

.detail-value {
    font-weight: 500;
    color: #333;
}

.detail-value.highlight {
    color: var(--primary-color);
    font-size: 1.1rem;
    font-weight: 600;
}

.low-stock {
    color: #e65100;
    background-color: #fff8e1;
    padding: 0.15rem 0.4rem;
    border-radius: 8px;
    font-size: 0.85rem;
}

.out-of-stock {
    color: #c62828;
    background-color: #ffebee;
    padding: 0.15rem 0.4rem;
    border-radius: 8px;
    font-size: 0.85rem;
}

.sold-count {
    color: #1e40af;
    background-color: #e1f5fe;
    padding: 0.15rem 0.4rem;
    border-radius: 8px;
    font-size: 0.85rem;
}

.status-badge {
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
    display: inline-block;
}

.status-badge.active {
    background-color: #e6f7ea;
    color: #22c55e;
}

.status-badge.inactive {
    background-color: #fee2e2;
    color: #ef4444;
}

.product-detail-section {
    border-top: 1px solid #eee;
    padding-top: 1.5rem;
}

.detail-section-title {
    font-size: 1.1rem;
    margin: 0 0 1rem 0;
    color: #333;
}

.product-description {
    line-height: 1.6;
    color: #4b5563;
}

.specs-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 1rem;
}

.spec-item {
    background-color: #ffffff;
    padding: 1rem;
    border-radius: 8px;
    border: 1px solid #f0f0f0;
}

.spec-label {
    color: #6b7280;
    font-size: 0.9rem;
    margin-bottom: 0.5rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.spec-value {
    font-weight: 500;
    color: #111827;
}

.colors-list {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.color-detail-item {
    background-color: #ffffff;
    border-radius: 8px;
    padding: 1rem;
    border: 1px solid #f0f0f0;
}

.color-header-detail {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
}

.color-name {
    margin: 0;
    font-size: 1rem;
    color: #111827;
}

.color-quantity-detail {
    font-size: 0.9rem;
}

.quantity-label-detail {
    color: #6b7280;
    margin-right: 0.5rem;
}

.quantity-value-detail {
    font-weight: 500;
    color: #111827;
}

.color-images-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
    gap: 0.75rem;
}

.color-image-container {
    position: relative;
    width: 120px;
    height: 120px;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid #eee;
}

.color-image-container img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.color-image-container.main-image {
    border: 2px solid var(--primary-color);
}

.main-image-badge {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.7);
    color: white;
    font-size: 0.75rem;
    padding: 0.25rem;
    text-align: center;
}

.main-image-badge i {
    color: #fbbf24;
    margin-right: 0.25rem;
}
</style>
