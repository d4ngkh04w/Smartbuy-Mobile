import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./assets/base.css"; // Import CSS chung
import GoogleLoginPlugin from "vue3-google-login";

const app = createApp(App);
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
app.use(GoogleLoginPlugin, {
    clientId: googleClientId,
});
app.use(router);
app.mount("#app");
