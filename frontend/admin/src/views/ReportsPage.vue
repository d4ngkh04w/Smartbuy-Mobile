<template>
    <div class="reports-container">
        <div class="page-header">
            <div class="header-top">
                <button class="back-button" @click="goBack">
                    <i class="fas fa-arrow-left"></i> Quay lại Dashboard
                </button>
                <h1>Báo cáo & Thống kê</h1>
                <button class="refresh-btn" @click="refreshData">
                    <i class="fas fa-sync-alt"></i> Làm mới dữ liệu
                </button>
            </div>
        </div>
        <!-- Date Filter (Common for all tabs) -->
        <div class="date-filter">
            <div class="filter-group">
                <label>Thời gian:</label>
                <select v-model="timeFilter" @change="applyFilters">
                    <option value="today">Hôm nay</option>
                    <option value="week">Tuần này</option>
                    <option value="month">Tháng này</option>
                    <option value="quarter">Quý này</option>
                    <option value="year">Năm nay</option>
                    <option value="custom">Tùy chỉnh</option>
                </select>
            </div>

            <div class="current-period">
                <i class="fas fa-calendar-alt"></i>
                {{ formattedTimePeriod }}
            </div>

            <div v-if="timeFilter === 'custom'" class="custom-date-range">
                <div class="date-input-group">
                    <label>Từ ngày:</label>
                    <input
                        type="date"
                        v-model="startDate"
                        @change="applyFilters"
                    />
                </div>
                <div class="date-input-group">
                    <label>Đến ngày:</label>
                    <input
                        type="date"
                        v-model="endDate"
                        @change="applyFilters"
                    />
                </div>
            </div>
        </div>
        <!-- Tabs for different report sections -->
        <div class="report-tabs">
            <button
                @click="activeTab = 'statistics'"
                :class="[
                    'tab-btn',
                    activeTab === 'statistics' ? 'active-tab' : '',
                ]"
            >
                <i class="fas fa-chart-bar"></i> Thống kê đơn hàng
            </button>
            <button
                @click="activeTab = 'products'"
                :class="[
                    'tab-btn',
                    activeTab === 'products' ? 'active-tab' : '',
                ]"
            >
                <i class="fas fa-trophy"></i> Sản phẩm bán chạy
            </button>
        </div>
        <!-- Content based on active tab -->
        <div class="tab-content">
            <transition name="fade" mode="out-in">
                <!-- Order Statistics Tab -->
                <div
                    v-if="activeTab === 'statistics'"
                    class="statistics-content"
                >
                    <OrderStatistics
                        :orderStats="orderStats"
                        :orderStatusDistribution="orderStatusDistribution"
                        :paymentMethods="paymentMethods"
                        :loading="loading.orderStats"
                        :timeFilter="timeFilter"
                        :formattedTimePeriod="formattedTimePeriod"
                    />
                </div>

                <!-- Best Selling Products Tab -->
                <div
                    v-else-if="activeTab === 'products'"
                    class="products-content"
                >
                    <BestSellingProducts
                        :products="bestSellingProducts"
                        :loading="loading.bestSelling"
                        :timeFilter="timeFilter"
                        :formattedTimePeriod="formattedTimePeriod"
                    />
                </div>
            </transition>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import BestSellingProducts from "../components/reports/BestSellingProducts.vue";
import OrderStatistics from "../components/reports/OrderStatistics.vue";
import {
    getCurrentDate,
    getLastMonthDate,
    getFormattedTimePeriod,
} from "../utils/dateTimeUtils.js";
import emitter from "../utils/evenBus.js";
import dashboardService from "../services/dashboardService.js";

// Tab State
const activeTab = ref("statistics");

// State
const timeFilter = ref("month");
const startDate = ref(getLastMonthDate());
const endDate = ref(getCurrentDate());
const loading = ref({
    bestSelling: true,
    orderStats: true,
});

// Data
const bestSellingProducts = ref([]);

// Order Statistics Data
const orderStats = ref({
    totalOrders: 0,
    totalRevenue: 0,
    avgOrderValue: 0,
    completionRate: 0,
    orderChange: 0,
    revenueChange: 0,
    avgChange: 0,
    completionChange: 0,
});

const orderStatusDistribution = ref([]);
const paymentMethods = ref([]);

const router = useRouter();

// Computed
const formattedTimePeriod = computed(() => {
    return getFormattedTimePeriod(
        timeFilter.value,
        startDate.value,
        endDate.value
    );
});

// Methods
/**
 * Làm mới dữ liệu báo cáo
 */
