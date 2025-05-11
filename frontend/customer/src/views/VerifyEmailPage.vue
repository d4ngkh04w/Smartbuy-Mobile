<template>
    <div class="verify-email-container">
        <div class="verify-email-card">
            <div class="card-header">
                <img
                    src="@/assets/logo.png"
                    alt="SmartBuy Mobile"
                    class="logo"
                />
                <h1 class="title">Xác thực địa chỉ email</h1>
            </div>

            <div class="card-content">
                <template v-if="loading">
                    <div class="loading-state">
                        <div class="spinner"></div>
                        <p>Đang xác thực email của bạn...</p>
                    </div>
                </template>

                <template v-else-if="verified">
                    <div class="success-state">
                        <div class="icon-container">
                            <i class="fas fa-check-circle"></i>
                        </div>
                        <h2>Xác thực thành công!</h2>
                        <p>
                            Địa chỉ email của bạn đã được xác thực thành công.
                            Bây giờ bạn có thể sử dụng đầy đủ các tính năng của
                            tài khoản.
                        </p>
                        <router-link to="/" class="btn btn-primary">
                            Về trang chủ
                        </router-link>
                    </div>
                </template>

                <template v-else>
                    <div class="error-state">
                        <div class="icon-container error">
                            <i class="fas fa-exclamation-circle"></i>
                        </div>
                        <h2>Xác thực thất bại</h2>
                        <p>{{ errorMessage }}</p>
                        <p>
                            Có thể liên kết xác thực đã hết hạn hoặc không hợp
                            lệ.
                        </p>
                        <div class="action-buttons">
                            <router-link to="/" class="btn btn-secondary">
                                Về trang chủ
                            </router-link>
                            <router-link to="/account" class="btn btn-primary">
                                Đi đến tài khoản
                            </router-link>
                        </div>
                    </div>
                </template>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute } from "vue-router";
import authService from "@/services/authService";

const route = useRoute();
const loading = ref(true);
const verified = ref(false);
const errorMessage = ref("Không thể xác thực email của bạn.");

onMounted(async () => {
    const token = route.query.token;
    const email = route.query.email;

    if (!token || !email) {
        loading.value = false;
        errorMessage.value =
            "Thiếu thông tin xác thực email. Vui lòng kiểm tra liên kết.";
        return;
    }

    try {
        const response = await authService.verifyEmail({ token, email });

        if (response.data && response.data.message) {
            verified.value = true;
        }
    } catch (error) {
        console.error("Error verifying email:", error);
        if (
            error.response &&
            error.response.data &&
            error.response.data.message
        ) {
            errorMessage.value = error.response.data.message;
        }
    } finally {
        loading.value = false;
    }
});
</script>

<style scoped>
.verify-email-container {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f8f0f4;
    padding: 2rem;
}

.verify-email-card {
    background-color: #fff;
    border-radius: 12px;
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 500px;
    overflow: hidden;
}

.card-header {
    padding: 2rem;
    text-align: center;
    background: linear-gradient(135deg, #ff82c4 0%, #ff5db3 100%);
    color: white;
}

.logo {
    height: 60px;
    margin-bottom: 1rem;
}

.title {
    font-size: 1.8rem;
    margin: 0;
    font-weight: 600;
}

.card-content {
    padding: 2.5rem;
}

.loading-state,
.success-state,
.error-state {
    text-align: center;
    padding: 1rem 0;
}

.icon-container {
    font-size: 4rem;
    color: #4caf50;
    margin-bottom: 1.5rem;
}

.icon-container.error {
    color: #f44336;
}

h2 {
    font-size: 1.5rem;
    margin-bottom: 1rem;
    color: #333;
}

p {
    color: #666;
    margin-bottom: 1.5rem;
    line-height: 1.6;
}

.btn {
    display: inline-block;
    padding: 0.8rem 1.5rem;
    border-radius: 50px;
    font-weight: 500;
    text-decoration: none;
    transition: all 0.3s ease;
    cursor: pointer;
    border: none;
}

.btn-primary {
    background: linear-gradient(to right, #ff82c4, #ff5db3);
    color: white;
    box-shadow: 0 4px 10px rgba(255, 93, 179, 0.3);
}

.btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 12px rgba(255, 93, 179, 0.4);
}

.btn-secondary {
    background-color: #f0f0f0;
    color: #333;
}

.btn-secondary:hover {
    background-color: #e0e0e0;
}

.action-buttons {
    display: flex;
    gap: 1rem;
    justify-content: center;
}

.spinner {
    display: inline-block;
    width: 50px;
    height: 50px;
    border: 3px solid rgba(255, 93, 179, 0.2);
    border-radius: 50%;
    border-top-color: #ff5db3;
    animation: spin 1s ease-in-out infinite;
    margin-bottom: 1rem;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

@media (max-width: 576px) {
    .verify-email-container {
        padding: 1rem;
    }

    .card-header {
        padding: 1.5rem;
    }

    .card-content {
        padding: 1.5rem;
    }

    .action-buttons {
        flex-direction: column;
    }

    .btn {
        width: 100%;
        margin-bottom: 0.5rem;
    }
}
</style>
