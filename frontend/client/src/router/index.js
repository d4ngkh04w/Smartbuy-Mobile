import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/Home.vue";
import ResetPasswordPage from "../views/ResetPasswordPage.vue";

const routes = [
    {
        path: "/",
        name: "home",
        component: HomeView,
    },
    {
        path: "/reset-password",
        name: "resetPassword",
        component: ResetPasswordPage,
        meta: { requiresAuth: false },
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

export default router;
