<script setup>
import { ref, onMounted, computed, watch, onUnmounted } from "vue";
import {
	getOrders,
	getOrderById,
	updateOrderStatus,
	deleteOrder,
} from "@/services/orderService";
import { formatCurrency } from "@/utils/dateTimeUtils.js";
import emitter from "@/utils/evenBus.js";

// State
const orders = ref([]);
const loading = ref(false);
const searchQuery = ref("");
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedOrder = ref(null);
const newStatus = ref("");
const statusReason = ref("");
const deliveryDate = ref("");

// Filter options
const statusFilter = ref("all");
const sortByFilter = ref("newest");

// Pagination
const currentPage = ref(1);
const itemsPerPage = ref(10);

// Fetch orders
const fetchOrders = async () => {
	loading.value = true;
	try {
		const response = await getOrders();
		console.log("Order data:", response.data);
		orders.value = response.data.data || [];
	} catch (error) {
		console.error("Error fetching orders:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể tải danh sách đơn hàng",
		});
	} finally {
		loading.value = false;
	}
};

// Apply filters to orders
const filteredOrders = computed(() => {
	let result = [...orders.value];

	// Apply search filter
	if (searchQuery.value) {
		const query = searchQuery.value.toLowerCase();
		result = result.filter(
			(order) =>
				(order.id && order.id.toString().includes(query)) ||
				(order.user &&
					order.user.name &&
					order.user.name.toLowerCase().includes(query)) ||
				(order.user &&
					order.user.email &&
					order.user.email.toLowerCase().includes(query)) ||
				(order.user &&
					order.user.phoneNumber &&
					order.user.phoneNumber.includes(query))
		);
	}

	// Apply status filter
	if (statusFilter.value !== "all") {
		result = result.filter((order) => order.status === statusFilter.value);
	}

	// Apply sorting
	switch (sortByFilter.value) {
		case "newest":
			result.sort(
				(a, b) => new Date(b.orderDate) - new Date(a.orderDate)
			);
			break;
		case "oldest":
			result.sort(
				(a, b) => new Date(a.orderDate) - new Date(b.orderDate)
			);
			break;
		case "total_desc":
			result.sort((a, b) => b.totalAmount - a.totalAmount);
			break;
		case "total_asc":
			result.sort((a, b) => a.totalAmount - b.totalAmount);
			break;
		default:
			break;
	}

	return result;
});

// Pagination
const paginatedOrders = computed(() => {
	const start = (currentPage.value - 1) * itemsPerPage.value;
	const end = start + itemsPerPage.value;
	return filteredOrders.value.slice(start, end);
});

const pageCount = computed(() => {
	return Math.ceil(filteredOrders.value.length / itemsPerPage.value);
});

// Calculate stats
const orderStats = computed(() => {
	return {
		total: orders.value.length,
		pending: orders.value.filter((o) => o.status === "Chờ xác nhận").length,
		processing: orders.value.filter(
			(o) => o.status === "Đã xác nhận" || o.status === "Đang giao hàng"
		).length,
		completed: orders.value.filter((o) => o.status === "Đã giao hàng")
			.length,
		cancelled: orders.value.filter(
			(o) =>
				o.status === "Đã huỷ" ||
				o.status === "Đã hoàn tiền" ||
				o.status === "Đã trả hàng"
		).length,
	};
});

// Format order date
const formatDate = (dateString) => {
	if (!dateString) return "N/A";
	const date = new Date(dateString);
	return date.toLocaleDateString("vi-VN", {
		year: "numeric",
		month: "2-digit",
		day: "2-digit",
		hour: "2-digit",
		minute: "2-digit",
	});
};

// View order details
const viewOrderDetails = async (order) => {
	try {
		const response = await getOrderById(order.id);
		selectedOrder.value = response.data.data;
		showModal.value = true;
	} catch (error) {
		console.error("Error fetching order details:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể tải chi tiết đơn hàng",
		});
	}
};

// Confirm delete order
const confirmDelete = (order) => {
	selectedOrder.value = order;
	showDeleteModal.value = true;
};

// Cancel delete
const cancelDelete = () => {
	selectedOrder.value = null;
	showDeleteModal.value = false;
};

