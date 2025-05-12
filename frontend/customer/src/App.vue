<script setup>
import { RouterView } from "vue-router";
import Notification from "../src/components/common/Notification.vue";
</script>

<template>
    <div class="app-container">
        <!-- Đơn giản chỉ hiển thị RouterView, các layout sẽ được định nghĩa trong router -->
        <RouterView />
        <Notification />
    </div>
</template>

<style>

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
