<template>
    <div class="order-statistics">
        <div class="section-header">
            <div class="left-section">
                <h2>
                    <i class="fas fa-chart-line"></i>
                    Thống kê đơn hàng
                </h2>
                <p>Tổng quan về tình hình đơn hàng và thanh toán</p>
            </div>
            <div class="right-section">
                <div class="time-period">
                    {{ props.formattedTimePeriod }}
                </div>
            </div>
        </div>

        <div v-if="loading" class="data-card">
            <div class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
        </div>

        <div v-else class="stats-content">
            <!-- Summary Cards -->
            <div class="summary-grid">
                <div class="stat-card total-orders">
                    <div class="stat-icon">
                        <i class="fas fa-shopping-bag"></i>
                    </div>
                    <div class="stat-info">
                        <h4>Tổng đơn hàng</h4>
                        <div class="stat-value">
                            {{ formatNumber(orderStats.totalOrders || 0) }}
                        </div>
                        <div
                            class="stat-change"
                            :class="
                                orderStats.ordersChange >= 0
                                    ? 'positive'
                                    : 'negative'
                            "
                        >
                            <i
                                :class="
                                    orderStats.ordersChange >= 0
                                        ? 'fas fa-arrow-up'
                                        : 'fas fa-arrow-down'
                                "
                            ></i>
                            {{
                                formatPercentage(
                                    Math.abs(orderStats.ordersChange || 0)
                                )
                            }}%
                        </div>
                    </div>
                </div>

                <div class="stat-card total-revenue">
                    <div class="stat-icon">
                        <i class="fas fa-money-bill-wave"></i>
                    </div>
                    <div class="stat-info">
                        <h4>Tổng doanh thu</h4>
                        <div class="stat-value">
                            {{ formatCurrency(orderStats.totalRevenue || 0) }}
                        </div>
                        <div
                            class="stat-change"
                            :class="
                                orderStats.revenueChange >= 0
                                    ? 'positive'
                                    : 'negative'
                            "
                        >
                            <i
                                :class="
                                    orderStats.revenueChange >= 0
                                        ? 'fas fa-arrow-up'
                                        : 'fas fa-arrow-down'
                                "
                            ></i>
                            {{
                                formatPercentage(
                                    Math.abs(orderStats.revenueChange || 0)
                                )
                            }}%
                        </div>
                    </div>
                </div>

                <div class="stat-card avg-order-value">
                    <div class="stat-icon">
                        <i class="fas fa-calculator"></i>
                    </div>
                    <div class="stat-info">
                        <h4>Giá trị đơn hàng TB</h4>
                        <div class="stat-value">
                            {{ formatCurrency(orderStats.avgOrderValue || 0) }}
                        </div>
                        <div
                            class="stat-change"
                            :class="
                                orderStats.avgChange >= 0
                                    ? 'positive'
                                    : 'negative'
                            "
                        >
                            <i
                                :class="
                                    orderStats.avgChange >= 0
                                        ? 'fas fa-arrow-up'
                                        : 'fas fa-arrow-down'
                                "
                            ></i>
                            {{
                                formatPercentage(
                                    Math.abs(orderStats.avgChange || 0)
                                )
                            }}%
                        </div>
                    </div>
                </div>

                <div class="stat-card completion-rate">
                    <div class="stat-icon">
                        <i class="fas fa-check-circle"></i>
                    </div>
                    <div class="stat-info">
                        <h4>Tỷ lệ hoàn thành</h4>
                        <div class="stat-value">
                            {{
                                formatPercentage(
                                    orderStats.completionRate || 0
                                )
                            }}%
                        </div>
                        <div
                            class="stat-change"
                            :class="
                                orderStats.completionChange >= 0
                                    ? 'positive'
                                    : 'negative'
                            "
                        >
                            <i
                                :class="
                                    orderStats.completionChange >= 0
                                        ? 'fas fa-arrow-up'
                                        : 'fas fa-arrow-down'
                                "
                            ></i>
                            {{
                                formatPercentage(
                                    Math.abs(orderStats.completionChange || 0)
                                )
                            }}%
                        </div>
                    </div>
                </div>
            </div>

            <!-- Status Distribution -->
            <div class="data-card" v-if="orderStatusDistribution.length > 0">
                <h4>
                    <i class="fas fa-pie-chart"></i> Phân bố trạng thái đơn hàng
                </h4>
                <div class="status-grid">
                    <div
                        v-for="status in orderStatusDistribution"
                        :key="status.status"
                        :class="[
                            'status-item',
                            status.status.toLowerCase().replace(/\s+/g, ''),
                        ]"
                    >
                        <div class="status-icon">
                            <i :class="getStatusIcon(status.status)"></i>
                        </div>
                        <div class="status-info">
                            <div class="status-name">
                                {{ getStatusName(status.status) }}
                            </div>
                            <div class="status-count">
                                {{ formatNumber(status.count) }} đơn hàng
                            </div>
                            <div class="status-percentage">
                                {{ formatPercentage(status.percentage) }}%
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- Payment Methods -->
            <div class="data-card" v-if="paymentMethods.length > 0">
                <h4>
                    <i class="fas fa-credit-card"></i> Phương thức thanh toán
                </h4>
                <div class="payment-list">
                    <div
                        v-for="method in paymentMethods"
                        :key="method.method"
                        class="payment-item"
                    >
                        <div class="payment-name">
                            <i :class="getPaymentIcon(method.method)"></i>
                            {{ getPaymentName(method.method) }}
                        </div>
                        <div class="payment-stats">
                            <div class="payment-count">
                                {{ formatNumber(method.count) }} lượt
                            </div>
                            <div class="payment-bar">
                                <div
                                    class="payment-fill"
                                    :style="{ width: method.percentage + '%' }"
                                ></div>
                            </div>
                            <div class="payment-percentage">
                                {{ formatPercentage(method.percentage) }}%
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
    orderStats: {
        type: Object,
        default: () => ({
            totalOrders: 0,
            totalRevenue: 0,
            avgOrderValue: 0,
            completionRate: 0,
            orderChange: 0,
            revenueChange: 0,
            avgChange: 0,
            completionChange: 0,
        }),
    },
    orderStatusDistribution: {
        type: Array,
        default: () => [],
    },
    paymentMethods: {
        type: Array,
        default: () => [],
    },
    loading: {
        type: Boolean,
        default: false,
    },
    timeFilter: {
        type: String,
        default: "month",
    },
    formattedTimePeriod: {
        type: String,
        default: "Tháng này",
    },
});

