import { createRouter, createWebHistory } from "vue-router";
import { authService } from "../services/authService.js";
const routes = [
    {
        path: "/login",
        name: "login",
        component: () => import("../views/LoginPage.vue"),
    },
    {
        path: "/dashboard",
        name: "dashboard",
        component: () => import("../views/DashboardPage.vue"),
        meta: { requiresAuth: true },
    },
    {
        path: "/reports",
        name: "reports",
        component: () => import("../views/ReportsPage.vue"),
        meta: { requiresAuth: true },
    },
    {
        path: "/accounts",
        name: "accounts",
        component: () => import("../views/AccountsPage.vue"),
        meta: { requiresAuth: true },
    },
    {
        path: "/",
        redirect: "/dashboard",
    },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (to.hash) {
            return {
                el: to.hash,
                behavior: "smooth",
            };
        } else if (savedPosition) {
            return savedPosition;
        } else {
            return { top: 0 };
        }
    },
});
router.beforeEach(async (to, from, next) => {
    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

    if (requiresAuth) {
        try {
            // Kiểm tra xác thực người dùng
            await authService.getMe();

            // Nếu đã xác thực và đang cố truy cập trang login, chuyển hướng về dashboard
            if (to.path === "/login") {
                next("/dashboard");
            } else {
                next();
            }
        } catch (error) {
            // Nếu là lỗi 401, thiết lập biến cờ để biết rằng đang xử lý refresh token
            if (error.response?.status === 401) {
                // Không làm gì cả và cho phép tiếp tục điều hướng
                // Axios interceptor sẽ tự động xử lý việc refresh token

                // Kiểm tra xem có cờ đánh dấu lỗi refresh token không
                if (localStorage.getItem("token_refresh_failed")) {
                    // Nếu đã có lỗi refresh token trước đó, xóa cờ và chuyển hướng đến trang login
                    localStorage.removeItem("token_refresh_failed");
                    if (to.path !== "/login") {
                        next("/login");
                    } else {
                        next();
                    }
                } else {
                    // Cho phép tiếp tục điều hướng, axios interceptor sẽ xử lý refresh token
                    next();
                }
            } else {
                // Nếu là lỗi khác (không phải 401), chuyển hướng đến trang login
                console.log("Lỗi xác thực không phải 401:", error);
                if (to.path !== "/login") {
                    next("/login");
                } else {
                    next();
                }
            }
        }
    } else {
        // Tiếp tục nếu không cần xác thực
        next();
    }
});

export default router;
