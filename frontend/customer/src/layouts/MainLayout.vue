<template>
    <div class="main-layout">
        <TheHeader />
        <main class="content">
            <router-view />
        </main>
        <TheFooter />
        <!-- Chatbot AI -->
        <Chatbot />
    </div>
</template>

<script setup>
import TheHeader from "../components/layout/TheHeader.vue";
import TheFooter from "../components/layout/TheFooter.vue";
import Chatbot from "../components/Chatbot.vue";
</script>

<style scoped>
.main-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

.content {
    flex: 1;
    margin: 20px;
}
</style>
