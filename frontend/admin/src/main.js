import { createApp } from "vue";
import "./style.css";
import App from "./App.vue";
import router from "./router/index.js";
import pinia from "./stores"; // Import Pinia instance from stores

const app = createApp(App);

app.use(pinia);
app.use(router);
app.mount("#app");
