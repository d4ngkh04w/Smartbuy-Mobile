<template>
	<div class="discount-table-container">
		<table class="discount-table" v-if="!loading && discounts.length > 0">
			<thead>
				<tr>
					<th>ID</th>
					<th>% Giảm giá</th>
					<th>Số tiền giảm</th>
					<th>Ngày bắt đầu</th>
					<th>Ngày kết thúc</th>
					<th>Trạng thái</th>
					<th>Thao tác</th>
				</tr>
			</thead>
			<tbody>
				<tr v-for="discount in discounts" :key="discount.id">
					<td>{{ discount.id }}</td>
					<td>{{ formatPercentage(discount.discountPercentage) }}</td>
					<td>{{ formatCurrency(discount.discountAmount) }}</td>
					<td>{{ formatDate(discount.startDate) }}</td>
					<td>{{ formatDate(discount.endDate) }}</td>
					<td>
						<span
							class="status-badge"
							:class="{
								active: discount.status === 'Active',
								expired: discount.status === 'Expired',
								upcoming: discount.status === 'Upcoming',
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
					</td>
					<td>
						<div class="actions-cell">
							<button
								class="action-button view"
								@click="$emit('view', discount)"
								title="Xem chi tiết"
							>
								<i class="fas fa-eye"></i>
							</button>
							<button
								class="action-button edit"
								@click="$emit('edit', discount)"
								title="Chỉnh sửa"
							>
								<i class="fas fa-edit"></i>
							</button>
							<button
								class="action-button delete"
								@click="$emit('delete', discount)"
								title="Xóa"
							>
								<i class="fas fa-trash"></i>
							</button>
						</div>
					</td>
				</tr>
			</tbody>
		</table>

		<div v-else-if="loading" class="loading-container">
			<div class="spinner"></div>
			<p>Đang tải dữ liệu...</p>
		</div>

		<div v-else class="empty-state">
			<i class="fas fa-tag empty-icon"></i>
			<h3>Không có mã giảm giá nào</h3>
			<p>Hãy thêm mã giảm giá mới để hiển thị ở đây</p>
		</div>
	</div>
</template>

<script setup>
import {  defineEmits } from "vue";

const props = defineProps({
	discounts: {
		type: Array,
		required: true,
	},
	loading: {
		type: Boolean,
		default: false,
	},
});

defineEmits(["view", "edit", "delete"]);

const formatPercentage = (value) => {
	if (value === 0 || value === null || value === undefined) return "-";
	return `${value}%`;
};

const formatCurrency = (amount) => {
	if (amount === 0 || amount === null || amount === undefined) return "-";
	return new Intl.NumberFormat("vi-VN", {
		style: "currency",
		currency: "VND",
	}).format(amount);
};

const formatDate = (dateString) => {
	if (!dateString) return "-";
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
</script>

<style scoped>
.discount-table-container {
	width: 100%;
	overflow-x: auto;
	border-radius: 12px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.discount-table {
	width: 100%;
	border-collapse: collapse;
	background-color: white;
}

.discount-table th,
.discount-table td {
	padding: 12px 16px;
	text-align: left;
	border-bottom: 1px solid #f0f0f0;
}

.discount-table th {
	background-color: #f9f9f9;
	color: #666;
	font-weight: 600;
	font-size: 0.9rem;
}

.discount-table tr:hover {
	background-color: #f8f9fd;
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

.actions-cell {
	display: flex;
	gap: 6px;
}

.action-button {
	width: 30px;
	height: 30px;
	border-radius: 6px;
	display: flex;
	align-items: center;
	justify-content: center;
	border: none;
	cursor: pointer;
	transition: all 0.2s;
	font-size: 0.8rem;
}

.action-button.view {
	background-color: #e0f2fe;
	color: var(--primary-color);
}

.action-button.edit {
	background-color: rgba(255, 190, 11, 0.1);
	color: #ffbe0b;
}

.action-button.delete {
	background-color: rgba(255, 71, 87, 0.1);
	color: #ff4757;
}

.action-button:hover {
	transform: scale(1.05);
	color: white;
}

.action-button.view:hover {
	background-color: #bae6fd;
}

.action-button.edit:hover {
	background-color: #ffbe0b;
}

.action-button.delete:hover {
	background-color: #ff4757;
}

.loading-container {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 3rem 0;
	background-color: white;
	border-radius: 12px;
	width: 100%;
}

.spinner {
	border: 3px solid rgba(0, 0, 0, 0.1);
	border-radius: 50%;
	border-top: 3px solid var(--primary-color);
	width: 30px;
	height: 30px;
	animation: spin 1s linear infinite;
	margin-bottom: 1rem;
}

@keyframes spin {
	0% {
		transform: rotate(0deg);
	}
	100% {
		transform: rotate(360deg);
	}
}

.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 3rem 0;
	background-color: white;
	border-radius: 12px;
	width: 100%;
}

.empty-icon {
	font-size: 3rem;
	color: #ddd;
	margin-bottom: 1rem;
}

.empty-state h3 {
	color: #666;
	margin: 0;
	margin-bottom: 0.5rem;
}

.empty-state p {
	color: #999;
	margin: 0;
}
</style>
