<template>
    <router-link 
      :to="{ name: 'product-detail', params: { id: product.id } }"
      class="product-card"
    >
      <div
        class="product-card"
        :style="{
          '--card-bg': cardColor,
          '--button-bg': buttonColor,
          '--height': height
        }"
      >
        <div class="product-top">
          <img 
            v-lazy="getUrlImg(product.imageUrl)" 
            alt="product image" 
            class="product-img"
          />
        </div>
    
        <div class="product-middle">
          <h3>{{ product.name }}</h3>
          <p>{{ format.formatPrice(product.price) }}₫</p>
        </div>
    
        <div class="product-bottom">
          <button class="add-cart-btn" @click="handleAddToCart">
            <i class="fa-solid fa-cart-plus"></i>
          </button>
          <button class="buy-now-btn" @click="handleBuy">
            Mua ngay
          </button>
        </div>
      </div>
    </router-link>
  </template>
  
  <script setup>
  import { defineProps } from 'vue'
  import { getUrlImage } from '../../services/productService.js'
  import format from '@/utils/format.js'
  
  const props = defineProps({
    product: {
      type: Object,
      required: true
    },
    cardColor: {
      type: String,
      default: '#fff'
    },
    buttonColor: {
      type: String,
      default: '#3b82f6'
    },
    height: {
      type: String,
      default: '400px'
    }
  })
  
  // Hàm xử lý khi click "Mua ngay"
  function handleBuy() {
    console.log('Mua ngay:', props.product.name)
  }
  
  // Hàm xử lý khi click "Thêm vào giỏ"
  function handleAddToCart() {
    console.log('Thêm vào giỏ:', props.product.name)
  }
  
  
  // Hàm lấy link ảnh
  const getUrlImg = (url) => {
      return getUrlImage(url)
  }
  </script>

  <style scoped>
.product-card {
  width: 230px;
  height: var(--height);
  background-color: var(--card-bg);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  transition: transform 0.3s;
}

.product-card:hover {
  transform: translateY(-4px);
}

.product-top {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 8px;
}

.product-img {
  width: 100%;
  height: 220px;
  object-fit: contain;
  border-bottom: 1px solid #e0e0e0;
}

.product-middle {
  padding: 8px;
  text-align: center;
  background-color: #fff;
}

.product-middle h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-color, #333);
  margin-bottom: 8px;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  height: 24px;
}

.product-middle p {
  font-size: 16px;
  font-weight: 500;
  color: #FF7043;
  margin: 0;
}

.product-bottom {
  display: flex;
  justify-content: space-between;
  padding: 8px;
  background-color: #fff;
}

/* Nút “Thêm vào giỏ” — outline style */
.add-cart-btn {
  flex: 1;
  margin-right: 4px;
  padding: 8px;
  font-size: 14px;
  font-weight: 600;
  border: 2px solid var(--primary-color);
  border-radius: 8px;
  background-color: transparent;
  color: var(--primary-color);
  cursor: pointer;
  transition: background-color 0.3s, color 0.3s, transform 0.2s;
}

.add-cart-btn:hover {
  background-color: var(--secondary-color);
  color: #fff;
  transform: scale(1.05);
}

/* Nút “Mua ngay” — filled style */
.buy-now-btn {
  flex: 2;
  margin-left: 4px;
  padding: 8px;
  font-size: 14px;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  background-color: var(--primary-color);
  color: #fff;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(248, 110, 211, 0.4);
  transition: background-color 0.3s, transform 0.2s;
}

.buy-now-btn:hover {
  background-color: #e358c2; /* đậm hơn 1 chút */
  transform: scale(1.05);
}
.product-card {
  display: block;
  text-decoration: none;
  color: inherit;
}
</style>
