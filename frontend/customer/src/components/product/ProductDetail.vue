<template>
	<Loading v-if="isLoading" />
	<div
		v-if="productData && productData.isActive"
		class="product-detail-container"
		:class="{ 'out-of-stock-disable': getQuanity <= 0 }"
	>
		<h2 class="product-title">{{ productData.name }}</h2>
		<div class="product-main-content">
			<!-- Phần ảnh sản phẩm -->
			<div class="product-image-section">
				<div class="product-image-slider">
					<!-- Ảnh chính -->
					<div class="main-image">
						<img
							:src="getCurrentMainImage"
							:alt="productData.name"
							class="active-image"
						/>
					</div>

					<!-- Danh sách thumbnail -->
					<div class="thumbnail-container">
						<div
							v-for="image in getAllImages()"
							:key="image.id"
							class="thumbnail"
							:class="{ active: currentImageId === image.id }"
							@click="selectedImgId = image.id"
						>
							<img
								:src="getImageUrl(image.imagePath)"
								:alt="`${productData.name} thumbnail`"
							/>
						</div>
					</div>
				</div>
			</div>

			<!-- Phần thông tin mua hàng (ngang với ảnh) -->
			<div class="product-info-section">
				<div class="product-info-buy">
					<!-- Thông tin giá -->
					<div class="price-section">
						<span class="current-price"
							>{{
								format.formatPrice(productData.salePrice)
							}}₫</span
						>
						<span
							v-if="productData.discounts.length > 0"
							class="original-price"
						>
							{{ format.formatPrice(productData.importPrice) }}₫
						</span>
						<span
							v-if="productData.discounts.length > 0"
							class="discount-badge"
						>
							-{{ calculateDiscountPercentage() }}%
						</span>
					</div>
					<!-- Chọn màu sắc -->
					<div
						class="color-selector"
						v-if="
							productData.colors && productData.colors.length > 0
						"
					>
						<label>Màu sắc:</label>
						<div class="color-options">
							<div
								v-for="color in productData.colors"
								:key="color.id"
								class="color-option"
								:class="{
									active: selectedColorId === color.id,
								}"
								@click="selectColor(color)"
							>
								<span class="color-name">{{ color.name }}</span>
							</div>
						</div>
					</div>

					<!-- Chọn số lượng -->
					<div class="quantity-selector">
						<label>Số lượng:</label>
						<div class="quantity-control">
							<button @click="decreaseQuantity">-</button>
							<input
								type="number"
								v-model="quantity"
								min="1"
								:max="getQuanity"
								@focus="handleQuantityFocus"
								@blur="handleQuantityBlur"
								@input="validateQuantity"
							/>
							<button @click="increaseQuantity">+</button>
						</div>
						<span class="stock-info" v-if="getQuanity > 0">
							Còn {{ getQuanity }} sản phẩm
						</span>
						<span class="stock-info out-of-stock" v-else>
							Màu này đã hết hàng. Vui lòng chọn màu khác để tiếp
							tục.
						</span>
					</div>

					<!-- Nút hành động -->
					<div class="action-buttons">
						<button
							class="add-to-cart-btn"
							@click="addToCart"
							:disabled="productData.quantity <= 0"
						>
							<i class="fas fa-shopping-cart"></i> Thêm vào giỏ
						</button>
						<button
							class="buy-now-btn"
							@click="buyNow"
							:disabled="productData.quantity <= 0"
						>
							Mua ngay
						</button>
					</div>
				</div>
			</div>
		</div>

		<!-- Thông số kỹ thuật -->
		<div class="product-specs">
			<div class="product-description" v-if="productData.description">
				<h3>Mô tả sản phẩm</h3>
				<div>{{ productData.description }}</div>
			</div>
			<div>
				<h3>Thông số kỹ thuật</h3>
				<div class="specs-grid">
					<!-- Bảo hành -->
					<div v-if="productData.detail.warranty" class="spec-item">
						<span class="spec-name">Bảo hành:</span>
						<span class="spec-value"
							>{{ productData.detail.warranty }} tháng</span
						>
					</div>

					<!-- RAM -->
					<div v-if="productData.detail.ram" class="spec-item">
						<span class="spec-name">RAM:</span>
						<span class="spec-value"
							>{{ productData.detail.ram }} GB</span
						>
					</div>

					<!-- Bộ nhớ trong -->
					<div v-if="productData.detail.storage" class="spec-item">
						<span class="spec-name">Bộ nhớ trong:</span>
						<span class="spec-value"
							>{{ productData.detail.storage }} GB</span
						>
					</div>

					<!-- CPU -->
					<div v-if="productData.detail.processor" class="spec-item">
						<span class="spec-name">Bộ xử lý:</span>
						<span class="spec-value">{{
							productData.detail.processor
						}}</span>
					</div>

					<!-- Hệ điều hành -->
					<div
						v-if="productData.detail.operatingSystem"
						class="spec-item"
					>
						<span class="spec-name">Hệ điều hành:</span>
						<span class="spec-value">{{
							productData.detail.operatingSystem
						}}</span>
					</div>

					<!-- Màn hình -->
					<div v-if="productData.detail.screenSize" class="spec-item">
						<span class="spec-name">Màn hình:</span>
						<span class="spec-value"
							>{{ productData.detail.screenSize }} inch</span
						>
					</div>

					<!-- Độ phân giải -->
					<div
						v-if="productData.detail.screenResolution"
						class="spec-item"
					>
						<span class="spec-name">Độ phân giải:</span>
						<span class="spec-value">{{
							productData.detail.screenResolution
						}}</span>
					</div>

					<!-- Dung lượng pin -->
					<div v-if="productData.detail.battery" class="spec-item">
						<span class="spec-name">Pin:</span>
						<span class="spec-value"
							>{{ productData.detail.battery }} mAh</span
						>
					</div>

					<!-- Số SIM -->
					<div v-if="productData.detail.simSlots" class="spec-item">
						<span class="spec-name">Số khe SIM:</span>
						<span class="spec-value">{{
							productData.detail.simSlots
						}}</span>
					</div>
				</div>
			</div>
		</div>
		<!-- Đánh giá sản phẩm -->
		<ProductComments
			:productId="Number(productId)"
			:isLoggedIn="isLoggedIn"
			:currentUserId="currentUserId"
		/>
	</div>
