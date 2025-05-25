<template>
	<div class="purchase-history">
		<h2 class="page-title">Lịch sử mua hàng</h2>

		<div v-if="loading" class="loading-state">
			<i class="fas fa-spinner fa-spin"></i> Đang tải lịch sử mua hàng...
		</div>

		<div v-else-if="error" class="error-state">
			<i class="fas fa-exclamation-circle"></i> Đã xảy ra lỗi khi tải lịch sử mua hàng. Vui lòng thử lại sau.
		</div>

		<div v-else-if="historyOrders.length === 0" class="empty-state">
			<i class="fas fa-box-open"></i> Bạn chưa có đơn hàng nào trong lịch sử.
		</div>

		<div v-else class="history-table-container">
			<table class="purchase-table">
				<thead>
					<tr>
						<th>Mã ĐH</th>
						<th>Ngày mua</th>
						<th>Sản phẩm</th>
						<th>Tổng tiền</th>
						<th>Trạng thái</th>
						<th>Hành động</th>
					</tr>
				</thead>
				<tbody>
					<tr v-for="order in historyOrders" :key="order.id">
						<td data-label="Mã ĐH" class="order-id-cell">#{{ order.id }}</td>
						<td data-label="Ngày mua">{{ formatDate(order.orderDate) }}</td>
						<td data-label="Sản phẩm">
							<div class="product-list-cell">
								<div v-for="item in order.items" :key="item.productId" class="product-item">
									<img :src="item.image" :alt="item.name" class="product-thumb-small" />
									<span class="product-name">{{ item.name }} (x{{ item.quantity }})</span>
								</div>
								<p v-if="order.items.length > 2" class="more-items-in-table">
									và {{ order.items.length - 2 }} sản phẩm khác...
								</p>
							</div>
						</td>
						<td data-label="Tổng tiền" class="total-amount-cell">{{ formatCurrency(order.totalAmount) }}</td>
						<td data-label="Trạng thái">
							<span :class="['status-badge', getStatusClass(order.status)]">{{ getStatusText(order.status) }}</span>
						</td>
						<td data-label="Hành động">
							<div class="action-buttons">
								<button @click="viewOrderDetails(order.id)" class="btn btn-view-detail">Chi tiết</button>
								<button @click="reviewOrder(order.id)" class="btn btn-review">Đánh giá</button>
								<button @click="reorder(order.id)" class="btn btn-reorder">Mua lại</button>
							</div>
						</td>
					</tr>
				</tbody>
			</table>
		</div>
	</div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const historyOrders = ref([]);
const loading = ref(true);
const error = ref(false);

// Giả lập dữ liệu lịch sử mua hàng
const mockHistoryOrders = [
	{
		id: 'DH0005',
		orderDate: '2024-12-10T10:00:00Z',
		totalAmount: 22000000,
		status: 'delivered',
		items: [
			{ productId: 'SP009', name: 'MacBook Air M2 256GB', quantity: 1, price: 22000000, image: 'https://via.placeholder.com/60?text=MacBook' },
		],
	},
	{
		id: 'DH0006',
		orderDate: '2024-11-25T15:30:00Z',
		totalAmount: 1200000,
		status: 'delivered',
		items: [
			{ productId: 'SP010', name: 'Loa Bluetooth JBL Flip 6', quantity: 1, price: 1200000, image: 'https://via.placeholder.com/60?text=Speaker' },
		],
	},
    {
		id: 'DH0007',
		orderDate: '2024-10-01T08:00:00Z',
		totalAmount: 800000,
		status: 'delivered',
		items: [
			{ productId: 'SP011', name: 'Bàn phím cơ DareU', quantity: 1, price: 800000, image: 'https://via.placeholder.com/60?text=Keyboard' },
		],
	},
    {
		id: 'DH0008',
		orderDate: '2024-09-15T18:00:00Z',
		totalAmount: 50000000,
		status: 'delivered',
		items: [
			{ productId: 'SP012', name: 'Gaming PC High-End', quantity: 1, price: 45000000, image: 'https://via.placeholder.com/60?text=PC' },
            { productId: 'SP013', name: 'Màn hình Gaming 27 inch', quantity: 1, price: 5000000, image: 'https://via.placeholder.com/60?text=Monitor' },
		],
	},
     {
		id: 'DH0009',
		orderDate: '2024-08-01T12:00:00Z',
		totalAmount: 600000,
		status: 'cancelled', // Đơn hàng đã hủy cũng có thể xuất hiện trong lịch sử
		items: [
			{ productId: 'SP014', name: 'Cáp sạc USB-C', quantity: 3, price: 200000, image: 'https://via.placeholder.com/60?text=Cable' },
		],
	},
];