// Delete order
const confirmDeleteOrder = async () => {
	if (!selectedOrder.value) return;

	loading.value = true;
	try {
		await deleteOrder(selectedOrder.value.id);
		await fetchOrders();
		showDeleteModal.value = false;
		selectedOrder.value = null;
		emitter.emit("show-notification", {
			status: "success",
			message: "Đã xóa đơn hàng thành công",
		});
	} catch (error) {
		console.error("Error deleting order:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể xóa đơn hàng",
		});
	} finally {
		loading.value = false;
	}
};

// Close modal
const closeModal = () => {
	showModal.value = false;
	selectedOrder.value = null;
	newStatus.value = "";
	statusReason.value = "";
	deliveryDate.value = "";
};

// Update order status
const updateOrder = async () => {
	if (!selectedOrder.value || !newStatus.value) {
		emitter.emit("show-notification", {
			status: "error",
			message: "Vui lòng chọn trạng thái mới",
		});
		return;
	}

	loading.value = true;
	try {
		const statusData = {
			status: newStatus.value,
		};

		// Only add delivery date if status is "Đã giao hàng"
		if (newStatus.value === "Đã giao hàng") {
			statusData.deliveryDate =
				deliveryDate.value || new Date().toISOString();
		}

		await updateOrderStatus(selectedOrder.value.id, statusData);
		await fetchOrders();

		// Get updated order details
		const response = await getOrderById(selectedOrder.value.id);
		selectedOrder.value = response.data.data;

		emitter.emit("show-notification", {
			status: "success",
			message: "Đã cập nhật trạng thái đơn hàng thành công",
		});

		// Reset fields
		newStatus.value = "";
		statusReason.value = "";
		deliveryDate.value = "";
	} catch (error) {
		console.error("Error updating order status:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể cập nhật trạng thái đơn hàng",
		});
	} finally {
		loading.value = false;
	}
};

// Get status color class
const getStatusClass = (status) => {
	switch (status) {
		case "Chờ xác nhận":
			return "pending";
		case "Đã xác nhận":
			return "confirmed";
		case "Đang giao hàng":
			return "shipping";
		case "Đã giao hàng":
			return "delivered";
		case "Đã huỷ":
			return "canceled";
		case "Đã hoàn tiền":
			return "refunded";
		case "Đã trả hàng":
			return "returned";
		default:
			return "default";
	}
};

// Get available status options based on current status
const getAvailableStatusOptions = (currentStatus) => {
	switch (currentStatus) {
		case "Chờ xác nhận":
			return ["Đã xác nhận", "Đã huỷ"];
		case "Đã xác nhận":
			return ["Đang giao hàng", "Đã huỷ"];
		case "Đang giao hàng":
			return ["Đã giao hàng", "Đã huỷ", "Đã trả hàng"];
		case "Đã giao hàng":
			return ["Đã hoàn tiền", "Đã trả hàng"];
		case "Đã huỷ":
			return ["Đã hoàn tiền"];
		default:
			return [];
	}
};

// Reset filters
const resetFilters = () => {
	searchQuery.value = "";
	statusFilter.value = "all";
	sortByFilter.value = "newest";
	currentPage.value = 1;
};

// Load data when component mounts
onMounted(async () => {
	await fetchOrders();

	// Listen for refresh events from parent component
	emitter.on("refresh-orders", async () => {
		await fetchOrders();
		emitter.emit("orders-loaded");
	});
});

// Clean up event listeners when component is unmounted
onUnmounted(() => {
	emitter.off("refresh-orders");
});
</script>