</template>

<script setup>
import { useRoute, useRouter } from "vue-router";
import { ref, onMounted, computed } from "vue";
import productService from "../../services/productService.js";
import authService from "../../services/authService.js";
import emitter from "../../utils/evenBus.js";
import format from "../../utils/format.js";
import Loading from "../common/Loading.vue";
import ProductComments from "./ProductComments.vue";

const route = useRoute();
const router = useRouter();

const productId = route.params.id;
const productData = ref(null);
const selectedColorId = ref(null);
const selectedImgId = ref(null);
const currentImageId = ref(null);
const quantity = ref(1);
const isLoading = ref(true);

const fetchProduct = async (productId) => {
	isLoading.value = true;
	const data = await productService.getProductById(productId);

	if (data) {
		productData.value = data;
		if (!productData.value.isActive) {
			// Chuyển hướng đến trang 404 hoặc trang thông báo
			router.push({ name: "not-found" });
			return;
		}
		// Chọn màu đầu tiên làm mặc định nếu có
		if (data.colors && data.colors.length > 0) {
			selectedColorId.value = data.colors[0].id;
		}
	} else {
		console.error("Product not found");
	}
	isLoading.value = false;
};

const getAllImages = () => {
	if (!productData.value || !productData.value.colors) return [];
	return productData.value.colors.flatMap((color) => color.images);
};

const findColorIdByImageId = (colorsArray, imageId) => {
	const foundColor = colorsArray.find((color) =>
		color.images.some((img) => img.id === imageId)
	);
	return foundColor ? foundColor.id : null;
};

