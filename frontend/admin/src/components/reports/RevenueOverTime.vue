<template>
    <div class="stats-card revenue-chart">
        <div class="card-header">
            <h2>Doanh thu theo thời gian</h2>
            <div class="card-actions">
                <div class="filter-group">
                    <label>Hiển thị:</label>
                    <select v-model="chartType">
                        <option value="line">Biểu đồ đường</option>
                        <option value="bar">Biểu đồ cột</option>
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
                <div class="chart-container">
                    <Line
                        v-if="chartType === 'line'"
                        :data="chartData"
                        :options="chartOptions"
                    />
                    <Bar v-else :data="chartData" :options="chartOptions" />
                </div>

                <div class="revenue-data-table">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Ngày</th>
                                <th>Số đơn</th>
                                <th>Doanh thu</th>
                                <th>Ghi chú</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr
                                v-for="(item, index) in revenueTableData"
                                :key="index"
                            >
                                <td>{{ item.date }}</td>
                                <td>{{ item.orderCount }}</td>
                                <td>{{ formatCurrency(item.revenue) }}</td>
                                <td>{{ item.note || "-" }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { Line, Bar } from "vue-chartjs";
import {
    Chart as ChartJS,
    Title,
    Tooltip,
    Legend,
    LineElement,
    LinearScale,
    CategoryScale,
    PointElement,
    BarElement,
} from "chart.js";

// Register ChartJS components
ChartJS.register(
    Title,
    Tooltip,
    Legend,
    LineElement,
    LinearScale,
    CategoryScale,
    PointElement,
    BarElement
);

const props = defineProps({
    revenueData: {
        type: Object,
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

const chartType = ref("line");

// Sample data for the revenue table
const revenueTableData = ref([
    { date: "01/04/2025", orderCount: 25, revenue: 12000000, note: "-" },
    {
        date: "02/04/2025",
        orderCount: 40,
        revenue: 18200000,
        note: "Có flash sale",
    },
    { date: "03/04/2025", orderCount: 30, revenue: 15500000, note: "-" },
    { date: "04/04/2025", orderCount: 22, revenue: 10800000, note: "-" },
    {
        date: "05/04/2025",
        orderCount: 35,
        revenue: 16900000,
        note: "Khuyến mãi",
    },
    { date: "06/04/2025", orderCount: 28, revenue: 14200000, note: "-" },
    {
        date: "07/04/2025",
        orderCount: 45,
        revenue: 21500000,
        note: "Cuối tuần",
    },
]);

const chartData = computed(() => {
    return {
        labels: props.revenueData.labels,
        datasets: props.revenueData.datasets,
    };
});

const chartOptions = computed(() => {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: "top",
            },
            tooltip: {
                callbacks: {
                    label: function (context) {
                        let label = context.dataset.label || "";
                        if (label) {
                            label += ": ";
                        }
                        if (context.parsed.y !== null) {
                            label += formatCurrency(context.parsed.y * 1000000);
                        }
                        return label;
                    },
                },
            },
        },
        scales: {
            y: {
                beginAtZero: true,
                ticks: {
                    callback: function (value) {
                        return value + " triệu";
                    },
                },
            },
        },
    };
});

function formatCurrency(value) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        maximumFractionDigits: 0,
    }).format(value);
}

onMounted(() => {
    // If you need to do anything when the component is mounted
});
</script>

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

.chart-container {
    height: 300px;
    margin-bottom: 2rem;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
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
</style>
