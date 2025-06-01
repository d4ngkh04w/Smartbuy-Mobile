<script setup>
import { ref, onMounted, computed, onUnmounted } from "vue";
import orderService from "@/services/orderService";
import { formatCurrency } from "@/utils/dateTimeUtils.js";
import emitter from "@/utils/evenBus.js";

// State
const orders = ref([]);
const loading = ref(false);
const searchQuery = ref("");
const showModal = ref(false);
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
        const response = await orderService.getOrders();
        console.log("Order data:", response.data);
        orders.value = response.data || [];
    } catch (error) {
        console.error("Error fetching orders:", error);
        if (error.response && error.response.status === 404) {
            orders.value = [];
        } else {
            emitter.emit("show-notification", {
                status: "error",
                message: "Không thể tải danh sách đơn hàng",
            });
        }
    } finally {
        loading.value = false;
    }
};

// Apply filters to orders
const filteredOrders = computed(() => {
    return orderService.applyFilters(orders.value, {
        searchQuery: searchQuery.value,
        statusFilter: statusFilter.value,
        sortBy: sortByFilter.value,
    });
});

// Pagination
const paginatedOrders = computed(() => {
    return orderService.getPaginatedOrders(
        filteredOrders.value,
        currentPage.value,
        itemsPerPage.value
    );
});

const pageCount = computed(() => {
    return orderService.getPageCount(
        filteredOrders.value.length,
        itemsPerPage.value
    );
});

// Calculate which page numbers to display in pagination
const displayedPageNumbers = computed(() => {
    return orderService.getDisplayedPageNumbers(
        currentPage.value,
        pageCount.value
    );
});

// Calculate stats
const orderStats = computed(() => {
    return orderService.calculateOrderStats(orders.value);
});

// Format order date
const formatDate = (dateString) => {
    return orderService.formatOrderDate(dateString);
};

// View order details
const viewOrderDetails = async (order) => {
    try {
        const response = await orderService.getOrderById(order.id);
        selectedOrder.value = response.data;
        showModal.value = true;
    } catch (error) {
        console.error("Error fetching order details:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải chi tiết đơn hàng",
        });
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
        const statusData = orderService.prepareStatusUpdateData(
            newStatus.value,
            deliveryDate.value
        );

        await orderService.updateOrderStatus(
            selectedOrder.value.id,
            statusData
        );
        await fetchOrders();

        // Get updated order details
        const response = await orderService.getOrderById(
            selectedOrder.value.id
        );
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
    return orderService.getStatusClass(status);
};

