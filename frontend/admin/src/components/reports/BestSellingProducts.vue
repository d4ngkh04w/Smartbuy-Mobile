<template>
    <div class="stats-card bestselling-products">
        <div class="card-header">
            <h2>Sản phẩm bán chạy</h2>
            <div class="card-actions">
                <div class="filter-group">
                    <label>Sắp xếp theo:</label>
                    <select v-model="sortBy" @change="sortProducts">
                        <option value="quantity">Số lượng bán</option>
                        <option value="revenue">Doanh thu</option>
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
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>STT</th>
                            <th>Tên sản phẩm</th>
                            <th>Mã SP</th>
                            <th>Số lượng bán</th>
                            <th>Doanh thu</th>
                            <th>Ngày bắt đầu bán</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="(product, index) in sortedProducts"
                            :key="product.id"
                        >
                            <td>{{ index + 1 }}</td>
                            <td>{{ product.name }}</td>
                            <td>{{ product.sku }}</td>
                            <td>{{ product.quantity }}</td>
                            <td>{{ formatCurrency(product.revenue) }}</td>
                            <td>{{ formatDate(product.firstSoldDate) }}</td>
                        </tr>
                    </tbody>
                </table>

                <div class="empty-state" v-if="sortedProducts.length === 0">
                    <p>Không có dữ liệu sản phẩm trong khoảng thời gian này</p>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch } from "vue";

const props = defineProps({
    products: {
        type: Array,
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
    startDate: {
        type: String,
        default: "",
    },
    endDate: {
        type: String,
        default: "",
    },
});

const sortBy = ref("quantity");

const sortedProducts = computed(() => {
    if (!props.products || props.products.length === 0) return [];

    return [...props.products].sort((a, b) => {
        if (sortBy.value === "quantity") {
            return b.quantity - a.quantity;
        } else if (sortBy.value === "revenue") {
            return b.revenue - a.revenue;
        }
        return 0;
    });
});

function sortProducts() {
    // Sorting is handled by the computed property
}

function formatCurrency(value) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        maximumFractionDigits: 0,
    }).format(value);
}

function formatDate(dateString) {
    if (!dateString) return "";

    const date = new Date(dateString);
    return new Intl.DateTimeFormat("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
    }).format(date);
}

// Watch for changes in props to update if needed
watch(
    () => props.timeFilter,
    () => {
        // In a real implementation, you would fetch new data here
        // based on the time filter changes
    }
);
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

.empty-state {
    text-align: center;
    padding: 2rem 0;
    color: #666;
}
</style>
