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
const bestSellingProducts = ref([
    // Mẫu dữ liệu - sẽ được thay thế bằng API thực tế
    {
        id: 1,
        name: "iPhone 15 Pro Max",
        sku: "IP15PM",
        quantity: 89,
        revenue: 267000000,
        firstSoldDate: "2025-01-15",
    },
    {
        id: 2,
        name: "Samsung Galaxy S25 Ultra",
        sku: "SGS25U",
        quantity: 72,
        revenue: 216000000,
        firstSoldDate: "2025-02-01",
    },
    {
        id: 3,
        name: "Xiaomi 14 Pro",
        sku: "XM14P",
        quantity: 65,
        revenue: 130000000,
        firstSoldDate: "2025-01-10",
    },
    {
        id: 4,
        name: "OPPO Find X7 Ultra",
        sku: "OPX7U",
        quantity: 58,
        revenue: 174000000,
        firstSoldDate: "2025-02-15",
    },
    {
        id: 5,
        name: "Google Pixel 9 Pro",
        sku: "GP9P",
        quantity: 45,
        revenue: 126000000,
        firstSoldDate: "2025-03-01",
    },
]);

const revenueData = ref({
    labels: [
        "01/03",
        "02/03",
        "03/03",
        "04/03",
        "05/03",
        "06/03",
        "07/03",
        "08/03",
        "09/03",
        "10/03",
        "11/03",
        "12/03",
        "13/03",
        "14/03",
        "15/03",
    ],
    datasets: [
        {
            data: [
                15000000, 17500000, 14000000, 21000000, 19500000, 23000000,
                18500000, 24500000, 29000000, 27500000, 22000000, 25000000,
                30500000, 28000000, 32000000,
            ],
        },
    ],
    totals: {
        revenue: 347000000,
        orders: 137,
        avgOrderValue: 2532847,
    },
});

const frequentCustomers = ref([
    {
        id: 1,
        name: "Nguyễn Văn A",
        email: "nguyenvana@example.com",
        phone: "0901234567",
        orderCount: 8,
        totalSpent: 45600000,
        lastOrderDate: "2025-03-15",
    },
    {
        id: 2,
        name: "Trần Thị B",
        email: "tranthib@example.com",
        phone: "0912345678",
        orderCount: 6,
        totalSpent: 32400000,
        lastOrderDate: "2025-03-10",
    },
    {
        id: 3,
        name: "Lê Văn C",
        email: "levanc@example.com",
        phone: "0923456789",
        orderCount: 5,
        totalSpent: 27500000,
        lastOrderDate: "2025-03-05",
    },
    {
        id: 4,
        name: "Phạm Thị D",
        email: "phamthid@example.com",
        phone: "0934567890",
        orderCount: 4,
        totalSpent: 19200000,
        lastOrderDate: "2025-03-12",
    },
    {
        id: 5,
        name: "Hoàng Văn E",
        email: "hoangvane@example.com",
        phone: "0945678901",
        orderCount: 4,
        totalSpent: 18800000,
        lastOrderDate: "2025-03-08",
    },
]);

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
        // Giả lập API call
        await new Promise((resolve) => setTimeout(resolve, 800));

        // Khi có API thực, thay thế bằng:
        // const params = {
        //   timeFilter: timeFilter.value,
        //   startDate: timeFilter.value === 'custom' ? startDate.value : null,
        //   endDate: timeFilter.value === 'custom' ? endDate.value : null
        // };
        // const response = await axios.get('/api/reports/best-selling', { params });
        // bestSellingProducts.value = response.data;

        // Kết thúc loading
        loading.value.bestSelling = false;
    } catch (error) {
        console.error("Lấy dữ liệu sản phẩm bán chạy thất bại:", error);
        loading.value.bestSelling = false;
    }
};

/**
 * Lấy dữ liệu doanh thu từ API
 */
const fetchRevenueData = async () => {
    try {
        // Giả lập API call
        await new Promise((resolve) => setTimeout(resolve, 1000));

        // Khi có API thực, thay thế bằng:
        // const params = {
        //   timeFilter: timeFilter.value,
        //   startDate: timeFilter.value === 'custom' ? startDate.value : null,
        //   endDate: timeFilter.value === 'custom' ? endDate.value : null
        // };
        // const response = await axios.get('/api/reports/revenue', { params });
        // revenueData.value = response.data;

        // Kết thúc loading
        loading.value.revenue = false;
    } catch (error) {
        console.error("Lấy dữ liệu doanh thu thất bại:", error);
        loading.value.revenue = false;
    }
};

/**
 * Lấy dữ liệu khách hàng thân thiết từ API
 */
const fetchFrequentCustomers = async () => {
    try {
        // Giả lập API call
        await new Promise((resolve) => setTimeout(resolve, 1200));

        // Khi có API thực, thay thế bằng:
        // const params = {
        //   timeFilter: timeFilter.value,
        //   startDate: timeFilter.value === 'custom' ? startDate.value : null,
        //   endDate: timeFilter.value === 'custom' ? endDate.value : null
        // };
        // const response = await axios.get('/api/reports/customers', { params });
        // frequentCustomers.value = response.data;

        // Kết thúc loading
        loading.value.customers = false;
    } catch (error) {
        console.error("Lấy dữ liệu khách hàng thất bại:", error);
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