const getQuanity = computed(() => {
	const color = productData.value.colors.find(
		(color) => color.id === selectedColorId.value
	);
	if (color) {
		const quantity = color.quantity;
		return quantity ? quantity : 0;
	}
	return 0;
});

const getCurrentMainImage = computed(() => {
	selectedColorId.value = findColorIdByImageId(
		productData.value.colors,
		selectedImgId.value
	);
	if (selectedImgId.value === null) {
		selectedImgId.value = productData.value.colors[0].images[0].id;
	}
	const images = getAllImages();
	if (!images?.length) return "";

	const foundImage = images.find((img) => img.id === selectedImgId.value);
	if (!foundImage) return "";

	currentImageId.value = selectedImgId.value;

	return getImageUrl(foundImage.imagePath) || "";
});

// Chọn màu
const selectColor = (color) => {
	selectedImgId.value = color.images[0].id;
};

// Helper functions
const getImageUrl = (imgPath) => {
	return productService.getUrlImage(imgPath);
};

const handleQuantityFocus = (event) => {
	// Khi focus vào input, chọn toàn bộ nội dung để dễ dàng xóa
	event.target.select();

	// Nếu giá trị là 1 (mặc định), xóa nó để người dùng có thể nhập số mới
	if (quantity.value === 1) {
		quantity.value = "";
	}
};

const handleQuantityBlur = () => {
	// Khi blur khỏi input, nếu giá trị trống hoặc không hợp lệ, đặt lại thành 1
	if (!quantity.value || quantity.value < 1) {
		quantity.value = 1;
	}
};

const validateQuantity = (event) => {
	const value = parseInt(event.target.value);
	const maxQuantity = getQuanity.value;

	// Nếu giá trị không phải là số hoặc nhỏ hơn 1
	if (isNaN(value) || value < 1) {
		// Nếu người dùng đang xóa giá trị (nhập chuỗi rỗng), cho phép
		if (event.target.value === "") {
			quantity.value = "";
			return;
		}
		quantity.value = 1;
	}
	// Nếu giá trị lớn hơn số lượng tồn kho
	else if (value > maxQuantity) {
		quantity.value = maxQuantity;
		emitter.emit("show-notification", {
			status: "error",
			message: "Đạt đến số lượng tối đa",
		});
	}
	// Giá trị hợp lệ
	else {
		quantity.value = value;
	}
};

const calculateDiscountPercentage = () => {
	if (!productData.value) return 0;
	return Math.round(
		((productData.value.salePrice - productData.value.importPrice) /
			productData.value.importPrice) *
			100
	);
};

const increaseQuantity = () => {
	if (quantity.value < getQuanity.value) quantity.value++;
	else {
		emitter.emit("show-notification", {
			status: "error",
			message: "Đạt đến số lượng tối đa",
		});
	}
};

const decreaseQuantity = () => {
	if (quantity.value > 1) quantity.value--;
	else {
		emitter.emit("show-notification", {
			status: "error",
			message: "Đạt đến số lượng tối thiểu",
		});
	}
};

const addToCart = async () => {
	try {
		const isAuthen = await checkAuth();
		if (!isAuthen) {
			return;
		}

		if (checkValidInfor()) {
			const res = await productService.addToCart(
				productId,
				quantity.value,
				selectedColorId.value
			);
			emitter.emit("show-notification", {
				status: "success",
				message: "Đã thêm vào giỏ hàng!",
			});
			emitter.emit("cart-updated");
		}
	} catch (error) {
		console.error("Lỗi khi thêm vào giỏ hàng:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Có lỗi xảy ra khi thêm vào giỏ hàng",
		});
	}
};

async function checkAuth() {
	try {
		const response = await authService.verifyUser();
		// Kiểm tra response kỹ hơn
		if (response?.data?.success) {
			return true;
		}

		// Thêm console.log để debug
		console.log("Chưa đăng nhập, chuyển hướng...");
		await router.push({ name: "not-logged-in" });
		return false;
	} catch (error) {
		console.error("Lỗi khi kiểm tra xác thực:", error);
		await router.push({ name: "not-logged-in" });
		return false;
	}
}

