<script setup>
import { RouterView } from "vue-router";
import Notification from "./components/Notification.vue";
</script>

<template>
    <div class="app-container">
        <main class="main-content">
            <RouterView />
        </main>
        <Notification />
    </div>
</template>

<style scoped>
.app-container {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

.main-content {
    flex: 1;
}
</style>
