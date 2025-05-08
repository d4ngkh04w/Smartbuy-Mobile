<template>
    <header class="header">
        <div class="container">
            <div class="header-content">
                <div class="logo">
                    <router-link to="/">
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
                    <div
                        class="action-item notification-icon"
                        @click="toggleNotifications"
                        ref="notificationRef"
                    >
                        <div class="icon-container">
                            <i class="fas fa-bell"></i>
                            <span
                                v-if="notificationCount > 0"
                                class="notification-badge"
                                >{{ notificationCount }}</span
                            >
                        </div>

                        <div
                            class="dropdown-menu notifications-menu"
                            v-if="showNotifications"
                        >
                            <div class="notifications-header">
                                <h3>Thông báo</h3>
                                <button
                                    class="mark-all-read"
                                    @click.stop="markAllAsRead"
                                >
                                    Đánh dấu đã đọc
                                </button>
                            </div>

                            <div
                                class="notifications-list"
                                v-if="notifications.length"
                            >
                                <div
                                    v-for="notification in notifications"
                                    :key="notification.id"
                                    class="notification-item"
                                    :class="{ unread: !notification.read }"
                                >
                                    <div class="notification-icon">
                                        <i
                                            :class="
                                                getNotificationIcon(
                                                    notification.type
                                                )
                                            "
                                        ></i>
                                    </div>
                                    <div class="notification-content">
                                        <p class="notification-text">
                                            {{ notification.message }}
                                        </p>
                                        <p class="notification-time">
                                            {{
                                                formatTime(
                                                    notification.createdAt
                                                )
                                            }}
                                        </p>
                                    </div>
                                </div>
                            </div>

                            <div class="no-notifications" v-else>
                                <p>Không có thông báo mới</p>
                            </div>

                            <div class="notifications-footer">
                                <router-link
                                    to="/notifications"
                                    @click="showNotifications = false"
                                >
                                    Xem tất cả thông báo
                                </router-link>
                            </div>
                        </div>
                    </div>

                    <div
                        class="action-item cart-icon"
                        @click="toggleCart"
                        ref="cartRef"
                    >
                        <div class="icon-container">
                            <i class="fas fa-shopping-cart"></i>
                            <span v-if="cartCount > 0" class="cart-badge">{{
                                cartCount
                            }}</span>
                        </div>

                        <div class="dropdown-menu cart-menu" v-if="showCart">
                            <div class="cart-header">
                                <h3>Giỏ hàng</h3>
                            </div>

                            <div class="cart-items" v-if="cartItems.length">
                                <div
                                    v-for="item in cartItems"
                                    :key="item.id"
                                    class="cart-item"
                                >
                                    <div class="cart-item-image">
                                        <img
                                            :src="item.image"
                                            :alt="item.name"
                                        />
                                    </div>
                                    <div class="cart-item-content">
                                        <p class="cart-item-name">
                                            {{ item.name }}
                                        </p>
                                        <p class="cart-item-price">
                                            {{ formatPrice(item.price) }} x
                                            {{ item.quantity }}
                                        </p>
                                    </div>
                                    <button
                                        class="remove-item"
                                        @click.stop="removeFromCart(item.id)"
                                    >
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                            </div>

                            <div class="empty-cart" v-else>
                                <p>Không có sản phẩm nào trong giỏ hàng</p>
                            </div>

                            <div class="cart-footer" v-if="cartItems.length">
                                <div class="cart-total">
                                    <span>Tổng tiền:</span>
                                    <span class="total-price">{{
                                        formatPrice(cartTotal)
                                    }}</span>
                                </div>
                                <div class="cart-actions">
                                    <router-link
                                        to="/cart"
                                        class="view-cart-btn"
                                        @click="showCart = false"
                                    >
                                        Xem giỏ hàng
                                    </router-link>
                                    <router-link
                                        to="/checkout"
                                        class="checkout-btn"
                                        @click="showCart = false"
                                    >
                                        Thanh toán
                                    </router-link>
                                </div>
                            </div>

                            <div class="cart-footer" v-else>
                                <router-link
                                    to="/products"
                                    class="continue-shopping"
                                    @click="showCart = false"
                                >
                                    Tiếp tục mua sắm
                                </router-link>
                            </div>
                        </div>
                    </div>

                    <div
                        class="action-item user-dropdown"
                        @click="
                            isLoggedIn ? router.push('/account') : goToLogin()
                        "
                        ref="userDropdown"
                    >
                        <template v-if="isLoggedIn">
                            <div class="icon-container">
                                <img
                                    v-if="userAvatar"
                                    :src="userAvatar"
                                    alt="User"
                                    class="user-avatar"
                                />
                                <i v-else class="fas fa-user"></i>
                            </div>
                        </template>
                        <template v-else>
                            <div class="icon-container">
                                <i class="fas fa-user"></i>
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
import authService from "@/services/authService";

const router = useRouter();
const searchQuery = ref("");
const isUserMenuOpen = ref(false);
const userDropdown = ref(null);
const notificationRef = ref(null);
const cartRef = ref(null);
const showNotifications = ref(false);
const showCart = ref(false);

// User data
const isLoggedIn = ref(false);
const currentUser = ref(null);
const userAvatar = ref(null);

// Demo data - Thay thế bằng state thực tế trong ứng dụng của bạn
const notificationCount = ref(2);
const cartItems = ref([
    {
        id: 1,
        name: "iPhone 13",
        image: "https://via.placeholder.com/50",
        price: 18000000,
        quantity: 1,
    },
    {
        id: 2,
        name: "Samsung Galaxy S21",
        image: "https://via.placeholder.com/50",
        price: 14000000,
        quantity: 2,
    },
]);

