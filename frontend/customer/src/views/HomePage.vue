<template>
    <div class="home-container">
        <!-- Slide hình ảnh thu nhỏ -->
        <div class="promotion-carousel">
            <div class="carousel-wrapper">
                <Carousel :items-to-show="1" :autoplay="4000" wrap-around>
                    <Slide v-for="(slide, index) in slides" :key="index">
                        <img
                            class="carousel-image"
                            :src="slide.image"
                            :alt="'Slide ' + index"
                        />
                    </Slide>
                    <template #addons>
                        <Navigation />
                        <Pagination />
                    </template>
                </Carousel>
            </div>
        </div>

        <!-- Nội dung trang -->
        <div class="home-content">
            <!-- Sidebar mới -->
            <div class="sideBar">
                <div class="filter-section">
                    <h3>Thương hiệu</h3>
                    <div class="brand-list">
                        <div
                            v-for="brand in activeBrands"
                            :key="brand.id"
                            class="brand-item"
                            :class="{ active: selectedBrand === brand.name }"
                            @click="selectBrand(brand.name)"
                            :title="brand.name"
                        >
                            <img
                                v-if="brand.logo"
                                :src="productService.getUrlImage(brand.logo)"
                                :alt="brand.name"
                                class="brand-logo"
                                loading="lazy"
                            />
                            <div class="brand-tooltip">{{ brand.name }}</div>
                        </div>
                    </div>

                    <label class="label">Mức giá</label>
                    <Slider
                        v-model="priceRange"
                        :min="0"
                        :max="60000000"
                        :step="1000000"
                        :tooltip="false"
                        :lazy="true"
                        :format="(val) => val.toLocaleString('vi-VN')"
                        class="price-slider flat"
                    />

                    <div class="input-boxes">
                        <input
                            type="text"
                            :value="priceRange[0].toLocaleString('vi-VN')"
                            readonly
                        />
                        <input
                            type="text"
                            :value="priceRange[1].toLocaleString('vi-VN')"
                            readonly
                        />
                    </div>

                    <button class="apply-btn" @click="applyPriceFilter">
                        ÁP DỤNG
                    </button>
                </div>
            </div>

            <div class="main-content">
                <!-- Phần sắp xếp -->
                <div class="sort-options">
                    <div class="sort-title">Sắp xếp theo:</div>
                    <div
                        v-for="option in sortOptions"
                        :key="option.value"
                        class="sort-option"
                        :class="{
                            active: sortBy === option.value,
                            'price-asc':
                                option.value === 'price' &&
                                sortBy === 'priceInc',
                            'price-desc':
                                option.value === 'price' &&
                                sortBy === 'priceDesc',
                        }"
                        @click="changeSort(option.value)"
                    >
                        {{ option.label }}
                        <span
                            v-if="option.value === 'price'"
                            class="sort-arrow"
                        >
                            <span v-if="sortBy === 'priceInc'">↑</span>
                            <span v-else-if="sortBy === 'priceDesc'">↓</span>
                        </span>
                    </div>
                </div>

                <!-- Product Lines Section -->
                <div
                    v-if="activeProductLines.length > 0"
                    class="product-line-section"
                >
                    <h3>Khám phá danh mục</h3>
                    <div class="product-line-grid">
                        <div
                            v-for="productLine in activeProductLines"
                            :key="productLine.id"
                            class="product-line-card"
                            :class="{
                                active: selectedProductLine === productLine.id,
                            }"
                            @click="selectProductLine(productLine.id)"
                        >
                            <img
                                v-if="productLine.image"
                                :src="
                                    productService.getUrlImage(
                                        productLine.image
                                    )
                                "
                                :alt="productLine.name"
                                class="product-line-card-image"
                                loading="lazy"
                            />
                            <span class="product-line-card-name">{{
                                productLine.name
                            }}</span>
                        </div>
                    </div>
                </div>

                <!-- Brand Description Section -->
                <div class="brand-description-section">
                    <template v-if="selectedProductLineInfo">
                        <h2 class="brand-name">
                            {{ selectedProductLineInfo.name }}
                        </h2>
                        <p class="brand-description">
                            {{
                                selectedProductLineInfo.description ||
                                `Khám phá dòng sản phẩm ${selectedProductLineInfo.name} với nhiều lựa chọn đa dạng, phù hợp với nhu cầu và ngân sách của bạn.`
                            }}
                        </p>
                    </template>
                    <template v-else-if="selectedBrandInfo">
                        <h2 class="brand-name">
                            Điện thoại {{ selectedBrandInfo.name }}
                        </h2>
                        <p class="brand-description">
                            {{
                                selectedBrandInfo.description ||
                                `Sản phẩm ${selectedBrandInfo.name} chính hãng, đa dạng mẫu mã với nhiều lựa chọn phù hợp nhu cầu khách hàng.`
                            }}
                        </p>
                    </template>
                    <template v-else>
                        <h2 class="brand-name">Tất cả điện thoại</h2>
                        <p class="brand-description">
                            SmartBuy - Đa dạng sản phẩm điện thoại thông minh từ
                            các thương hiệu hàng đầu như Apple, Samsung, Xiaomi
                            và nhiều hãng khác. Tìm kiếm sản phẩm phù hợp với
                            nhu cầu và ngân sách của bạn với đầy đủ phân khúc từ
                            cao cấp đến giá rẻ, đi kèm chế độ bảo hành chính
                            hãng.
                        </p>
                    </template>
                </div>

                <!-- Danh sách sản phẩm -->
                <div class="products-container">
                    <Loading v-if="isLoading" />
                    <template v-else-if="hasProducts">
                        <ProductCard
                            v-for="product in products"
                            :key="product.id"
                            :product="product"
                            class="product-card"
                        />
                    </template>
                    <div v-else class="no-products">
                        Không tìm thấy sản phẩm phù hợp
                    </div>
                </div>

                <Pagi
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
import { ref, computed, onMounted, watch } from "vue";
import { Carousel, Slide, Pagination, Navigation } from "vue3-carousel";
import { useRoute } from "vue-router";
import Slider from "@vueform/slider";
import "@vueform/slider/themes/default.css";
import "vue3-carousel/dist/carousel.css";
import ProductCard from "../components/product/ProductCard.vue";
import Pagi from "../components/common/Pagination.vue";
import productService from "../services/productService.js";
import Loading from "../components/common/Loading.vue";

