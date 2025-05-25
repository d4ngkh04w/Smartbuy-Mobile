<script setup>
import { defineEmits } from "vue";

const props = defineProps({
    searchQuery: {
        type: String,
        required: true,
    },
    statusFilter: {
        type: String,
        required: true,
        default: "all",
    },
});

const emit = defineEmits([
    "update:searchQuery",
    "add-product",
    "update:statusFilter",
]);

const updateSearchQuery = (e) => {
    emit("update:searchQuery", e.target.value);
};

const updateStatusFilter = (e) => {
    emit("update:statusFilter", e.target.value);
};
</script>

<template>
    <div class="section-header">
        <div class="left-section">
            <h2><i class="fas fa-mobile-alt"></i> Quản lý Sản phẩm</h2>
            <p>Quản lý sản phẩm điện thoại di động</p>
        </div>
        <div class="right-section">
            <div class="search-box">
                <i class="fas fa-search"></i>
                <input
                    type="text"
                    :value="searchQuery"
                    @input="updateSearchQuery"
                    placeholder="Tìm kiếm sản phẩm..."
                />
            </div>
            <div class="status-filter">
                <i class="fas fa-filter"></i>
                <select :value="statusFilter" @change="updateStatusFilter">
                    <option value="all">Tất cả trạng thái</option>
                    <option value="active">Đang bán</option>
                    <option value="inactive">Ngừng bán</option>
                </select>
            </div>
            <button
                class="add-button"
                @click="$emit('add-product')"
                title="Thêm sản phẩm mới"
            >
                <i class="fas fa-plus"></i>
                <span>Thêm sản phẩm</span>
            </button>
        </div>
    </div>
</template>

<style scoped>
/* Section Header Styling */
.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
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
    gap: 1rem;
    align-items: center;
}

.filter-box {
    display: flex;
    gap: 0.5rem;
}

.select-filter select {
    padding: 0.6rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    background-color: white;
    min-width: 180px;
    cursor: pointer;
    transition: all 0.3s;
}

.select-filter select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

/* Search Box Styling */
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

/* Add Button Styling */
.add-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.6rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s;
}

.add-button:hover {
    background-color: #e94e9c;
}

/* Status Filter Styling */
.status-filter {
    position: relative;
    width: 200px;
}

.status-filter i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.status-filter select {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    background-color: white;
    cursor: pointer;
    transition: all 0.3s;
    appearance: none;
}

.status-filter select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}
</style>
