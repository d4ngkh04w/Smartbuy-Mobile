<template>
  <div class="home-container">
    <div class="slideShow-container">
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
    </div>
    <div class="home-content">
      <div class="sideBar"></div>
      <div>
        <div class="products-container">
          <template v-if="products.length > 0">
            <ProductCard
              v-for="product in products"
              :key="product.id"
              :product="product"
            />
          </template>
          <p v-else>Không có sản phẩm nào để hiển thị.</p>
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
import { ref, onMounted } from 'vue';

const products = ref([]);
const currentPage = ref(1);
const pageSize = ref(10);
const totalProducts = ref(0);

const slides = [
  {
    id: 1,
    image: 'https://deviet.vn/wp-content/uploads/2019/04/vuong-quoc-anh.jpg',
    title: 'Slide 1',
    link: '#',
  },
  {
    id: 2,
    image: 'https://aphoto.vn/wp-content/uploads/2018/02/anh-chup-dien-thoai-dep.jpg',
    title: 'Slide 2',
    link: '#',
  },
  {
    id: 3,
    image: 'https://aphoto.vn/wp-content/uploads/2018/02/anh-dep-chup-va-blend-bang-dien-thoai-9.jpg',
    title: 'Slide 3',
    link: '#',
  },
  {
    id: 4,
    image: 'https://cdn.thoitiet247.edu.vn/wp-content/uploads/2024/05/anh-cute-ngang-1.jpg',
    title: 'Slide 4',
    link: '#',
  },
];

const fetchProducts = async () => {
  const data = await getProducts(currentPage.value, pageSize.value);
  if (!data) {
    alert('Không thể tải sản phẩm. Vui lòng thử lại sau!');
    return;
  }
  products.value = data.items;
  totalProducts.value = data.totalItems;
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
  gap: 20px;
  width: 100%;
  padding: 10px;
  box-sizing: border-box;
}
.products-container > * {
  background-color: #fff;
  border: 1px solid #ddd;
  border-radius: 5px;
  overflow: hidden;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}
.products-container > *:hover {
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
}
.slideShow-container {
  width: 100%;
  height: 400px;
  margin-bottom: 30px; /* Tăng khoảng cách giữa carousel và nội dung */
  overflow: hidden;
}
.carousel-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 8px;
  transition: transform 0.3s ease-in-out;
}
.carousel-image:hover {
  transform: scale(1.05); /* Hiệu ứng phóng to khi hover */
}
</style>