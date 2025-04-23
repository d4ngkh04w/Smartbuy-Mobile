<script setup>
import { ref, computed, watch } from "vue";

const props = defineProps({
	customers: {
		type: Array,
		required: true,
	},
	loading: {
		type: Boolean,
		default: false,
	},
	timeFilter: {
		type: String,
		default: "month",
	},
});

const sortBy = ref("totalSpent");

const sortedCustomers = computed(() => {
	if (!props.customers || props.customers.length === 0) return [];

	return [...props.customers].sort((a, b) => {
		if (sortBy.value === "orderCount") {
			return b.orderCount - a.orderCount;
		} else if (sortBy.value === "totalSpent") {
			return b.totalSpent - a.totalSpent;
		}
		return 0;
	});
});

function sortCustomers() {
	// Sorting is handled by the computed property
}

function formatCurrency(value) {
	return new Intl.NumberFormat("vi-VN", {
		style: "currency",
		currency: "VND",
		maximumFractionDigits: 0,
	}).format(value);
}

function formatDate(dateString) {
	if (!dateString) return "";

	const date = new Date(dateString);
	return new Intl.DateTimeFormat("vi-VN", {
		day: "2-digit",
		month: "2-digit",
		year: "numeric",
	}).format(date);
}

function exportCustomerData() {
	// Logic to export customer data (CSV, Excel, etc.)
	alert("Tính năng xuất dữ liệu đang được phát triển");
}

function createPromotion() {
	// Logic to create promotion for top customers
	alert("Tính năng tạo khuyến mãi đang được phát triển");
}

// Watch for changes in props to update if needed
watch(
	() => props.timeFilter,
	() => {
		// In a real implementation, you would fetch new data here
		// based on the time filter changes
	}
);
</script>

<template>
	<div class="stats-card frequent-customers">
		<div class="card-header">
			<h2>Khách hàng thường xuyên</h2>
			<div class="card-actions">
				<div class="filter-group">
					<label>Sắp xếp theo:</label>
					<select v-model="sortBy" @change="sortCustomers">
						<option value="orderCount">Số đơn đã đặt</option>
						<option value="totalSpent">Tổng chi tiêu</option>
					</select>
				</div>
			</div>
		</div>

		<div class="card-content">
			<div v-if="loading" class="loading-indicator">
				<div class="spinner"></div>
				<p>Đang tải dữ liệu...</p>
			</div>

			<div v-else>
				<table class="data-table">
					<thead>
						<tr>
							<th>STT</th>
							<th>Tên khách hàng</th>
							<th>Email</th>
							<th>Số đơn đã đặt</th>
							<th>Tổng chi tiêu</th>
							<th>Thành viên từ</th>
						</tr>
					</thead>
					<tbody>
						<tr
							v-for="(customer, index) in sortedCustomers"
							:key="customer.id"
						>
							<td>{{ index + 1 }}</td>
							<td>{{ customer.name }}</td>
							<td>{{ customer.email }}</td>
							<td>{{ customer.orderCount }}</td>
							<td>{{ formatCurrency(customer.totalSpent) }}</td>
							<td>{{ formatDate(customer.memberSince) }}</td>
						</tr>
					</tbody>
				</table>

				<div class="empty-state" v-if="sortedCustomers.length === 0">
					<p>
						Không có dữ liệu khách hàng trong khoảng thời gian này
					</p>
				</div>

				<div class="customer-actions">
					<button class="export-btn" @click="exportCustomerData">
						<i class="fas fa-download"></i> Xuất dữ liệu
					</button>
					<button class="promotion-btn" @click="createPromotion">
						<i class="fas fa-gift"></i> Tạo khuyến mãi
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped>
.stats-card {
	background-color: white;
	border-radius: 12px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
	overflow: hidden;
	margin-bottom: 2rem;
}

.card-header {
	padding: 1.5rem;
	border-bottom: 1px solid #f0f0f0;
	display: flex;
	justify-content: space-between;
	align-items: center;
	flex-wrap: wrap;
	gap: 1rem;
}

.card-header h2 {
	margin: 0;
	font-size: 1.2rem;
	color: #333;
	font-weight: 600;
}

.card-actions {
	display: flex;
	gap: 1rem;
}

.filter-group {
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.filter-group label {
	font-size: 0.9rem;
	color: #666;
}

.filter-group select {
	padding: 0.4rem 0.8rem;
	border-radius: 4px;
	border: 1px solid #ddd;
	font-size: 0.9rem;
	background-color: white;
}

.card-content {
	padding: 1.5rem;
}

.data-table {
	width: 100%;
	border-collapse: collapse;
	text-align: left;
	margin-bottom: 1.5rem;
}

.data-table th {
	background-color: #f9f9f9;
	padding: 0.8rem 1rem;
	font-weight: 600;
	color: #555;
	font-size: 0.9rem;
	border-bottom: 2px solid #eee;
}

.data-table td {
	padding: 0.8rem 1rem;
	border-bottom: 1px solid #eee;
	color: #333;
	font-size: 0.9rem;
}

.data-table tr:last-child td {
	border-bottom: none;
}

.data-table tr:hover td {
	background-color: #f5f5f5;
}

.loading-indicator {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 2rem 0;
}

.spinner {
	width: 40px;
	height: 40px;
	border: 4px solid #f3f3f3;
	border-top: 4px solid var(--primary-color, #f86ed3);
	border-radius: 50%;
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
	text-align: center;
	padding: 1rem 0;
	color: #666;
	margin-bottom: 1.5rem;
}

.customer-actions {
	display: flex;
	gap: 1rem;
	justify-content: flex-end;
	margin-top: 1rem;
}

.export-btn,
.promotion-btn {
	padding: 0.6rem 1.2rem;
	border-radius: 6px;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	cursor: pointer;
	font-weight: 500;
	transition: all 0.3s;
}

.export-btn {
	background-color: #f5f5f5;
	color: #555;
	border: 1px solid #ddd;
}

.export-btn:hover {
	background-color: #eaeaea;
	color: #333;
}

.promotion-btn {
	background-color: #fff0f8;
	color: var(--primary-color, #f86ed3);
	border: 1px solid var(--primary-color, #f86ed3);
}

.promotion-btn:hover {
	background-color: var(--primary-color, #f86ed3);
	color: white;
}
</style>