// Hàm lấy dữ liệu (giả lập API call)
const fetchHistoryOrders = async () => {
	loading.value = true;
	error.value = false;
	try {
		// Giả lập độ trễ mạng
		await new Promise(resolve => setTimeout(resolve, 1000));
		historyOrders.value = mockHistoryOrders;
	} catch (err) {
		console.error("Failed to fetch purchase history:", err);
		error.value = true;
	} finally {
		loading.value = false;
	}
};

// Hàm định dạng ngày
const formatDate = (dateString) => {
    const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('vi-VN', options);
};

// Hàm định dạng tiền tệ
const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

// Hàm trả về text trạng thái
const getStatusText = (status) => {
	switch (status) {
		case 'delivered': return 'Đã giao hàng';
		case 'cancelled': return 'Đã hủy';
		case 'refunded': return 'Đã hoàn tiền';
		default: return 'Hoàn thành';
	}
};

// Hàm trả về class cho trạng thái để custom CSS
const getStatusClass = (status) => {
	switch (status) {
		case 'delivered': return 'status-delivered';
		case 'cancelled': return 'status-cancelled';
		case 'refunded': return 'status-refunded';
		default: return '';
	}
};

// Xử lý xem chi tiết đơn hàng
const viewOrderDetails = (orderId) => {
	router.push({ name: 'OrderDetail', params: { id: orderId } });
	console.log("Xem chi tiết đơn hàng lịch sử:", orderId);
};

// Xử lý đánh giá đơn hàng/sản phẩm
const reviewOrder = (orderId) => {
	alert(`Chức năng đánh giá cho đơn hàng ${orderId} sẽ sớm ra mắt!`);
	console.log("Đánh giá đơn hàng:", orderId);
	// Có thể điều hướng đến trang đánh giá
	// router.push({ name: 'ReviewOrder', params: { id: orderId } });
};

// Xử lý mua lại đơn hàng
const reorder = (orderId) => {
	if (confirm(`Bạn có muốn mua lại các sản phẩm trong đơn hàng ${orderId}?`)) {
		console.log("Mua lại đơn hàng:", orderId);
		// Logic thêm các sản phẩm từ đơn hàng cũ vào giỏ hàng
		alert(`Các sản phẩm của đơn hàng ${orderId} đã được thêm vào giỏ hàng.`);
	}
};

// Khi component được mount, fetch dữ liệu
onMounted(() => {
	fetchHistoryOrders();
});
</script>

<style scoped>
.purchase-history {
	padding: 2rem;
	background-color: #f8f9fa;
	min-height: calc(100vh - 100px); /* Adjust as needed */
}

.page-title {
	font-size: 2rem;
	color: #333;
	margin-bottom: 2rem;
	text-align: center;
}

.loading-state,
.error-state,
.empty-state {
	text-align: center;
	padding: 3rem;
	font-size: 1.2rem;
	color: #6c757d;
	background-color: #e9ecef;
	border-radius: 8px;
	margin: 2rem auto;
	max-width: 800px;
}

.loading-state i,
.error-state i,
.empty-state i {
	margin-right: 0.8rem;
	color: #007bff; /* Primary color */
}
.error-state i {
	color: #dc3545; /* Danger color */
}

.history-table-container {
	overflow-x: auto; /* Allow horizontal scroll on small screens */
	background-color: #ffffff;
	border-radius: 8px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
	padding: 1.5rem;
}