const refreshData = async () => {
    try {
        // Đặt tất cả trạng thái loading thành true
        loading.value = {
            bestSelling: true,
            orderStats: true,
        };

        // Giả lập gọi API - Trong thực tế, đây sẽ là các lời gọi đến backend API
        await Promise.all([fetchBestSellingProducts(), fetchOrderStatistics()]);

        // Thông báo khi làm mới thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Dữ liệu báo cáo đã được cập nhật",
        });
    } catch (error) {
        console.error("Làm mới dữ liệu thất bại:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể cập nhật dữ liệu báo cáo",
        });
    }
};

/**
 * Áp dụng bộ lọc thời gian cho báo cáo
 */
const applyFilters = async () => {
    try {
        loading.value = {
            bestSelling: true,
            orderStats: true,
        };

        // Gọi API với các tham số lọc
        await Promise.all([fetchBestSellingProducts(), fetchOrderStatistics()]);
    } catch (error) {
        console.error("Áp dụng bộ lọc thất bại:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể áp dụng bộ lọc",
        });
    }
};

/**
 * Lấy dữ liệu sản phẩm bán chạy từ API
 */
const fetchBestSellingProducts = async () => {
    try {
        loading.value.bestSelling = true;

        // Lấy date range từ timeFilter
        const dateRange = dashboardService.getDateRange(
            timeFilter.value,
            startDate.value,
            endDate.value
        ); // Gọi API với parameters
        const data = await dashboardService.getTopProducts({
            startDate: dateRange.startDate,
            endDate: dateRange.endDate,
            limit: 5, // Lấy top 5 sản phẩm
        });

        console.log("Best selling products data:", data);
        bestSellingProducts.value = data.map((product) => ({
            id: product.productId,
            name: product.productName,
            sku: `SKU${product.productId}`, // Generate SKU from product ID
            quantity: product.quantity,
            revenue: product.revenue,
            firstSoldDate:
                product.createdAtFormatted ||
                new Date().toISOString().split("T")[0],
        }));

        loading.value.bestSelling = false;
    } catch (error) {
        console.error("Lấy dữ liệu sản phẩm bán chạy thất bại:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải dữ liệu sản phẩm bán chạy",
        });
        loading.value.bestSelling = false;
    }
};

/**
 * Lấy dữ liệu thống kê đơn hàng
 */
const fetchOrderStatistics = async () => {
    try {
        loading.value.orderStats = true; // Gọi API thống kê đơn hàng
        const dateRange = dashboardService.getDateRange(
            timeFilter.value,
            startDate.value,
            endDate.value
        );
        const params = {
            startDate: dateRange.startDate,
            endDate: dateRange.endDate,
            timeFilter: timeFilter.value, // Thêm timeFilter để backend xác định period
        };

        const data = await dashboardService.getOrderStatistics(params);

        console.log("Dữ liệu API:", data); // Cập nhật dữ liệu từ API - Backend trả về OrderReportDTO
        orderStats.value = data.statistics || {
            totalOrders: 0,
            totalRevenue: 0,
            avgOrderValue: 0,
            completionRate: 0,
            orderChange: 0,
            revenueChange: 0,
            avgChange: 0,
            completionChange: 0,
        };
        orderStatusDistribution.value = data.statusDistribution || [];
        paymentMethods.value = data.paymentMethods || [];

        loading.value.orderStats = false;
    } catch (error) {
        console.error("Lấy dữ liệu thống kê đơn hàng thất bại:", error); // Fallback data nếu API lỗi
        orderStats.value = {
            totalOrders: 0,
            totalRevenue: 0,
            avgOrderValue: 0,
            completionRate: 0,
            orderChange: 0,
            revenueChange: 0,
            avgChange: 0,
            completionChange: 0,
        };
        orderStatusDistribution.value = [];
        paymentMethods.value = [];

        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải dữ liệu thống kê đơn hàng",
        });
        loading.value.orderStats = false;
    }
};

/**
 * Quay lại Dashboard
 */
const goBack = () => {
    router.push("/dashboard");
};

// Khởi tạo dữ liệu khi component được tạo
onMounted(async () => {
    await refreshData();
});
</script>

<style scoped>
.reports-container {
    min-height: 100vh;
    background-color: #f9f9f9;
    padding: 2rem;
}

.page-header {
    display: flex;
    flex-direction: column;
    margin-bottom: 2rem;
    background-color: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.header-top {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
    margin-bottom: 0.5rem;
}

.back-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: #fff;
    color: #666;
    border: 1px solid #e9ecef;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.back-button i {
    color: var(--primary-color);
}

