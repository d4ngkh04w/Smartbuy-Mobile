import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/authStore.js"; // Import at the top level but don't use it yet

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

// Navigation guard
router.beforeEach((to, from, next) => {
    // Get the auth store instance when the guard runs
    const authStore = useAuthStore();

    const admin = authStore.admin;
    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

    // Nếu route yêu cầu xác thực và chưa đăng nhập, chuyển hướng đến trang đăng nhập
    if (requiresAuth && !admin) {
        next("/login");
    }
    // Nếu đã đăng nhập và đang cố truy cập trang login, chuyển hướng đến dashboard
    else if (to.path === "/login" && admin) {
        next("/dashboard");
    }
    // Các trường hợp khác, cho phép truy cập
    else {
        next();
    }
});

export default router;
