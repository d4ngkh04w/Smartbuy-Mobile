<template>
	<div class="modal-backdrop">
		<div class="modal">
			<div class="modal-header">
				<h2>Chi tiết mã giảm giá</h2>
				<div class="modal-actions">
					<button
						class="edit-button"
						@click="$emit('edit', discount)"
						title="Chỉnh sửa mã giảm giá"
					>
						<i class="fas fa-edit"></i>
					</button>
					<button
						@click="$emit('close')"
						class="close-button"
						title="Đóng"
					>
						<i class="fas fa-times"></i>
					</button>
				</div>
			</div>
			<div class="modal-body">
				<div v-if="discount" class="discount-details">
					<div class="detail-section">
						<h3 class="section-title">Thông tin mã giảm giá</h3>

						<div class="detail-row">
							<div class="detail-label">ID:</div>
							<div class="detail-value">{{ discount.id }}</div>
						</div>

						<div class="detail-row">
							<div class="detail-label">Phần trăm giảm giá:</div>
							<div class="detail-value">
								{{
									discount.discountPercentage
										? `${discount.discountPercentage}%`
										: "---"
								}}
							</div>
						</div>

						<div class="detail-row">
							<div class="detail-label">Số tiền giảm:</div>
							<div class="detail-value">
								{{
									discount.discountAmount
										? formatCurrency(
												discount.discountAmount
										  )
										: "---"
								}}
							</div>
						</div>

						<div class="detail-row">
							<div class="detail-label">Ngày bắt đầu:</div>
							<div class="detail-value">
								{{ formatDate(discount.startDate) }}
							</div>
						</div>

						<div class="detail-row">
							<div class="detail-label">Ngày kết thúc:</div>
							<div class="detail-value">
								{{ formatDate(discount.endDate) }}
							</div>
						</div>

						<div class="detail-row">
							<div class="detail-label">Trạng thái:</div>
							<div class="detail-value">
								<span
									class="status-badge"
									:class="{
										active: discount.status === 'Active',
										expired: discount.status === 'Expired',
										upcoming:
											discount.status === 'Upcoming',
									}"
								>
									{{
										discount.status === "Active"
											? "Đang hoạt động"
											: discount.status === "Expired"
											? "Hết hạn"
											: "Sắp tới"
									}}
								</span>
							</div>
						</div>
					</div>

					<div class="detail-section products-section">
						<div class="section-header">
							<h3 class="section-title">Sản phẩm áp dụng</h3>
							<button
								class="add-product-button"
								@click="$emit('add-products', discount)"
							>
								<i class="fas fa-plus"></i> Thêm sản phẩm
							</button>
						</div>

						<div
							class="product-list"
							v-if="!loading && products.length > 0"
						>
							<div
								v-for="product in products"
								:key="product.id"
								class="product-item"
							>
								<div class="product-info">
									<img
										:src="
											getProductMainImage(product)
										"
										:alt="product.name"
										class="product-image"
									/>
									<div class="product-details">
										<div class="product-name">
											{{ product.name }}
										</div>
										<div class="product-price">
											{{
												formatCurrency(
													product.salePrice
												)
											}}
										</div>
									</div>
								</div>
								<button
									class="remove-button"
									@click="removeProduct(product.id)"
									title="Xóa sản phẩm khỏi mã giảm giá"
								>
									<i class="fas fa-times"></i>
								</button>
							</div>
						</div>

						<div v-else-if="loading" class="loading-container">
							<div class="spinner"></div>
							<p>Đang tải danh sách sản phẩm...</p>
						</div>

						<div v-else class="empty-products">
							<i class="fas fa-box-open empty-icon"></i>
							<p>
								Chưa có sản phẩm nào được áp dụng mã giảm giá
								này
							</p>
							<button
								class="empty-add-button"
								@click="$emit('add-products', discount)"
							>
								<i class="fas fa-plus"></i> Thêm sản phẩm
							</button>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { defineEmits } from "vue";
import discountService from "../../../services/discountService";
import productService from "../../../services/productService";
import emitter from "../../../utils/evenBus.js";

const props = defineProps({
	discount: {
		type: Object,
		required: true,
	},
	products: {
		type: Array,
		default: () => [],
	},
	loading: {
		type: Boolean,
		default: false,
	},
});

const emit = defineEmits(["close", "edit", "add-products"]);

const formatCurrency = (amount) => {
	if (!amount) return "---";
	return new Intl.NumberFormat("vi-VN", {
		style: "currency",
		currency: "VND",
	}).format(amount);
};

const formatDate = (dateString) => {
	if (!dateString) return "---";
	const date = new Date(dateString);
	return new Intl.DateTimeFormat("vi-VN", {
		year: "numeric",
		month: "2-digit",
		day: "2-digit",
		hour: "2-digit",
		minute: "2-digit",
	}).format(date);
};

// Status is now handled by the backend

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

