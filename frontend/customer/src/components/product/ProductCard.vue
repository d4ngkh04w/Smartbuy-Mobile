<template>
    <div
        class="product-card"
        :style="{
            '--card-bg': cardColor,
            '--button-bg': buttonColor,
            '--height': height,
        }"
    >
        <router-link
            :to="{ name: 'product-detail', params: { id: product.id } }"
            class="product-top-link"
        >
            <div class="product-top">
                <img
                    v-lazy="getUrlImg(product.imageUrl)"
                    alt="product image"
                    class="product-img"
                />
            </div>
            <div class="product-middle">
                <h3>{{ product.name }}</h3>
                <!-- Price section giống ProductDetail -->
                <div class="price-section">
                    <span class="current-price">
                        {{ format.formatPrice(product.price) }}₫
                    </span>
                    <span v-if="product.discount" class="original-price">
                        {{ format.formatPrice(product.salePrice) }}₫
                    </span>
                    <span v-if="product.discount" class="discount-badge">
                        {{ product.discount }}
                    </span>
                </div>
            </div>
            <div class="rating">
                <div class="stars">
                    <i
                        v-for="i in 5"
                        :key="i"
                        :class="getStarClass(i, product.rating)"
                    ></i>
                </div>
                <span class="rating-count">({{ product.ratingCount }})</span>
            </div>

            <!-- Số lượt đã bán -->
            <div class="sold-count">Đã bán: {{ product.sold }}</div>
        </router-link>
    </div>
</template>

<script setup>
import productService from "../../services/productService.js";
import format from "@/utils/format.js";

const props = defineProps({
    product: {
        type: Object,
        required: true,
    },
    cardColor: {
        type: String,
        default: "#fff",
    },
    buttonColor: {
        type: String,
        default: "#3b82f6",
    },
    height: {
        type: String,
        default: "370px",
    },
});
// Hàm lấy link ảnh
const getUrlImg = (url) => {
    return productService.getUrlImage(url);
};

// Tính toán lớp CSS cho hiển thị sao dựa trên điểm đánh giá
const getStarClass = (position, rating) => {
    if (position <= Math.floor(rating)) {
        // Sao đầy đủ khi vị trí <= phần nguyên của rating
        return "fas fa-star active";
    } else if (position === Math.ceil(rating) && rating % 1 >= 0.5) {
        // Nửa sao khi vị trí = phần nguyên + 1 và phần thập phân >= 0.5
        return "fas fa-star-half-alt active";
    } else {
        // Sao trống
        return "far fa-star";
    }
};
</script>

<style scoped>
.product-card {
    width: 100%;
    height: var(--height, 380px);
    background-color: var(--card-bg, #fff);
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    overflow: hidden;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    transition: transform 0.3s, box-shadow 0.3s;
}

.product-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 18px rgba(0, 0, 0, 0.12);
}

.product-top {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 12px;
    background-color: #ffffff;
    border: 1px solid #f0f0f0;
    border-bottom: none;
}

.product-img {
    width: 100%;
    height: 180px;
    object-fit: contain;
    transition: transform 0.3s;
    border-radius: 4px;
}

.product-card:hover .product-img {
    transform: scale(1.05);
}

.product-middle {
    padding: 12px 16px;
    text-align: left;
    background-color: #fff;
}

.product-middle h3 {
    font-size: 15px;
    font-weight: 600;
    color: var(--text-color, #333);
    margin-bottom: 10px;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
    min-height: 40px;
}

/* Price section styles giống ProductDetail */
.price-section {
    margin-bottom: 8px;
}

.current-price {
    font-size: 16px;
    font-weight: bold;
    color: #ef4444;
    margin-right: 8px;
}

.original-price {
    font-size: 14px;
    color: #999;
    text-decoration: line-through;
    margin-right: 8px;
}

.discount-badge {
    background-color: #ef4444;
    color: white;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 500;
}

.product-bottom {
    display: flex;
    justify-content: space-between;
    padding: 12px;
    background-color: #fff;
}
.product-card {
    display: block;
    text-decoration: none;
    color: inherit;
}
/* Đánh giá sản phẩm */
.rating {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
    padding: 4px 0;
    font-size: 14px;
}

.stars {
    display: flex;
    gap: 2px;
}

.stars i {
    color: #ccc;
    font-size: 14px;
}

.stars i.active {
    color: #ffd700; /* vàng */
}

.stars .fa-star-half-alt.active {
    color: #ffd700; /* vàng cho nửa sao */
}

.rating-count {
    font-size: 13px;
    color: #555;
}

/* Số lượng đã bán */
.sold-count {
    text-align: center;
    font-size: 13px;
    color: #666;
    padding-bottom: 8px;
}

/* Responsive: Mobile */
@media (max-width: 768px) {
    .product-card {
        width: 100%;
        max-width: 100%;
        height: auto;
    }

    .product-img {
        height: 180px;
    }
    .product-middle h3 {
        font-size: 16px;
    }

    .current-price {
        font-size: 15px;
    }

    .original-price {
        font-size: 13px;
    }

    .discount-badge {
        font-size: 11px;
        padding: 2px 5px;
    }

    .rating,
    .sold-count {
        font-size: 12px;
    }

    .stars i {
        font-size: 12px;
    }
}
</style>
