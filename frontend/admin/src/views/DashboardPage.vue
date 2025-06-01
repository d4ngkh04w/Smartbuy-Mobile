<script setup>
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import authService from "../services/authService.js";
import emitter from "../utils/evenBus.js";

const router = useRouter();

/**
 * Xóa toàn bộ lịch sử điều hướng của trình duyệt
 * Ngăn chặn việc quay lại Dashboard sau khi đăng xuất
 */
function clearBrowserHistory() {
	const locationOrigin = window.location.origin;
	const currentPath = "/login";
	const maxHistoryLength = 50;

	// Thêm nhiều state vào lịch sử để đảm bảo ghi đè lên lịch sử trước đó
	for (let i = 0; i < maxHistoryLength; i++) {
		window.history.pushState(null, "", locationOrigin + currentPath);
	}

	// Làm mới trang để đảm bảo trạng thái mới
	window.location.replace(locationOrigin + currentPath);
}

// Hook lifecycle
onMounted(() => {
	// Thêm meta tags để ngăn cache trang
	// addNoCacheMeta();
});

/**
 * Xử lý đăng xuất
 */
const handleLogout = async () => {
	try {
		// Gọi API đăng xuất
		await authService.logout();

		// Xóa lịch sử trình duyệt
		// clearBrowserHistory();

		router.replace("/login");

		// Thông báo thành công
		emitter.emit("show-notification", {
			status: "success",
			message: "Đăng xuất thành công",
		});
	} catch (error) {
		console.error("Logout failed:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Đăng xuất thất bại",
		});
	}
};
</script>

<template>
	<div class="dashboard-container">
		<header class="dashboard-header">
			<h1>SmartBuy Admin Dashboard</h1>
			<button class="logout-button" @click="handleLogout">
				<i class="fas fa-sign-out-alt"></i> Đăng xuất
			</button>
		</header>

		<div class="dashboard-content">
			<div class="welcome-message">
				<h2>Chào mừng trở lại, Admin!</h2>
				<p>Đây là trang quản trị hệ thống SmartBuy Mobile.</p>
			</div>

			<div class="dashboard-cards">
				<!-- Quản lý Tài khoản -->
				<div class="dashboard-card" @click="router.push('/accounts')">
					<div class="card-icon">
						<i class="fas fa-user-shield"></i>
					</div>
					<div class="card-content">
						<h3>Quản lý Tài khoản</h3>
						<p>Quản lý tài khoản admin</p>
					</div>
				</div>

				<!-- Quản lý Sản phẩm -->
				<div class="dashboard-card" @click="router.push('/products')">
					<div class="card-icon">
						<i class="fas fa-mobile-alt"></i>
					</div>
					<div class="card-content">
						<h3>Sản phẩm</h3>
						<p>Quản lý thông tin sản phẩm</p>
					</div>
				</div>

				<!-- Quản lý Danh mục -->
				<div class="dashboard-card" @click="router.push('/categories')">
					<div class="card-icon">
						<i class="fas fa-tags"></i>
					</div>
					<div class="card-content">
						<h3>Danh mục</h3>
						<p>Quản lý danh mục sản phẩm</p>
					</div>
				</div>

				<!-- Quản lý Đơn hàng -->
				<div class="dashboard-card" @click="router.push('/orders')">
					<div class="card-icon">
						<i class="fas fa-shopping-cart"></i>
					</div>
					<div class="card-content">
						<h3>Đơn hàng</h3>
						<p>Quản lý đơn hàng</p>
					</div>
				</div>

				<!-- Quản lý Khách hàng -->
				<div class="dashboard-card" @click="router.push('/customers')">
					<div class="card-icon">
						<i class="fas fa-users"></i>
					</div>
					<div class="card-content">
						<h3>Khách hàng</h3>
						<p>Quản lý tài khoản khách hàng</p>
					</div>
				</div>

				<!-- Quản lý Thanh toán/Doanh thu
				<div class="dashboard-card" @click="router.push('/payments')">
					<div class="card-icon">
						<i class="fas fa-money-bill-wave"></i>
					</div>
					<div class="card-content">
						<h3>Thanh toán & Doanh thu</h3>
						<p>Quản lý thanh toán và doanh thu</p>
					</div>
				</div> -->

				<!-- Quản lý Phản hồi đánh giá
				<div class="dashboard-card" @click="router.push('/reviews')">
					<div class="card-icon">
						<i class="fas fa-star"></i>
					</div>
					<div class="card-content">
						<h3>Phản hồi & Đánh giá</h3>
						<p>Quản lý phản hồi và đánh giá</p>
					</div>
				</div> -->

				<!-- Quản lý Nội dung website
				<div class="dashboard-card" @click="router.push('/contents')">
					<div class="card-icon">
						<i class="fas fa-file-alt"></i>
					</div>
					<div class="card-content">
						<h3>Nội dung website</h3>
						<p>Quản lý nội dung trang web</p>
					</div>
				</div> -->

				<!-- Báo cáo thống kê -->
				<div class="dashboard-card" @click="router.push('/reports')">
					<div class="card-icon">
						<i class="fas fa-chart-bar"></i>
					</div>
					<div class="card-content">
						<h3>Báo cáo thống kê</h3>
						<p>Xem báo cáo và thống kê</p>
					</div>
				</div>
				<!-- Quản lý Khuyến mãi -->
				<div class="dashboard-card" @click="router.push('/discounts')">
					<div class="card-icon">
						<i class="fas fa-percent"></i>
					</div>
					<div class="card-content">
						<h3>Khuyến mãi</h3>
						<p>Quản lý khuyến mãi và giảm giá</p>
					</div>
				</div>

				<!-- Cài đặt hệ thống
				<div class="dashboard-card" @click="router.push('/settings')">
					<div class="card-icon">
						<i class="fas fa-cog"></i>
					</div>
					<div class="card-content">
						<h3>Cài đặt hệ thống</h3>
						<p>Cấu hình và cài đặt hệ thống</p>
					</div>
				</div> -->
			</div>
		</div>
	</div>
