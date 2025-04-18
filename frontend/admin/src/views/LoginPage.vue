<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/authStore";

const router = useRouter();
const authStore = useAuthStore();

const form = {
  email: ref(""),
  password: ref(""),
};

const loading = ref(false);

const login = async () => {
  if (!form.email.value || !form.password.value) {
    notification.showError("Vui lòng nhập email và mật khẩu");
    return;
  }

  try {
    loading.value = true;

    // TODO: Replace with real API call
    setTimeout(() => {
      authStore.setAdmin({
        id: 1,
        email: form.email.value,
        name: "Admin User",
      });
      notification.showSuccess("Đăng nhập thành công");
      router.push("/dashboard");
      loading.value = false;
    }, 1000);
  } catch (error) {
    notification.showError(
      error.response?.data?.message || "Đăng nhập thất bại"
    );
    loading.value = false;
  }
};
</script>

<template>
  <div class="login-container">
    <div class="login-box">
      <div class="logo-section">
        <img src="../assets/logo.png" alt="Smartbuy Logo" class="logo" />
        <h1>SmartBuy Admin</h1>
      </div>

      <div class="form-group">
        <label for="email">Email</label>
        <input
          id="email"
          type="email"
          v-model="form.email"
          placeholder="Nhập email của bạn"
          :disabled="loading"
        />
      </div>

      <div class="form-group">
        <label for="password">Mật khẩu</label>
        <input
          id="password"
          type="password"
          v-model="form.password"
          placeholder="Nhập mật khẩu"
          :disabled="loading"
        />
      </div>

      <button class="login-button" @click="login" :disabled="loading">
        <span v-if="loading" class="spinner">
          <i class="fas fa-spinner fa-spin"></i>
        </span>
        <span v-else>Đăng nhập</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: #f5f5f5;
}

.login-box {
  width: 100%;
  max-width: 400px;
  padding: 2rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.logo-section {
  text-align: center;
  margin-bottom: 2rem;
}

.logo {
  width: 80px;
  margin-bottom: 0.5rem;
}

h1 {
  font-size: 1.5rem;
  color: #333;
  margin: 0;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

input:focus {
  outline: none;
  border-color: #3498db;
}

.login-button {
  width: 100%;
  padding: 0.75rem;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
}

.login-button:hover {
  background-color: #2980b9;
}

.login-button:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
