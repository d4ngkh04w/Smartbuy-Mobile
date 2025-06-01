<template>
	<div class="modal-backdrop">
		<div class="modal">
			<div class="modal-header">
				<h2>Thêm sản phẩm vào mã giảm giá</h2>
				<button @click="$emit('close')" class="close-button">
					<i class="fas fa-times"></i>
				</button>
			</div>
			<div class="modal-body">
				<div class="search-container">
					<div class="search-input-container">
						<i class="fas fa-search search-icon"></i>
						<input
							type="text"
							placeholder="Tìm kiếm sản phẩm..."
							v-model="searchQuery"
							class="search-input"
						/>
						<button
							v-if="searchQuery"
							@click="clearSearch"
							class="clear-button"
						>
							<i class="fas fa-times"></i>
						</button>
					</div>
					<div class="select-all-container">
						<label class="select-all-label">
							<input
								type="checkbox"
								@change="toggleSelectAll"
								:checked="allDisplayedProductsSelected"
								class="select-all-checkbox"
							/>
							<span class="select-all-text">
								{{
									allDisplayedProductsSelected
										? "Bỏ chọn tất cả"
										: "Chọn tất cả"
								}}
							</span>
						</label>
					</div>
				</div>

				<div class="product-selection">
					<div v-if="loading" class="loading-container">
						<div class="spinner"></div>
						<p>Đang tải sản phẩm...</p>
					</div>

					<div
						v-else-if="filteredProducts.length === 0"
						class="empty-state"
					>
						<i class="fas fa-box empty-icon"></i>
						<p>Không tìm thấy sản phẩm nào</p>
					</div>

					<div v-else class="product-list">
						<div
							v-for="product in filteredProducts"
							:key="product.id"
							class="product-item"
							:class="{ selected: isSelected(product.id) }"
							@click="toggleSelection(product.id)"
						>
							<div class="product-info">
								<img
									:src="
										productService.getProductMainImage(
											product
										)
									"
									:alt="product.name"
									class="product-image"
								/>
								<div class="product-details">
									<div class="product-name">
										{{ product.name }}
									</div>
									<div class="product-price">
										{{ formatCurrency(product.salePrice) }}
									</div>
								</div>
							</div>
							<div class="selection-indicator">
								<input
									type="checkbox"
									:checked="isSelected(product.id)"
									@click.stop
									@change="toggleSelection(product.id)"
									class="product-checkbox"
								/>
							</div>
						</div>
					</div>
				</div>

				<div
					class="selected-count"
					v-if="selectedProductIds.length > 0"
				>
					Đã chọn {{ selectedProductIds.length }} sản phẩm
				</div>

				<div class="form-actions">
					<button
						type="button"
						class="cancel-button"
						@click="$emit('close')"
						:disabled="submitting"
					>
						Hủy bỏ
					</button>
					<button
						type="button"
						class="submit-button"
						@click="handleSubmit"
						:disabled="
							submitting || selectedProductIds.length === 0
						"
					>
						<span v-if="submitting" class="spinner"></span>
						<span v-else>Thêm sản phẩm</span>
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue";
import productService from "../../../services/productService.js";

const props = defineProps({
	discountId: {
		type: Number,
		required: true,
	},
});

const emit = defineEmits(["close", "add-products"]);

const products = ref([]);
const loading = ref(false);
const submitting = ref(false);
const searchQuery = ref("");
const selectedProductIds = ref([]);

const filteredProducts = computed(() => {
	if (!searchQuery.value) return products.value;

	const query = searchQuery.value.toLowerCase();
	return products.value.filter(
		(product) =>
			product.name.toLowerCase().includes(query) ||
			product.id.toString().includes(query)
	);
});

// Computed property to check if all displayed products are selected
const allDisplayedProductsSelected = computed(() => {
	return (
		filteredProducts.value.length > 0 &&
		filteredProducts.value.every((product) =>
			selectedProductIds.value.includes(product.id)
		)
	);
});

