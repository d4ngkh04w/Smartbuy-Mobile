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
			await authService.verifyAdmin();

			// Nếu đã xác thực và đang cố truy cập trang login, chuyển hướng về dashboard
			if (to.path === "/login") {
				next("/dashboard");
			} else {
				next();
			}
		} catch (error) {
			if (error.response?.status === 401) {
				throw error; // Ném lại lỗi để xử lý ở interceptor
			} else {
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