.back-button:hover {
    background-color: #f8f0f4;
    color: var(--primary-color);
}

.page-header h1 {
    margin: 0;
    font-size: 1.8rem;
    font-weight: 600;
    color: #333;
}

.refresh-btn {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: #fff;
    color: #666;
    border: 1px solid #e9ecef;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.refresh-btn i {
    color: var(--primary-color);
}

.refresh-btn:hover {
    background-color: #f8f0f4;
    color: var(--primary-color);
}

.date-filter {
    background-color: white;
    padding: 1.5rem;
    border-radius: 12px;
    margin-bottom: 1.5rem;
    display: flex;
    flex-wrap: wrap;
    gap: 1.5rem;
    align-items: center;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.filter-group {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.filter-group label {
    font-weight: 500;
    color: #495057;
    min-width: 80px;
}

.date-filter select {
    padding: 0.5rem 1rem;
    border: 1px solid #ced4da;
    border-radius: 6px;
    background-color: white;
    min-width: 150px;
    font-size: 0.95rem;
    color: #495057;
    transition: border-color 0.15s ease-in-out;
}

.date-filter select:focus {
    border-color: var(--primary-color);
    outline: 0;
    box-shadow: 0 0 0 0.2rem rgba(248, 110, 211, 0.25);
}

.current-period {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: #f8f0f4;
    border-radius: 6px;
    color: var(--primary-color);
    font-weight: 500;
    border: 1px solid rgba(248, 110, 211, 0.2);
}

.current-period i {
    color: var(--primary-color);
}

.custom-date-range {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
}

.date-input-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.date-input-group label {
    font-weight: 500;
    color: #495057;
}

.date-input-group input {
    padding: 0.5rem 1rem;
    border: 1px solid #ced4da;
    border-radius: 6px;
    background-color: white;
    font-size: 0.95rem;
    color: #495057;
    transition: border-color 0.15s ease-in-out;
}

.date-input-group input:focus {
    border-color: var(--primary-color);
    outline: 0;
    box-shadow: 0 0 0 0.2rem rgba(248, 110, 211, 0.25);
}

/* Tab Styles */
.report-tabs {
    display: flex;
    border-bottom: 1px solid #e2e8f0;
    margin-bottom: 1.5rem;
    background-color: white;
    border-radius: 12px 12px 0 0;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
    overflow: hidden;
}

.tab-btn {
    padding: 1rem 1.5rem;
    background-color: white;
    border: none;
    cursor: pointer;
    color: #666;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s;
    position: relative;
    flex: 1;
    justify-content: center;
}

.tab-btn i {
    font-size: 1.2rem;
}

.tab-btn:hover {
    color: var(--primary-color);
    background-color: #fff5fc;
}

.active-tab {
    color: var(--primary-color);
    border-bottom: 3px solid var(--primary-color);
    font-weight: 600;
}

.tab-content {
    background-color: white;
    border-radius: 0 0 12px 12px;
    padding: 1.5rem;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    min-height: 500px;
}

/* Content Styles */
.statistics-content,
.revenue-content,
.products-content {
    width: 100%;
}

/* Fade Transition */
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}

/* Responsive Design */
@media (max-width: 768px) {
    .reports-container {
        padding: 1rem;
    }

    .page-header {
        padding: 1.5rem;
    }

    .header-top {
        flex-direction: column;
        gap: 1rem;
        align-items: stretch;
    }

    .date-filter {
        padding: 1rem;
        flex-direction: column;
        align-items: stretch;
    }

    .filter-group {
        flex-direction: column;
        align-items: stretch;
        gap: 0.5rem;
    }

    .filter-group label {
        min-width: auto;
    }

    .custom-date-range {
        flex-direction: column;
    }

    .date-input-group {
        flex-direction: column;
        align-items: stretch;
        gap: 0.5rem;
    }

    .report-tabs {
        flex-direction: column;
    }

    .tab-btn {
        justify-content: flex-start;
        padding: 1rem;
    }

    .tab-content {
        padding: 1rem;
    }
}

@media (max-width: 480px) {
    .reports-container {
        padding: 0.5rem;
    }

    .page-header {
        padding: 1rem;
    }

    .page-header h1 {
        font-size: 1.5rem;
    }

    .back-button,
    .refresh-btn {
        padding: 0.4rem 0.8rem;
        font-size: 0.9rem;
    }

    .date-filter {
        padding: 0.75rem;
    }

    .tab-content {
        padding: 0.75rem;
    }
}
</style>
