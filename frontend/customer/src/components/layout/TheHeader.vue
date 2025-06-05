<template>
    <header class="header">
        <div class="container">
            <div class="header-content">
                <div class="logo">
                    <router-link to="/" @click="resetSearchText">
                        <img
                            src="../../assets/logo_letter.png"
                            alt="SmartBuy Mobile"
                        />
                    </router-link>
                </div>
                <div class="search-bar">
                    <input
                        type="text"
                        placeholder="Tìm kiếm sản phẩm..."
                        v-model="searchQuery"
                        @keyup.enter="handleSearch"
                    />
                    <button class="search-btn" @click="handleSearch">
                        <i class="fas fa-search"></i>
                    </button>
                </div>

                <div class="header-actions">
                    <router-link
                        to="/cart"
                        @click.native.prevent="handleCartClick"
                    >
                        <div class="action-item cart-icon">
                            <div class="icon-container">
                                <i class="fas fa-shopping-cart"></i>
                                <span v-if="cartCount > 0" class="cart-badge">{{
                                    cartCount
                                }}</span>
                            </div>
                        </div>
                    </router-link>

                    <div
                        class="action-item user-dropdown"
                        @click="
                            isLoggedIn
                                ? router.push('/account')
                                : router.push('/login')
                        "
                        ref="userDropdown"
                    >
                        <template v-if="isLoggedIn">
                            <div class="user-info">
                                <div class="icon-container logged-in">
                                    <img
                                        v-if="userAvatar"
                                        :src="userAvatar"
                                        alt="User"
                                        class="user-avatar"
                                    />
                                    <i v-else class="fas fa-user"></i>
                                </div>
                                <span class="user-name" v-if="currentUser"
                                    >Hi,
                                    {{
                                        currentUser.name ||
                                        currentUser.phoneNumber ||
                                        "Tài Khoản"
                                    }}</span
                                >
                            </div> </template
                        ><template v-else>
                            <div class="login-button">
                                <button class="login-btn">Đăng Nhập</button>
                            </div>
                        </template>
                    </div>
                </div>
            </div>
        </div>
    </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from "vue";
import { useRouter } from "vue-router";
import meService from "@/services/meService";
import productService from "@/services/productService";
import emitter from "../../utils/evenBus.js";

const router = useRouter();
const searchQuery = ref("");
const userDropdown = ref(null);

// User data
const isLoggedIn = ref(false);
const currentUser = ref(null);
const userAvatar = ref(null);
const isReload = ref(0);
const isUserMenuOpen = ref(false);

// Base API URL
const baseApiUrl = import.meta.env.VITE_API_URL;

const cartCount = ref(0);

const resetSearchText = () => {
    searchQuery.value = "";
    isReload.value = 1 - isReload.value;
    handleSearch();
};

const getCartItems = async () => {
    if (!isLoggedIn.value) return;
    const res = await productService.getCarts();
    if (res && res.data && res.data.cartItems) {
        cartCount.value = res.data.cartItems.length;
    }
};

const handleCartClick = (e) => {
    if (!isLoggedIn.value) {
        e.preventDefault();
        router.push("/not-logged-in");
    }
};

const handleSearch = () => {
    router.push({
        path: "/",
        query: { search: searchQuery.value, reload: isReload.value },
    });
};

const closeMenus = (event) => {
    if (
        userDropdown.value &&
        !userDropdown.value.contains(event.target) &&
        isUserMenuOpen.value
    ) {
        isUserMenuOpen.value = false;
    }
};

const fetchUserData = async () => {
    try {
        // Get user data with special header to avoid redirect loops
        const userData = await meService.getMe({
            headers: {
                "X-From-Header": "true",
            },
            skipAuthRedirect: true,
        });

        // Update user state
        currentUser.value = userData;
        isLoggedIn.value = true;

        // Format avatar URL
        userAvatar.value = userData.avatar
            ? meService.getUrlImage(userData.avatar)
            : null;
    } catch (error) {
        //console.error("Error fetching user data:", error);
        // Reset user state on error
        isLoggedIn.value = false;
        currentUser.value = null;
        userAvatar.value = null;
    }
};

// Handle logout event
const handleUserLoggedOut = () => {
    isLoggedIn.value = false;
    currentUser.value = null;
    userAvatar.value = null;
};

