<<<<<<< HEAD
<template>
	<div
		class="product-card"
		:style="{
			'--card-bg': cardColor,
			'--button-bg': buttonColor,
			'--height': height,
		}"
	>
		<img :src="product.image" alt="image" class="product-img" />
		<div class="product-info">
			<h3>{{ product.title }}</h3>
			<p>{{ product.price }}₫</p>
			<button @click="handleBuy">Mua ngay</button>
			<button @click="handleAddToCart">Thêm vào giỏ hàng</button>
		</div>
	</div>
</template>

<script setup>
defineProps({
	product: Object,
	cardColor: {
		type: String,
		default: "var(--secondary-color, #fff)",
	},
	buttonColor: {
		type: String,
		default: "var(--background-color, #35AFFB)",
	},
	height: {
		type: String,
		default: "250px",
	},
});
const handleAddToCart = () => {
	// thay thế bằng component thông báo
	alert("Đã thêm sản phẩm vào giỏ hàng!");
};
const handleBuy = () => {
	// xử lý mua hàng
};
</script>

<style scoped>
.product-card {
	width: 200px;
	height: var(--height);
	border-radius: 10px;
	background-color: var(--card-bg);
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
	overflow: hidden;
	transition: transform 0.2s ease, box-shadow 0.3s ease;
}
.product-card:hover {
	transform: translateY(-5px);
	box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}
.product-img {
	width: 100%;
	height: 150px;
	object-fit: cover;
}
.product-info {
	padding: 12px;
	text-align: center;
}
.product-info h3 {
	font-size: 18px;
	color: var(--text-color, #333);
}
.product-info p {
	font-size: 16px;
	margin: 5px 0;
	color: var(--price-color, #000);
}
button {
	background-color: var(--button-bg);
	color: #fff;
	border: none;
	border-radius: 5px;
	padding: 10px 20px;
	font-size: 14px;
	cursor: pointer;
	transition: background-color 0.3s ease;
}
button:hover {
	background-color: var(--button-hover-bg, #0078d4);
=======
   <template>
    <div
      class="product-card"
      :style="{
        '--card-bg': cardColor,
        '--button-bg': buttonColor,
        '--height': height
      }"
    >
      <div class="product-top">
        <img :src="getUrlImage(product.imageUrl)" alt="image" class="product-img" />
      </div>
  
      <div class="product-middle">
        <h3>{{ product.name }}</h3>
        <p>{{ formatCurrency(product.price) }}₫</p>
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
  </template>
  
  <script setup>
  import { defineProps } from 'vue'
  
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
  
  // Hàm format tiền tệ
  function formatCurrency(value) {
    return value.toLocaleString('vi-VN')
  }
  
  // Hàm lấy link ảnh
  const getUrlImage = (url) => {
      let newUrlImage = url.startsWith('http') ? url : "http://localhost:5000" + `/${url}`;
      return newUrlImage;
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
>>>>>>> feature/customer-ui
}
</style>
