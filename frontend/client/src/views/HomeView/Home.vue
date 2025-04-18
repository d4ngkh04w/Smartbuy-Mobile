<template>
    <div class="home-container">
        <div class="slide-container">
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