<template>
	<footer class="footer">
		<div class="container">
			<div class="footer-grid">
				<!-- Cột 1: Logo + mô tả -->
				<div class="footer-column">
					<router-link to="/" class="logo">
						<img src="../../assets/logo.png" alt="SmartBuy Mobile" />
					</router-link>
					<p class="description">
						SmartBuy Mobile - Nơi mua sắm điện thoại chính hãng, giá tốt, dịch vụ tận tâm.
					</p>
				</div>

				<!-- Cột 2: Liên kết -->
				<div class="footer-column">
					<h3 class="title">Liên kết</h3>
					<ul class="link-list">
						<li><router-link to="/products">Sản phẩm</router-link></li>
						<li><router-link to="/about">Giới thiệu</router-link></li>
						<li><router-link to="/contact">Liên hệ</router-link></li>
						<li><router-link to="/warranty">Bảo hành</router-link></li>
						<li><router-link to="/faq">Câu hỏi thường gặp</router-link></li>
					</ul>
				</div>

				<!-- Cột 3: Thông tin liên hệ -->
				<div class="footer-column">
					<h3 class="title">Liên hệ</h3>
					<ul class="contact-list">
						<li><i class="fas fa-map-marker-alt"></i> 54 Nguyễn Lương Bằng, Đà Nẵng</li>
						<li><i class="fas fa-phone-alt"></i> +84 333 666 999</li>
						<li><i class="fas fa-envelope"></i> smartbuymobile.team@gmail.com</li>
					</ul>
				</div>

				<!-- Cột 4: Đăng ký & Mạng xã hội -->
				<div class="footer-column">
					<h3 class="title">Nhận tin mới</h3>
					<p>Đăng ký để nhận ưu đãi và tin tức sản phẩm mới nhất.</p>
					<form @submit.prevent="subscribeNewsletter" class="newsletter-form">
						<input type="email" v-model="email" placeholder="Email của bạn" required />
						<button type="submit"><i class="fas fa-paper-plane"></i></button>
					</form>
					<div class="social-icons">
						<a href="https://facebook.com" target="_blank"><i class="fab fa-facebook-f"></i></a>
						<a href="https://instagram.com" target="_blank"><i class="fab fa-instagram"></i></a>
						<a href="https://youtube.com" target="_blank"><i class="fab fa-youtube"></i></a>
						<a href="https://tiktok.com" target="_blank"><i class="fab fa-tiktok"></i></a>
					</div>
				</div>
			</div>

			<!-- Dòng bản quyền -->
			<div class="footer-bottom">
				<p>&copy; {{ currentYear }} SmartBuy Mobile. All rights reserved.</p>
				<div class="payment-icons">
					<i class="fab fa-cc-visa"></i>
					<i class="fab fa-cc-mastercard"></i>
					<i class="fab fa-cc-paypal"></i>
					<i class="fas fa-money-bill-wave"></i>
				</div>
			</div>
		</div>
	</footer>
</template>

<script setup>
import { ref, computed } from "vue";
const email = ref("");
const currentYear = computed(() => new Date().getFullYear());

const subscribeNewsletter = () => {
	alert("Cảm ơn bạn đã đăng ký nhận thông tin!");
	email.value = "";
};
</script>

<style scoped>
.footer {
	background-color: #483d63;
	color: #e5e7eb;
	padding: 3rem 1rem 2rem;
	font-family: 'Segoe UI', sans-serif;
}
.container {
	max-width: 1200px;
	margin: 0 auto;
}
.footer-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
	gap: 2rem;
}
.footer-column .logo img {
	height: 40px;
	filter: brightness(0) invert(1);
	margin-bottom: 1rem;
}
.description {
	line-height: 1.6;
	color: #d1d5db;
}
.title {
	font-weight: 600;
	margin-bottom: 1rem;
	position: relative;
}
.title::after {
	content: "";
	position: absolute;
	left: 0;
	bottom: -6px;
	width: 40px;
	height: 2px;
	background-color: var(--primary-color);
}
.link-list,
.contact-list {
	list-style: none;
	padding: 0;
	margin: 0;
}
.link-list li,
.contact-list li {
	margin-bottom: 0.75rem;
}
.link-list a {
	color: #d1d5db;
	text-decoration: none;
	transition: color 0.3s;
}
.link-list a:hover {
	color: var(--primary-color);
}
.contact-list i {
	margin-right: 8px;
	color: var(--primary-color);
}
.newsletter-form {
	display: flex;
	margin-top: 0.75rem;
}
.newsletter-form input {
	flex: 1;
	padding: 0.6rem;
	border: none;
	border-radius: 4px 0 0 4px;
}
.newsletter-form button {
	background-color: var(--primary-color);
	color: white;
	border: none;
	padding: 0 1rem;
	border-radius: 0 4px 4px 0;
}
.social-icons {
	display: flex;
	gap: 1rem;
	margin-top: 1rem;
}
.social-icons a {
	width: 36px;
	height: 36px;
	background: rgba(255, 255, 255, 0.1);
	border-radius: 50%;
	display: flex;
	justify-content: center;
	align-items: center;
	color: white;
	transition: 0.3s;
}
.social-icons a:hover {
	background-color: var(--primary-color);
	transform: translateY(-3px);
}
.footer-bottom {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-top: 2rem;
	border-top: 1px solid rgba(255, 255, 255, 0.1);
	padding-top: 1rem;
	font-size: 0.875rem;
	flex-wrap: wrap;
	gap: 1rem;
}
.payment-icons i {
	font-size: 1.5rem;
	margin-right: 1rem;
	color: #d1d5db;
}
@media (max-width: 768px) {
	.footer-bottom {
		flex-direction: column;
		text-align: center;
	}
}
</style>