</template>

<style scoped>
.dashboard-container {
	min-height: 100vh;
	background-color: #f9f9f9;
}

.dashboard-header {
	background-color: #fff;
	box-shadow: 0 2px 15px rgba(248, 110, 211, 0.1);
	padding: 1rem 2rem;
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.dashboard-header h1 {
	color: #333;
	font-size: 1.5rem;
	font-weight: 600;
}

.logout-button {
	padding: 0.5rem 1rem;
	background-color: #f5f5f5;
	color: #555;
	border: 1px solid #ddd;
	border-radius: 6px;
	cursor: pointer;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	transition: all 0.3s;
}

.logout-button:hover {
	background-color: #f8d7e3;
	color: var(--primary-color);
	border-color: var(--primary-color);
}

.dashboard-content {
	padding: 2rem;
}

.welcome-message {
	background-color: white;
	padding: 2rem;
	border-radius: 12px;
	box-shadow: 0 4px 12px rgba(248, 110, 211, 0.08);
	margin-bottom: 2rem;
	border-left: 4px solid var(--primary-color);
}

.welcome-message h2 {
	margin-top: 0;
	color: #333;
	font-size: 1.5rem;
	margin-bottom: 0.5rem;
}

.welcome-message p {
	color: #666;
	margin: 0;
}

.dashboard-cards {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
	gap: 1.5rem;
}

.dashboard-card {
	background-color: white;
	border-radius: 12px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
	padding: 1.5rem;
	display: flex;
	align-items: center;
	transition: all 0.3s ease;
	cursor: pointer;
	border-bottom: 3px solid transparent;
}

.dashboard-card:hover {
	transform: translateY(-5px);
	box-shadow: 0 8px 15px rgba(248, 110, 211, 0.15);
	border-bottom: 3px solid var(--primary-color);
}

.card-icon {
	background-color: #fff5fc;
	color: var(--primary-color);
	width: 55px;
	height: 55px;
	border-radius: 12px;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 1.5rem;
	margin-right: 1rem;
	transition: all 0.3s ease;
}

.dashboard-card:hover .card-icon {
	background-color: var(--primary-color);
	color: white;
}

.card-content h3 {
	margin: 0 0 0.5rem 0;
	color: #333;
	font-size: 1.1rem;
}

.card-content p {
	margin: 0;
	color: #666;
	font-size: 0.9rem;
}
</style>
