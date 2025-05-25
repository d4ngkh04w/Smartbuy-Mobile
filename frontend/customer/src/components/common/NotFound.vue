<template>
  <div class="not-found-container">
    <div class="not-found-content">
      <div class="error-code" @mouseover="wiggle">404</div>
      <h1 class="error-title">Oops! Trang không tồn tại</h1>
      <p class="error-message">
        Xin lỗi, trang bạn đang tìm kiếm đã "tan biến" như mây hồng...
      </p>
      <div class="action-buttons">
        <router-link 
          to="/" 
          class="home-button"
          @mouseenter="pulse"
        >
          <i class="fas fa-home"></i> Về trang chủ
        </router-link>
      </div>
    </div>
    <!-- Floating hearts decoration -->
    <div class="heart" v-for="i in 8" :key="i" :style="heartStyle(i)"></div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

// Hiệu ứng lắc cho số 404
const wiggle = (e) => {
  e.target.classList.add('wiggle');
  setTimeout(() => e.target.classList.remove('wiggle'), 1000);
};

// Hiệu ứng pulse cho nút
const pulse = (e) => {
  e.target.classList.add('pulse');
  setTimeout(() => e.target.classList.remove('pulse'), 600);
};

// Style ngẫu nhiên cho trái tim
const heartStyle = (i) => {
  return {
    '--size': `${Math.random() * 20 + 10}px`,
    '--delay': `${Math.random() * 5}s`,
    '--duration': `${Math.random() * 10 + 10}s`,
    '--left': `${Math.random() * 100}%`,
    '--color': `hsl(${Math.random() * 30 + 330}, 100%, ${Math.random() * 30 + 70}%)`
  };
};
</script>

<style scoped>
/* Keyframes */
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes wiggle {
  0%, 100% { transform: rotate(0); }
  25% { transform: rotate(-5deg); }
  50% { transform: rotate(5deg); }
  75% { transform: rotate(-3deg); }
}

@keyframes pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); }
}

@keyframes float {
  0% { transform: translateY(100vh) scale(0.5); opacity: 0; }
  50% { opacity: 0.7; }
  100% { transform: translateY(-100px) scale(1); opacity: 0; }
}

/* Main styles */
.not-found-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #fff0f5 0%, #ffe6eb 100%);
  padding: 20px;
  overflow: hidden;
  position: relative;
}

.not-found-content {
  text-align: center;
  max-width: 600px;
  padding: 40px;
  background: rgba(255, 255, 255, 0.95);
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(255, 105, 180, 0.2);
  position: relative;
  z-index: 2;
  animation: fadeIn 0.8s ease-out forwards;
  backdrop-filter: blur(3px);
  border: 1px solid rgba(255, 182, 193, 0.3);
}

.error-code {
  font-size: 120px;
  font-weight: bold;
  color: #ff6b9d;
  line-height: 1;
  margin-bottom: 20px;
  text-shadow: 3px 3px 0 rgba(255, 107, 157, 0.1);
  transition: all 0.3s;
  display: inline-block;
}

.error-code.wiggle {
  animation: wiggle 0.7s ease;
}

.error-title {
  font-size: 28px;
  color: #d23369;
  margin-bottom: 15px;
  font-weight: 700;
}

.error-message {
  font-size: 16px;
  color: #9e4a6b;
  margin-bottom: 30px;
  line-height: 1.6;
}

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 15px;
}

.home-button {
  padding: 12px 25px;
  border-radius: 50px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.3s ease;
  background: linear-gradient(45deg, #ff6b9d, #ff8fab);
  color: white;
  text-decoration: none;
  border: none;
  box-shadow: 0 4px 15px rgba(255, 107, 157, 0.3);
}

.home-button:hover {
  background: linear-gradient(45deg, #ff8fab, #ff6b9d);
  transform: translateY(-3px);
  box-shadow: 0 6px 20px rgba(255, 107, 157, 0.4);
}

.home-button.pulse {
  animation: pulse 0.6s ease;
}

/* Floating hearts */
.heart {
  position: absolute;
  width: var(--size);
  height: var(--size);
  background: var(--color);
  border-radius: 50%;
  left: var(--left);
  bottom: -50px;
  opacity: 0;
  animation: float var(--duration) linear var(--delay) infinite;
  z-index: 1;
  filter: blur(1px);
}

@media (max-width: 576px) {
  .error-code {
    font-size: 80px;
  }
  
  .error-title {
    font-size: 22px;
  }
  
  .not-found-content {
    padding: 30px 20px;
    margin: 0 15px;
  }
}
</style>