const checkValidInfor = () => {
	if (productData.value.quantity <= quantity.value) {
		emitter.emit("show-notification", {
			status: "error",
			message: "Sản phẩm không đáp ứng đủ số lượng",
		});
		return false;
	}
	if (selectedColorId.value === null) {
		emitter.emit("show-notification", {
			status: "warning",
			message: "Vui lòng chọn màu sắc",
		});
		return false;
	}
	return true;
};
console.log("Product:", productService.getProductById(productId));

const buyNow = async () => {
	const isAuthen = await checkAuth();
	if (isAuthen && checkValidInfor()) {
		const selectedColor = productData.value.colors.find(
			(c) => c.id === selectedColorId.value
		);

		// Tạo object sản phẩm
		const product = {
			productId: productId,
			colorId: selectedColorId.value,
			productName: productData.value.name,
			salePrice: productData.value.salePrice,
			quantity: quantity.value,
			colorName: selectedColor?.name || "",
			imagePath: selectedColor?.images[0]?.imagePath || "",
		};

		// Chuyển thành JSON string rồi encode
		const productsStr = encodeURIComponent(JSON.stringify([product]));

		router.push({
			name: "order",
			query: {
				direct: "true",
				products: productsStr,
			},
		});
	}
};

onMounted(async () => {
	await fetchProduct(productId);
	document.title = `${productData.value.name} - SmartBuy Mobile`;
	if (productData.value?.colors?.length > 0 && !selectedColorId.value) {
		selectedColorId.value = productData.value.colors[0].id;
		selectedImgId.value = productData.value.colors[0].images[0]?.id;
	}
});
// User authentication status
const isLoggedIn = computed(() => localStorage.getItem("token") !== null);
const currentUserId = computed(() => {
	try {
		const userInfo = JSON.parse(localStorage.getItem("user") || "{}");
		return userInfo.id || "";
	} catch (error) {
		console.error("Error parsing user info:", error);
		return "";
	}
});

// Xem ảnh lớn
const openImagePreview = (imgUrl) => {
	// Có thể triển khai lightbox ở đây
	console.log("Xem ảnh:", imgUrl);
};

// Cuộn đến form đánh giá
const scrollToReviewForm = () => {
	const element = document.querySelector(".comments-section");
	if (element) {
		element.scrollIntoView({ behavior: "smooth", block: "start" });
	}
};
</script>

<style scoped>
.product-detail-container {
	max-width: 1200px;
	margin: 0 auto;
	padding: 20px;
}

.product-title {
	font-size: 24px;
	margin-bottom: 20px;
	color: #333;
}

.product-main-content {
	display: flex;
	gap: 40px;
	margin-bottom: 40px;
}

.product-image-section {
	flex: 1;
}

.product-info-section {
	flex: 1;
	max-width: 400px;
}

/* Slider styles */
.product-image-slider {
	margin-bottom: 30px;
}

.main-image {
	width: 100%;
	height: 400px;
	border: 1px solid #eee;
	border-radius: 8px;
	overflow: hidden;
	margin-bottom: 15px;
}

.product-description {
	margin-bottom: 20px;
}

.main-image img {
	width: 100%;
	height: 100%;
	object-fit: contain;
}

.thumbnail-container {
	display: flex;
	gap: 10px;
	flex-wrap: wrap;
}

.thumbnail {
	width: 70px;
	height: 70px;
	border: 1px solid #ddd;
	border-radius: 4px;
	overflow: hidden;
	cursor: pointer;
	transition: all 0.2s;
}

.thumbnail:hover {
	border-color: #f63bce;
}

.thumbnail.active {
	border-color: #f63bc4;
	border-width: 2px;
}

.thumbnail img {
	width: 100%;
	height: 100%;
	object-fit: cover;
}

/* Product info styles */
.product-info-buy {
	position: sticky;
	top: 20px;
	padding: 20px;
	border: 1px solid #eee;
	border-radius: 8px;
	background: #fff;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.price-section {
	margin-bottom: 20px;
}