<template>
	<div class="order-management">
		<div class="section-header">
			<div class="left-section">
				<h2><i class="fas fa-shopping-cart"></i> Quản lý Đơn hàng</h2>
				<p>Quản lý và theo dõi đơn hàng khách hàng</p>
			</div>

			<div class="right-section">
				<div class="search-box">
					<i class="fas fa-search"></i>
					<input
						type="text"
						v-model="searchQuery"
						placeholder="Tìm kiếm đơn hàng..."
					/>
				</div>

				<div class="filter-section">
					<div class="filter-group">
						<i class="fas fa-filter"></i>
						<select v-model="statusFilter">
							<option value="all">Tất cả trạng thái</option>
							<option value="Chờ xác nhận">Chờ xác nhận</option>
							<option value="Đã xác nhận">Đã xác nhận</option>
							<option value="Đang giao hàng">
								Đang giao hàng
							</option>
							<option value="Đã giao hàng">Đã giao hàng</option>
							<option value="Đã huỷ">Đã huỷ</option>
							<option value="Đã hoàn tiền">Đã hoàn tiền</option>
							<option value="Đã trả hàng">Đã trả hàng</option>
						</select>
					</div>

					<div class="filter-group">
						<i class="fas fa-sort"></i>
						<select v-model="sortByFilter">
							<option value="newest">Mới nhất</option>
							<option value="oldest">Cũ nhất</option>
							<option value="total_desc">
								Giá trị cao đến thấp
							</option>
							<option value="total_asc">
								Giá trị thấp đến cao
							</option>
						</select>
					</div>

					<button @click="resetFilters" class="reset-button">
						<i class="fas fa-undo"></i> Đặt lại
					</button>
				</div>
			</div>
		</div>

		<!-- Status Cards -->
		<div class="status-cards">
			<div class="status-card">
				<div class="icon-wrapper bg-blue">
					<i class="fas fa-shopping-bag"></i>
				</div>
				<div class="status-content">
					<h3>{{ orderStats.total }}</h3>
					<p>Tổng đơn hàng</p>
				</div>
			</div>

			<div class="status-card">
				<div class="icon-wrapper bg-yellow">
					<i class="fas fa-clock"></i>
				</div>
				<div class="status-content">
					<h3>{{ orderStats.pending }}</h3>
					<p>Chờ xác nhận</p>
				</div>
			</div>

			<div class="status-card">
				<div class="icon-wrapper bg-orange">
					<i class="fas fa-truck"></i>
				</div>
				<div class="status-content">
					<h3>{{ orderStats.processing }}</h3>
					<p>Đang xử lý</p>
				</div>
			</div>

			<div class="status-card">
				<div class="icon-wrapper bg-green">
					<i class="fas fa-check-circle"></i>
				</div>
				<div class="status-content">
					<h3>{{ orderStats.completed }}</h3>
					<p>Hoàn thành</p>
				</div>
			</div>

			<div class="status-card">
				<div class="icon-wrapper bg-red">
					<i class="fas fa-times-circle"></i>
				</div>
				<div class="status-content">
					<h3>{{ orderStats.cancelled }}</h3>
					<p>Đã huỷ/Hoàn tiền/Trả hàng</p>
				</div>
			</div>
		</div>

		<!-- Orders Table -->
		<div class="data-card">
			<div v-if="loading" class="loading-state">
				<div class="spinner"></div>
				<p>Đang tải dữ liệu...</p>
			</div>

			<div v-else-if="orders.length === 0" class="empty-state">
				<i class="fas fa-shopping-cart"></i>
				<p>Chưa có đơn hàng nào.</p>
			</div>

			<div v-else>
				<table class="data-table">
					<thead>
						<tr>
							<th>Mã đơn</th>
							<th>Khách hàng</th>
							<th>Ngày đặt</th>
							<th>Tổng tiền</th>
							<th>Phương thức</th>
							<th>Trạng thái</th>
							<th>Thao tác</th>
						</tr>
					</thead>
					<tbody>
						<tr v-for="order in paginatedOrders" :key="order.id">
							<td>{{ order.id.substring(0, 8) }}...</td>
							<td>
								<div class="customer-info">
									<div class="customer-name">
										{{ order.user?.name || "Không rõ" }}
									</div>
									<div class="customer-email">
										{{ order.user?.email || "Không rõ" }}
									</div>
								</div>
							</td>
							<td>{{ formatDate(order.orderDate) }}</td>
							<td>{{ formatCurrency(order.totalAmount) }}</td>
							<td>{{ order.paymentMethod }}</td>
							<td>
								<span
									:class="[
										'status-badge',
										getStatusClass(order.status),
									]"
								>
									{{ order.status }}
								</span>
							</td>
							<td class="actions">
								<button
									@click="viewOrderDetails(order)"
									class="view-button"
									title="Xem chi tiết"
								>
									<i class="fas fa-eye"></i>
								</button>
								<button
									@click="confirmDelete(order)"
									class="delete-button"
									title="Xóa"
								>
									<i class="fas fa-trash-alt"></i>
								</button>
							</td>
						</tr>
					</tbody>
				</table>

				<!-- Pagination -->
				<div class="pagination" v-if="pageCount > 1">
					<button
						@click="currentPage--"
						:disabled="currentPage === 1"
						class="pagination-button"
					>
						<i class="fas fa-chevron-left"></i>
					</button>
					<span class="page-info"
						>Trang {{ currentPage }} / {{ pageCount }}</span
					>
					<button
						@click="currentPage++"
						:disabled="currentPage === pageCount"
						class="pagination-button"
					>
						<i class="fas fa-chevron-right"></i>
					</button>
				</div>
			</div>
		</div>

		<!-- Order Detail Modal -->
		<div v-if="showModal" class="modal-backdrop">
			<div class="modal-container order-modal">
				<div class="modal-header">
					<h3>
						Chi tiết đơn hàng #{{
							selectedOrder?.id?.substring(0, 8)
						}}
					</h3>
					<button @click="closeModal" class="close-button">
						<i class="fas fa-times"></i>
					</button>
				</div>

				<div class="modal-body">
					<div class="order-detail-sections">
						<!-- Order Info Section -->
						<div class="order-info-section">
							<h4 class="section-title">Thông tin đơn hàng</h4>
							<div class="order-info-grid">
								<div class="info-group">
									<label>Mã đơn hàng</label>
									<div class="info-value">
										{{ selectedOrder?.id }}
									</div>
								</div>
								<div class="info-group">
									<label>Ngày đặt</label>
									<div class="info-value">
										{{
											formatDate(selectedOrder?.orderDate)
										}}
									</div>
								</div>
								<div class="info-group">
									<label>Trạng thái</label>
									<div class="info-value">
										<span
											:class="[
												'status-badge',
												getStatusClass(
													selectedOrder?.status
												),
											]"
										>
											{{ selectedOrder?.status }}
										</span>
									</div>
								</div>
								<div class="info-group">
									<label>Phương thức thanh toán</label>
									<div class="info-value">
										{{ selectedOrder?.paymentMethod }}
									</div>
								</div>
								<div class="info-group">
									<label>Phí vận chuyển</label>
									<div class="info-value">
										{{
											formatCurrency(
												selectedOrder?.shippingFee
											)
										}}
									</div>
								</div>
								<div class="info-group">
									<label>Tổng tiền</label>
									<div class="info-value order-total">
										{{
											formatCurrency(
												selectedOrder?.totalAmount
											)
										}}
									</div>
								</div>
							</div>
						</div>

						<!-- Customer Info Section -->
						<div class="customer-info-section">
							<h4 class="section-title">Thông tin khách hàng</h4>
							<div class="customer-info-grid">
								<div class="info-group">
									<label>Tên khách hàng</label>
									<div class="info-value">
										{{
											selectedOrder?.user?.name ||
											"Không rõ"
										}}
									</div>
								</div>
								<div class="info-group">
									<label>Email</label>
									<div class="info-value">
										{{
											selectedOrder?.user?.email ||
											"Không rõ"
										}}
									</div>
								</div>
								<div class="info-group">
									<label>Số điện thoại</label>
									<div class="info-value">
										{{
											selectedOrder?.user?.phoneNumber ||
											"Không rõ"
										}}
									</div>
								</div>
								<div class="info-group">
									<label>Địa chỉ</label>
									<div class="info-value">
										{{
											selectedOrder?.user?.address ||
											"Không rõ"
										}}
									</div>
								</div>
							</div>
						</div>

						<!-- Order Items Section -->
						<div class="order-items-section">
							<h4 class="section-title">Sản phẩm đã đặt</h4>
							<table class="order-items-table">
								<thead>
									<tr>
										<th>Sản phẩm</th>
										<th>Đơn giá</th>
										<th>Số lượng</th>
										<th>Giảm giá</th>
										<th>Thành tiền</th>
									</tr>
								</thead>
								<tbody>
									<tr
										v-for="item in selectedOrder?.orderItems"
										:key="item.id"
									>
										<td class="product-cell">
											<div class="product-info">
												<div
													class="product-image"
													v-if="
														item.product?.colors &&
														item.product.colors
															.length > 0 &&
														item.product.colors[0]
															.images &&
														item.product.colors[0]
															.images.length > 0
													"
												>
													<img
														v-if="
															item.product
																.colors[0]
																.images[0]
																.imagePath
														"
														:src="
															item.product
																.colors[0]
																.images[0]
																.imagePath
														"
														alt="Product image"
													/>
													<div
														v-else
														class="no-image"
													>
														<i
															class="fas fa-image"
														></i>
													</div>
												</div>
												<div
													v-else
													class="product-image no-image"
												>
													<i class="fas fa-image"></i>
												</div>
												<div class="product-details">
													<div class="product-name">
														{{
															item.product
																?.name ||
															"Sản phẩm không tồn tại"
														}}
													</div>
												</div>
											</div>
										</td>
										<td>
											{{ formatCurrency(item.price) }}
										</td>
										<td>{{ item.quantity }}</td>
										<td>
											{{ formatCurrency(item.discount) }}
										</td>
										<td>
											{{
												formatCurrency(
													item.price * item.quantity -
														item.discount
												)
											}}
										</td>
									</tr>
								</tbody>
								<tfoot>
									<tr>
										<td colspan="4" class="text-right">
											Tổng giá sản phẩm
										</td>
										<td>
											{{
												formatCurrency(
													selectedOrder?.totalAmount -
														selectedOrder?.shippingFee
												)
											}}
										</td>
									</tr>
									<tr>
										<td colspan="4" class="text-right">
											Phí vận chuyển
										</td>
										<td>
											{{
												formatCurrency(
													selectedOrder?.shippingFee
												)
											}}
										</td>
									</tr>
									<tr class="total-row">
										<td colspan="4" class="text-right">
											Tổng cộng
										</td>
										<td>
											{{
												formatCurrency(
													selectedOrder?.totalAmount
												)
											}}
										</td>
									</tr>
								</tfoot>
							</table>
						</div>

						<!-- Update Status Section -->
						<div
							class="update-status-section"
							v-if="
								selectedOrder &&
								getAvailableStatusOptions(selectedOrder.status)
									.length > 0
							"
						>
							<h4 class="section-title">Cập nhật trạng thái</h4>
							<div class="status-form">
								<div class="form-group">
									<label>Trạng thái mới</label>
									<select v-model="newStatus">
										<option value="">
											Chọn trạng thái mới
										</option>
										<option
											v-for="status in getAvailableStatusOptions(
												selectedOrder.status
											)"
											:key="status"
											:value="status"
										>
											{{ status }}
										</option>
									</select>
								</div>

								<div
									class="form-group"
									v-if="newStatus === 'Đã giao hàng'"
								>
									<label>Ngày giao hàng</label>
									<input
										type="datetime-local"
										v-model="deliveryDate"
									/>
								</div>

								<div class="form-group">
									<label>Ghi chú (tùy chọn)</label>
									<textarea
										v-model="statusReason"
										placeholder="Thêm ghi chú về việc thay đổi trạng thái"
										rows="3"
									></textarea>
								</div>

								<div class="form-actions">
									<button
										@click="updateOrder"
										class="update-status-button"
										:disabled="!newStatus || loading"
									>
										<span
											v-if="loading"
											class="spinner small"
										></span>
										<template v-else>
											<i class="fas fa-check"></i> Cập
											nhật trạng thái
										</template>
									</button>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>

		<!-- Delete Confirmation Modal -->
		<div v-if="showDeleteModal" class="modal-backdrop">
			<div class="modal-container delete-modal">
				<div class="modal-header warning">
					<h3>Xác nhận xóa</h3>
					<button @click="cancelDelete" class="close-button">
						<i class="fas fa-times"></i>
					</button>
				</div>

				<div class="modal-body text-center">
					<div class="warning-icon">
						<i class="fas fa-exclamation-triangle"></i>
					</div>
					<h4 class="warning-message">
						Bạn có chắc chắn muốn xóa đơn hàng này?
					</h4>
					<p class="warning-details">
						Hành động này không thể hoàn tác. Tất cả dữ liệu liên
						quan đến đơn hàng này sẽ bị xóa vĩnh viễn khỏi hệ thống.
					</p>
				</div>

				<div class="modal-actions">
					<button @click="cancelDelete" class="cancel-button">
						<i class="fas fa-arrow-left"></i> Quay lại
					</button>
					<button
						@click="confirmDeleteOrder"
						class="delete-confirm-button"
						:disabled="loading"
					>
						<span v-if="loading" class="spinner small"></span>
						<i v-else class="fas fa-trash-alt"></i> Xóa đơn hàng
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped>
.order-management {
	width: 100%;
}