const route = useRoute();
const searchKeyword = ref(route.query.search || null);
const slides = ref([
    {
        image: "https://cdn.hoanghamobile.com/i/home/Uploads/2025/04/02/iphone-16-series-w.png",
    },
    {
        image: "https://cdn.hoanghamobile.com/i/home/Uploads/2025/05/05/a06-a16-a26-5g-1200x375.jpg",
    },
    {
        image: "https://cdnv2.tgdd.vn/mwg-static/tgdd/Banner/bb/8d/bb8dfe11adb6e77d6383d7cbea2e12ab.png",
    },
]);

const priceRange = ref([0, 60000000]);
const products = ref([]);
const currentPage = ref(1);
const pageSize = ref(20);
const totalProducts = ref(0);
const selectedBrand = ref(null);
const selectedProductLine = ref(null);
const sortBy = ref("newest");
const clickCount = ref(0);
const isLoading = ref(true);

const brands = ref([]);
const productLines = ref([]);
const selectedBrandInfo = ref(null);
const selectedProductLineInfo = ref(null);

const sortOptions = ref([
    { value: "newest", label: "Mới nhất" },
    { value: "bestselling", label: "Bán chạy" },
    { value: "price", label: "Giá tiền" },
]);

const applyPriceFilter = () => {
    fetchProducts();
};
watch(
    () => route.query.search,
    async (newVal) => {
        searchKeyword.value = newVal;
        fetchProducts();
    }
);
watch(
    () => route.query.reload,
    async () => {
        selectedBrand.value = null;
        selectedBrandInfo.value = null;
        selectedProductLine.value = null;
        selectedProductLineInfo.value = null;
        productLines.value = [];
        priceRange.value = [0, 60000000];
        sortBy.value = "newest";
        searchKeyword.value = null;
        fetchProducts();
    }
);

const hasProducts = computed(() => products.value?.length > 0);

const activeBrands = computed(() => {
    return brands.value.filter((brand) => brand.isActive == true);
});

const activeProductLines = computed(() => {
    const result = productLines.value.filter(
        (productLine) => productLine.isActive == true
    );
    console.log("activeProductLines computed:", result);
    return result;
});

const fetchBrands = async () => {
    const data = await productService.getBrands();
    if (!data) {
        alert("Không thể tải thương hiệu. Vui lòng thử lại sau!");
        return;
    }
    brands.value = data.data;
};

const fetchProductLinesByBrand = async (brandId) => {
    console.log("fetchProductLinesByBrand called with brandId:", brandId);
    if (!brandId) {
        productLines.value = [];
        return;
    }
    const data = await productService.getProductLinesByBrand(brandId);
    console.log("ProductLines data received:", data);
    if (!data) {
        alert("Không thể tải dòng sản phẩm. Vui lòng thử lại sau!");
        return;
    }
    productLines.value = data.data || [];
    console.log("productLines.value after update:", productLines.value);
};

