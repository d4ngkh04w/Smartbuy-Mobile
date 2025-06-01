<template>
    <div class="status-change-row">
        <div :class="['status-item', getStatusClass(fromStatus)]">
            <i :class="getStatusIcon(fromStatus)"></i>
            {{ fromStatus }}
        </div>
        <div class="arrow">
            <i class="fas fa-arrow-right"></i>
        </div>
        <div :class="['status-item', getStatusClass(toStatus)]">
            <i :class="getStatusIcon(toStatus)"></i>
            {{ toStatus }}
        </div>
    </div>
</template>

<script setup>
import { defineProps } from "vue";
import orderService from "@/services/orderService";

const props = defineProps({
    fromStatus: {
        type: String,
        required: true,
    },
    toStatus: {
        type: String,
        required: true,
    },
});

const getStatusClass = (status) => {
    return orderService.getStatusClass(status);
};

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
</script>

<style scoped>
/* The component will use the same CSS defined in OrderManagement.vue */
/* We're importing this component to be used across the application */
</style>
