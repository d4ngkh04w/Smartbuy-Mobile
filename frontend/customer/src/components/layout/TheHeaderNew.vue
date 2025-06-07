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

                <div class="search-bar" ref="searchBarRef">
                    <input
                        type="text"
                        placeholder="Tìm kiếm sản phẩm..."
                        v-model="searchQuery"
                        @input="handleSearchInput"
                        @focus="showSearchPopup = true"
                        @keydown.enter="handleSearch"
                        ref="searchInputRef"
                    />
                    <button class="search-btn" @click="handleSearch">
                        <i class="fas fa-search"></i>
                    </button>

                    <!-- Search Popup -->
                    <div
                        v-if="showSearchPopup && searchQuery"
                        class="search-popup"
                    >
                        <div v-if="isSearching" class="search-loading">
                            <i class="fas fa-spinner fa-spin"></i>
                            <span>Đang tìm kiếm...</span>
                        </div>
                        <div
                            v-else-if="searchResults.length > 0"
                            class="search-results"
                        >
                            <div
                                v-for="product in searchResults"
                                :key="product.id"
                                class="search-result-item"
                                @click="selectProduct(product)"
                            >
                                <div class="product-image">
                                    <img
                                        :src="getProductImage(product)"
                                        :alt="product.name"
                                    />
                                </div>
                                <div class="product-info">
                                    <h4 class="product-name">
                                        {{ product.name }}
                                    </h4>
                                    <p class="product-price">
                                        {{ formatPrice(product.price) }} ₫
                                    </p>
                                </div>
                            </div>
                            <div
                                class="show-all-results"
                                @click="showAllResults"
                            >
                                <span
                                    >Hiển thị kết quả cho
                                    <strong>{{ searchQuery }}</strong></span
                                >
                                <i class="fas fa-arrow-right"></i>
                            </div>
                        </div>
                        <div
                            v-else-if="searchQuery && !isSearching"
                            class="no-results"
                        >
                            <p>Không tìm thấy sản phẩm nào</p>
                        </div>
                    </div>
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
                                <span class="user-name" v-if="currentUser">
                                    Hi,
                                    {{
                                        currentUser.name ||
                                        currentUser.phoneNumber ||
                                        "Tài Khoản"
                                    }}
                                </span>
                            </div>
                        </template>
                        <template v-else>
                            <div class="login-button">
                                <button class="login-btn">Đăng Nhập</button>
                            </div>
                        </template>
                    </div>
                </div>
            </div>
        </div>

        <!-- Search Overlay -->
        <div
            v-if="showSearchPopup"
            class="search-overlay"
            @click="closeSearchPopup"
        ></div>
    </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import meService from "@/services/meService";
import productService from "@/services/productService";
import emitter from "../../utils/evenBus.js";

const router = useRouter();
const searchQuery = ref("");
const userDropdown = ref(null);
const searchBarRef = ref(null);
const searchInputRef = ref(null);

// Search popup states
const showSearchPopup = ref(false);
const searchResults = ref([]);
const isSearching = ref(false);
let searchTimeout = null;

// User data
const isLoggedIn = ref(false);
const currentUser = ref(null);
const userAvatar = ref(null);
const isReload = ref(0);
const isUserMenuOpen = ref(false);

const cartCount = ref(0);

// Tìm kiếm sản phẩm
const searchProducts = async (query) => {
    if (!query || query.length < 1) {
        searchResults.value = [];
        return;
    }

    try {
        isSearching.value = true;
        const response = await productService.getProducts({
            search: query,
            page: 1,
            pageSize: 10,
        });

        if (response && response.data && response.data.items) {
            searchResults.value = response.data.items;
        } else {
            searchResults.value = [];
        }
    } catch (error) {
        console.error("Error searching products:", error);
        searchResults.value = [];
    } finally {
        isSearching.value = false;
    }
};

// Xử lý input tìm kiếm
const handleSearchInput = () => {
    // Debounce search
    if (searchTimeout) {
        clearTimeout(searchTimeout);
    }

    searchTimeout = setTimeout(() => {
        if (searchQuery.value && searchQuery.value.length >= 1) {
            searchProducts(searchQuery.value);
        } else {
            searchResults.value = [];
        }
    }, 300);
};

// Chọn sản phẩm từ kết quả tìm kiếm
const selectProduct = (product) => {
    router.push(`/product/${product.id}`);
    closeSearchPopup();
};

// Hiển thị tất cả kết quả
const showAllResults = () => {
    router.push({
        path: "/",
        query: { search: searchQuery.value, reload: Date.now() },
    });
    closeSearchPopup();
};

