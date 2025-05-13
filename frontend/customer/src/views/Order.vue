<template>
  <div class="order-container">
    <!-- Thanh lộ trình -->
    <div class="steps">
      <div
        v-for="(label, index) in stepLabels"
        :key="index"
        :class="['step-item', step >= index + 1 ? 'active' : '', step === index + 1 ? 'current' : '']"
        @click="handleStepClick(index + 1)"
      >
        {{ index + 1 }}. {{ label }}
      </div>
    </div>

    <!-- Bước 1: Thông tin -->
    <div v-if="step === 1" class="step-content">
      <h2>Thông tin khách hàng</h2>
      <form @submit.prevent="nextStep">
        <input v-model="form.name" type="text" placeholder="Họ và tên" required />
        <input v-model="form.phone" type="tel" placeholder="Số điện thoại" required />
        <input v-model="form.address" type="text" placeholder="Địa chỉ giao hàng" required />
        <button type="submit" class="btn">Tiếp theo</button>
      </form>
    </div>

    <!-- Bước 2: Thanh toán -->
    <div v-else-if="step === 2" class="step-content">
      <h2>Phương thức thanh toán</h2>
      <form @submit.prevent="submitOrder">
        <label v-for="option in paymentOptions" :key="option.value">
          <input type="radio" v-model="form.payment" :value="option.value" required />
          {{ option.label }}
        </label>
        <button type="submit" class="btn">Xác nhận đơn hàng</button>
      </form>
    </div>

    <!-- Bước 3: Hoàn tất -->
    <div v-else-if="step === 3" class="step-content">
      <h2>Đặt hàng thành công</h2>
      <div class="invoice">
        <p><strong>Khách hàng:</strong> {{ form.name }}</p>
        <p><strong>SĐT:</strong> {{ form.phone }}</p>
        <p><strong>Địa chỉ:</strong> {{ form.address }}</p>
        <p><strong>Thanh toán:</strong> {{ paymentText }}</p>
        <p><strong>Tổng tiền:</strong> 12.000.000đ</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, computed } from 'vue';

const step = ref(1);
const stepLabels = ['Thông tin', 'Thanh toán', 'Hoàn tất'];
const form = reactive({
  name: '',
  phone: '',
  address: '',
  payment: '',
});

const paymentOptions = [
  { label: 'Thanh toán khi nhận hàng (COD)', value: 'cod' },
  { label: 'Chuyển khoản Ngân hàng', value: 'bank' },
  { label: 'MoMo / VNPay / ZaloPay', value: 'momo' },
];

const paymentText = computed(() => {
  return paymentOptions.find(opt => opt.value === form.payment)?.label || '';
});

function nextStep() {
  if (form.name && form.phone && form.address) step.value = 2;
}

function submitOrder() {
  if (form.payment) step.value = 3;
}

function handleStepClick(targetStep) {
  if (targetStep < step.value) {
    step.value = targetStep;
  }
}
</script>

<style scoped>
.order-container {
  max-width: 600px;
  margin: auto;
  padding: 24px;
  background: #fff0f6;
  border-radius: 16px;
  font-family: 'Segoe UI', sans-serif;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.steps {
  display: flex;
  justify-content: space-between;
  margin-bottom: 24px;
}

.step-item {
  flex: 1;
  text-align: center;
  padding: 10px;
  cursor: pointer;
  font-weight: bold;
  color: #aaa;
  border-bottom: 3px solid #ccc;
  transition: all 0.3s;
}

.step-item.active {
  color: #d63384;
  border-color: #d63384;
}

.step-item.current {
  font-size: 18px;
}

.step-content {
  animation: fade 0.3s ease-in-out;
}

input[type="text"],
input[type="tel"] {
  width: 100%;
  margin-bottom: 16px;
  padding: 10px;
  border: 1px solid #d63384;
  border-radius: 8px;
}

label {
  display: block;
  margin-bottom: 12px;
  color: #444;
}

.btn {
  background: #d63384;
  color: #fff;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  margin-top: 12px;
}

.invoice {
  background: white;
  padding: 20px;
  border-radius: 10px;
  margin-top: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

@keyframes fade {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: none;
  }
}

@media (max-width: 600px) {
  .steps {
    flex-direction: column;
    gap: 8px;
  }
  .step-item {
    border: none;
    border-left: 4px solid #ccc;
    text-align: left;
    padding-left: 12px;
  }
  .step-item.active {
    border-color: #d63384;
  }
}
</style>
