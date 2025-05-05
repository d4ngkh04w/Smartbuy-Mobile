<template>
    <footer class="footer">
        <div class="container">
            <div class="footer-content">
                <div class="footer-section about">
                    <div class="logo">
                        <router-link to="/">
                            <img
                                src="../../assets/logo.png"
                                alt="SmartBuy Mobile"
                            />
                        </router-link>
                    </div>
                    <p class="about-text">
                        SmartBuy Mobile - Cửa hàng điện thoại và phụ kiện chính
                        hãng với nhiều ưu đãi hấp dẫn và dịch vụ chăm sóc khách
                        hàng tận tâm.
                    </p>
                    <div class="contact-info">
                        <div class="contact-item">
                            <i class="fas fa-map-marker-alt"></i>
                            <span
                                >54 Nguyễn Lương Bằng, Liên Chiểu, Đà Nẵng</span
                            >
                        </div>
                        <div class="contact-item">
                            <i class="fas fa-phone-alt"></i>
                            <span>+84 123 456 789</span>
                        </div>
                        <div class="contact-item">
                            <i class="fas fa-envelope"></i>
                            <span>support@smartbuy.com</span>
                        </div>
                    </div>
                </div>

                <div class="footer-section links">
                    <h3 class="footer-title">Liên kết nhanh</h3>
                    <ul class="footer-links">
                        <li>
                            <router-link to="/products">Sản phẩm</router-link>
                        </li>
                        <li>
                            <router-link to="/about">Giới thiệu</router-link>
                        </li>
                        <li>
                            <router-link to="/contact">Liên hệ</router-link>
                        </li>
                        <li>
                            <router-link to="/warranty">Bảo hành</router-link>
                        </li>
                        <li>
                            <router-link to="/faq"
                                >Câu hỏi thường gặp</router-link
                            >
                        </li>
                    </ul>
                </div>

                <div class="footer-section categories">
                    <h3 class="footer-title">Danh mục sản phẩm</h3>
                    <ul class="footer-links">
                        <li>
                            <router-link to="/products/phones"
                                >Điện thoại</router-link
                            >
                        </li>
                        <li>
                            <router-link to="/products/tablets"
                                >Máy tính bảng</router-link
                            >
                        </li>
                        <li>
                            <router-link to="/products/accessories"
                                >Phụ kiện</router-link
                            >
                        </li>
                        <li>
                            <router-link to="/products/watches"
                                >Đồng hồ thông minh</router-link
                            >
                        </li>
                        <li>
                            <router-link to="/products/earbuds"
                                >Tai nghe</router-link
                            >
                        </li>
                    </ul>
                </div>

                <div class="footer-section newsletter">
                    <h3 class="footer-title">Đăng ký nhận tin</h3>
                    <p>Nhận thông tin khuyến mãi và sản phẩm mới</p>
                    <form
                        @submit.prevent="subscribeNewsletter"
                        class="newsletter-form"
                    >
                        <input
                            type="email"
                            placeholder="Email của bạn"
                            v-model="email"
                            required
                        />
                        <button type="submit">
                            <i class="fas fa-paper-plane"></i>
                        </button>
                    </form>
                    <div class="social-links">
                        <a
                            href="https://facebook.com"
                            target="_blank"
                            rel="noopener noreferrer"
                        >
                            <i class="fab fa-facebook-f"></i>
                        </a>
                        <a
                            href="https://instagram.com"
                            target="_blank"
                            rel="noopener noreferrer"
                        >
                            <i class="fab fa-instagram"></i>
                        </a>
                        <a
                            href="https://youtube.com"
                            target="_blank"
                            rel="noopener noreferrer"
                        >
                            <i class="fab fa-youtube"></i>
                        </a>
                        <a
                            href="https://tiktok.com"
                            target="_blank"
                            rel="noopener noreferrer"
                        >
                            <i class="fab fa-tiktok"></i>
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <div class="footer-bottom">
            <div class="container">
                <div class="copyright">
                    &copy; {{ currentYear }} SmartBuy Mobile. Tất cả các quyền
                    được bảo lưu.
                </div>
                <div class="payment-methods">
                    <span class="payment-icon"
                        ><i class="fab fa-cc-visa"></i
                    ></span>
                    <span class="payment-icon"
                        ><i class="fab fa-cc-mastercard"></i
                    ></span>
                    <span class="payment-icon"
                        ><i class="fab fa-cc-paypal"></i
                    ></span>
                    <span class="payment-icon"
                        ><i class="fas fa-money-bill-wave"></i
                    ></span>
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
    // Xử lý đăng ký nhận tin ở đây
    console.log("Subscribed with:", email.value);
    alert("Cảm ơn bạn đã đăng ký nhận thông tin từ SmartBuy Mobile!");
    email.value = "";
};
</script>

<style scoped>
.footer {
    background-color: #2d3748;
    color: #e2e8f0;
    padding-top: 3rem;
    margin-top: auto;
}

.footer-content {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 2rem;
    padding-bottom: 2rem;
}

.footer-section {
    display: flex;
    flex-direction: column;
}

.about-text {
    margin: 1rem 0;
    line-height: 1.7;
    color: #cbd5e0;
}

.contact-info {
    margin-top: 1rem;
}

.contact-item {
    display: flex;
    align-items: center;
    margin-bottom: 0.75rem;
}

.contact-item i {
    margin-right: 0.75rem;
    color: var(--primary-color);
}

.footer-title {
    font-size: 1.2rem;
    margin-bottom: 1.5rem;
    position: relative;
    font-weight: 600;
}

.footer-title::after {
    content: "";
    position: absolute;
    left: 0;
    bottom: -10px;
    width: 40px;
    height: 2px;
    background-color: var(--primary-color);
}

.footer-links {
    list-style: none;
    padding: 0;
}

.footer-links li {
    margin-bottom: 0.75rem;
}

.footer-links a {
    color: #cbd5e0;
    transition: color 0.3s;
}

.footer-links a:hover {
    color: var(--primary-color);
}

.newsletter p {
    margin-bottom: 1rem;
    color: #cbd5e0;
}

.newsletter-form {
    display: flex;
    margin-bottom: 1.5rem;
}

.newsletter-form input {
    flex: 1;
    padding: 0.75rem;
    border: none;
    border-radius: 4px 0 0 4px;
    outline: none;
}

.newsletter-form button {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 0 1rem;
    border-radius: 0 4px 4px 0;
}

.social-links {
    display: flex;
    gap: 1rem;
}

.social-links a {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    background-color: rgba(255, 255, 255, 0.1);
    border-radius: 50%;
    color: white;
    transition: all 0.3s;
}

.social-links a:hover {
    background-color: var(--primary-color);
    transform: translateY(-3px);
}

.footer-bottom {
    background-color: #1a202c;
    padding: 1.5rem 0;
}

.footer-bottom .container {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.payment-methods {
    display: flex;
    gap: 1rem;
}

.payment-icon {
    font-size: 1.5rem;
}

.logo img {
    height: 40px;
    filter: brightness(0) invert(1);
}

@media (max-width: 768px) {
    .footer-bottom .container {
        flex-direction: column;
        gap: 1rem;
        text-align: center;
    }
}
</style>