onMounted(() => {
    document.addEventListener("click", closeMenus);

    fetchUserData().then(() => {
        if (isLoggedIn.value) {
            getCartItems();
        }
    });

    // Lắng nghe sự kiện cập nhật giỏ hàng
    emitter.on("cart-updated", () => {
        if (isLoggedIn.value) {
            getCartItems();
        }
    });
    emitter.on("update-cart-count", () => {
        if (isLoggedIn.value) {
            getCartItems();
        }
    });
    emitter.on("logout", () => {
        cartCount.value = 0;
    });

    // Lắng nghe sự kiện đăng xuất
    emitter.on("user-logged-out", handleUserLoggedOut);

    // Lắng nghe sự kiện cập nhật thông tin người dùng
    emitter.on("user-updated", (userData) => {
        if (userData) {
            currentUser.value = userData;
            userAvatar.value = userData.avatar
                ? meService.getUrlImage(userData.avatar)
                : null;
        }
    });
});

onUnmounted(() => {
    document.removeEventListener("click", closeMenus);
    emitter.off("cart-updated");
    emitter.off("update-cart-count");
    emitter.off("logout");
    emitter.off("user-logged-out", handleUserLoggedOut);
    emitter.off("user-updated");
});
</script>

<style scoped>
.header {
    background-color: #f86ed3;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    position: sticky;
    top: 0;
    z-index: 100;
}

.header-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.75rem 0;
    flex-wrap: wrap;
    gap: 0.75rem;
    max-width: 1200px;
    margin: 0 auto;
}

.logo {
    flex: 0 0 auto;
}

.logo img {
    width: 175px;
    height: 49px;
    object-fit: cover;
}

.search-bar {
    flex: 1;
    max-width: 600px;
    position: relative;
    margin: 0 1rem;
}

.search-bar input {
    width: 100%;
    padding: 0.75rem 1rem;
    padding-right: 40px;
    border: 1px solid #e0e0e0;
    border-radius: 20px;
    font-size: 0.9rem;
    background-color: white;
}

.search-bar input:focus {
    outline: none;
    border-color: #f86ed3;
}

.search-btn {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #888;
    font-size: 1rem;
    cursor: pointer;
    padding: 0.5rem;
}

.header-actions {
    display: flex;
    align-items: center;
    gap: 1.5rem;
}

.action-item {
    position: relative;
    cursor: pointer;
    color: white;
    font-size: 1.1rem;
    display: flex;
    align-items: center;
    transition: color 0.3s ease;
}

.action-item:hover {
    color: #ffcef0;
}

.icon-container {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    background-color: transparent;
    border: 2px solid #ffffff;
    border-radius: 50%;
}

/* Style for logged in users */
.icon-container.logged-in {
    background-color: transparent;
    border: 2px solid #ffffff;
    padding: 0;
    overflow: hidden;
}

.icon-container i {
    color: #ffffff;
    font-size: 1.2rem;
}

.icon-container.logged-in i {
    color: #ffffff;
    font-size: 1.2rem;
}

/* Style for guest users */
.icon-container.guest {
    background-color: rgba(255, 255, 255, 0.15);
    border: 1px dashed rgba(255, 255, 255, 0.7);
}

.icon-container.guest i {
    color: rgba(255, 255, 255, 0.85);
    font-size: 1rem;
}

.cart-badge {
    position: absolute;
    top: -10px;
    right: -10px;
    background-color: red;
    color: white;
    border-radius: 50%;
    width: 22px;
    height: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.75rem;
    font-weight: bold;
    border: 2px solid #f86ed3; /* Match background color */
    z-index: 10; /* Ensure the badge appears on top */
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

.cart-icon,
.user-dropdown {
    position: relative;
}

.user-avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    object-fit: cover;
}

.user-info {
    display: flex;
    align-items: center;
    gap: 10px;
}

.user-name {
    color: white;
    font-size: 0.9rem;
    font-weight: 500;
}

.login-button {
    display: flex;
    align-items: center;
    cursor: pointer;
}

.login-btn {
    background-color: white;
    color: #f86ed3;
    border: none;
    border-radius: 20px;
    padding: 8px 20px;
    font-size: 0.9rem;
    font-weight: 500;
    cursor: pointer;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    transition: all 0.3s ease;
}

.login-btn:hover {
    background-color: #f5f5f5;
    transform: translateY(-1px);
}

@media (max-width: 992px) {
    .search-bar {
        order: 3;
        flex-basis: 100%;
        margin: 1rem 0 0 0;
        max-width: none;
    }

    .header-content {
        flex-wrap: wrap;
    }
}

@media (max-width: 768px) {
    .header-actions {
        gap: 1rem;
    }
}

@media (max-width: 576px) {
    .notification-icon .dropdown-menu {
        width: 280px;
        left: -100px;
    }
}
</style>