.current-price {
	font-size: 28px;
	font-weight: bold;
	color: #ef4444;
	margin-right: 10px;
}

.original-price {
	font-size: 18px;
	color: #999;
	text-decoration: line-through;
	margin-right: 10px;
}

.discount-badge {
	background-color: #ef4444;
	color: white;
	padding: 3px 8px;
	border-radius: 4px;
	font-size: 14px;
}

/* Color selector */
.color-selector {
	margin-bottom: 20px;
}

.color-selector label {
	display: block;
	margin-bottom: 8px;
	font-weight: 500;
}

.color-options {
	pointer-events: auto;
	display: flex;
	gap: 10px;
	flex-wrap: wrap;
}

.color-option {
	padding: 8px 12px;
	border: 1px solid #ddd;
	border-radius: 4px;
	cursor: pointer;
	transition: all 0.2s;
}

.color-option:hover {
	border-color: #f7ccee;
}

.color-option.active {
	border-color: #e878d7;
	background-color: #fcefff;
}

.color-name {
	font-size: 14px;
}

/* Quantity selector */
.quantity-selector {
	margin-bottom: 25px;
}

.quantity-selector label {
	display: block;
	margin-bottom: 8px;
	font-weight: 500;
}

.quantity-control {
	display: flex;
	align-items: center;
	gap: 5px;
	margin-bottom: 5px;
}

.quantity-control button {
	width: 30px;
	height: 30px;
	border: 1px solid #ddd;
	background: white;
	cursor: pointer;
	font-size: 16px;
	border-radius: 4px;
}

.quantity-control input {
	width: 60px;
	height: 30px;
	text-align: center;
	border: 1px solid #ddd;
	border-radius: 4px;
	transition: border-color 0.3s;
}

.quantity-control input:focus {
	border-color: var(--primary-color);
	outline: none;
}

.quantity-control input:placeholder-shown {
	color: #999;
}

.stock-info {
	font-size: 18px;
	color: #f2b0db;
}

.stock-info.out-of-stock {
	color: #ef4444;
	margin-top: 12px;
	padding: 10px 15px;
	background-color: #fff3cd;
	color: #856404;
	border: 1px solid #ffeeba;
	border-radius: 6px;
	font-size: 14px;
	display: flex;
	align-items: center;
	gap: 8px;
}

/* Action buttons */
.action-buttons {
	display: flex;
	gap: 15px;
	margin-bottom: 30px;
}

.add-to-cart-btn,
.buy-now-btn {
	padding: 12px 20px;
	border: none;
	border-radius: 6px;
	font-size: 16px;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
}

.add-to-cart-btn {
	background-color: white;
	border: 1px solid #fc7caf;
	color: var(--primary-color);
	flex: 1;
}

.add-to-cart-btn:hover:not(:disabled) {
	background-color: #ffefff;
}

.add-to-cart-btn:disabled {
	opacity: 0.6;
	cursor: not-allowed;
}

.buy-now-btn {
	background-color: var(--primary-color);
	color: white;
	flex: 2;
}

.buy-now-btn:hover:not(:disabled) {
	background-color: var(--hover-color);
}

.buy-now-btn:disabled {
	opacity: 0.6;
	cursor: not-allowed;
}

/* Product specs */
.product-specs {
	border-top: 1px solid #eee;
	padding-top: 20px;
}

.product-specs h3 {
	font-size: 18px;
	margin-bottom: 15px;
}

.specs-grid {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 12px;
}

.spec-item {
	display: flex;
	flex-direction: column;
}

.spec-name {
	font-weight: 500;
	color: #666;
	font-size: 14px;
}

.spec-value {
	font-size: 15px;
}

/* Product reviews */
.product-reviews {
	margin-top: 40px;
	padding-top: 20px;
	border-top: 1px solid #eee;
}

