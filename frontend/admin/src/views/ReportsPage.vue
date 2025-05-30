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

        <div class="report-container">
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

            <div class="reports-grid">
                <!-- Best Selling Products Section -->
                <BestSellingProducts
                    :products="bestSellingProducts"
                    :loading="loading.bestSelling"
                    :timeFilter="timeFilter"
                    :startDate="startDate"
                    :endDate="endDate"
                />

                <!-- Revenue Over Time Section -->
                <RevenueOverTime
                    :revenueData="revenueData"
                    :loading="loading.revenue"
                    :timeFilter="timeFilter"
                />

                <!-- Frequent Customers Section -->
                <FrequentCustomers
                    :customers="frequentCustomers"
                    :loading="loading.customers"
                    :timeFilter="timeFilter"
                />
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import BestSellingProducts from "../components/reports/BestSellingProducts.vue";
import RevenueOverTime from "../components/reports/RevenueOverTime.vue";
import FrequentCustomers from "../components/reports/FrequentCustomers.vue";
import { getCurrentDate, getLastMonthDate } from "../utils/dateTimeUtils.js";
import emitter from "../utils/evenBus.js";
import dashboardService from "../services/dashboardService.js";

// State
const timeFilter = ref("month");
const startDate = ref(getLastMonthDate());
const endDate = ref(getCurrentDate());
const loading = ref({
    bestSelling: true,
    revenue: true,
    customers: true,
});

// Data
const bestSellingProducts = ref([]);

const revenueData = ref({
    labels: [],
    datasets: [
        {
            data: [],
        },
    ],
    totals: {
        revenue: 0,
        orders: 0,
        avgOrderValue: 0,
    },
});

const frequentCustomers = ref([]);

const router = useRouter();

// Methods
/**
 * Làm mới dữ liệu báo cáo
 */
