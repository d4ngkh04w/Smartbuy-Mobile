<template>
  <div class="cart-container">
    <h2 class="cart-title">Giỏ hàng của bạn</h2>

    <div v-if="cartItems.length > 0" class="cart-grid">
      <!-- Danh sách sản phẩm -->
      <div class="cart-items">
        <!-- Chọn tất cả -->
        <div class="select-all">
          <input type="checkbox" v-model="selectAll" @change="toggleSelectAll" />
          <label>Chọn tất cả</label>
        </div>

        <!-- Sản phẩm -->
        <div class="cart-item" v-for="item in cartItems" :key="item.id">
          <input type="checkbox" v-model="selectedItems" :value="item.id" />

          <img :src="getImage(item.imageUrl)" alt="product" />
          <div class="item-info">
            <h3>{{ item.name }}</h3>
            <p>Giá: {{ format.formatPrice(item.price) }}₫</p>
            <div class="quantity-control">
              <button @click="decreaseQuantity(item)">−</button>
              <span>{{ item.quantity }}</span>
              <button @click="increaseQuantity(item)">＋</button>
            </div>
          </div>
          <button class="remove-btn" @click="removeItem(item.id)">
            <i class="fa-solid fa-trash-can"></i>
          </button>
        </div>
      </div>

      <!-- Tóm tắt đơn hàng -->
      <div class="cart-summary">
        <div class="summary-box">
          <h3>Tóm tắt đơn hàng</h3>
          <p><strong>Đã chọn:</strong> {{ selectedItems.length }} sản phẩm</p>
          <p>
            <strong>Tổng tiền:</strong>
            {{ format.formatPrice(totalSelectedPrice) }}₫
          </p>
          <button class="checkout-btn" @click="checkout">Thanh toán</button>
        </div>
      </div>
    </div>

    <div v-else class="cart-empty">
        <img src="../../../emty-cart.gif" alt="empty" />
        <p>Chưa có gì trong giỏ hàng!</p>
        <router-link to="/" class="shop-now-btn">Mua sắm ngay thôi nào</router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import format from '@/utils/format.js'
import { getUrlImage } from '@/services/productService'
import { getCarts } from "../../services/productService.js"
const cartItems = ref([
  { id: 1, name: 'iPhone 14 Pro Max', price: 32990000, quantity: 1, imageUrl: 'iphone14.jpg' },
  { id: 2, name: 'Samsung Galaxy Buds', price: 2990000, quantity: 2, imageUrl: 'buds.jpg' },
   { id: 3, name: 'iPhone 14 Pro Max', price: 32990000, quantity: 1, imageUrl: 'iphone14.jpg' },
  { id: 4, name: 'Samsung Galaxy Buds', price: 2990000, quantity: 2, imageUrl: 'buds.jpg' },
   { id:5, name: 'iPhone 14 Pro Max', price: 32990000, quantity: 1, imageUrl: 'iphone14.jpg' },
  { id: 6, name: 'Samsung Galaxy Buds', price: 2990000, quantity: 2, imageUrl: 'buds.jpg' },
   { id: 7, name: 'iPhone 14 Pro Max', price: 32990000, quantity: 1, imageUrl: 'iphone14.jpg' },
  { id: 8, name: 'Samsung Galaxy Buds', price: 2990000, quantity: 2, imageUrl: 'buds.jpg' }
])

const getCartItems = () => {
  getCarts()
}
getCartItems()

const selectedItems = ref(cartItems.value.map(item => item.id))
const selectAll = ref(true)

const totalSelectedPrice = computed(() => {
  return cartItems.value
    .filter(item => selectedItems.value.includes(item.id))
    .reduce((sum, item) => sum + item.price * item.quantity, 0)
})

function getImage(url) {
  return getUrlImage(url)
}

function removeItem(id) {
  cartItems.value = cartItems.value.filter(item => item.id !== id)
  selectedItems.value = selectedItems.value.filter(i => i !== id)
}

function toggleSelectAll() {
  if (selectAll.value) {
    selectedItems.value = cartItems.value.map(item => item.id)
  } else {
    selectedItems.value = []
  }
}

watch(selectedItems, val => {
  selectAll.value = val.length === cartItems.value.length
})

function increaseQuantity(item) {
  item.quantity++
}