const fetchProducts = async (page = 1) => {
    isLoading.value = true;
    currentPage.value = page;
    const data = await productService.getProducts(
        currentPage.value,
        pageSize.value,
        {
            brandName: selectedBrand.value,
            productLineId: selectedProductLine.value,
            minPrice: priceRange.value[0],
            maxPrice: priceRange.value[1],
            sortBy: sortBy.value,
            search: searchKeyword.value,
        }
    );
    if (!data) {
        alert("Không thể tải sản phẩm. Vui lòng thử lại sau!");
        return;
    }
    products.value = data.data.items;
    totalProducts.value = data.data.totalItems;
    isLoading.value = false;
    // window.scrollTo({ top: 0, behavior: 'smooth' })
    // console.log('products.value', products.value)
};

const selectBrand = (brandName) => {
    selectedBrand.value = selectedBrand.value === brandName ? null : brandName;
    selectedProductLine.value = null; // Reset product line when brand changes

    // Update selectedBrandInfo when a brand is selected or deselected
    if (selectedBrand.value) {
        selectedBrandInfo.value = brands.value.find(
            (brand) => brand.name === selectedBrand.value
        );
        // Fetch product lines for selected brand
        if (selectedBrandInfo.value) {
            fetchProductLinesByBrand(selectedBrandInfo.value.id);
        }
    } else {
        selectedBrandInfo.value = null;
        productLines.value = [];
    }

    console.log("selectedBrand.value", selectedBrand.value);
    fetchProducts();
};

const selectProductLine = (productLineId) => {
    selectedProductLine.value =
        selectedProductLine.value === productLineId ? null : productLineId;

    // Update selectedProductLineInfo when a product line is selected or deselected
    if (selectedProductLine.value) {
        selectedProductLineInfo.value = productLines.value.find(
            (productLine) => productLine.id === selectedProductLine.value
        );
    } else {
        selectedProductLineInfo.value = null;
    }

    console.log("selectedProductLine.value", selectedProductLine.value);
    fetchProducts();
};

const changeSort = (option) => {
    sortBy.value = option;
    if (option === "price") {
        switch (clickCount.value) {
            case 0:
                sortBy.value = "priceInc";
                clickCount.value++;
                break;
            case 1:
                sortBy.value = "priceDesc";
                clickCount.value = 0;
                break;
        }
    } else {
        clickCount.value = 0;
    }
    fetchProducts();
};

onMounted(() => {
    fetchBrands();
    fetchProducts();
});
</script>

<style scoped>
:root {
    --primary-color-rgb: 255, 64, 180; /* Định nghĩa màu dưới dạng RGB cho các hiệu ứng opacity */
}

.home-container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 15px;
}

.promotion-carousel {
    width: 100%;
    margin-bottom: 30px;
}

.carousel-wrapper {
    width: 100%;
    max-width: 1200px;
    margin: 0 auto;
}

.carousel-image {
    width: 100%;
    height: 300px;
    object-fit: cover;
    border-radius: 8px;
}

.home-content {
    display: flex;
    gap: 20px;
}

.sideBar {
    width: 240px;
    background: #fff;
    border-radius: 8px;
    padding: 15px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    height: 500px;
    overflow-y: auto;
}

.filter-section {
    margin-bottom: 20px;
}

.filter-section h3 {
    font-size: 18px;
    margin-bottom: 15px;
    color: #333;
    font-weight: 600;
}

.brand-list {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-bottom: 20px;
    justify-content: space-between;
}

.brand-item {
    padding: 6px;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
    width: 98px;
    height: 50px;
    background-color: #ffffff;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);
}

.brand-logo {
    max-width: 85%;
    max-height: 80%;
    object-fit: contain;
    display: block;
}

.brand-tooltip {
    position: absolute;
    top: -40px;
    left: 50%;
    transform: translateX(-50%);
    background-color: #333;
    color: white;
    padding: 5px 10px;
    border-radius: 4px;
    font-size: 12px;
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.2s, visibility 0.2s;
    white-space: nowrap;
    z-index: 100;
    pointer-events: none;
}

.brand-tooltip:after {
    content: "";
    position: absolute;
    top: 100%;
    left: 50%;
    margin-left: -5px;
    border-width: 5px;
    border-style: solid;
    border-color: #333 transparent transparent transparent;
}

.brand-item:hover .brand-tooltip {
    opacity: 1;
    visibility: visible;
}

.brand-item:hover {
    border-color: var(--primary-color);
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
}

.brand-item.active {
    border-color: var(--primary-color);
    border-width: 2px;
}

.main-content {
    flex: 1;
}

