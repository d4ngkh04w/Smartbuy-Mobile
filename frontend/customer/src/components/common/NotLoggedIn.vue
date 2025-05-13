<template>
  <div class="not-logged-in-container">
    <div class="animated-background">
      <div class="shape shape-1"></div>
      <div class="shape shape-2"></div>
      <div class="shape shape-3"></div>
    </div>
    
    <div class="auth-card">
      <div class="card-header">
        <div class="icon-wrapper">
          <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"></path>
            <polyline points="10 17 15 12 10 7"></polyline>
            <line x1="15" y1="12" x2="3" y2="12"></line>
          </svg>
        </div>
        <h2>Oops! Bạn cần đăng nhập</h2>
        <p class="subtext">Để tiếp tục trải nghiệm, vui lòng đăng nhập tài khoản của bạn</p>
      </div>
      
      <div class="card-body">
        <button @click="goToLogin" class="login-btn pulse">
          <span>Đăng nhập ngay</span>
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"></path>
            <polyline points="10 17 15 12 10 7"></polyline>
            <line x1="15" y1="12" x2="3" y2="12"></line>
          </svg>
        </button>
        
        <button @click="goHome" class="home-btn">
          <span>Về trang chủ</span>
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
            <polyline points="9 22 9 12 15 12 15 22"></polyline>
          </svg>
        </button>
      </div>
      
      <div class="card-footer">
        <p>Chưa có tài khoản? <span @click="goToRegister" class="register-link">Đăng ký ngay</span></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const router = useRouter();

const goToLogin = () => {
  router.push('/login');
};

const goHome = () => {
  router.push('/');
};

const goToRegister = () => {
  router.push('/register');
};
</script>

<style scoped>
.not-logged-in-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #f86ed3 100%);
  position: relative;
  overflow: hidden;
  padding: 2rem;
}

.animated-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 0;
}

.shape {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
}

.shape-1 {
  width: 300px;
  height: 300px;
  top: -100px;
  left: -100px;
  animation: float 15s infinite ease-in-out;
}

.shape-2 {
  width: 200px;
  height: 200px;
  bottom: -50px;
  right: -50px;
  animation: float 12s infinite ease-in-out reverse;
}

.shape-3 {
  width: 150px;
  height: 150px;
  top: 50%;
  right: 20%;
  animation: float 10s infinite ease-in-out;
}

.auth-card {
  position: relative;
  z-index: 1;
  background: white;
  border-radius: 20px;
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 450px;
  overflow: hidden;
  transform: translateY(0);
  transition: transform 0.3s ease;
}

.auth-card:hover {
  transform: translateY(-5px);
}

.card-header {
  padding: 2.5rem 2rem 1.5rem;
  text-align: center;
  background: linear-gradient(135deg, #f86ed3 0%, #f866d1 100%);
  color: white;
}

.icon-wrapper {
  background: white;
  width: 80px;
  height: 80px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.icon-wrapper svg {
  color: #f86ed3;
}

h2 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
}

.subtext {
  margin: 0.5rem 0 0;
  opacity: 0.9;
  font-size: 0.9rem;
}

.card-body {
  padding: 2rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

button {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 1rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  border: none;
  font-size: 1rem;
}

.login-btn {
  background: linear-gradient(135deg, #f86ed3 0%, #f866d1 100%);

  color: white;
  box-shadow: 0 4px 15px rgba(168, 61, 255, 0.3);
}

.login-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(168, 61, 255, 0.4);
}

.home-btn {
  background: white;
  color: #333;
  border: 1px solid #ddd;
}

.home-btn:hover {
  background: #f5f5f5;
  border-color: #ccc;
}

.card-footer {
  padding: 1rem 2rem 2rem;
  text-align: center;
  font-size: 0.9rem;
  color: #666;
}

.register-link {
  color: #f86ed3;
  font-weight: 600;
  cursor: pointer;
  text-decoration: underline;
  transition: color 0.3s ease;
}

.register-link:hover {
  color: #a93dff;
}

.pulse {
  animation: pulse 2s infinite;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-20px);
  }
}

@keyframes pulse {
  0% {
    box-shadow: 0 0 0 0 rgba(168, 61, 255, 0.4);
  }
  70% {
    box-shadow: 0 0 0 10px rgba(168, 61, 255, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(168, 61, 255, 0);
  }
}

@media (max-width: 480px) {
  .auth-card {
    border-radius: 15px;
  }
  
  .card-header {
    padding: 1.5rem 1rem;
  }
  
  h2 {
    font-size: 1.3rem;
  }
}
</style>