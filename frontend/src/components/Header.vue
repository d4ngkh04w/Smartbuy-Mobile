<script setup>
import { ref } from "vue";
import LoginModal from "@/components/LoginModal.vue";

// Biến trạng thái xác thực
const auth = ref({
    isAuthenticated: false,
    user: {
        phoneNumber: ""
    }
});

const searchQuery = ref("");
const showLoginModal = ref(false);
const showDropdown = ref(false);

// Xử lý khi đăng nhập thành công
const handleLoginSuccess = (userData) => {
    auth.value.isAuthenticated = true;
    auth.value.user = userData;
    showLoginModal.value = false;
};

const toggleDropdown = () => {
    showDropdown.value = !showDropdown.value;
};

const logout = () => {
    auth.value.isAuthenticated = false;
    auth.value.user = {
        phoneNumber: ""
    };
    showDropdown.value = false;
};
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
                <div class="action-item user-menu" v-if="auth.isAuthenticated">
                    <div class="user-trigger" @click="toggleDropdown">
                        <img
                            src="https://scontent.fdad7-2.fna.fbcdn.net/v/t39.30808-6/481020844_662021206282518_5230218385143721393_n.jpg?_nc_cat=100&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=C6FpJeE63ycQ7kNvwERM-vE&_nc_oc=AdnWNQqBxaLvO8d3NPHKoenC_ZT0tGwKOjDHliGetAkzQyv0ZnTxzUjvi-B4yxgZ6DAmQxtfnt7O4rCOcnUUkQNX&_nc_zt=23&_nc_ht=scontent.fdad7-2.fna&_nc_gid=Q5MDJChrlxIC047bjNLKdQ&oh=00_AfFBLsvbPWPOsJV-WLSmR2hHEyjBVWfyYTcyEGVhmdI71Q&oe=68024B48"
                            alt="Avatar"
                            class="avatar-img"
                        />
                       
                        <span>{{ auth.user.phoneNumber }}</span>
                    </div>

                    <div class="dropdown-menu" v-if="showDropdown">
                        <div class="arrow-up"></div>
                        <router-link to="/profile"
                            >Thông tin cá nhân</router-link
                        >
                        <router-link to="/orders">Đơn hàng</router-link>
                        <a @click="logout">Đăng xuất</a>
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
    padding: 5px 10px;
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
}


/* Responsive */
@media (max-width: 1200px) {
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