.purchase-table {
	width: 100%;
	border-collapse: collapse;
	min-width: 700px; /* Minimum width for the table to prevent squishing */
}

.purchase-table th,
.purchase-table td {
	padding: 1rem;
	text-align: left;
	border-bottom: 1px solid #eee;
}

.purchase-table th {
	background-color: #e9ecef;
	font-weight: 600;
	color: #555;
	text-transform: uppercase;
	font-size: 0.9rem;
}

.purchase-table tbody tr:hover {
	background-color: #f1f3f5;
}

.order-id-cell {
	font-weight: 600;
	color: #333;
}

.product-list-cell {
	display: flex;
	flex-direction: column;
	gap: 0.5rem;
}

.product-item {
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.product-thumb-small {
	width: 40px;
	height: 40px;
	object-fit: cover;
	border-radius: 4px;
	border: 1px solid #eee;
}

.product-name {
	font-size: 0.9rem;
	color: #555;
}

.more-items-in-table {
	font-size: 0.85rem;
	color: #6c757d;
	font-style: italic;
	margin-top: 0.3rem;
}

.total-amount-cell {
	font-weight: 600;
	color: #007bff; /* Primary color */
}

.status-badge {
	padding: 0.4rem 0.8rem;
	border-radius: 20px;
	font-size: 0.85rem;
	font-weight: 500;
	color: white;
	display: inline-block; /* For proper padding and border-radius */
}

.status-delivered { background-color: #28a745; } /* Success */
.status-cancelled { background-color: #dc3545; } /* Danger */
.status-refunded { background-color: #ffc107; color: #333; } /* Warning */


.action-buttons {
	display: flex;
	flex-wrap: wrap; /* Allow buttons to wrap */
	gap: 0.5rem;
}

.btn {
	padding: 0.5rem 1rem;
	border: none;
	border-radius: 5px;
	cursor: pointer;
	font-size: 0.85rem;
	transition: background-color 0.3s ease, color 0.3s ease;
	white-space: nowrap; /* Prevent button text from breaking */
}

.btn-view-detail {
	background-color: #007bff;
	color: white;
}
.btn-view-detail:hover {
	background-color: #0056b3;
}

.btn-review {
	background-color: #6c757d;
	color: white;
}
.btn-review:hover {
	background-color: #5a6268;
}

.btn-reorder {
	background-color: #17a2b8;
	color: white;
}
.btn-reorder:hover {
	background-color: #138496;
}

/* Responsive Table */
@media (max-width: 768px) {
	.purchase-table,
	.purchase-table tbody,
	.purchase-table tr,
	.purchase-table td {
		display: block;
		width: 100%;
	}

	.purchase-table thead {
		display: none; /* Hide table headers on small screens */
	}

	.purchase-table tr {
		margin-bottom: 1rem;
		border: 1px solid #ddd;
		border-radius: 8px;
		overflow: hidden; /* For rounded corners */
		background-color: #fff;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
	}

	.purchase-table td {
		text-align: right;
		padding-left: 50%; /* Space for pseudo-element label */
		position: relative;
		border-bottom: 1px dashed #eee;
	}

	.purchase-table td:last-child {
		border-bottom: none;
	}

	.purchase-table td::before {
		content: attr(data-label);
		position: absolute;
		left: 0;
		width: 50%;
		padding-left: 1rem;
		font-weight: 600;
		text-align: left;
		color: #333;
	}

	.product-list-cell {
		align-items: flex-start;
	}
	.action-buttons {
		justify-content: flex-end; /* Align buttons to the right */
	}
}

@media (max-width: 480px) {
	.purchase-history {
		padding: 1rem;
	}
	.page-title {
		font-size: 1.8rem;
	}
	.history-table-container {
		padding: 1rem;
	}
	.btn {
		width: 100%;
		text-align: center;
	}
	.action-buttons {
		flex-direction: column;
		width: 100%;
	}
	.purchase-table td::before {
		font-size: 0.9rem;
	}
	.product-name {
		font-size: 0.85rem;
	}
}
</style>