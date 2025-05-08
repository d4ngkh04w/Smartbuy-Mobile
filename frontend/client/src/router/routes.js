import Home from '@/views/HomeView/Home.vue';
import ResetPasswordPage from "../views/ResetPasswordPage.vue";
import ProductDetail from "../components/ProductDetail.vue"

const routes = [
    {
        path: "/",
        name: "home",
        component: Home,
    },
    {
        path: "/reset-password",
        name: "resetPassword",
        component: ResetPasswordPage,
        meta: { requiresAuth: false },
    },
    {
      path: '/product/:id',
      name: 'product-detail',
      component: ProductDetail,
      props: true 
    }
];

export default routes
