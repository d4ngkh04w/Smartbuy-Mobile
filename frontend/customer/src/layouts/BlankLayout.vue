<template>
    <div class="blank-layout">
        <main class="content">
            <router-view />
        </main>
    </div>
</template>

<script setup>
// Không cần import header và footer
</script>

<style scoped>
.blank-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

.content {
    flex: 1;
}
</style>