.product-reviews h3 {
	font-size: 20px;
	margin-bottom: 20px;
	color: #333;
	padding-bottom: 10px;
	border-bottom: 1px solid #eee;
}

/* Khi chưa có đánh giá */
.no-reviews {
	text-align: center;
	padding: 40px 20px;
	background-color: #f9f9f9;
	border-radius: 8px;
	color: #666;
}

.no-reviews i {
	font-size: 40px;
	color: #ccc;
	margin-bottom: 15px;
}

.no-reviews p {
	margin-bottom: 20px;
	font-size: 16px;
}

.btn-write-review {
	background-color: var(--primary-color);
	color: white;
	border: none;
	padding: 10px 20px;
	border-radius: 4px;
	cursor: pointer;
	font-size: 14px;
	transition: all 0.3s;
}

.btn-write-review:hover {
	background-color: var(--hover-color);
}

.btn-write-review i {
	font-size: 14px;
	margin-right: 5px;
}

/* Khi có đánh giá */
.reviews-summary {
	display: flex;
	gap: 40px;
	margin-bottom: 30px;
	padding: 20px;
	background-color: #f9f9f9;
	border-radius: 8px;
}

.average-rating {
	display: flex;
	flex-direction: column;
	align-items: center;
	min-width: 150px;
}

.rating-number {
	font-size: 36px;
	font-weight: bold;
	color: var(--primary-color);
}

.stars {
	margin: 5px 0;
	color: #ffc107;
}

.total-reviews {
	font-size: 14px;
	color: #666;
}

.rating-bars {
	flex: 1;
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.rating-bar {
	display: flex;
	align-items: center;
	gap: 10px;
}

.star-label {
	width: 60px;
	font-size: 14px;
	color: #666;
}

.bar-container {
	flex: 1;
	height: 8px;
	background-color: #e0e0e0;
	border-radius: 4px;
	overflow: hidden;
}

.bar-fill {
	height: 100%;
	background-color: var(--primary-color);
	border-radius: 4px;
}

.percentage {
	width: 40px;
	font-size: 14px;
	color: #666;
}

/* Danh sách đánh giá */
.reviews-list {
	display: flex;
	flex-direction: column;
	gap: 25px;
}

.review-item {
	padding: 20px;
	border: 1px solid #eee;
	border-radius: 8px;
}

.review-header {
	display: flex;
	align-items: center;
	gap: 15px;
	margin-bottom: 15px;
}

.review-avatar {
	width: 50px;
	height: 50px;
	border-radius: 50%;
	object-fit: cover;
}

.review-user {
	flex: 1;
}

.user-name {
	font-weight: 500;
	display: block;
	margin-bottom: 3px;
}

.review-rating {
	color: #ffc107;
}

.review-date {
	font-size: 14px;
	color: #999;
}

.review-content {
	margin-bottom: 15px;
	line-height: 1.6;
}

.review-images {
	display: flex;
	gap: 10px;
	flex-wrap: wrap;
}

.review-images img {
	width: 80px;
	height: 80px;
	object-fit: cover;
	border-radius: 4px;
	cursor: pointer;
	transition: transform 0.3s;
}

.review-images img:hover {
	transform: scale(1.05);
}

/* Responsive */
@media (max-width: 768px) {
	.product-main-content {
		flex-direction: column;
	}

	.product-info-section {
		max-width: 100%;
	}

	.main-image {
		height: 300px;
	}

	.specs-grid {
		grid-template-columns: 1fr;
	}

	.product-info-buy {
		position: static;
	}

	.reviews-summary {
		flex-direction: column;
		gap: 20px;
	}

	.average-rating {
		flex-direction: row;
		justify-content: center;
		gap: 15px;
		align-items: center;
	}

	.rating-number {
		font-size: 28px;
	}

	.review-header {
		flex-wrap: wrap;
	}

	.review-date {
		width: 100%;
		margin-top: 5px;
		padding-left: 65px;
	}
}

.out-of-stock-disable {
	pointer-events: none;
	opacity: 0.5;
	position: relative;
}
</style>