const refreshData = async () => {
    try {
        // Đặt tất cả trạng thái loading thành true
        loading.value = {
            bestSelling: true,
            revenue: true,
            customers: true,
        };

        // Giả lập gọi API - Trong thực tế, đây sẽ là các lời gọi đến backend API
        await Promise.all([
            fetchBestSellingProducts(),
            fetchRevenueData(),
            fetchFrequentCustomers(),
        ]);

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
            revenue: true,
            customers: true,
        };

        // Gọi API với các tham số lọc
        await Promise.all([
            fetchBestSellingProducts(),
            fetchRevenueData(),
            fetchFrequentCustomers(),
        ]);
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
        );

        // Gọi API với parameters
        const data = await dashboardService.getTopProducts({
            startDate: dateRange.startDate,
            endDate: dateRange.endDate,
            limit: 5, // Lấy top 5 sản phẩm
        });

        // Transform data to match component format
        bestSellingProducts.value = data.map((product) => ({
            id: product.productId,
            name: product.productName,
            sku: product.sku || `SKU${product.productId}`, // Fallback nếu không có SKU
            quantity: product.quantitySold,
            revenue: product.totalRevenue,
            firstSoldDate:
                product.firstSoldDate || new Date().toISOString().split("T")[0],
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
 * Lấy dữ liệu doanh thu từ API
 */
const fetchRevenueData = async () => {
    try {
        loading.value.revenue = true;

        // Lấy date range và period từ timeFilter
        const dateRange = dashboardService.getDateRange(
            timeFilter.value,
            startDate.value,
            endDate.value
        );
        const period = dashboardService.getPeriodFromTimeFilter(
            timeFilter.value
        );

        // Gọi API với parameters
        const data = await dashboardService.getRevenue({
            startDate: dateRange.startDate,
            endDate: dateRange.endDate,
            period: period,
        });

        // Transform data to match component format
        const labels = data.map((item) => {
            const date = new Date(item.date);
            if (period === "daily") {
                return `${date.getDate().toString().padStart(2, "0")}/${(
                    date.getMonth() + 1
                )
                    .toString()
                    .padStart(2, "0")}`;
            } else if (period === "weekly") {
                return `Tuần ${Math.ceil(date.getDate() / 7)}`;
            } else if (period === "monthly") {
                return `${date.getMonth() + 1}/${date.getFullYear()}`;
            }
            return item.date;
        });

        const revenueValues = data.map((item) => item.totalRevenue);
        const totalRevenue = revenueValues.reduce(
            (sum, value) => sum + value,
            0
        );
        const totalOrders = data.reduce(
            (sum, item) => sum + (item.orderCount || 0),
            0
        );
        const avgOrderValue =
            totalOrders > 0 ? Math.round(totalRevenue / totalOrders) : 0;

        revenueData.value = {
            labels: labels,
            datasets: [
                {
                    data: revenueValues,
                },
            ],
            totals: {
                revenue: totalRevenue,
                orders: totalOrders,
                avgOrderValue: avgOrderValue,
            },
        };

        loading.value.revenue = false;
    } catch (error) {
        console.error("Lấy dữ liệu doanh thu thất bại:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải dữ liệu doanh thu",
        });
        loading.value.revenue = false;
    }
};

/**
 * Lấy dữ liệu khách hàng thân thiết từ API
 */
const fetchFrequentCustomers = async () => {
    try {
        loading.value.customers = true;

        // Lấy date range từ timeFilter
        const dateRange = dashboardService.getDateRange(
            timeFilter.value,
            startDate.value,
            endDate.value
        );

        // Gọi API với parameters
        const data = await dashboardService.getFrequentCustomers({
            startDate: dateRange.startDate,
            endDate: dateRange.endDate,
            limit: 5, // Lấy top 5 khách hàng
        });

        // Transform data to match component format
        frequentCustomers.value = data.map((customer) => ({
            id: customer.id,
            name:
                customer.fullName ||
                customer.name ||
                `Khách hàng ${customer.id}`,
            email: customer.email || "",
            phone: customer.phoneNumber || customer.phone || "",
            orderCount: customer.orderCount || 0,
            totalSpent: customer.totalSpent || 0,
            lastOrderDate: customer.lastOrderDate
                ? new Date(customer.lastOrderDate).toISOString().split("T")[0]
                : new Date().toISOString().split("T")[0],
        }));

        loading.value.customers = false;
    } catch (error) {
        console.error("Lấy dữ liệu khách hàng thất bại:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải dữ liệu khách hàng",
        });
        loading.value.customers = false;
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
    background: linear-gradient(to right, #ffffff, #f8f8f8);
    color: #666;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.refresh-btn i {
    font-size: 0.9rem;
    color: var(--primary-color);
}

.refresh-btn:hover {
    background: linear-gradient(to right, #f8f0f4, #ffeef8);
    border-color: var(--primary-color);
    color: var(--primary-color);
    box-shadow: 0 3px 6px rgba(248, 110, 211, 0.15);
    transform: translateY(-1px);
}

.report-container {
    background-color: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.report-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.report-header p {
    color: #666;
    margin: 0;
}

.header-actions {
    display: flex;
    justify-content: flex-end;
}

.date-filter {
    background-color: white;
    padding: 1.5rem;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    margin-bottom: 2rem;
    display: flex;
    flex-wrap: wrap;
    gap: 1.5rem;
    align-items: center;
}

.filter-group {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.filter-group label {
    font-weight: 500;
    color: #444;
    min-width: 80px;
}

.date-filter select {
    padding: 0.5rem 1rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    background-color: white;
    min-width: 150px;
    font-size: 0.95rem;
    color: #444;
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
    color: #444;
}

.date-input-group input {
    padding: 0.5rem 1rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    background-color: white;
    font-size: 0.95rem;
    color: #444;
}

.reports-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 1.5rem;
}

@media (min-width: 768px) {
    .reports-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (min-width: 1280px) {
    .reports-grid > :first-child {
        grid-column: span 2;
    }
}
</style>
