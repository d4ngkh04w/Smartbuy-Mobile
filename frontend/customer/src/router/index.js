// router/index.js
import { createRouter, createWebHistory } from "vue-router";
import { routes } from "./routes";
import { authService } from "../services/authService.js";

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
      next(); // Tiếp tục khi xác thực thành công
    } catch (error) {
      if (error.response?.status === 401) {
        next("/");
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
