<template>
    <div class="orders-management">
        <div class="page-header">
            <div class="header-top">
                <button class="back-button" @click="goBack">
                    <i class="fas fa-arrow-left"></i> Quay lại Dashboard
                </button>
                <h1>Quản lý Đơn hàng</h1>
                <button class="refresh-button" @click="refreshData">
                    <i class="fas fa-sync-alt"></i> Làm mới
                </button>
            </div>
        </div>

        <div class="order-content">
            <OrderManagement ref="orderManagement" @refresh="refreshOrders" />
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import OrderManagement from "../components/order/OrderManagement.vue";
import emitter from "../utils/evenBus.js";
// Example of importing the new component:
// import StatusTransition from "../components/order/StatusTransition.vue";

const router = useRouter();
const orderManagement = ref(null);
const loading = ref(false);

function goBack() {
    router.push("/dashboard");
}

// Direct refresh function instead of just emitting an event
async function refreshData() {
    try {
        loading.value = true;
        // Emit refresh event to trigger the component's refresh method
        emitter.emit("refresh-orders");

        // Show notification
        emitter.emit("show-notification", {
            status: "success",
            message: "Dữ liệu đơn hàng đã được cập nhật",
        });
    } catch (error) {
        console.error("Error refreshing data:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể làm mới dữ liệu đơn hàng",
        });
    } finally {
        loading.value = false;
    }
}

function refreshOrders() {
    // This function is now only used for initial data load feedback
    // The refreshData function handles subsequent refreshes
}

onMounted(() => {
    // Listen for the first successful load
    emitter.on("orders-loaded", refreshOrders);
});
</script>

<style scoped>
.orders-management {
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

.refresh-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: white;
    color: #666;
    border: 1px solid #e9ecef;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.refresh-button i {
    color: var(--primary-color);
}

.refresh-button:hover {
    background-color: #f8f0f4;
    color: var(--primary-color);
}

h1 {
    margin: 0;
    font-size: 1.8rem;
    font-weight: 600;
    color: #333;
}

.order-content {
    background-color: white;
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

@media (max-width: 768px) {
    .orders-management {
        padding: 1rem;
    }

    .page-header {
        padding: 1.5rem;
    }

    .header-top {
        flex-direction: column;
        gap: 1rem;
        align-items: flex-start;
    }

    .back-button,
    .refresh-button {
        width: 100%;
    }
}
</style>