const formatTimePeriod = computed(() => {
    const periods = {
        today: "Hôm nay",
        week: "Tuần này",
        month: "Tháng này",
        quarter: "Quý này",
        year: "Năm nay",
        custom: "Khoảng thời gian tùy chỉnh",
    };
    return periods[props.timeFilter] || "Tháng này";
});

const formatNumber = (number) => {
    return new Intl.NumberFormat("vi-VN").format(number);
};

const formatCurrency = (amount) => {
    if (!amount && amount !== 0) return "0 ₫";
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
    }).format(amount);
};

const formatPercentage = (value) => {
    if (!value && value !== 0) return "0";
    // Làm tròn đến 1 chữ số thập phân
    return Number(value).toFixed(1);
};

const getStatusIcon = (status) => {
    const statusKey = status.toLowerCase().replace(/\s+/g, "");
    const icons = {
        chờxửlý: "fas fa-clock",
        choxuly: "fas fa-clock",
        pending: "fas fa-clock",
        đãxácnhận: "fas fa-check",
        daxacnhan: "fas fa-check",
        confirmed: "fas fa-check",
        đangxửlý: "fas fa-cog fa-spin",
        dangxuly: "fas fa-cog fa-spin",
        processing: "fas fa-cog fa-spin",
        đanggiao: "fas fa-truck",
        danggiao: "fas fa-truck",
        shipping: "fas fa-truck",
        đãgiao: "fas fa-check-double",
        dagiao: "fas fa-check-double",
        delivered: "fas fa-check-double",
        hoànthành: "fas fa-check-double",
        hoanthanh: "fas fa-check-double",
        completed: "fas fa-check-double",
        đãhủy: "fas fa-times-circle",
        dahuy: "fas fa-times-circle",
        cancelled: "fas fa-times-circle",
        đãtrả: "fas fa-undo",
        datra: "fas fa-undo",
        returned: "fas fa-undo",
    };
    return icons[statusKey] || "fas fa-question-circle";
};

const getStatusName = (status) => {
    const statusKey = status.toLowerCase().replace(/\s+/g, "");
    const names = {
        chờxửlý: "Chờ xử lý",
        choxuly: "Chờ xử lý",
        pending: "Chờ xử lý",
        đãxácnhận: "Đã xác nhận",
        daxacnhan: "Đã xác nhận",
        confirmed: "Đã xác nhận",
        đangxửlý: "Đang xử lý",
        dangxuly: "Đang xử lý",
        processing: "Đang xử lý",
        đanggiao: "Đang giao",
        danggiao: "Đang giao",
        shipping: "Đang giao",
        đãgiao: "Đã giao",
        dagiao: "Đã giao",
        delivered: "Đã giao",
        hoànthành: "Hoàn thành",
        hoanthanh: "Hoàn thành",
        completed: "Hoàn thành",
        đãhủy: "Đã hủy",
        dahuy: "Đã hủy",
        cancelled: "Đã hủy",
        đãtrả: "Đã trả",
        datra: "Đã trả",
        returned: "Đã trả",
    };
    return names[statusKey] || status;
};