const removeProduct = async (productId) => {
	try {
		await discountService.removeProductFromDiscount(
			props.discount.id,
			productId
		);
		showSuccessNotification("Xóa sản phẩm khỏi mã giảm giá thành công");
		// Re-fetch products for this discount
		emit("refresh-products");
	} catch (error) {
		console.error("Error removing product from discount:", error);
		showErrorNotification(
			"Có lỗi xảy ra khi xóa sản phẩm khỏi mã giảm giá"
		);
	}
};

const getProductMainImage = (product) => {
	console .log("Getting main image for product:", product);
    const url = productService.getProductMainImage(product);
	console.log("Product main image URL:", url);
	return url || "https://via.placeholder.com/150"; // Fallback image if no main image is available
};
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
	width: 650px;
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

.modal-actions {
	display: flex;
	gap: 0.5rem;
}

.edit-button {
	background-color: rgba(255, 190, 11, 0.1);
	border: none;
	color: #ffbe0b;
	font-size: 1rem;
	cursor: pointer;
	transition: all 0.2s;
	width: 30px;
	height: 30px;
	border-radius: 6px;
	display: flex;
	align-items: center;
	justify-content: center;
}

.edit-button:hover {
	transform: scale(1.05);
	background-color: #ffbe0b;
	color: white;
}

.close-button {
	background-color: rgba(153, 153, 153, 0.1);
	border: none;
	font-size: 0.9rem;
	color: #999;
	cursor: pointer;
	transition: all 0.2s;
	width: 30px;
	height: 30px;
	border-radius: 6px;
	display: flex;
	align-items: center;
	justify-content: center;
}

.close-button:hover {
	background-color: #999;
	color: white;
	transform: scale(1.05);
}

.modal-body {
	padding: 1.5rem;
}

.discount-details {
	display: flex;
	flex-direction: column;
	gap: 2rem;
}

.detail-section {
	border-radius: 8px;
	border: 1px solid #eee;
	padding: 1.2rem;
	background-color: #fcfcfc;
}

.section-title {
	margin-top: 0;
	margin-bottom: 1rem;
	font-size: 1.1rem;
	color: #444;
	font-weight: 600;
	border-bottom: 1px solid #eee;
	padding-bottom: 0.5rem;
}

.detail-row {
	display: flex;
	padding: 0.5rem 0;
}

.detail-label {
	width: 150px;
	font-weight: 500;
	color: #666;
	flex-shrink: 0;
}

.detail-value {
	flex: 1;
	color: #333;
}

.status-badge {
	display: inline-block;
	padding: 3px 8px;
	border-radius: 50px;
	font-size: 0.75rem;
	font-weight: 500;
}

.status-badge.active {
	background-color: rgba(46, 213, 115, 0.15);
	color: #2ed573;
}

.status-badge.expired {
	background-color: rgba(255, 71, 87, 0.15);
	color: #ff4757;
}

.status-badge.upcoming {
	background-color: rgba(28, 126, 214, 0.15);
	color: #1c7ed6;
}

.section-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 1rem;
	position: relative;
}

.add-product-button {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	padding: 0.5rem 0.8rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 6px;
	font-size: 0.85rem;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
	position: relative;
	z-index: 10;
}

.add-product-button:hover {
	background-color: var(--primary-hover);
}

.product-list {
	display: flex;
	flex-direction: column;
	gap: 0.8rem;
	max-height: 300px;
	overflow-y: auto;
}

.product-item {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 0.8rem;
	background-color: white;
	border: 1px solid #eee;
	border-radius: 8px;
	transition: all 0.2s;
}

.product-item:hover {
	border-color: #ddd;
	background-color: #f9f9f9;
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

.remove-button {
	width: 30px;
	height: 30px;
	display: flex;
	align-items: center;
	justify-content: center;
	background-color: rgba(255, 71, 87, 0.1);
	color: #ff4757;
	border: none;
	border-radius: 6px;
	cursor: pointer;
	transition: all 0.2s;
	font-size: 0.8rem;
}

.remove-button:hover {
	background-color: #ff4757;
	color: white;
	transform: scale(1.05);
}

.loading-container {
	display: flex;
	flex-direction: column;
	align-items: center;
	padding: 2rem 0;
}

.spinner {
	width: 30px;
	height: 30px;
	border: 3px solid rgba(var(--primary-rgb), 0.2);
	border-radius: 50%;
	border-top-color: var(--primary-color);
	animation: spin 0.8s linear infinite;
	margin-bottom: 1rem;
}

@keyframes spin {
	to {
		transform: rotate(360deg);
	}
}

.empty-products {
	display: flex;
	flex-direction: column;
	align-items: center;
	padding: 2rem 0;
	position: relative;
}

.empty-icon {
	font-size: 2.5rem;
	color: #ddd;
	margin-bottom: 1rem;
}

.empty-products p {
	color: #888;
	margin-bottom: 1rem;
}

.empty-add-button {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	padding: 0.6rem 1rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 6px;
	font-size: 0.9rem;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
	position: relative;
	z-index: 10;
}

.empty-add-button:hover {
	background-color: var(--primary-hover);
}
</style>