.section-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 1.5rem;
	flex-wrap: wrap;
	gap: 1rem;
}

.left-section h2 {
	font-size: 1.25rem;
	color: #333;
	margin: 0 0 0.25rem 0;
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.left-section p {
	color: #666;
	margin: 0;
	font-size: 0.9rem;
}

.right-section {
	display: flex;
	flex-wrap: wrap;
	gap: 1rem;
	align-items: center;
}

.search-box {
	position: relative;
	width: 250px;
}

.search-box i {
	position: absolute;
	left: 10px;
	top: 50%;
	transform: translateY(-50%);
	color: #666;
}

.search-box input {
	width: 100%;
	padding: 0.6rem 0.6rem 0.6rem 2rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.9rem;
	transition: all 0.3s;
}

.search-box input:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
	outline: none;
}

.filter-section {
	display: flex;
	gap: 0.5rem;
	align-items: center;
}

.filter-group {
	position: relative;
}

.filter-group i {
	position: absolute;
	left: 10px;
	top: 50%;
	transform: translateY(-50%);
	color: #666;
	pointer-events: none;
}

.filter-group select {
	padding: 0.6rem 0.6rem 0.6rem 2rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.9rem;
	appearance: none;
	background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%236e6e6e' viewBox='0 0 16 16'%3E%3Cpath d='M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z'/%3E%3C/svg%3E");
	background-repeat: no-repeat;
	background-position: right 8px center;
	padding-right: 28px;
	transition: all 0.3s;
}