// Get available status options based on current status
const getAvailableStatusOptions = (currentStatus) => {
    return orderService.getAvailableStatusOptions(currentStatus);
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
                            <option value="Hoàn thành">Hoàn thành</option>
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
                <div class="icon-wrapper bg-blue">
                    <i class="fas fa-box"></i>
                </div>
                <div class="status-content">
                    <h3>{{ orderStats.delivered }}</h3>
                    <p>Đã giao hàng</p>
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
                            </td>
                        </tr>
                    </tbody>
                </table>
                <!-- Pagination -->
                <div class="pagination" v-if="filteredOrders.length > 0">
                    <button
                        @click="currentPage = 1"
                        :disabled="currentPage === 1"
                        class="pagination-button"
                        title="Trang đầu tiên"
                    >
                        <i class="fas fa-angle-double-left"></i>
                    </button>
                    <button
                        @click="currentPage--"
                        :disabled="currentPage === 1"
                        class="pagination-button"
                        title="Trang trước"
                    >
                        <i class="fas fa-chevron-left"></i>
                    </button>

                    <!-- Page Numbers -->
                    <div class="page-numbers">
                        <button
                            v-for="page in displayedPageNumbers"
                            :key="page"
                            @click="currentPage = page"
                            :class="[
                                'page-number',
                                { active: currentPage === page },
                            ]"
                        >
                            {{ page }}
                        </button>
                    </div>

                    <button
                        @click="currentPage++"
                        :disabled="currentPage === pageCount"
                        class="pagination-button"
                        title="Trang sau"
                    >
                        <i class="fas fa-chevron-right"></i>
                    </button>
                    <button
                        @click="currentPage = pageCount"
                        :disabled="currentPage === pageCount"
                        class="pagination-button"
                        title="Trang cuối"
                    >
                        <i class="fas fa-angle-double-right"></i>
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
                    <div class="order-detail-content">
                        <!-- Order Header -->
                        <div class="order-detail-header">
                            <div class="order-detail-badge">
                                <div class="badge-icon">
                                    <i class="fas fa-shopping-bag"></i>
                                </div>
                                <div class="badge-content">
                                    <h4 class="order-detail-id">
                                        #{{
                                            selectedOrder?.id?.substring(0, 8)
                                        }}
                                    </h4>
                                    <p class="order-date">
                                        {{
                                            formatDate(selectedOrder?.orderDate)
                                        }}
                                    </p>
                                </div>
                            </div>
                            <div class="order-detail-status">
                                <span
                                    :class="[
                                        'status-badge-large',
                                        getStatusClass(selectedOrder?.status),
                                    ]"
                                >
                                    {{ selectedOrder?.status }}
                                </span>
                            </div>
                        </div>

                        <!-- Order Summary Cards -->
                        <div class="order-summary-cards">
                            <div class="summary-card payment-card">
                                <div class="card-icon">
                                    <i class="fas fa-credit-card"></i>
                                </div>
                                <div class="card-content">
                                    <h5 class="card-label">
                                        Phương thức thanh toán
                                    </h5>
                                    <p class="card-value">
                                        {{ selectedOrder?.paymentMethod }}
                                    </p>
                                </div>
                            </div>
                            <div class="summary-card shipping-card">
                                <div class="card-icon">
                                    <i class="fas fa-truck"></i>
                                </div>
                                <div class="card-content">
                                    <h5 class="card-label">Phí vận chuyển</h5>
                                    <p class="card-value">
                                        {{
                                            formatCurrency(
                                                selectedOrder?.shippingFee
                                            )
                                        }}
                                    </p>
                                </div>
                            </div>
                            <div class="summary-card total-card">
                                <div class="card-icon">
                                    <i class="fas fa-money-bill-wave"></i>
                                </div>
                                <div class="card-content">
                                    <h5 class="card-label">Tổng tiền</h5>
                                    <p class="card-value total-amount">
                                        {{
                                            formatCurrency(
                                                selectedOrder?.totalAmount
                                            )
                                        }}
                                    </p>
                                </div>
                            </div>
                        </div>
                        <!-- Customer Information Section -->
                        <div class="order-detail-section">
                            <h5 class="detail-section-title">
                                <i class="fas fa-user"></i>
                                Thông tin khách hàng
                            </h5>
                            <div class="customer-detail-grid">
                                <div class="customer-detail-item">
                                    <div class="detail-label">
                                        <i class="fas fa-user-circle"></i>
                                        Tên khách hàng
                                    </div>
                                    <div class="detail-value">
                                        {{
                                            selectedOrder?.user?.name ||
                                            "Không rõ"
                                        }}
                                    </div>
                                </div>
                                <div class="customer-detail-item">
                                    <div class="detail-label">
                                        <i class="fas fa-envelope"></i>
                                        Email
                                    </div>
                                    <div class="detail-value">
                                        {{
                                            selectedOrder?.user?.email ||
                                            "Không rõ"
                                        }}
                                    </div>
                                </div>
                                <div class="customer-detail-item">
                                    <div class="detail-label">
                                        <i class="fas fa-phone"></i>
                                        Số điện thoại
                                    </div>
                                    <div class="detail-value">
                                        {{
                                            selectedOrder?.user?.phoneNumber ||
                                            "Không rõ"
                                        }}
                                    </div>
                                </div>
                            </div>
                            <div class="customer-detail-address">
                                <div class="detail-label">
                                    <i class="fas fa-map-marker-alt"></i>
                                    Địa chỉ
                                </div>
                                <div class="detail-value">
                                    {{
                                        selectedOrder?.user?.address ||
                                        "Không rõ"
                                    }}
                                </div>
                            </div>
                        </div>
                        <!-- Order Items Section -->
                        <div class="order-detail-section">
                            <h5 class="detail-section-title">
                                <i class="fas fa-shopping-cart"></i>
                                Sản phẩm đã đặt
                            </h5>
                            <div class="order-items-list">
                                <div
                                    v-for="item in selectedOrder?.orderItems"
                                    :key="item.id"
                                    class="order-item-card"
                                >
                                    <div class="item-image">
                                        <img
                                            v-if="item.colorImage"
                                            :src="
                                                orderService.getUrlImage(
                                                    item.colorImage
                                                )
                                            "
                                            :alt="item.product?.name"
                                        />
                                        <div v-else class="no-image">
                                            <i class="fas fa-mobile-alt"></i>
                                        </div>
                                    </div>
                                    <div class="item-details">
                                        <h6 class="item-name">
                                            {{
                                                item.product?.name ||
                                                "Sản phẩm không tồn tại"
                                            }}
                                        </h6>
                                        <div class="item-info-row">
                                            <span class="item-label"
                                                >Đơn giá:</span
                                            >
                                            <span class="item-value">{{
                                                formatCurrency(item.price)
                                            }}</span>
                                        </div>
                                        <div class="item-info-row">
                                            <span class="item-label"
                                                >Số lượng:</span
                                            >
                                            <span class="item-value quantity">{{
                                                item.quantity
                                            }}</span>
                                        </div>
                                        <div
                                            class="item-info-row"
                                            v-if="item.discount > 0"
                                        >
                                            <span class="item-label"
                                                >Giảm giá:</span
                                            >
                                            <span class="item-value discount">{{
                                                formatCurrency(item.discount)
                                            }}</span>
                                        </div>
                                        <div class="item-info-row total-row">
                                            <span class="item-label"
                                                >Thành tiền:</span
                                            >
                                            <span class="item-value item-total">
                                                {{
                                                    formatCurrency(
                                                        item.price *
                                                            item.quantity -
                                                            item.discount
                                                    )
                                                }}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Order Summary -->
                            <div class="order-summary">
                                <div class="summary-row">
                                    <span class="summary-label"
                                        >Tổng giá sản phẩm:</span
                                    >
                                    <span class="summary-value">
                                        {{
                                            formatCurrency(
                                                selectedOrder?.totalAmount -
                                                    selectedOrder?.shippingFee
                                            )
                                        }}
                                    </span>
                                </div>
                                <div class="summary-row">
                                    <span class="summary-label"
                                        >Phí vận chuyển:</span
                                    >
                                    <span class="summary-value">
                                        {{
                                            formatCurrency(
                                                selectedOrder?.shippingFee
                                            )
                                        }}
                                    </span>
                                </div>
                                <div class="summary-row total">
                                    <span class="summary-label"
                                        >Tổng cộng:</span
                                    >
                                    <span class="summary-value">
                                        {{
                                            formatCurrency(
                                                selectedOrder?.totalAmount
                                            )
                                        }}
                                    </span>
                                </div>
                            </div>
                        </div>
                        <!-- Update Status Section -->
                        <div
                            class="order-detail-section update-status-section"
                            v-if="
                                selectedOrder &&
                                getAvailableStatusOptions(selectedOrder.status)
                                    .length > 0
                            "
                        >
                            <h5 class="detail-section-title">
                                <i class="fas fa-edit"></i>
                                Cập nhật trạng thái
                            </h5>

                            <div class="status-form">
                                <div class="form-row">
                                    <div class="form-group">
                                        <label>
                                            <i class="fas fa-tasks"></i>
                                            Trạng thái mới
                                        </label>
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
                                        <label>
                                            <i class="fas fa-calendar-alt"></i>
                                            Ngày giao hàng
                                        </label>
                                        <input
                                            type="datetime-local"
                                            v-model="deliveryDate"
                                        />
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label>
                                        <i class="fas fa-sticky-note"></i>
                                        Ghi chú (tùy chọn)
                                    </label>
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
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
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

