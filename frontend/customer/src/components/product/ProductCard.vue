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
				<p>{{ format.formatPrice(product.price) }}₫</p>
			</div>
			<div class="rating">
				<div class="stars" v-if="product.ratingCount > 0">
					<i
						v-for="i in 5"
						:key="i"
						class="fas fa-star"
						:class="{ active: i <= Math.round(product.rating) }"
					></i>
					<span class="rating-count"
						>({{ product.ratingCount }})</span
					>
				</div>
				<div class="no-rating" v-else>
					<span>Chưa có đánh giá</span>
				</div>
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
	background-color: #fafafa;
}

.product-img {
	width: 100%;
	height: 180px;
	object-fit: contain;
	transition: transform 0.3s;
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

.price {
	font-size: 16px;
	font-weight: 600;
	color: #ff7043;
	margin: 0;
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

.rating-count {
	font-size: 13px;
	color: #555;
}

.no-rating {
	font-size: 13px;
	color: #888;
	font-style: italic;
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

	.product-middle p {
		font-size: 14px;
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