.filter-group select:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
	outline: none;
}

.reset-button {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	padding: 0.6rem 1rem;
	background: none;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.9rem;
	cursor: pointer;
	transition: all 0.3s;
	color: #666;
}

.reset-button:hover {
	border-color: var(--primary-color);
	color: var(--primary-color);
}

/* Status Cards */
.status-cards {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
	gap: 1rem;
	margin-bottom: 1.5rem;
}

.status-card {
	background-color: white;
	border-radius: 12px;
	padding: 1.25rem;
	display: flex;
	align-items: center;
	gap: 1rem;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
	transition: all 0.3s ease;
}

.status-card:hover {
	transform: translateY(-5px);
	box-shadow: 0 8px 15px rgba(248, 110, 211, 0.15);
}

.icon-wrapper {
	width: 48px;
	height: 48px;
	border-radius: 12px;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 1.25rem;
}

.bg-blue {
	background-color: #e1f5fe;
	color: #0288d1;
}

.bg-green {
	background-color: #e8f5e9;
	color: #2e7d32;
}

.bg-yellow {
	background-color: #fffde7;
	color: #fbc02d;
}

.bg-orange {
	background-color: #fff3e0;
	color: #ef6c00;
}

.bg-red {
	background-color: #ffebee;
	color: #c62828;
}

