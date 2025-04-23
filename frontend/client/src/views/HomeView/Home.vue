<!-- <template>
    <div class="home-container">
        <div class="slideShow-container">
        </div>
        <div class="home-content">
            <div class="sideBar"></div>
            <div class="products-container">
                <ProductCard
                    v-for="product in products"
                    :key="product.id"
                    :product="product"
                />
            </div>
            <Pagitation
                :totalProducts="totalProducts"
                :currentPage="currentPage"
                :pageSize="10"
                @pageChanged="fetchProducts"
            />
        </div>
    </div>
</template>

<script setup>
    import ProductCard from '../../components/ProductCard.vue';
    import Pagitation from '../../components/Pagitation.vue';
    import { ref, onMounted } from 'vue';
    import axios from 'axios';
    const products = ref([]);
    const currentPage = ref(1);
    const totalProducts = ref(0);
    const fetchProducts = async () => {
        const res = await axios.get(`http://localhost:3000/api/products?page=${currentPage.value}&pageSize=${pageSize}`)
        products.value = res.data.items
        totalProducts.value = res.data.totalItems 
        window.scrollTo({
            top: 0,
            behavior: 'smooth', 
        });
    }
    onMounted(fetchProducts);
</script>

<style scoped>
    .home-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 20px;
    }
    .slide-container {
        width: 100%;
        height: 300px;
        background-color: #f0f0f0;
        margin-bottom: 20px;
    }
    .home-content {
        display: flex;
        width: 100%;
    }
    .sideBar {
        width: 200px;
        background-color: #f0f0f0;
    }
    .products-container {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-around;
        width: 100%;
    }
    .products-container > * {
        margin: 10px;
    }
    .products-container > *:nth-child(odd) {
        background-color: #f9f9f9;
    }
    .products-container > *:nth-child(even) {
        background-color: #fff;
    }
    .products-container > *:hover {
        background-color: #e0e0e0;
    }
    .products-container > *:active {
        background-color: #d0d0d0;
    }
    .products-container > *:focus {
        outline: none;
        box-shadow: 0 0 5px #007bff;
    } 
</style>
 -->




<!-- fake data -->
<script setup>
import ProductCard from '../../components/ProductCard.vue';
import Pagination from '../../components/Pagitation.vue';
import { ref, onMounted } from 'vue';
import { Carousel, Slide, Navigation } from 'vue3-carousel'
import 'vue3-carousel/dist/carousel.css'; // Đảm bảo bạn đã import CSS của Vue 3 Carousel

const allProducts = Array.from({ length: 52 }, (_, i) => ({
  id: i + 1,
  title: `Sản phẩm ${i + 1}`,
  price: (Math.random() * 1000000).toFixed(0),
}));

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

const products = ref([]);
const currentPage = ref(1);
const pageSize = 20;
const totalProducts = ref(allProducts.length);

const fetchProducts = (page = 1) => {
  currentPage.value = page;
  const start = (page - 1) * pageSize;
  const end = start + pageSize;
  products.value = allProducts.slice(start, end);
  window.scrollTo({
    top: 0,
    behavior: 'smooth', // Smooth scrolling effect
  });
};

onMounted(() => fetchProducts(currentPage.value));
</script>

<template>
  <div class="home-container">
    <div class="slideShow-container">
      <!-- Carousel -->
      <Carousel
        :items-to-show="1"
        :autoplay="3000"
        wrap-around
        :modules="[Navigation]"        
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
          <ProductCard
            v-for="product in products"
            :key="product.id"
            :product="product"
          />
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
  margin-bottom: 20px;
  overflow: hidden;
}
.carousel-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 8px;
}
</style>