const fetchProducts = async () => {
	loading.value = true;
	try {
		const response = await productService.getProducts();
		if (response.data && response.data.data) {
			products.value = response.data.data.filter((p) => p.isActive);
		} else {
			products.value = [];
		}
	} catch (error) {
		console.error("Error fetching products:", error);
		products.value = [];
	} finally {
		loading.value = false;
	}
};

const clearSearch = () => {
	searchQuery.value = "";
};

const isSelected = (productId) => {
	return selectedProductIds.value.includes(productId);
};

const toggleSelection = (productId) => {
	const index = selectedProductIds.value.indexOf(productId);
	if (index === -1) {
		selectedProductIds.value.push(productId);
	} else {
		selectedProductIds.value.splice(index, 1);
	}
};

const toggleSelectAll = () => {
	if (allDisplayedProductsSelected.value) {
		// Deselect all currently displayed products
		filteredProducts.value.forEach((product) => {
			const index = selectedProductIds.value.indexOf(product.id);
			if (index !== -1) {
				selectedProductIds.value.splice(index, 1);
			}
		});
	} else {
		// Select all currently displayed products
		filteredProducts.value.forEach((product) => {
			if (!selectedProductIds.value.includes(product.id)) {
				selectedProductIds.value.push(product.id);
			}
		});
	}
};

const formatCurrency = (amount) => {
	if (!amount) return "---";
	return new Intl.NumberFormat("vi-VN", {
		style: "currency",
		currency: "VND",
	}).format(amount);
};

const handleSubmit = () => {
	if (selectedProductIds.value.length === 0) return;

	submitting.value = true;
	emit("add-products", props.discountId, selectedProductIds.value);
	submitting.value = false;
};

// const getProductImage = (product) => {
// 	// Nếu sản phẩm có imageUrl, sử dụng nó
// 	if (product.imageUrl) {
// 		return product.imageUrl;
// 	}

// 	// Tìm hình ảnh từ colors
// 	if (product.colors && product.colors.length > 0) {
// 		// Tìm ảnh chính nếu có
// 		for (const color of product.colors) {
// 			if (color.images && color.images.length > 0) {
// 				const mainImage = color.images.find((img) => img.isMain);
// 				if (mainImage) {
// 					return productService.getUrlImage(mainImage.imagePath);
// 				}
// 				// Nếu không có ảnh chính, lấy ảnh đầu tiên
// 				return productService.getUrlImage(color.images[0].imagePath);
// 			}
// 		}
// 	}

// 	// Trả về ảnh mặc định nếu không tìm thấy
// 	return "https://via.placeholder.com/40";
// };

onMounted(() => {
	fetchProducts();
});
</script>

<style scoped>
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
}

.modal {
	background-color: white;
	border-radius: 12px;
	width: 600px;
	max-width: 95%;
	max-height: 90vh;
	overflow-y: auto;
	box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
	animation: slide-in 0.3s ease;
}

@keyframes slide-in {
	from {
		transform: translateY(-30px);
		opacity: 0;
	}
	to {
		transform: translateY(0);
		opacity: 1;
	}
}

.modal-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 1.5rem;
	border-bottom: 1px solid #eee;
}

.modal-header h2 {
	margin: 0;
	font-size: 1.3rem;
	color: #333;
}

.close-button {
	background: none;
	border: none;
	font-size: 1.2rem;
	color: #999;
	cursor: pointer;
	transition: color 0.2s;
}

.close-button:hover {
	color: #333;
}

.modal-body {
	padding: 1.5rem;
}

.search-container {
	margin-bottom: 1.5rem;
	display: flex;
	flex-wrap: wrap;
	gap: 0.5rem;
	align-items: center;
}

.search-input-container {
	position: relative;
	flex: 1;
	min-width: 200px;
}

.search-icon {
	position: absolute;
	left: 12px;
	top: 50%;
	transform: translateY(-50%);
	color: #aaa;
}

