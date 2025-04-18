<script setup>
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/authStore";

const router = useRouter();
const authStore = useAuthStore();

onMounted(() => {
    // Kiểm tra xem người dùng đã đăng nhập hay chưa
    if (!authStore.isLoggedIn()) {
        router.push("/login");
    }
});
</script>

<template>
    <div class="dashboard-container">
        <header class="dashboard-header">
            <h1>SmartBuy Admin Dashboard</h1>
            <button
                class="logout-button"
                @click="
                    authStore.logout();
                    router.push('/login');
                "
            >
                <i class="fas fa-sign-out-alt"></i> Đăng xuất
            </button>
        </header>

        <div class="dashboard-content">
            <div class="welcome-message">
                <h2>
                    Chào mừng trở lại, {{ authStore.admin?.name || "Admin" }}!
                </h2>
                <p>Đây là trang quản trị hệ thống SmartBuy Mobile.</p>
            </div>

            <div class="dashboard-cards">
                <div class="dashboard-card">
                    <div class="card-icon">
                        <i class="fas fa-box"></i>
                    </div>
                    <div class="card-content">
                        <h3>Sản phẩm</h3>
                        <p>Quản lý thông tin sản phẩm</p>
                    </div>
                </div>

                <div class="dashboard-card">
                    <div class="card-icon">
                        <i class="fas fa-tags"></i>
                    </div>
                    <div class="card-content">
                        <h3>Danh mục</h3>
                        <p>Quản lý danh mục sản phẩm</p>
                    </div>
                </div>

                <div class="dashboard-card">
                    <div class="card-icon">
                        <i class="fas fa-trademark"></i>
                    </div>
                    <div class="card-content">
                        <h3>Thương hiệu</h3>
                        <p>Quản lý thương hiệu sản phẩm</p>
                    </div>
                </div>

                <div class="dashboard-card">
                    <div class="card-icon">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="card-content">
                        <h3>Người dùng</h3>
                        <p>Quản lý tài khoản người dùng</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.dashboard-container {
    min-height: 100vh;
    background-color: #f5f5f5;
}

.dashboard-header {
    background-color: #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    padding: 1rem 2rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.logout-button {
    padding: 0.5rem 1rem;
    background-color: #dc3545;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: background-color 0.3s;
}

.logout-button:hover {
    background-color: #c82333;
}

.dashboard-content {
    padding: 2rem;
}

.welcome-message {
    background-color: white;
    padding: 2rem;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    margin-bottom: 2rem;
}

.welcome-message h2 {
    margin-top: 0;
    color: #333;
}

.welcome-message p {
    color: #666;
}

.dashboard-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1.5rem;
}

.dashboard-card {
    background-color: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    padding: 1.5rem;
    display: flex;
    align-items: center;
    transition: transform 0.3s, box-shadow 0.3s;
    cursor: pointer;
}

.dashboard-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.card-icon {
    background-color: #f0f7ff;
    color: #3498db;
    width: 48px;
    height: 48px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    margin-right: 1rem;
}

.card-content h3 {
    margin: 0 0 0.5rem 0;
    color: #333;
}

.card-content p {
    margin: 0;
    color: #666;
    font-size: 0.9rem;
}
</style>
