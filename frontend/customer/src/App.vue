<script setup>
import { RouterView } from "vue-router";
import Notification from "./components/Notification.vue";
</script>

<template>
    <div class="app-container">
        <!-- Đơn giản chỉ hiển thị RouterView, các layout sẽ được định nghĩa trong router -->
        <RouterView />
        <Notification />
    </div>
</template>

<style>
:root {
    --primary-color: #e85db3;
    --primary-hover: #d64da1;
    --secondary-color: #4a5568;
    --accent-color: #38b2ac;
    --light-bg: #f9f9f9;
    --border-color: #e2e8f0;
    --text-primary: #1a202c;
    --text-secondary: #4a5568;
    --text-muted: #718096;
    --danger: #e53e3e;
    --success: #48bb78;
    --warning: #ed8936;
    --info: #4299e1;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: "Roboto", sans-serif;
    line-height: 1.6;
    color: var(--text-primary);
    background-color: var(--light-bg);
}

.app-container {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

a {
    text-decoration: none;
    color: inherit;
}

.container {
    width: 100%;
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 1rem;
}

button {
    cursor: pointer;
    font-family: inherit;
}

img {
    max-width: 100%;
    height: auto;
}
</style>
