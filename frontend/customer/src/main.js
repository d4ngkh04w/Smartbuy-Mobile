import { createApp } from "vue";
import App from "./App.vue";
import "./assets/base.css"; 
import router from "./router";
import vue3GoogleLogin from "vue3-google-login";
import VueLazyload from 'vue-lazyload';
const app = createApp(App);
app.use(VueLazyload, {
    preLoad: 1.3,
    error: '/error-image.png', // Đường dẫn ảnh lỗi
    attempt: 3, // Số lần thử tải
    observer: true, // Sử dụng IntersectionObserver
    observerOptions: {
      rootMargin: '20px',
      threshold: 0.1
    }
})
app.use(router);
app.use(vue3GoogleLogin, {
    clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
});
app.mount("#app");
