<script setup>
const props = defineProps({
    totalProducts: {
        type: Number,
        required: true,
    },
    activeProducts: {
        type: Number,
        required: true,
    },
    totalValue: {
        type: Number,
        required: true,
    },
});

// Format currency function
const formatCurrency = (value) => {
    if (!value) return "0 ₫";

    // Convert to shorter format for large numbers
    if (value >= 1000000000) {
        return (value / 1000000000).toFixed(1) + "B ₫";
    } else if (value >= 1000000) {
        return (value / 1000000).toFixed(1) + "M ₫";
    } else if (value >= 1000) {
        return (value / 1000).toFixed(1) + "K ₫";
    }

    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(value);
};
</script>

<template>
    <div class="status-cards">
        <div class="status-card">
            <div class="icon-wrapper">
                <i class="fas fa-mobile-alt"></i>
            </div>
            <div class="status-content">
                <h3>{{ totalProducts }}</h3>
                <p>Tổng số sản phẩm</p>
            </div>
        </div>

        <div class="status-card">
            <div class="icon-wrapper">
                <i class="fas fa-check-circle"></i>
            </div>
            <div class="status-content">
                <h3>{{ activeProducts }}</h3>
                <p>Sản phẩm đang bán</p>
            </div>
        </div>

        <div class="status-card">
            <div class="icon-wrapper">
                <i class="fas fa-money-bill-wave"></i>
            </div>
            <div class="status-content">
                <h3>{{ formatCurrency(totalValue) }}</h3>
                <p>Tổng giá trị tồn kho</p>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Status Cards Styling */
.status-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
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
    transition: all 0.3s;
}

.status-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.icon-wrapper {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background-color: #fff5fc;
    color: var(--primary-color);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
}

.status-content h3 {
    font-size: 1.5rem;
    margin: 0 0 0.25rem 0;
    color: #333;
    word-break: break-word;
    line-height: 1.2;
}

.status-content p {
    margin: 0;
    color: #666;
    font-size: 0.85rem;
    line-height: 1.2;
}
</style>
