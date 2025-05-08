import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./assets/base.css"; // Import CSS chung
import GoogleLoginPlugin from "vue3-google-login";
import pinia from "./stores"; // Import Pinia store

const app = createApp(App);
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
// const facebookAppId = import.meta.env.VITE_FACEBOOK_APP_ID;
app.use(GoogleLoginPlugin, {
    clientId: googleClientId,
});
import VueLazyload from 'vue-lazyload'


app.use(VueLazyload, {
  preLoad: 1.3,
  error: '/error-image.png', // Đường dẫn ảnh lỗi
  loading: '/loading-spinner.gif', // Ảnh loading
  attempt: 3, // Số lần thử tải
  observer: true, // Sử dụng IntersectionObserver
  observerOptions: {
    rootMargin: '20px',
    threshold: 0.1
  }
})
app.use(pinia);
app.use(router);
app.mount("#app");

// window.FB_APP_ID = facebookAppId;
