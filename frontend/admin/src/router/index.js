import { createRouter, createWebHistory } from "vue-router";
import authService from "../services/authService.js";

// Định nghĩa các routes
const routes = [
	{
		path: "/login",
		name: "login",
		component: () => import("../views/LoginPage.vue"),
		meta: { requiresAuth: false },
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
		path: "/categories",
		name: "categories",
		component: () => import("../views/CategoryPage.vue"),
		meta: { requiresAuth: true },
	},
	{
		path: "/products",
		name: "products",
		component: () => import("../views/ProductsPage.vue"),
		meta: { requiresAuth: true },
	},
	{
		path: "/orders",
		name: "orders",
		component: () => import("../views/OrdersPage.vue"),
		meta: { requiresAuth: true },
	},
	{
		path: "/customers",
		name: "customers",
		component: () => import("../views/CustomersPage.vue"),
		meta: { requiresAuth: true },
	},
	{
		path: "/discounts",
		name: "discounts",
		component: () => import("../views/DiscountsPage.vue"),
		meta: { requiresAuth: true },
	},
	{
		path: "/",
		redirect: "/login",
	},
	// Catch all route for 404
	{
		path: "/:catchAll(.*)",
		redirect: "/login",
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

// Navigation guards
router.beforeEach(async (to, from, next) => {
	const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

	if (requiresAuth) {
		try {
			await authService.verifyAdmin();
			next();
		} catch (error) {
			if (error.response?.status != 401) {
				console.error("Lỗi xác thực:", error);
				next("/login");
			} else {
				next("/login");
			}
		}
	} else {
		next();
	}
});

export default router;
