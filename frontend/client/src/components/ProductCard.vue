<template>
	<div
		class="product-card"
		:style="{
			'--card-bg': cardColor,
			'--button-bg': buttonColor,
			'--height': height,
		}"
	>
		<img :src="product.image" alt="image" class="product-img" />
		<div class="product-info">
			<h3>{{ product.title }}</h3>
			<p>{{ product.price }}₫</p>
			<button @click="handleBuy">Mua ngay</button>
			<button @click="handleAddToCart">Thêm vào giỏ hàng</button>
		</div>
	</div>
</template>

<script setup>
defineProps({
	product: Object,
	cardColor: {
		type: String,
		default: "var(--secondary-color, #fff)",
	},
	buttonColor: {
		type: String,
		default: "var(--background-color, #35AFFB)",
	},
	height: {
		type: String,
		default: "250px",
	},
});
const handleAddToCart = () => {
	// thay thế bằng component thông báo
	alert("Đã thêm sản phẩm vào giỏ hàng!");
};
const handleBuy = () => {
	// xử lý mua hàng
};
</script>

<style scoped>
.product-card {
	width: 200px;
	height: var(--height);
	border-radius: 10px;
	background-color: var(--card-bg);
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
	overflow: hidden;
	transition: transform 0.2s ease, box-shadow 0.3s ease;
}
.product-card:hover {
	transform: translateY(-5px);
	box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}
.product-img {
	width: 100%;
	height: 150px;
	object-fit: cover;
}
.product-info {
	padding: 12px;
	text-align: center;
}
.product-info h3 {
	font-size: 18px;
	color: var(--text-color, #333);
}
.product-info p {
	font-size: 16px;
	margin: 5px 0;
	color: var(--price-color, #000);
}
button {
	background-color: var(--button-bg);
	color: #fff;
	border: none;
	border-radius: 5px;
	padding: 10px 20px;
	font-size: 14px;
	cursor: pointer;
	transition: background-color 0.3s ease;
}
button:hover {
	background-color: var(--button-hover-bg, #0078d4);
}
</style>