.view-button {
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
    background-color: #e3f2fd;
    color: #1565c0;
}

.status-badge.shipping {
    background-color: #e1f5fe;
    color: #0288d1;
}

.status-badge.delivered {
    background-color: #e8f5e9;
    color: #43a047;
}

.status-badge.completed {
    background-color: #f1f8e9;
    color: #388e3c;
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
    gap: 0.5rem;
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

.page-numbers {
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.page-number {
    width: 36px;
    height: 36px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #ddd;
    background-color: white;
    color: #666;
    font-size: 0.9rem;
    cursor: pointer;
    transition: all 0.2s;
}

.page-number:hover {
    border-color: var(--primary-color);
    color: var(--primary-color);
}

.page-number.active {
    background-color: var(--primary-color);
    border-color: var(--primary-color);
    color: white;
    font-weight: 600;
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

.order-detail-content {
    display: flex;
    flex-direction: column;
    gap: 2rem;
}

/* Order Detail Header */
.order-detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem;
    background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    border-radius: 12px;
    border-left: 4px solid var(--primary-color);
}

.order-detail-badge {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.badge-icon {
    width: 60px;
    height: 60px;
    background: var(--primary-color);
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    color: white;
}

.order-detail-id {
    margin: 0;
    font-size: 1.5rem;
    color: #333;
    font-weight: 600;
}

.order-date {
    margin: 0;
    color: #666;
    font-size: 0.9rem;
}

.status-badge-large {
    padding: 0.5rem 1rem;
    border-radius: 20px;
    font-size: 0.9rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

/* Order Summary Cards */
.order-summary-cards {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 1rem;
}

.summary-card {
    background: white;
    border-radius: 12px;
    padding: 1.5rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    border: 1px solid #f0f0f0;
    transition: all 0.3s ease;
}

.summary-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.card-icon {
    width: 50px;
    height: 50px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
}

.payment-card .card-icon {
    background-color: #e3f2fd;
    color: #1976d2;
}

.shipping-card .card-icon {
    background-color: #f3e5f5;
    color: #7b1fa2;
}

.total-card .card-icon {
    background-color: #e8f5e9;
    color: #388e3c;
}

.card-label {
    margin: 0 0 0.5rem 0;
    font-size: 0.85rem;
    color: #666;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.card-value {
    margin: 0;
    font-size: 1.1rem;
    font-weight: 600;
    color: #333;
}

.total-amount {
    color: var(--primary-color);
    font-size: 1.3rem;
}

/* Order Detail Sections */
.order-detail-section {
    background: white;
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    border: 1px solid #f0f0f0;
}

.detail-section-title {
    margin: 0 0 1.5rem 0;
    font-size: 1.1rem;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    border-bottom: 2px solid #f0f0f0;
    padding-bottom: 0.75rem;
}

.detail-section-title i {
    color: var(--primary-color);
}

/* Customer Details */
.customer-detail-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 1rem;
    margin-bottom: 1rem;
}

.customer-detail-item {
    background: #f8f9fa;
    padding: 1rem;
    border-radius: 8px;
    border-left: 3px solid var(--primary-color);
}

.customer-detail-address {
    background: #f8f9fa;
    padding: 1rem;
    border-radius: 8px;
    border-left: 3px solid var(--primary-color);
    margin-top: 1rem;
}

.detail-label {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    color: #666;
    font-size: 0.85rem;
    margin-bottom: 0.5rem;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.detail-label i {
    color: var(--primary-color);
}

.detail-value {
    font-weight: 500;
    color: #333;
    font-size: 0.95rem;
}

/* Order Items */
.order-items-list {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.order-item-card {
    display: flex;
    gap: 1rem;
    padding: 1.25rem;
    background: #f8f9fa;
    border-radius: 12px;
    border: 1px solid #e9ecef;
    transition: all 0.3s ease;
}

.order-item-card:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    border-color: var(--primary-color);
}

.item-image {
    width: 80px;
    height: 80px;
    border-radius: 8px;
    overflow: hidden;
    flex-shrink: 0;
    border: 2px solid white;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.item-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.item-image .no-image {
    width: 100%;
    height: 100%;
    background: #e9ecef;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #6c757d;
    font-size: 1.5rem;
}

.item-details {
    flex: 1;
}

.item-name {
    margin: 0 0 0.75rem 0;
    font-size: 1.1rem;
    font-weight: 600;
    color: #333;
    line-height: 1.3;
}

.item-info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
}

.item-info-row.total-row {
    margin-top: 0.75rem;
    padding-top: 0.75rem;
    border-top: 1px solid #dee2e6;
}

.item-label {
    color: #666;
    font-size: 0.9rem;
}

.item-value {
    font-weight: 500;
    color: #333;
}

.item-value.quantity {
    background: #e3f2fd;
    color: #1976d2;
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.85rem;
}

.item-value.discount {
    color: #d32f2f;
}

.item-value.item-total {
    color: var(--primary-color);
    font-weight: 600;
    font-size: 1.05rem;
}

/* Order Summary */
.order-summary {
    margin-top: 1.5rem;
    padding: 1.25rem;
    background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    border-radius: 12px;
    border: 1px solid #dee2e6;
}

.summary-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.75rem;
}

.summary-row.total {
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 2px solid #dee2e6;
    font-size: 1.1rem;
    font-weight: 600;
}

.summary-label {
    color: #666;
}

.summary-row.total .summary-label {
    color: #333;
    font-weight: 600;
}

.summary-value {
    font-weight: 500;
    color: #333;
}

.summary-row.total .summary-value {
    color: var(--primary-color);
    font-size: 1.3rem;
    font-weight: 700;
}

/* Update Status Section */
.update-status-section {
    background: linear-gradient(135deg, #fff8e1 0%, #ffecb3 100%);
    border-left: 4px solid #ff9800;
}

.form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
}

.form-group {
    margin-bottom: 1rem;
}

.form-group label {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9rem;
    font-weight: 500;
    margin-bottom: 0.5rem;
    color: #333;
}

.form-group label i {
    color: var(--primary-color);
}

.form-group select,
.form-group input[type="datetime-local"],
.form-group textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.95rem;
    transition: all 0.3s ease;
    background: white;
}

.form-group select:focus,
.form-group input[type="datetime-local"]:focus,
.form-group textarea:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(248, 110, 211, 0.1);
    outline: none;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    margin-top: 1.5rem;
}

.update-status-button {
    padding: 0.75rem 1.5rem;
    background: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 500;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(248, 110, 211, 0.3);
}

.update-status-button:hover {
    background: #e94e9c;
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.4);
}

.update-status-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
    transform: none;
}

.text-center {
    text-align: center;
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

    /* Modal responsive */
    .modal-container {
        width: 98%;
        max-height: 95vh;
    }

    .order-detail-header {
        flex-direction: column;
        text-align: center;
        gap: 1rem;
    }

    .order-summary-cards {
        grid-template-columns: 1fr;
    }

    .customer-detail-grid {
        grid-template-columns: 1fr;
    }

    .order-item-card {
        flex-direction: column;
        text-align: center;
    }

    .item-image {
        align-self: center;
    }

    .form-row {
        grid-template-columns: 1fr;
    }

    .form-actions {
        justify-content: center;
    }

    .update-status-button {
        width: 100%;
        justify-content: center;
    }
}

@media (max-width: 480px) {
    .modal-body {
        padding: 1rem;
    }

    .order-detail-section {
        padding: 1rem;
    }

    .order-summary-cards {
        gap: 0.75rem;
    }

    .summary-card {
        padding: 1rem;
    }

    .order-item-card {
        padding: 1rem;
    }
}
</style>