.sort-options {
    display: flex;
    align-items: center;
    justify-content: end;
    gap: 15px;
    margin-bottom: 20px;
    padding: 15px;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.sort-title {
    font-weight: 600;
    color: #555;
}

.sort-option {
    position: relative;
    padding: 6px 12px;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 4px;
}

.sort-option:hover {
    background: #f5f5f5;
}

.sort-option.active {
    background: var(--primary-color);
    color: white;
}

.sort-option.price-asc,
.sort-option.price-desc {
    background: var(--primary-color);

    color: white;
}

.sort-arrow {
    font-size: 12px;
    margin-left: 4px;
}

.products-container {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 20px;
    margin-bottom: 30px;
    justify-items: start;
}

.no-products {
    grid-column: 1 / -1;
    text-align: center;
    padding: 40px;
    color: #666;
}

@media (max-width: 768px) {
    .home-content {
        flex-direction: column;
    }

    .sideBar {
        width: 100%;
        height: 100px;
    }

    .sort-options {
        justify-content: center;
        flex-wrap: wrap;
    }

    .carousel-image {
        height: 180px;
    }

    .products-container {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 1200px) and (min-width: 769px) {
    .products-container {
        grid-template-columns: repeat(3, 1fr);
    }
}
.filter-box {
    background: white;
    border-radius: 10px;
    padding: 16px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    width: 100%;
}

.label {
    font-weight: bold;
    margin-bottom: 8px;
    display: block;
}

.input-boxes {
    display: flex;
    justify-content: space-between;
    gap: 10px;
    margin-top: 10px;
}

.input-boxes input {
    width: 100%;
    padding: 6px 10px;
    border: 1px solid #ccc;
    border-radius: 5px;
    text-align: center;
    font-weight: bold;
}

.apply-btn {
    width: 100%;
    margin-top: 12px;
    padding: 10px 0;
    background-color: #ff40b4;
    color: white;
    font-weight: bold;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: 0.3s;
}

.apply-btn:hover {
    background-color: #d8359e;
}
.slider-connect {
    background-color: var(--primary-color) !important;
}

.slider-handle {
    box-shadow: none !important;
    border: 2px solid white !important;
}
.flat {
    --slider-handle-shadow: 0 0 0 1px rgba(0, 0, 0, 0.1);
    --slider-handle-ring: none;
}

/* Ghi đè toàn bộ style của slider */
:deep(.slider-connect) {
    background-color: var(--primary-color) !important;
    height: 4px !important;
}

:deep(.slider-base) {
    background-color: #e0e0e0 !important;
    height: 4px !important;
    box-shadow: none !important;
}

:deep(.slider-handle) {
    width: 16px !important;
    height: 16px !important;
    background-color: var(--primary-color) !important;
    border: 2px solid white !important;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1) !important;
    top: -6px !important;
}

:deep(.slider-tooltip) {
    display: none !important;
}

/* Brand description section styles */
.brand-description-section {
    margin: 15px 0 25px 0;
}

.brand-name {
    font-size: 24px;
    font-weight: 600;
    color: #333;
    margin-bottom: 10px;
    border-bottom: 1px solid #eee;
    padding-bottom: 10px;
}

.brand-description {
    color: #555;
    line-height: 1.6;
    font-size: 15px;
    margin: 0;
    text-align: justify;
}

/* Product Line Section in Main Content */
.product-line-section {
    margin-bottom: 20px;
    padding: 20px;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.product-line-section h3 {
    font-size: 18px;
    margin-bottom: 15px;
    color: #333;
    font-weight: 600;
}

.product-line-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 15px;
}

.product-line-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 15px;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s;
    background-color: #ffffff;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);
    text-align: center;
}

.product-line-card:hover {
    border-color: var(--primary-color);
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
    transform: translateY(-2px);
}

.product-line-card.active {
    border-color: var(--primary-color);
    border-width: 2px;
    background-color: rgba(255, 64, 180, 0.05);
}

.product-line-card-image {
    width: 60px;
    height: 60px;
    object-fit: cover;
    border-radius: 8px;
    margin-bottom: 10px;
}

.product-line-card-name {
    font-size: 14px;
    font-weight: 500;
    color: #333;
    line-height: 1.3;
}

.product-line-card.active .product-line-card-name {
    color: var(--primary-color);
    font-weight: 600;
}

@media (max-width: 768px) {
    .product-line-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

/* Product Line styles */
.product-line-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 20px;
}

.product-line-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s;
    background-color: #ffffff;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);
}

.product-line-item:hover {
    border-color: var(--primary-color);
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
}

.product-line-item.active {
    border-color: var(--primary-color);
    border-width: 2px;
    background-color: rgba(255, 64, 180, 0.05);
}

.product-line-image {
    width: 40px;
    height: 40px;
    object-fit: cover;
    border-radius: 6px;
    flex-shrink: 0;
}

.product-line-name {
    font-size: 14px;
    font-weight: 500;
    color: #333;
    line-height: 1.3;
}

.product-line-item.active .product-line-name {
    color: var(--primary-color);
    font-weight: 600;
}
</style>
