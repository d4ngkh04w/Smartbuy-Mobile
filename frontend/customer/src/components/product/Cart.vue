<template>
  <div class="cart-container">
    <h2>Giỏ hàng</h2>

    <div v-if="cartItems.length === 0">
      <p>Giỏ hàng của bạn đang trống.</p>
    </div>

    <div v-else>
      <div class="cart-item" v-for="item in cartItems" :key="item.id">
        <img :src="getUrlImage(item.imageUrl)" alt="product" class="item-img" />

        <div class="item-info">
          <h3>{{ item.name }}</h3>
          <p>{{ formatPrice(item.price) }}₫</p>
          <div class="quantity-control">
            <button @click="decreaseQuantity(item)">-</button>
            <span>{{ item.quantity }}</span>
            <button @click="increaseQuantity(item)">+</button>
          </div>
        </div>

        <button class="remove-btn" @click="removeItem(item.id)">
          Xoá
        </button>
      </div>

      <div class="cart-summary">
        <p><strong>Tổng tiền:</strong> {{ formatPrice(totalPrice) }}₫</p>
        <button class="checkout-btn" @click="checkout">
          Thanh toán
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { getUrlImage } from '@/services/productService.js'
import format from '@/utils/format.js'

// Dữ liệu giả để demo
const cartItems = ref([
  { id: 1, name: 'Điện thoại A', price: 5000000, quantity: 1, imageUrl: 'a.jpg' },
  { id: 2, name: 'Điện thoại B', price: 3000000, quantity: 2, imageUrl: 'b.jpg' }
])

const formatPrice = (price) => format.formatPrice(price)

const increaseQuantity = (item) => {
  item.quantity++
}

const decreaseQuantity = (item) => {
  if (item.quantity > 1) item.quantity--
}

const removeItem = (id) => {
  cartItems.value = cartItems.value.filter(i => i.id !== id)
}

const totalPrice = computed(() =>
  cartItems.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
)

const checkout = () => {
  alert('Thanh toán thành công!')
}
</script>

<style scoped>
.cart-container {
  padding: 16px;
  max-width: 700px;
  margin: auto;
  background-color: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.cart-item {
  display: flex;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #eee;
}

.item-img {
  width: 80px;
  height: 80px;
  object-fit: contain;
  margin-right: 16px;
}

.item-info {
  flex: 1;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 4px;
}

.quantity-control button {
  padding: 4px 8px;
  background-color: #ddd;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.remove-btn {
  background-color: #e53935;
  color: white;
  padding: 6px 12px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.cart-summary {
  margin-top: 16px;
  text-align: right;
}

.checkout-btn {
  background-color: #43a047;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}
</style>
