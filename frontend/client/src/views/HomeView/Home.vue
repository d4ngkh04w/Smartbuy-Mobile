<template>
  <div class="home-container">
    <!-- <div class="slideShow-container">
      <Carousel
        :items-to-show="1"
        :autoplay="3000"
        wrap-around
        :pause-autoplay-on-hover="true"
        navigation
      >
        <Slide v-for="item in slides" :key="item.id">
          <a :href="item.link">
            <img :src="item.image" :alt="item.title" class="carousel-image" />
          </a>
        </Slide>
      </Carousel>
    </div> -->
    <div class="home-content">
      <div class="sideBar"></div>
      <div>
        <div class="products-container">
          <template v-if="hasProducts"> 
            <ProductCard
              v-for="product in products"
              :key="product.id"
              :product="product"
            />
          </template>
        </div>
        <Pagination
          :totalProducts="totalProducts"
          :currentPage="currentPage"
          :pageSize="pageSize"
          @pageChanged="fetchProducts"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
  import ProductCard from '../../components/ProductCard.vue';
  import Pagination from '../../components/Pagination.vue';
  import { getProducts } from '../../services/productService';
  import { ref, onMounted, computed } from 'vue';

  const products = ref([]);
  const currentPage = ref(1);
  const pageSize = ref(20);
  const totalProducts = ref(0);
  const hasProducts = computed(() => products.value?.length > 0);

  const fetchProducts = async (page = 1) => {
    currentPage.value = page;
    const data = await getProducts( currentPage.value , pageSize.value);
    if (!data) {
      alert('Không thể tải sản phẩm. Vui lòng thử lại sau!');
      return;
    }
    products.value = data.products.items;
    totalProducts.value = data.products.totalItems;
    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  };

  onMounted(fetchProducts);
</script>

<style scoped>

.products-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); /* Responsive grid */
  gap: 24px; 
  width: 100%;
  padding: 16px; 
  box-sizing: border-box;
  background-color: #f9f9f9; 
  border-radius: 8px; 
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05); 
}

/* Individual product card */
.products-container > * {
  background-color: #fff;
  border: 1px solid #e0e0e0; /* Màu viền nhẹ */
  border-radius: 10px; /* Bo góc cho sản phẩm */
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1); /* Hiệu ứng đổ bóng */
  transition: transform 0.3s ease, box-shadow 0.3s ease; /* Hiệu ứng hover mượt */
}

/* Hover effect for product card */
.products-container > *:hover {
  transform: translateY(-5px); /* Hiệu ứng nổi lên khi hover */
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15); /* Tăng đổ bóng khi hover */
  cursor: pointer;
}

/* Slideshow container */
.slideShow-container {
  width: 100%;
  height: 450px; 
  margin-bottom: 40px; /* Tăng khoảng cách giữa carousel và nội dung */
  overflow: hidden;
  border-radius: 12px; /* Bo góc cho container carousel */
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); /* Hiệu ứng đổ bóng */
}

/* Carousel image */
.carousel-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 12px; /* Bo góc cho hình ảnh */
  transition: transform 0.4s ease-in-out; /* Hiệu ứng hover mượt */
}

/* Hover effect for carousel image */
.carousel-image:hover {
  transform: scale(1.08); /* Hiệu ứng phóng to khi hover */
}

/* Pagination container */
.pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 20px;
}

/* Add smooth scrolling for better UX */
html {
  scroll-behavior: smooth;
}

.no-products {
  text-align: center; /* Căn giữa nội dung */
  font-size: 18px; /* Tăng kích thước chữ */
  font-weight: 500; /* Tăng độ đậm */
  color: #888; /* Màu chữ nhẹ nhàng */
  margin-top: 40px; /* Tăng khoảng cách phía trên */
  padding: 20px; /* Thêm khoảng cách bên trong */
  background-color: #f9f9f9; /* Màu nền nhẹ */
  border-radius: 8px; /* Bo góc */
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1); /* Hiệu ứng đổ bóng nhẹ */
}
</style> 