const getPaymentIcon = (method) => {
    const icons = {
        cash: "fas fa-money-bill-wave",
        card: "fas fa-credit-card",
        bank: "fas fa-university",
        wallet: "fas fa-wallet",
        momo: "fas fa-mobile-alt",
        zalopay: "fas fa-money-check-alt",
    };
    return icons[method.toLowerCase()] || "fas fa-question-circle";
};

const getPaymentName = (method) => {
    const names = {
        cash: "Tiền mặt",
        card: "Thẻ tín dụng",
        bank: "Chuyển khoản",
        wallet: "Ví điện tử",
        momo: "MoMo",
        zalopay: "ZaloPay",
    };
    return names[method.toLowerCase()] || method;
};
</script>

<style scoped>
.order-statistics {
    width: 100%;
    margin-bottom: 1.5rem;
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

.left-section h2 i {
    color: #3182ce;
}

.left-section p {
    color: #666;
    margin: 0;
    font-size: 0.9rem;
}

.time-period {
    color: #666;
    font-size: 0.9rem;
    font-weight: 500;
    background: #f7fafc;
    padding: 0.5rem 1rem;
    border-radius: 8px;
    border: 1px solid #e2e8f0;
}

.data-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    margin-bottom: 1.5rem;
}

.data-card h4 {
    margin: 0 0 1rem 0;
    font-size: 1.1rem;
    font-weight: 600;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.data-card h4 i {
    color: #3182ce;
}

.loading-state {
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

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}

.stats-content {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.summary-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 1rem;
}

.stat-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.25rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s ease;
}

.stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
}

.stat-icon {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
    color: white;
}

.total-orders .stat-icon {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.total-revenue .stat-icon {
    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.avg-order-value .stat-icon {
    background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}

.completion-rate .stat-icon {
    background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
}

.stat-info {
    flex: 1;
}

.stat-info h4 {
    margin: 0 0 0.25rem 0;
    font-size: 0.9rem;
    color: #666;
    font-weight: 500;
}

.stat-value {
    font-size: 1.5rem;
    font-weight: 700;
    color: #333;
    margin-bottom: 0.25rem;
    line-height: 1.2;
}

.stat-change {
    font-size: 0.8rem;
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.stat-change.positive {
    color: #38a169;
}

.stat-change.negative {
    color: #e53e3e;
}

.status-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 1rem;
}

.status-item {
    background: #f8f9fa;
    border-radius: 8px;
    padding: 1rem;
    display: flex;
    align-items: center;
    gap: 0.75rem;
    border-left: 4px solid #ddd;
    transition: all 0.2s;
}

.status-item:hover {
    background: #f1f3f4;
}

.status-item.pending,
.status-item.choxuly,
.status-item.chờxửlý {
    border-left-color: #ffc107;
}

.status-item.confirmed,
.status-item.daxacnhan,
.status-item.đãxácnhận {
    border-left-color: #17a2b8;
}

.status-item.processing,
.status-item.dangxuly,
.status-item.đangxửlý {
    border-left-color: #007bff;
}

.status-item.shipping,
.status-item.danggiao,
.status-item.đanggiao {
    border-left-color: #fd7e14;
}

.status-item.delivered,
.status-item.dagiao,
.status-item.đãgiao,
.status-item.hoanthanh,
.status-item.hoànthành {
    border-left-color: #28a745;
}

.status-item.cancelled,
.status-item.dahuy,
.status-item.đãhủy {
    border-left-color: #dc3545;
}

.status-item.returned,
.status-item.datra,
.status-item.đãtrả {
    border-left-color: #6c757d;
}

/* Responsive Design */
@media (max-width: 768px) {
    .section-header {
        flex-direction: column;
        align-items: stretch;
        text-align: center;
    }

    .summary-grid {
        grid-template-columns: 1fr;
    }

    .stat-card {
        padding: 1rem;
    }

    .stat-value {
        font-size: 1.25rem;
    }

    .status-grid {
        grid-template-columns: 1fr;
    }

    .payment-item {
        flex-direction: column;
        align-items: stretch;
        gap: 0.75rem;
    }

    .payment-stats {
        justify-content: space-between;
    }

    .chart-header,
    .chart-controls,
    .chart-container {
        display: none;
    }
}

@media (max-width: 480px) {
    .data-card {
        padding: 1rem;
    }

    .summary-grid {
        gap: 0.75rem;
    }

    .chart-controls,
    .chart-display-options,
    .chart-type-selector,
    .chart-control-select,
    .chart-container,
    .chart-legend {
        display: none;
    }
}
</style>
