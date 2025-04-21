<template>
    <div class="reports-container">
        <header class="page-header">
            <h1>Báo cáo & Thống kê</h1>
            <div class="header-actions">
                <button class="refresh-btn" @click="refreshData">
                    <i class="fas fa-sync-alt"></i> Làm mới dữ liệu
                </button>
            </div>
        </header>

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
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import BestSellingProducts from "../components/reports/BestSellingProducts.vue";
import RevenueOverTime from "../components/reports/RevenueOverTime.vue";
import FrequentCustomers from "../components/reports/FrequentCustomers.vue";

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
    ],
    datasets: [
        {
            label: "Doanh thu (triệu VNĐ)",
            data: [18, 25, 12, 30, 22, 28, 15, 35, 27, 32],
            backgroundColor: "rgba(255, 99, 132, 0.2)",
            borderColor: "rgba(255, 99, 132, 1)",
            borderWidth: 2,
            tension: 0.4,
            fill: true,
        },
    ],
});

const frequentCustomers = ref([
    // Mẫu dữ liệu - sẽ được thay thế bằng API thực tế
    {
        id: 1,
        name: "Nguyễn Văn A",
        email: "nguyenvana@gmail.com",
        orderCount: 12,
        totalSpent: 45000000,
        memberSince: "2024-01-15",
    },
    {
        id: 2,
        name: "Trần Thị B",
        email: "tranthib@gmail.com",
        orderCount: 10,
        totalSpent: 38500000,
        memberSince: "2024-02-20",
    },
    {
        id: 3,
        name: "Lê Minh C",
        email: "leminhc@gmail.com",
        orderCount: 8,
        totalSpent: 32000000,
        memberSince: "2024-03-10",
    },
    {
        id: 4,
        name: "Phạm Thanh D",
        email: "phamthanhd@gmail.com",
        orderCount: 7,
        totalSpent: 28500000,
        memberSince: "2024-01-05",
    },
    {
        id: 5,
        name: "Hoàng Văn E",
        email: "hoangvane@gmail.com",
        orderCount: 6,
        totalSpent: 25000000,
        memberSince: "2024-04-01",
    },
]);

// Methods
function refreshData() {
    loading.value = {
        bestSelling: true,
        revenue: true,
        customers: true,
    };

    // Giả lập tải dữ liệu từ API
    setTimeout(() => {
        // Lấy dữ liệu mới từ API (trong thực tế)
        loading.value.bestSelling = false;
        loading.value.revenue = false;
        loading.value.customers = false;
    }, 1000);
}

function applyFilters() {
    loading.value = {
        bestSelling: true,
        revenue: true,
        customers: true,
    };

    // Thực hiện lấy dữ liệu theo bộ lọc mới (trong thực tế)
    setTimeout(() => {
        // Dữ liệu đã được lọc
        loading.value.bestSelling = false;
        loading.value.revenue = false;
        loading.value.customers = false;
    }, 800);
}

function getCurrentDate() {
    const date = new Date();
    return date.toISOString().split("T")[0];
}

function getLastMonthDate() {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().split("T")[0];
}

onMounted(() => {
    refreshData();
});
</script>

<style scoped>
.reports-container {
    padding: 2rem;
    background-color: #f9f9f9;
    min-height: 100vh;
}

.page-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
}

.page-header h1 {
    font-size: 1.8rem;
    color: #333;
    margin: 0;
}

.header-actions {
    display: flex;
    gap: 1rem;
}

.refresh-btn {
    background-color: #f5f5f5;
    color: #555;
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 0.5rem 1rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.3s;
}

.refresh-btn:hover {
    background-color: #f8d7e3;
    color: var(--primary-color);
    border-color: var(--primary-color);
}

.date-filter {
    background-color: white;
    border-radius: 8px;
    padding: 1rem;
    margin-bottom: 2rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    display: flex;
    flex-wrap: wrap;
    gap: 1.5rem;
    align-items: center;
}

.filter-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.filter-group label {
    font-weight: 500;
    color: #555;
}

.filter-group select {
    padding: 0.5rem;
    border-radius: 4px;
    border: 1px solid #ddd;
    background-color: white;
}

.custom-date-range {
    display: flex;
    gap: 1rem;
}

.date-input-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.date-input-group label {
    font-weight: 500;
    color: #555;
}

.date-input-group input {
    padding: 0.5rem;
    border-radius: 4px;
    border: 1px solid #ddd;
}

.reports-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 2rem;
}

@media (min-width: 1200px) {
    .reports-grid {
        grid-template-columns: 1fr 1fr;
    }
}
</style>
