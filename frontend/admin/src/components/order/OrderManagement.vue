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
}; // Update order status
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
            newStatus.value
        );

        await orderService.updateOrderStatus(
            selectedOrder.value.id,
            statusData
        );

        // Close modal first
        closeModal();

        // Then refresh orders
        await fetchOrders();

        emitter.emit("show-notification", {
            status: "success",
            message: "Đã cập nhật trạng thái đơn hàng thành công",
        });
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

// Check if status is completed in the workflow
const isStatusCompleted = (status) => {
    const statusOrder = [
        "Chờ xác nhận",
        "Đã xác nhận",
        "Đang giao hàng",
        "Đã giao hàng",
        "Hoàn thành",
    ];

    // If current status is "Hoàn thành", all statuses are considered completed
    if (selectedOrder.value?.status === "Hoàn thành") {
        return true;
    }

    // If current status is "Đã huỷ", "Đã hoàn tiền", or "Đã trả hàng",
    // only consider the statuses up to the last valid one as completed
    if (
        ["Đã huỷ", "Đã hoàn tiền", "Đã trả hàng"].includes(
            selectedOrder.value?.status
        )
    ) {
        return false;
    }

    const currentIndex = statusOrder.indexOf(selectedOrder.value?.status);
    const statusIndex = statusOrder.indexOf(status);

    // Return true if the status index is less than the current status index
    return statusIndex < currentIndex;
};

// Get icon for status
const getStatusIcon = (status) => {
    const statusIcons = {
        "Chờ xác nhận": "fas fa-clock",
        "Đã xác nhận": "fas fa-check-circle",
        "Đang giao hàng": "fas fa-shipping-fast",
        "Đã giao hàng": "fas fa-box",
        "Hoàn thành": "fas fa-flag-checkered",
        "Đã huỷ": "fas fa-ban",
        "Đã hoàn tiền": "fas fa-money-bill-wave",
        "Đã trả hàng": "fas fa-undo",
    };

    return statusIcons[status] || "fas fa-question-circle";
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
                                        }}</span
                                    >
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
                                        }}</span
                                    >
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
                                <i class="fas fa-exchange-alt"></i>
                                Thay đổi trạng thái đơn hàng
                            </h5>

                            <!-- Status Timeline -->
                            <div class="status-timeline">
                                <div
                                    class="timeline-item"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Chờ xác nhận'),
                                        current:
                                            selectedOrder.status ===
                                            'Chờ xác nhận',
                                    }"
                                >
                                    <div class="timeline-icon">
                                        <i class="fas fa-clock"></i>
                                    </div>
                                    <div class="timeline-content">
                                        <div class="timeline-status">
                                            Chờ xác nhận
                                        </div>
                                    </div>
                                    <div class="timeline-tooltip">
                                        Đơn hàng đang chờ xác nhận từ quản trị
                                        viên
                                    </div>
                                </div>

                                <div
                                    class="timeline-connector"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Đã xác nhận'),
                                    }"
                                ></div>

                                <div
                                    class="timeline-item"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Đã xác nhận'),
                                        current:
                                            selectedOrder.status ===
                                            'Đã xác nhận',
                                    }"
                                >
                                    <div class="timeline-icon">
                                        <i class="fas fa-check-circle"></i>
                                    </div>
                                    <div class="timeline-content">
                                        <div class="timeline-status">
                                            Đã xác nhận
                                        </div>
                                    </div>
                                    <div class="timeline-tooltip">
                                        Đơn hàng đã được xác nhận và đang chuẩn
                                        bị
                                    </div>
                                </div>

                                <div
                                    class="timeline-connector"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Đang giao hàng'),
                                    }"
                                ></div>

                                <div
                                    class="timeline-item"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Đang giao hàng'),
                                        current:
                                            selectedOrder.status ===
                                            'Đang giao hàng',
                                    }"
                                >
                                    <div class="timeline-icon">
                                        <i class="fas fa-shipping-fast"></i>
                                    </div>
                                    <div class="timeline-content">
                                        <div class="timeline-status">
                                            Đang giao hàng
                                        </div>
                                    </div>
                                    <div class="timeline-tooltip">
                                        Đơn hàng đang được vận chuyển đến khách
                                        hàng
                                    </div>
                                </div>

                                <div
                                    class="timeline-connector"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Đã giao hàng'),
                                    }"
                                ></div>

                                <div
                                    class="timeline-item"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Đã giao hàng'),
                                        current:
                                            selectedOrder.status ===
                                            'Đã giao hàng',
                                    }"
                                >
                                    <div class="timeline-icon">
                                        <i class="fas fa-box"></i>
                                    </div>
                                    <div class="timeline-content">
                                        <div class="timeline-status">
                                            Đã giao hàng
                                        </div>
                                    </div>
                                    <div class="timeline-tooltip">
                                        Đơn hàng đã được giao đến khách hàng
                                    </div>
                                </div>

                                <div
                                    class="timeline-connector"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Hoàn thành'),
                                    }"
                                ></div>

                                <div
                                    class="timeline-item"
                                    :class="{
                                        completed:
                                            isStatusCompleted('Hoàn thành'),
                                        current:
                                            selectedOrder.status ===
                                            'Hoàn thành',
                                    }"
                                >
                                    <div class="timeline-icon">
                                        <i class="fas fa-flag-checkered"></i>
                                    </div>
                                    <div class="timeline-content">
                                        <div class="timeline-status">
                                            Hoàn thành
                                        </div>
                                    </div>
                                    <div class="timeline-tooltip">
                                        Đơn hàng đã hoàn thành toàn bộ quy trình
                                    </div>
                                </div>
                            </div>
                            <div class="status-selection-container">
                                <h6 class="status-selection-title">
                                    <i class="fas fa-exchange-alt"></i> Chọn
                                    trạng thái mới:
                                </h6>
                                <div class="status-options">
                                    <button
                                        v-for="status in getAvailableStatusOptions(
                                            selectedOrder.status
                                        )"
                                        :key="status"
                                        @click="newStatus = status"
                                        :class="[
                                            'status-option-button',
                                            getStatusClass(status),
                                            { selected: newStatus === status },
                                        ]"
                                        type="button"
                                    >
                                        <div class="status-option-icon">
                                            <i
                                                :class="getStatusIcon(status)"
                                            ></i>
                                        </div>
                                        <div class="status-option-label">
                                            {{ status }}
                                        </div>
                                    </button>
                                </div>

                                <!-- Status Transition Indicator -->
                                <div
                                    class="status-transition-indicator"
                                    v-if="newStatus"
                                >
                                    <div
                                        :class="[
                                            'current-status',
                                            getStatusClass(
                                                selectedOrder.status
                                            ),
                                        ]"
                                    >
                                        <i
                                            :class="
                                                getStatusIcon(
                                                    selectedOrder.status
                                                )
                                            "
                                        ></i>
                                        {{ selectedOrder.status }}
                                    </div>
                                    <div class="status-arrow">
                                        <i class="fas fa-arrow-right"></i>
                                    </div>
                                    <div
                                        :class="[
                                            'new-status',
                                            getStatusClass(newStatus),
                                        ]"
                                    >
                                        <i
                                            :class="getStatusIcon(newStatus)"
                                        ></i>
                                        {{ newStatus }}
                                    </div>
                                </div>
                                <div class="update-button-container">
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
                                            <i class="fas fa-check-circle"></i>
                                            Cập nhật trạng thái
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
    border-radius: 10px;
    width: 90%;
    max-width: 900px;
    max-height: 85vh;
    display: flex;
    flex-direction: column;
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.order-modal {
    max-width: 900px;
}

