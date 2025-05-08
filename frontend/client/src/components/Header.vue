<script setup>
import { ref, onMounted } from "vue";
import LoginModal from "@/components/LoginModal.vue";
import { getUserInfo, logout, refreshToken } from "../services/authService.js";
import { useAuthStore } from "../stores/authStore.js";
import emitter from "../utils/evenBus.js";
import { useRouter } from "vue-router";

// Lấy store xác thực
const authStore = useAuthStore();
const router = useRouter();

const searchQuery = ref("");
const showLoginModal = ref(false);
const showDropdown = ref(false);

// Xử lý khi đăng nhập thành công
const handleLoginSuccess = async () => {
    try {
        const res = await getUserInfo();

        authStore.setUser(res.data);

        console.log("Lấy thông tin người dùng thành công", res.data);

        showLoginModal.value = false;
    } catch (err) {
        console.error("Lấy thông tin người dùng thất bại", err);
    }
};

const toggleDropdown = () => {
    showDropdown.value = !showDropdown.value;
};

const handleLogout = async () => {
    await logout();
    authStore.logout();
    showDropdown.value = false;

    router.push("/");

    emitter.emit("show-notification", {
        status: "success",
        message: "Đăng xuất thành công",
    });
};

onMounted(async () => {
    // try {
    //     // Try to refresh the token first
    //     await refreshToken();
    //     // Then get user data
    //     const res = await getUserInfo();
    //     authStore.setUser(res.data);
    // } catch (error) {
    //     console.error("Failed to initialize user session:", error);
    //     // User is not logged in or session expired - nothing to do here
    // }
});
</script>

<template>
    <header class="header">
        <div class="header-container">
            <!-- Logo -->
            <router-link to="/" class="logo">
                <img src="@/assets/image/logo.png" alt="Logo" />
            </router-link>

            <!-- Ô tìm kiếm -->
            <div class="search-bar">
                <input
                    type="text"
                    v-model="searchQuery"
                    placeholder="Tìm kiếm..."
                />
                <button>
                    <i class="fa-solid fa-magnifying-glass"></i>
                </button>
            </div>

            <!-- Các icon điều hướng -->
            <div class="header-actions">
                <div class="action-item">
                    <i class="fa-solid fa-cart-shopping"></i>
                    <span>Giỏ hàng</span>
                </div>
                <div class="action-item">
                    <i class="fa-solid fa-bell"></i>
                    <span>Thông báo</span>
                </div>
                <!-- Nếu đã đăng nhập, hiển thị menu tài khoản -->
                <div class="action-item user-menu" v-if="authStore.user">
                    <div class="user-trigger" @click="toggleDropdown">
                        <img
                            :src="
                                authStore.user.avatar
                                    ? `http://localhost:3000${authStore.user.avatar}`
                                    : 'https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg'
                            "
                            alt="Avatar"
                            class="avatar-img"
                        />

                        <span>{{
                            authStore.user.name
                                ? authStore.user.name
                                : authStore.user.phoneNumber
                        }}</span>
                    </div>

                    <div class="dropdown-menu" v-if="showDropdown">
                        <div class="arrow-up"></div>
                        <router-link to="/profile"
                            >Thông tin cá nhân</router-link
                        >
                        <router-link to="/orders">Đơn hàng</router-link>
                        <a @click="handleLogout">Đăng xuất</a>
                    </div>
                </div>

                <div class="action-item" v-else @click="showLoginModal = true">
                    <i class="fa-solid fa-user"></i>
                    <span>Đăng nhập</span>
                </div>
            </div>
            <!-- Modal đăng nhập -->
            <LoginModal
                v-if="showLoginModal"
                @close="showLoginModal = false"
                @login-success="handleLoginSuccess"
            />
        </div>
    </header>
</template>

<style scoped>
/* Bố cục chính */
.header {
    background-color: var(--primary-color);
    padding: 5px 20px;
}

.header-container {
    display: flex;
    align-items: center;
    justify-content: space-between;
    max-width: 100vw;
    margin: 0;
    padding: 0 10px;
}

/* Logo */
.logo img {
    width: 208px;
    height: 59px;
    object-fit: cover;
}

/* Thanh tìm kiếm */
.search-bar {
    display: flex;
    align-items: center;
    background: white;
    border-radius: 20px;
    padding: 5px 10px;
    width: 50%;
    max-width: 600x;
}

.search-bar input {
    border: none;
    outline: none;
    width: 100%;
    padding: 5px;
    font-size: 16px;
}

.search-bar button {
    background: none;
    border: none;
    cursor: pointer;
}

.search-bar img {
    width: 20px;
    height: 20px;
}

/* Các icon điều hướng */
.header-actions {
    display: flex;
    gap: 20px;
}

.action-item {
    display: flex;
    align-items: center;
    gap: 5px;
    cursor: pointer;
    color: black; /* Màu ban đầu */
    transition: color 0.3s ease; /* Thêm transition cho mượt */
}

.action-item:hover {
    color: #ffd9f2; /* Màu khi hover */
}

/* Responsive */
@media (max-width: 960px) {
    .header-container {
        flex-direction: column;
    }

    .search-bar {
        width: 80%;
        margin-bottom: 10px;
    }
    
}
.user-menu {
    position: relative;
    display: inline-block;
}

.user-trigger {
    display: flex;
    align-items: center;
    gap: 6px;
    cursor: pointer;
    color: #fff;
    font-weight: 500;
}

.user-trigger i {
    font-size: 16px;
}

.dropdown-menu {
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: 10px;
    background-color: #fff;
    border: 1px solid #ddd;
    border-radius: 6px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    width: 180px;
    padding: 12px 0;
    z-index: 999;
    display: flex;
    flex-direction: column;
    text-align: left;
}

/* Mũi tên */
.arrow-up {
    position: absolute;
    top: -8px;
    right: 16px;
    width: 0;
    height: 0;
    border-left: 8px solid transparent;
    border-right: 8px solid transparent;
    border-bottom: 8px solid white;
}

.dropdown-menu a,
.dropdown-menu router-link {
    padding: 10px 16px;
    text-decoration: none;
    color: #333;
    font-size: 14px;
}

.dropdown-menu a:hover,
.dropdown-menu router-link:hover {
    background-color: #f5f5f5;
    color: #ee4d2d;
}

.avatar-img {
    width: 30px;
    height: 30px;
    border-radius: 50%;
    object-fit: cover;
    margin-right: 5px;
    vertical-align: middle;
}
</style>