.search-input {
	width: 100%;
	padding: 10px 10px 10px 38px;
	border: 1px solid #e1e1e1;
	border-radius: 8px;
	font-size: 14px;
	background-color: white;
	transition: all 0.2s;
}

.search-input:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(var(--primary-rgb), 0.2);
	outline: none;
}

.clear-button {
	position: absolute;
	right: 12px;
	top: 50%;
	transform: translateY(-50%);
	background: none;
	border: none;
	color: #aaa;
	cursor: pointer;
	font-size: 14px;
	padding: 0;
}

.clear-button:hover {
	color: #666;
}

.select-all-container {
	margin-left: 1rem;
	white-space: nowrap;
}

.select-all-label {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	cursor: pointer;
	font-size: 0.9rem;
	font-weight: 500;
	color: #666;
	user-select: none;
}

.select-all-checkbox {
	width: 20px;
	height: 20px;
	cursor: pointer;
	accent-color: var(--primary-color);
}

.select-all-text {
	transition: color 0.2s;
}

.select-all-checkbox:checked + .select-all-text {
	color: var(--primary-color);
	font-weight: 600;
}

.product-selection {
	border: 1px solid #eee;
	border-radius: 8px;
	height: 350px;
	overflow-y: auto;
	margin-bottom: 1rem;
}

.product-list {
	display: flex;
	flex-direction: column;
}

.product-item {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 1rem;
	border-bottom: 1px solid #f0f0f0;
	cursor: pointer;
	transition: background-color 0.2s;
}

.product-item:last-child {
	border-bottom: none;
}

.product-item:hover {
	background-color: #f8f9fa;
}

.product-item.selected {
	background-color: rgba(var(--primary-rgb), 0.05);
}

.product-info {
	display: flex;
	align-items: center;
	gap: 1rem;
}

.product-image {
	width: 40px;
	height: 40px;
	border-radius: 6px;
	object-fit: cover;
}

.product-name {
	font-weight: 500;
	color: #333;
	margin-bottom: 0.2rem;
}

.product-price {
	font-size: 0.85rem;
	color: #666;
}

.selection-indicator {
	color: var(--primary-color);
	font-size: 1.2rem;
}

.product-checkbox {
	width: 18px;
	height: 18px;
	cursor: pointer;
	accent-color: var(--primary-color);
}

.loading-container {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	height: 100%;
}

.spinner {
	width: 25px;
	height: 25px;
	border: 2px solid rgba(var(--primary-rgb), 0.2);
	border-radius: 50%;
	border-top-color: var(--primary-color);
	animation: spin 0.8s linear infinite;
}

.loading-container .spinner {
	margin-bottom: 1rem;
	width: 40px;
	height: 40px;
}

@keyframes spin {
	to {
		transform: rotate(360deg);
	}
}

.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	height: 100%;
	color: #888;
}

.empty-icon {
	font-size: 2rem;
	color: #ddd;
	margin-bottom: 1rem;
}

.selected-count {
	padding: 0.5rem 0;
	font-size: 0.9rem;
	color: var(--primary-color);
	font-weight: 500;
}

.form-actions {
	display: flex;
	justify-content: flex-end;
	gap: 1rem;
	margin-top: 1.5rem;
	position: relative;
	z-index: 5;
}

.cancel-button,
.submit-button {
	padding: 0.6rem 1.2rem;
	border-radius: 8px;
	font-size: 0.95rem;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
}

.cancel-button {
	background-color: #f1f1f1;
	color: #666;
	border: none;
}

.cancel-button:hover {
	background-color: #e5e5e5;
}

.submit-button {
	background-color: var(--primary-color);
	color: white;
	border: none;
	display: flex;
	align-items: center;
	justify-content: center;
	min-width: 120px;
	position: relative;
	z-index: 10;
}

.submit-button:hover {
	background-color: var(--primary-hover);
}

.submit-button:disabled,
.cancel-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}
</style>