.modal-header {
    padding: 0.7rem 1rem;
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
    font-size: 0.95rem;
    color: #333;
    font-weight: 600;
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
    padding: 0.8rem;
    overflow-y: auto;
    max-height: calc(85vh - 60px);
}

.order-detail-content {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

/* Order Detail Header */
.order-detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.7rem;
    background-color: #ffffff;
    border-radius: 8px;
    border-left: 3px solid var(--primary-color);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.order-detail-badge {
    display: flex;
    align-items: center;
    gap: 0.6rem;
}

.badge-icon {
    width: 38px;
    height: 38px;
    background: var(--primary-color);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1rem;
    color: white;
}

.order-detail-id {
    margin: 0;
    font-size: 1.05rem;
    color: #333;
    font-weight: 600;
}

.order-date {
    margin: 0;
    color: #666;
    font-size: 0.75rem;
}

.status-badge-large {
    padding: 0.4rem 0.8rem;
    border-radius: 15px;
    font-size: 0.8rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

/* Order Summary Cards */
.order-summary-cards {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    gap: 0.6rem;
}

.summary-card {
    background: white;
    border-radius: 8px;
    padding: 0.75rem;
    display: flex;
    align-items: center;
    gap: 0.6rem;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
    border: 1px solid #f0f0f0;
    transition: all 0.3s ease;
}

.summary-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.08);
}