.status-content h3 {
	font-size: 1.5rem;
	margin: 0 0 0.25rem 0;
	color: #333;
}

.status-content p {
	margin: 0;
	color: #666;
	font-size: 0.85rem;
}

/* Data Card */
.data-card {
	background-color: white;
	border-radius: 12px;
	padding: 1.5rem;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.loading-state,
.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 3rem 0;
	color: #666;
}

.spinner {
	width: 40px;
	height: 40px;
	border: 3px solid rgba(248, 110, 211, 0.1);
	border-top: 3px solid var(--primary-color);
	border-radius: 50%;
	animation: spin 1s linear infinite;
	margin-bottom: 1rem;
}

.spinner.small {
	width: 16px;
	height: 16px;
	border-width: 2px;
	margin: 0;
	margin-right: 0.5rem;
}

@keyframes spin {
	0% {
		transform: rotate(0deg);
	}
	100% {
		transform: rotate(360deg);
	}
}

.empty-state i {
	font-size: 3rem;
	color: #ddd;
	margin-bottom: 1rem;
}

/* Data Table */
.data-table {
	width: 100%;
	border-collapse: collapse;
}

.data-table th {
	padding: 0.75rem 1rem;
	border-bottom: 1px solid #eee;
	text-align: left;
	font-weight: 600;
	color: #333;
	font-size: 0.9rem;
	white-space: nowrap;
}

.data-table td {
	padding: 0.75rem 1rem;
	border-bottom: 1px solid #f5f5f5;
	font-size: 0.9rem;
	color: #555;
	vertical-align: middle;
}