// Demo notifications data - sẽ được thay thế bằng API call
const notifications = ref([
    {
        id: 1,
        type: "order",
        message: "Đơn hàng #12345 đã được giao thành công",
        read: false,
        createdAt: new Date(Date.now() - 30 * 60000), // 30 phút trước
    },
    {
        id: 2,
        type: "promotion",
        message: "Giảm giá 20% cho tất cả sản phẩm Samsung",
        read: false,
        createdAt: new Date(Date.now() - 2 * 3600000), // 2 giờ trước
    },
]);

// Base API URL
const baseApiUrl = import.meta.env.VITE_API_URL || "http://localhost:5000";

const cartCount = ref(cartItems.value.length);
const cartTotal = computed(() => {
    return cartItems.value.reduce(
        (total, item) => total + item.price * item.quantity,
        0
    );
});

const handleSearch = () => {
    if (searchQuery.value.trim()) {
        router.push({
            path: "/search",
            query: { q: searchQuery.value },
        });
    }
};

const goToLogin = () => {
    router.push("/login");
};

const toggleNotifications = () => {
    showNotifications.value = !showNotifications.value;
    if (showNotifications.value) {
        isUserMenuOpen.value = false;
        showCart.value = false;
    }
};

const toggleCart = () => {
    showCart.value = !showCart.value;
    if (showCart.value) {
        isUserMenuOpen.value = false;
        showNotifications.value = false;
    }
};

const closeMenus = (event) => {
    if (
        userDropdown.value &&
        !userDropdown.value.contains(event.target) &&
        isUserMenuOpen.value
    ) {
        isUserMenuOpen.value = false;
    }

    if (
        notificationRef.value &&
        !notificationRef.value.contains(event.target) &&
        showNotifications.value
    ) {
        showNotifications.value = false;
    }

    if (
        cartRef.value &&
        !cartRef.value.contains(event.target) &&
        showCart.value
    ) {
        showCart.value = false;
    }
};

const removeFromCart = (id) => {
    // Xóa sản phẩm khỏi giỏ hàng
    cartItems.value = cartItems.value.filter((item) => item.id !== id);
};

const formatTime = (date) => {
    // Tính thời gian tương đối
    const now = new Date();
    const diffInMinutes = Math.floor(
        (now.getTime() - date.getTime()) / (1000 * 60)
    );

    if (diffInMinutes < 60) {
        return `${diffInMinutes} phút trước`;
    } else if (diffInMinutes < 24 * 60) {
        return `${Math.floor(diffInMinutes / 60)} giờ trước`;
    } else {
        return date.toLocaleDateString("vi-VN");
    }
};

const getNotificationIcon = (type) => {
    switch (type) {
        case "order":
            return "fas fa-shopping-bag";
        case "promotion":
            return "fas fa-percentage";
        case "system":
            return "fas fa-bell";
        default:
            return "fas fa-bell";
    }
};

const markAllAsRead = () => {
    notifications.value = notifications.value.map((notification) => ({
        ...notification,
        read: true,
    }));
};

const formatPrice = (price) => {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(price);
};

// Fetch user data on component mount
const fetchUserData = async () => {
    try {
        // Gọi trực tiếp API getMe để lấy thông tin người dùng
        const userData = await meService.getMe();
        currentUser.value = userData;
        isLoggedIn.value = true;

        // Set user avatar if available
        if (userData.avatar) {
            // Nếu avatar đã là URL đầy đủ (bắt đầu bằng http hoặc https)
            if (userData.avatar.startsWith("http")) {
                userAvatar.value = userData.avatar;
            } else {
                // Lấy base URL từ cấu hình API
                const apiUrl = baseApiUrl;
                const baseUrl = apiUrl.includes("/api")
                    ? apiUrl.split("/api")[0]
                    : "";

                // Chuẩn hóa đường dẫn file (chuyển \ thành /)
                const normalizedPath = userData.avatar.replace(/\\/g, "/");

                // Kiểm tra xem có prefix / hay không
                const avatarPath = normalizedPath.startsWith("/")
                    ? normalizedPath
                    : `/${normalizedPath}`;

                userAvatar.value = `${baseUrl}${avatarPath}`;
            }
        } else {
            userAvatar.value = null;
        }
    } catch (error) {
        console.error("Error fetching user data:", error);
        // Nếu API getMe trả về lỗi, xóa dữ liệu người dùng
        isLoggedIn.value = false;
        currentUser.value = null;
        userAvatar.value = null;

        // Nếu lỗi 401 (Unauthorized), chuyển hướng sang trang đăng nhập
        if (error.response && error.response.status === 401) {
            router.push("/login");
        }
    }
};

onMounted(() => {
    document.addEventListener("click", closeMenus);
    fetchUserData();
});

onUnmounted(() => {
    document.removeEventListener("click", closeMenus);
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
    padding: 1rem 0;
    flex-wrap: wrap;
    gap: 1rem;
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
    background-color: rgba(255, 255, 255, 0.2);
    border-radius: 50%;
}

.cart-badge {
    position: absolute;
    top: -5px;
    right: -5px;
    background-color: red;
    color: white;
    border-radius: 50%;
    width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.7rem;
}

.notification-badge {
    position: absolute;
    top: -5px;
    right: -5px;
    background-color: red;
    color: white;
    border-radius: 50%;
    width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.7rem;
}

.notification-icon,
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
