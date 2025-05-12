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
                        to = "/cart" >
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
							<div class="icon-container logged-in">
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
							<div class="icon-container guest">
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
import productService from "@/services/productService";
import emitter from "../../utils/evenBus.js";

const router = useRouter();
const searchQuery = ref("");
const isUserMenuOpen = ref(false);
const userDropdown = ref(null);
const cartRef = ref(null);
const showCart = ref(false);
const cartItems = ref([]);

// User data
const isLoggedIn = ref(false);
const currentUser = ref(null);
const userAvatar = ref(null);
const isReload = ref(0);

// Base API URL
const baseApiUrl = import.meta.env.VITE_API_URL;

const cartCount = ref(0);

const resetSearchText = () => {
	searchQuery.value = "";
	isReload.value = 1 - isReload.value;
	handleSearch();
};
const getCartItems = async () => {
  const res = await productService.getCarts()
  if (res && res.data && res.data.cartItems) {
    cartCount.value = res.data.cartItems.length;
  }
}

const handleSearch = () => {
	router.push({
		path: "/",
		query: { search: searchQuery.value, reload: isReload.value },
	});
};

const goToLogin = () => {
    router.push("/login");
};

// Fetch user data on component mount
const fetchUserData = async () => {
	try {
		// Gọi API getMe với cờ isHeaderRequest để tránh vòng lặp chuyển hướng
		const userData = await meService.getMe({
			headers: {
				"X-From-Header": "true",
			},
			skipAuthRedirect: true,
		});
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

		// Không chuyển hướng sang trang đăng nhập khi gọi từ header
		// Hiển thị UI cho trạng thái chưa đăng nhập
	}
};

// Xử lý sự kiện đăng xuất
const handleUserLoggedOut = () => {
	// Cập nhật trạng thái đăng nhập khi người dùng đăng xuất
	isLoggedIn.value = false;
	currentUser.value = null;
	userAvatar.value = null;
};

onMounted(() => {
	getCartItems();
	fetchUserData();
    emitter.on("cart-updated", () => {
        getCartItems();
    });
    emitter.on('update-cart-count', () => {
        getCartItems();
    
	// Lắng nghe sự kiện đăng xuất
	emitter.on("user-logged-out", handleUserLoggedOut);
});
});
onUnmounted(() => {
	emitter.off("cart-updated");

	// Ngừng lắng nghe sự kiện khi component bị hủy
	emitter.off("user-logged-out", handleUserLoggedOut);
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

/* Style for logged in users */
.icon-container.logged-in {
	background-color: rgba(255, 255, 255, 0.4);
	border: 2px solid #ffffff;
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