// Đóng popup tìm kiếm
const closeSearchPopup = () => {
    showSearchPopup.value = false;
    searchResults.value = [];
    if (searchTimeout) {
        clearTimeout(searchTimeout);
    }
};

// Lấy URL ảnh sản phẩm
const getProductImage = (product) => {
    if (product.colors && product.colors.length > 0) {
        const firstColor = product.colors[0];
        if (firstColor.images && firstColor.images.length > 0) {
            const mainImage =
                firstColor.images.find((img) => img.isMain) ||
                firstColor.images[0];
            return productService.getUrlImage(mainImage.imagePath);
        }
    }
    return "/placeholder-image.jpg";
};

// Format giá
const formatPrice = (price) => {
    return new Intl.NumberFormat("vi-VN").format(price);
};

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
    closeSearchPopup();
};

const closeMenus = (event) => {
    // Đóng search popup nếu click bên ngoài
    if (searchBarRef.value && !searchBarRef.value.contains(event.target)) {
        closeSearchPopup();
    }

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
        const userData = await meService.getMe({
            headers: {
                "X-From-Header": "true",
            },
            skipAuthRedirect: true,
        });

        currentUser.value = userData;
        isLoggedIn.value = true;

        userAvatar.value = userData.avatar
            ? meService.getUrlImage(userData.avatar)
            : null;
    } catch (error) {
        isLoggedIn.value = false;
        currentUser.value = null;
        userAvatar.value = null;
    }
};

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

    // Event listeners
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
    emitter.on("user-logged-out", handleUserLoggedOut);
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

    if (searchTimeout) {
        clearTimeout(searchTimeout);
    }
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

/* Search Popup Styles */
.search-popup {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: white;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    max-height: 400px;
    overflow-y: auto;
    z-index: 1000;
}

.search-loading {
    padding: 20px;
    text-align: center;
    color: #666;
}

.search-loading i {
    margin-right: 8px;
}

.search-results {
    padding: 8px 0;
}

.search-result-item {
    display: flex;
    align-items: center;
    padding: 12px 16px;
    cursor: pointer;
    transition: background-color 0.2s;
    border-bottom: 1px solid #f0f0f0;
}

.search-result-item:hover {
    background-color: #f8f9fa;
}

.search-result-item:last-child {
    border-bottom: none;
}

.product-image {
    width: 50px;
    height: 50px;
    margin-right: 12px;
    flex-shrink: 0;
}

.product-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 4px;
}

.product-info {
    flex: 1;
}

.product-name {
    font-size: 14px;
    font-weight: 500;
    margin: 0 0 4px 0;
    color: #333;
    line-height: 1.3;
}

.product-price {
    font-size: 13px;
    color: #f86ed3;
    font-weight: 600;
    margin: 0;
}

.show-all-results {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 16px;
    background-color: #f8f9fa;
    cursor: pointer;
    border-top: 1px solid #e0e0e0;
    color: #666;
    font-size: 14px;
}

.show-all-results:hover {
    background-color: #e9ecef;
}

.show-all-results i {
    font-size: 12px;
}

.no-results {
    padding: 20px;
    text-align: center;
    color: #666;
    font-size: 14px;
}

.no-results p {
    margin: 0;
}

/* Search Overlay */
.search-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.3);
    z-index: 50;
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
    transform: translateY(-1px);
}

.cart-icon:hover .icon-container {
    background-color: rgba(255, 255, 255, 0.1);
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.user-dropdown:hover .icon-container {
    background-color: rgba(255, 255, 255, 0.1);
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
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
    transition: all 0.3s ease;
}

.icon-container.logged-in {
    background-color: transparent;
    border: 2px solid #ffffff;
    padding: 0;
    overflow: hidden;
}

.icon-container i {
    color: #ffffff;
    font-size: 1.2rem;
    transition: all 0.3s ease;
}

.icon-container.logged-in i {
    color: #ffffff;
    font-size: 1.2rem;
}

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
    border: 2px solid #f86ed3;
    z-index: 10;
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
    transition: all 0.3s ease;
}

.user-info:hover {
    transform: translateY(-1px);
}

.user-name {
    color: white;
    font-size: 0.9rem;
    font-weight: 500;
    transition: color 0.3s ease;
}

.user-dropdown:hover .user-name {
    color: #ffcef0;
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

    .search-popup {
        max-height: 300px;
    }

    .product-image {
        width: 40px;
        height: 40px;
    }

    .product-name {
        font-size: 13px;
    }

    .product-price {
        font-size: 12px;
    }
}

@media (max-width: 576px) {
    .notification-icon .dropdown-menu {
        width: 280px;
        left: -100px;
    }
}
</style>
