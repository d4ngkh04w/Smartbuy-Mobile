// router/routes.js
import MainLayout from "../layouts/MainLayout.vue";
import BlankLayout from "../layouts/BlankLayout.vue";
import HomePage from "../views/HomePage.vue";
import ProductDetail from '../components/product/ProductDetail.vue'

export const routes = [
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
          title: "SmartBuy Mobile - Hệ thống bán lẻ điện thoại di động",
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
      {
        path: "product/:id", // Thêm route cho chi tiết sản phẩm
        name: "product-detail",
        component: ProductDetail,
        props: true, // Chuyển các params vào component dưới dạng props
        meta: {
          title: "Chi tiết sản phẩm - SmartBuy Mobile",
          requiresAuth: false,
        },
      }
      // Các route khác với MainLayout...
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
      // Có thể thêm các trang khác không cần header/footer...
    ],
  },
];