function decreaseQuantity(item) {
  if (item.quantity > 1) {
    item.quantity--
  }
}

function checkout() {
  if (selectedItems.value.length === 0) {
    alert('Vui lòng chọn ít nhất một sản phẩm để thanh toán.')
    return
  }
  alert('Đang chuyển đến trang thanh toán...')
}
</script>

<style scoped>
.cart-container {
  max-width: 1200px;
  margin: 40px auto;
  padding: 32px 24px;
  background: #eeeff0;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.05);
}

.cart-title {
  font-size: 30px;
  font-weight: 700;
  color: var(--primary-color);
  width: fit-content;
  margin: 0 auto;
  border-radius: 8px;
  padding: 15px 15px;
  text-align: center;
  z-index: 10;
  border-bottom: 2px solid var(--border-color);
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.cart-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 24px;
}

.cart-items {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.select-all {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 18px;
  background: #ffffff;
  border-radius: 10px;
  font-weight: 500;
  color: #1f2937;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.cart-item {
  display: flex;
  align-items: center;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
  padding: 16px;
  gap: 16px;
  transition: transform 0.2s ease;
}

.cart-item:hover {
  transform: scale(1.01);
}

.cart-item img {
  width: 100px;
  height: 100px;
  object-fit: cover;
  border-radius: 10px;
  border: 1px solid #e5e7eb;
}

.item-info {
  flex: 1;
}

.item-info h3 {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 6px;
  color: #111827;
}

.item-info p {
  margin: 0;
  font-size: 15px;
  color: #374151;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 10px;
}

.quantity-control button {
  width: 28px;
  height: 28px;
  font-size: 18px;
  font-weight: bold;
  background: #e5e7eb;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.2s;
}

.quantity-control button:hover {
  background: #d1d5db;
}

.remove-btn {
  background: none;
  border: none;
  color: #ef4444;
  font-size: 20px;
  cursor: pointer;
  transition: color 0.2s ease;
}

.remove-btn:hover {
  color: #b91c1c;
}

.cart-summary {
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
  padding: 24px;
  height: fit-content;
  position: sticky;
  top: 100px;
}

.summary-box h3 {
  font-size: 20px;
  margin-bottom: 12px;
  font-weight: 700;
  color: #111827;
}

.summary-box p {
  font-size: 16px;
  margin-bottom: 8px;
  color: #374151;
}

.checkout-btn {
  margin-top: 16px;
  width: 100%;
  padding: 12px;
  background-color: var(--primary-color);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.checkout-btn:hover {
  background-color: var(--secondary-color);
  color: var(--text-color);
}
.cart-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 16px;
  background-color: #fefefe;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  color: #4b5563;
  text-align: center;
}

.cart-empty img {
  width: 280px;
  max-width: 90%;
  margin-bottom: 20px;
  display: block;
}

.cart-empty p {
  margin: 4px 0;
  font-size: 18px;
}

.cart-empty p:first-of-type {
  font-weight: 600;
  color: #1f2937;
  font-size: 20px;
}


/* Responsive */
@media (max-width: 768px) {
  .cart-grid {
    grid-template-columns: 1fr;
  }

  .cart-item {
    flex-direction: column;
    align-items: flex-start;
  }

  .cart-item img {
    width: 100%;
    height: auto;
  }

  .cart-summary {
    position: relative;
    top: unset;
    transform: unset;
  }
}
.shop-now-btn {
  margin-top: 16px;
  padding: 10px 20px;
  background-color: var(--primary-color);
  color: white;
  border-radius: 8px;
  font-weight: 600;
  text-decoration: none;
  transition: background-color 0.3s;
}

.shop-now-btn:hover {
  background-color: var(--secondary-color);
  color: var(--text-color);
}

input[type="checkbox"] {
  /* Đảm bảo rằng checkbox không có đường viền và làm cho nó sạch sẽ */
  appearance: none;
  width: 20px;
  height: 20px;
  border: 2px solid var(--primary-color);
  border-radius: 4px;
  position: relative;
}

input[type="checkbox"]:checked {
  background-color: var(--primary-color);
  border-color: var(--primary-color);
}

input[type="checkbox"]:checked::before {
  content: '\2713'; /* Dấu tích */
  position: absolute;
  top: 2px;
  left: 5px;
  font-size: 14px;
  color: white;
}


</style>