.data-table tr:hover td {
	background-color: #f9f9f9;
}

.customer-info {
	display: flex;
	flex-direction: column;
}

.customer-name {
	font-weight: 500;
}

.customer-email {
	font-size: 0.8rem;
	color: #777;
}

.actions {
	display: flex;
	gap: 0.5rem;
	justify-content: center;
}

.view-button,
.delete-button {
	width: 32px;
	height: 32px;
	border-radius: 6px;
	display: flex;
	align-items: center;
	justify-content: center;
	border: none;
	cursor: pointer;
	transition: all 0.2s;
}

.view-button {
	background-color: #e0f2fe;
	color: var(--primary-color);
}

.view-button:hover {
	background-color: #bae6fd;
}

.delete-button {
	background-color: #ffebee;
	color: #e53935;
}

.delete-button:hover {
	background-color: #e53935;
	color: white;
}

/* Status Badge */
.status-badge {
	display: inline-block;
	padding: 0.35rem 0.65rem;
	border-radius: 9999px;
	font-size: 0.75rem;
	font-weight: 500;
	white-space: nowrap;
}

.status-badge.pending {
	background-color: #fff8e1;
	color: #ff8f00;
}

.status-badge.confirmed {
	background-color: #e8f5e9;
	color: #2e7d32;
}

.status-badge.shipping {
	background-color: #e1f5fe;
	color: #0288d1;
}

.status-badge.delivered {
	background-color: #e8f5e9;
	color: #2e7d32;
}

.status-badge.canceled {
	background-color: #ffebee;
	color: #c62828;
}

.status-badge.refunded {
	background-color: #f3e5f5;
	color: #7b1fa2;
}

.status-badge.returned {
	background-color: #ede7f6;
	color: #512da8;
}

/* Pagination */
.pagination {
	display: flex;
	align-items: center;
	justify-content: center;
	margin-top: 1.5rem;
	gap: 1rem;
}

.pagination-button {
	width: 36px;
	height: 36px;
	border-radius: 8px;
	display: flex;
	align-items: center;
	justify-content: center;
	border: 1px solid #ddd;
	background-color: white;
	color: #666;
	cursor: pointer;
	transition: all 0.2s;
}

.pagination-button:hover:not(:disabled) {
	border-color: var(--primary-color);
	color: var(--primary-color);
}

.pagination-button:disabled {
	opacity: 0.5;
	cursor: not-allowed;
}

.page-info {
	color: #666;
	font-size: 0.9rem;
}

/* Modal Styling */
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
	backdrop-filter: blur(3px);
}

.modal-container {
	background-color: white;
	border-radius: 12px;
	width: 95%;
	max-width: 800px;
	max-height: 90vh;
	display: flex;
	flex-direction: column;
	box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
	overflow: hidden;
}

.delete-modal {
	max-width: 500px;
}

.order-modal {
	max-width: 1000px;
}

