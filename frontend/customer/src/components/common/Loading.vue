<template>
    <div class="loading">
        Đang tải...
    </div>
</template>
<style scoped>

.loading {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px; /* hoặc 100vh nếu bạn muốn phủ cả màn */
  font-size: 1.2rem;
  color: #555;
  position: relative;
}

/* Thêm spinner */
.loading::before {
  content: "";
  width: 24px;
  height: 24px;
  border: 3px solid #ccc;
  border-top-color: var(--primary-color);
  border-radius: 50%;
  margin-right: 10px;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>