.card-icon {
    width: 34px;
    height: 34px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.95rem;
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
    margin: 0 0 0.25rem 0;
    font-size: 0.75rem;
    color: #666;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.card-value {
    margin: 0;
    font-size: 1rem;
    font-weight: 600;
    color: #333;
}

.total-amount {
    color: var(--primary-color);
    font-size: 1.1rem;
}

/* Order Detail Sections */
.order-detail-section {
    background: white;
    border-radius: 8px;
    padding: 0.8rem;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
    border: 1px solid #f0f0f0;
}

.detail-section-title {
    margin: 0 0 0.8rem 0;
    font-size: 0.9rem;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.4rem;
    border-bottom: 1px solid #f0f0f0;
    padding-bottom: 0.5rem;
}

.detail-section-title i {
    color: var(--primary-color);
}

/* Customer Details */
.customer-detail-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
    gap: 0.6rem;
    margin-bottom: 0.6rem;
}

.customer-detail-item {
    background: #ffffff;
    padding: 0.6rem;
    border-radius: 6px;
    border-left: 2px solid var(--primary-color);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.customer-detail-address {
    background: #ffffff;
    padding: 0.6rem;
    border-radius: 6px;
    border-left: 2px solid var(--primary-color);
    margin-top: 0.6rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.detail-label {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    color: #666;
    font-size: 0.75rem;
    margin-bottom: 0.3rem;
    text-transform: uppercase;
    letter-spacing: 0.3px;
}

.detail-label i {
    color: var(--primary-color);
}

.detail-value {
    font-weight: 500;
    color: #333;
    font-size: 0.9rem;
}

/* Order Items */
.order-items-list {
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}

.order-item-card {
    display: flex;
    gap: 0.6rem;
    padding: 0.75rem;
    background: #ffffff;
    border-radius: 8px;
    border: 1px solid #e9ecef;
    transition: all 0.3s ease;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.item-image {
    width: 50px;
    height: 50px;
    border-radius: 6px;
    overflow: hidden;
    flex-shrink: 0;
    border: 1px solid white;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.1);
}

.item-image img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.item-details {
    flex: 1;
}

.item-name {
    margin: 0 0 0.4rem 0;
    font-size: 0.9rem;
    font-weight: 600;
    color: #333;
    line-height: 1.2;
}

.item-info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.35rem;
}

.item-info-row.total-row {
    margin-top: 0.4rem;
    padding-top: 0.4rem;
    border-top: 1px solid #dee2e6;
}

.item-label {
    color: #666;
    font-size: 0.8rem;
}

.item-value {
    font-weight: 500;
    color: #333;
    font-size: 0.85rem;
}

.item-value.quantity {
    background: #e3f2fd;
    color: #1976d2;
    padding: 0.2rem 0.4rem;
    border-radius: 10px;
    font-size: 0.75rem;
}

.item-value.discount {
    color: #d32f2f;
}

.item-value.item-total {
    color: var(--primary-color);
    font-weight: 600;
    font-size: 0.9rem;
}

/* Order Summary */
.order-summary {
    margin-top: 0.8rem;
    padding: 0.75rem;
    background: #ffffff;
    border-radius: 8px;
    border: 1px solid #dee2e6;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.summary-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.4rem;
}

.summary-row.total {
    margin-top: 0.6rem;
    padding-top: 0.6rem;
    border-top: 1px solid #dee2e6;
    font-size: 0.95rem;
    font-weight: 600;
}

.summary-label {
    color: #666;
    font-size: 0.85rem;
}

.summary-row.total .summary-label {
    color: #333;
    font-weight: 600;
}

.summary-value {
    font-weight: 500;
    color: #333;
    font-size: 0.85rem;
}

.summary-row.total .summary-value {
    color: var(--primary-color);
    font-size: 1.1rem;
    font-weight: 700;
}

/* Update Status Section */
.update-status-section {
    background-color: #ffffff;
    border-left: 3px solid var(--primary-color);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

/* Status Timeline */
.status-timeline {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin: 1rem 0 1.5rem;
    position: relative;
    padding: 1rem;
    background-color: #ffffff;
    border-radius: 6px;
    border: 1px solid #e8e8e8;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.timeline-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    position: relative;
    z-index: 2;
    transition: transform 0.15s ease;
}

.timeline-item:hover {
    transform: translateY(-2px);
}

.timeline-icon {
    width: 30px;
    height: 30px;
    border-radius: 50%;
    background-color: #f5f5f5;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #666;
    margin-bottom: 0.5rem;
    transition: all 0.2s ease;
    border: 1px solid #e0e0e0;
    box-shadow: none;
}

.timeline-content {
    text-align: center;
}

.timeline-status {
    font-size: 0.75rem;
    font-weight: 500;
    color: #666;
    max-width: 80px;
    white-space: normal;
    text-align: center;
}

.timeline-connector {
    flex-grow: 1;
    height: 2px;
    background-color: #e0e0e0;
    position: relative;
    z-index: 1;
}

/* Current Status */
.timeline-item.current .timeline-icon {
    background-color: var(--primary-color);
    color: white;
    border-color: var(--primary-color);
    transform: scale(1.05);
}

.timeline-item.current .timeline-status {
    color: var(--primary-color);
    font-weight: 600;
}

/* Completed Status */
.timeline-item.completed .timeline-icon {
    background-color: #4caf50;
    color: white;
    border-color: #4caf50;
}

.timeline-item.completed .timeline-status {
    color: #4caf50;
    font-weight: 500;
}

.timeline-connector.completed {
    background-color: #4caf50;
}

@keyframes fadeIn {
    0% {
        opacity: 0;
    }
    100% {
        opacity: 1;
    }
}

/* Status Selection */
.status-selection-container {
    background-color: white;
    border-radius: 6px;
    padding: 1rem;
    margin-top: 1rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.03);
    border: 1px solid #eaeaea;
}

.status-selection-title {
    margin: 0 0 0.75rem 0;
    font-size: 0.9rem;
    color: #333;
    position: relative;
    padding-bottom: 0.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.status-options {
    display: flex;
    flex-wrap: wrap;
    gap: 0.75rem;
    margin-bottom: 1rem;
}

.status-option-button {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 0.6rem 0.5rem;
    border-radius: 4px;
    border: 1px solid #e0e0e0;
    background-color: white;
    cursor: pointer;
    transition: all 0.2s ease;
    width: 90px;
    box-shadow: none;
}

.status-option-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

/* Status options with colors matching the status badges */
.status-option-button.pending {
    background-color: #fff8e1;
    color: #ff8f00;
    border-color: #ffe082;
}

.status-option-button.confirmed {
    background-color: #e3f2fd;
    color: #1565c0;
    border-color: #bbdefb;
}

.status-option-button.shipping {
    background-color: #e1f5fe;
    color: #0288d1;
    border-color: #b3e5fc;
}

.status-option-button.delivered {
    background-color: #e8f5e9;
    color: #43a047;
    border-color: #c8e6c9;
}

.status-option-button.completed {
    background-color: #f1f8e9;
    color: #388e3c;
    border-color: #dcedc8;
}

.status-option-button.canceled {
    background-color: #ffebee;
    color: #c62828;
    border-color: #ffcdd2;
}

.status-option-button.refunded {
    background-color: #f3e5f5;
    color: #7b1fa2;
    border-color: #e1bee7;
}

.status-option-button.returned {
    background-color: #ede7f6;
    color: #512da8;
    border-color: #d1c4e9;
}

.status-option-button.selected {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.2);
}

.status-option-icon {
    font-size: 1.1rem;
    margin-bottom: 0.4rem;
}

/* Colored buttons should have matching icon colors */
.status-option-button.pending .status-option-icon,
.status-option-button.confirmed .status-option-icon,
.status-option-button.shipping .status-option-icon,
.status-option-button.delivered .status-option-icon,
.status-option-button.completed .status-option-icon,
.status-option-button.canceled .status-option-icon,
.status-option-button.refunded .status-option-icon,
.status-option-button.returned .status-option-icon {
    color: inherit;
}

.status-option-label {
    font-size: 0.8rem;
    font-weight: 500;
    text-align: center;
}

/* Tooltip */
.timeline-tooltip {
    position: absolute;
    bottom: 100%;
    left: 50%;
    transform: translateX(-50%);
    background-color: #444;
    color: white;
    padding: 0.3rem 0.5rem;
    border-radius: 3px;
    font-size: 0.7rem;
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.2s ease;
    width: max-content;
    max-width: 150px;
    text-align: center;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    z-index: 10;
    pointer-events: none;
}

.timeline-tooltip::after {
    content: "";
    position: absolute;
    top: 100%;
    left: 50%;
    transform: translateX(-50%);
    border-width: 3px;
    border-style: solid;
    border-color: #444 transparent transparent transparent;
}

.timeline-item:hover .timeline-tooltip {
    opacity: 1;
    visibility: visible;
}

/* Enhanced Update Button */
.update-button-container {
    display: flex;
    justify-content: center;
    margin-top: 1rem;
}

.update-status-button {
    padding: 0.6rem 1.2rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 4px;
    font-weight: 500;
    font-size: 0.85rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.4rem;
    transition: background-color 0.2s ease;
    box-shadow: none;
    min-width: 140px;
}

.update-status-button:hover {
    background-color: #e5348e;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.update-status-button:active {
    transform: translateY(1px);
}

.update-status-button:disabled {
    background-color: #cccccc;
    box-shadow: none;
    cursor: not-allowed;
    opacity: 0.7;
}

.update-status-button i {
    font-size: 0.9rem;
}

/* Additional styles */
.form-helper {
    font-size: 0.75rem;
    color: #777;
    margin-top: 0.4rem;
    font-style: normal;
}

/* Status transition visualization */
.status-transition-indicator {
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0.75rem 0;
    padding: 0.6rem;
    background-color: #ffffff;
    border-radius: 8px;
    border: 1px solid #e8e8e8;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.current-status,
.new-status {
    padding: 0.5rem 0.8rem;
    border-radius: 20px;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.85rem;
}

/* Apply the same color scheme as the status badges */
.current-status.pending,
.new-status.pending {
    background-color: #fff8e1;
    color: #ff8f00;
}

.current-status.confirmed,
.new-status.confirmed {
    background-color: #e3f2fd;
    color: #1565c0;
}

.current-status.shipping,
.new-status.shipping {
    background-color: #e1f5fe;
    color: #0288d1;
}

.current-status.delivered,
.new-status.delivered {
    background-color: #e8f5e9;
    color: #43a047;
}

.current-status.completed,
.new-status.completed {
    background-color: #f1f8e9;
    color: #388e3c;
}

.current-status.canceled,
.new-status.canceled {
    background-color: #ffebee;
    color: #c62828;
}

.current-status.refunded,
.new-status.refunded {
    background-color: #f3e5f5;
    color: #7b1fa2;
}

.current-status.returned,
.new-status.returned {
    background-color: #ede7f6;
    color: #512da8;
}

.current-status i,
.new-status i {
    font-size: 0.95rem;
}

.status-arrow {
    margin: 0 0.8rem;
    color: var(--primary-color);
    font-size: 1.1rem;
    animation: arrowPulse 1s infinite ease-in-out;
    display: flex;
    align-items: center;
}

@keyframes arrowPulse {
    0% {
        transform: translateX(0);
    }
    50% {
        transform: translateX(3px);
    }
    100% {
        transform: translateX(0);
    }
}

/* Standalone status transition for "Chọn trạng thái mới" */
.status-change-row {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    margin: 0.5rem 0;
    padding: 0.3rem;
    border-radius: 20px;
    background: white;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);
}

.status-change-row .status-item {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    padding: 0.4rem 0.8rem;
    border-radius: 20px;
    font-size: 0.85rem;
    font-weight: 500;
}

.status-change-row .arrow {
    padding: 0 0.5rem;
    color: var(--primary-color);
    animation: arrowPulse 1s infinite ease-in-out;
}

/* Status colors for standalone displays */
.status-item.delivered {
    background-color: #e8f5e9;
    color: #43a047;
}

.status-item.returned {
    background-color: #ede7f6;
    color: #512da8;
}

.status-item.pending {
    background-color: #fff8e1;
    color: #ff8f00;
}

.status-item.confirmed {
    background-color: #e3f2fd;
    color: #1565c0;
}

.status-item.shipping {
    background-color: #e1f5fe;
    color: #0288d1;
}

.status-item.completed {
    background-color: #f1f8e9;
    color: #388e3c;
}

.status-item.canceled {
    background-color: #ffebee;
    color: #c62828;
}

.status-item.refunded {
    background-color: #f3e5f5;
    color: #7b1fa2;
}

/* Mobile responsiveness for new elements */
@media (max-width: 768px) {
    .timeline-tooltip {
        max-width: 120px;
        font-size: 0.65rem;
        padding: 0.25rem 0.4rem;
    }

    .status-timeline {
        padding: 0.75rem 0.5rem;
    }

    .timeline-icon {
        width: 26px;
        height: 26px;
        font-size: 0.8rem;
    }

    .timeline-status {
        font-size: 0.7rem;
        max-width: 70px;
    }

    .status-options {
        justify-content: space-around;
        gap: 0.5rem;
    }

    .status-option-button {
        width: calc(33.33% - 0.5rem);
        min-width: 80px;
        padding: 0.5rem 0.3rem;
    }

    .status-option-icon {
        font-size: 1rem;
    }

    .status-option-label {
        font-size: 0.75rem;
    }

    .delivery-date-input {
        padding: 0.6rem;
    }

    .update-status-button {
        width: 100%;
        padding: 0.5rem 1rem;
    }
}
</style>