.modal-header {
	padding: 1.25rem 1.5rem;
	border-bottom: 1px solid #eee;
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.modal-header.warning {
	border-bottom-color: #fee2e2;
	color: #ef4444;
}

.modal-header h3 {
	margin: 0;
	font-size: 1.25rem;
	color: #333;
}

.close-button {
	background: none;
	border: none;
	color: #666;
	font-size: 1rem;
	cursor: pointer;
	width: 32px;
	height: 32px;
	border-radius: 8px;
	display: flex;
	align-items: center;
	justify-content: center;
	transition: all 0.2s;
}

.close-button:hover {
	background-color: #f9f9f9;
	color: #333;
}

.modal-body {
	padding: 1.5rem;
	overflow-y: auto;
	max-height: calc(90vh - 70px);
}

.order-detail-sections {
	display: flex;
	flex-direction: column;
	gap: 2rem;
}

.section-title {
	font-size: 1.1rem;
	color: #333;
	margin: 0 0 1rem;
	border-bottom: 1px solid #f0f0f0;
	padding-bottom: 0.5rem;
}

.order-info-grid,
.customer-info-grid {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
	gap: 1rem;
}

.info-group {
	margin-bottom: 0.5rem;
}

.info-group label {
	display: block;
	font-size: 0.85rem;
	color: #666;
	margin-bottom: 0.25rem;
}

.info-value {
	font-size: 0.95rem;
	color: #333;
}

.order-total {
	font-weight: 600;
	color: var(--primary-color);
}

.order-items-table {
	width: 100%;
	border-collapse: collapse;
	margin-top: 0.5rem;
}

.order-items-table th {
	padding: 0.75rem;
	text-align: left;
	border-bottom: 1px solid #eee;
	font-size: 0.85rem;
	font-weight: 600;
	color: #333;
}

.order-items-table td {
	padding: 0.75rem;
	border-bottom: 1px solid #f5f5f5;
	font-size: 0.9rem;
	color: #555;
}

.product-cell {
	width: 40%;
}

.product-info {
	display: flex;
	align-items: center;
	gap: 1rem;
}

.product-image {
	width: 50px;
	height: 50px;
	border-radius: 8px;
	overflow: hidden;
	border: 1px solid #eee;
}

.product-image img {
	width: 100%;
	height: 100%;
	object-fit: cover;
}

.no-image {
	display: flex;
	align-items: center;
	justify-content: center;
	background-color: #f9f9f9;
	color: #ccc;
}

.product-name {
	font-weight: 500;
	margin-bottom: 0.25rem;
}

.product-variant {
	font-size: 0.85rem;
	color: #666;
}

.text-right {
	text-align: right;
}

tfoot td {
	padding-top: 0.5rem;
	color: #555;
	font-weight: 500;
}

.total-row td {
	font-weight: 600;
	color: var(--primary-color);
	border-top: 2px solid #f0f0f0;
}

.update-status-section {
	background-color: #f9fafb;
	border-radius: 8px;
	padding: 1.25rem;
}

.status-form .form-group {
	margin-bottom: 1rem;
}

.status-form label {
	display: block;
	font-size: 0.9rem;
	font-weight: 500;
	margin-bottom: 0.5rem;
}

.status-form select,
.status-form input[type="datetime-local"],
.status-form textarea {
	width: 100%;
	padding: 0.75rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.95rem;
}

.status-form select:focus,
.status-form input[type="datetime-local"]:focus,
.status-form textarea:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
	outline: none;
}

.form-actions {
	display: flex;
	justify-content: flex-end;
	margin-top: 1rem;
}

.update-status-button {
	padding: 0.75rem 1.5rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 8px;
	font-weight: 500;
	cursor: pointer;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	transition: all 0.2s;
}

.update-status-button:hover {
	background-color: #e94e9c;
}

.update-status-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

.text-center {
	text-align: center;
}

.warning-icon {
	width: 64px;
	height: 64px;
	border-radius: 50%;
	background-color: #fee2e2;
	color: #ef4444;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 2rem;
	margin: 0 auto 1.5rem auto;
}

.warning-message {
	font-size: 1.1rem;
	color: #333;
	margin-bottom: 1rem;
}

.warning-details {
	color: #666;
	font-size: 0.9rem;
	margin-bottom: 1rem;
}

.modal-actions {
	padding: 1.25rem 1.5rem;
	border-top: 1px solid #eee;
	display: flex;
	justify-content: flex-end;
	gap: 1rem;
}

.cancel-button {
	padding: 0.75rem 1.25rem;
	background-color: #f9f9f9;
	border: 1px solid #ddd;
	color: #666;
	border-radius: 8px;
	font-weight: 500;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	cursor: pointer;
	transition: all 0.2s;
}

.cancel-button:hover {
	background-color: #f0f0f0;
}

.delete-confirm-button {
	padding: 0.75rem 1.25rem;
	background-color: #ef4444;
	border: none;
	color: white;
	border-radius: 8px;
	font-weight: 500;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	cursor: pointer;
	transition: all 0.2s;
}

.delete-confirm-button:hover {
	background-color: #dc2626;
}

.delete-confirm-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

/* Responsive */
@media (max-width: 768px) {
	.section-header {
		flex-direction: column;
		align-items: flex-start;
	}

	.right-section {
		width: 100%;
		flex-direction: column;
	}

	.search-box {
		width: 100%;
	}

	.filter-section {
		width: 100%;
		flex-direction: column;
		align-items: flex-start;
	}

	.filter-group {
		width: 100%;
	}

	.status-cards {
		grid-template-columns: 1fr;
	}

	.data-table {
		display: block;
		overflow-x: auto;
	}

	.order-info-grid,
	.customer-info-grid {
		grid-template-columns: 1fr;
	}
}
</style>
