import { createRouter, createWebHistory } from "vue-router";
import { authService } from "../services/authService.js";

// Import layouts
import MainLayout from "../layouts/MainLayout.vue";
import BlankLayout from "../layouts/BlankLayout.vue";

// Import trang chủ
import HomePage from "../views/HomePage.vue";

const routes = [
    // Routes với MainLayout (có header và footer)
    {
        path: "/",
        component: MainLayout,
        children: [
            {
                path: "",
                name: "home",
                component: HomePage,
                meta: {
                    title: "Trang chủ - SmartBuy Mobile",
                    requiresAuth: false,
                },
            },
            {
                path: "account",
                name: "account",
                component: () => import("../views/AccountPage.vue"),
                meta: {
                    title: "Tài khoản của tôi - SmartBuy Mobile",
                    requiresAuth: true,
                },
            },
            // // Thêm các trang khác sử dụng MainLayout ở đây
            // {
            //     path: "cart",
            //     name: "cart",
            //     component: () => import("../views/CartPage.vue"),
            //     meta: {
            //         title: "Giỏ hàng - SmartBuy Mobile",
            //         requiresAuth: false,
            //     },
            // },
            // {
            //     path: "product/:id",
            //     name: "product-detail",
            //     component: () => import("../views/ProductDetailPage.vue"),
            //     meta: {
            //         title: "Chi tiết sản phẩm - SmartBuy Mobile",
            //         requiresAuth: false,
            //     },
            // },
            // // Các route khác với MainLayout...
        ],
    },

    // Routes với BlankLayout (không có header và footer)
    {
        path: "/",
        component: BlankLayout,
        children: [
            {
                path: "login",
                name: "login",
                component: () => import("../views/LoginPage.vue"),
                meta: {
                    title: "Đăng nhập - SmartBuy Mobile",
                    requiresAuth: false,
                },
            },
            {
                path: "register",
                name: "register",
                component: () => import("../views/RegisterPage.vue"),
                meta: {
                    title: "Đăng ký - SmartBuy Mobile",
                    requiresAuth: false,
                },
            },
            {
                path: "profile-setup",
                name: "profile-setup",
                component: () => import("../views/ProfileSetupPage.vue"),
                meta: {
                    title: "Hoàn tất thông tin - SmartBuy Mobile",
                    requiresAuth: true,
                },
            },
            {
                path: "forgot-password",
                name: "forgot-password",
                component: () => import("../views/ForgotPasswordPage.vue"),
                meta: {
                    title: "Quên mật khẩu - SmartBuy Mobile",
                    requiresAuth: false,
                },
            },
            {
                path: "reset-password",
                name: "reset-password",
                component: () => import("../views/ResetPasswordPage.vue"),
                meta: {
                    title: "Đặt lại mật khẩu - SmartBuy Mobile",
                    requiresAuth: false,
                },
            },
            {
                path: "verify-email",
                name: "verify-email",
                component: () => import("../views/VerifyEmailPage.vue"),
                meta: {
                    title: "Xác thực email - SmartBuy Mobile",
                    requiresAuth: false,
                },
            },
            // // Có thể thêm các trang khác không cần header/footer như trang lỗi
            // {
            //     path: "/:pathMatch(.*)*",
            //     name: "not-found",
            //     component: () => import("../views/NotFoundPage.vue"),
            //     meta: {
            //         title: "Không tìm thấy trang - SmartBuy Mobile",
            //         requiresAuth: false,
            //     },
            // },
        ],
    },
];

// Khởi tạo router
const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (to.hash) {
            return { el: to.hash, behavior: "smooth" };
        }
        return savedPosition || { top: 0 };
    },
});

// Update document title based on route meta
router.beforeEach(async (to, from, next) => {
    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
    document.title = to.meta.title || "SmartBuy Mobile";
    if (requiresAuth) {
        try {
            await authService.verifyUser();
            next(); // Thêm dòng này để tiếp tục khi xác thực thành công
        } catch (error) {
            if (error.response?.status === 401) { // Sửa thành === 401
                next("/login");
            } else {
                console.error("Lỗi xác thực:", error);
                next(); // Vẫn cho phép tiếp tục nếu không phải lỗi 401
            }
        }
    } else {
        next();
    }
});

export default router;
