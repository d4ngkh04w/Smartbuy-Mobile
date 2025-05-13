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
						>
							{{ brand.name }}
						</div>
					</div>

					<h3 class="price-filter-title">Mức giá</h3>
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

					<div class="filter-actions">
						<button class="reset-btn" @click="resetFilters">
							<i class="fas fa-sync-alt"></i> Đặt lại bộ lọc
						</button>
					</div>
				</div>
			</div>

			<div class="main-content">
				<!-- Phần sắp xếp -->
				<div class="sort-container">
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
								<span v-else-if="sortBy === 'priceDesc'"
									>↓</span
								>
							</span>
						</div>
					</div>

					<div
						class="result-summary"
						v-if="!isLoading && hasProducts"
					>
						<span
							>Hiển thị {{ products.length }} trên
							{{ totalProducts }} sản phẩm</span
						>
					</div>
				</div>

				<!-- Danh sách sản phẩm -->
				<div class="products-container">
					<div v-if="isLoading" class="loading">
						<div class="spinner"></div>
						<span>Đang tải sản phẩm...</span>
					</div>
					<template v-else-if="hasProducts">
						<ProductCard
							v-for="product in products"
							:key="product.id"
							:product="product"
							class="product-card"
						/>
					</template>
					<div v-else class="no-products">
						<i class="fas fa-search"></i>
						<p>Không tìm thấy sản phẩm phù hợp</p>
						<button class="reset-search-btn" @click="resetFilters">
							Xóa bộ lọc
						</button>
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
import { ref, computed, onMounted, onBeforeUnmount, watch } from "vue";
import ProductCard from "../components/product/ProductCard.vue";
import Pagi from "../components/Pagination.vue";
import { getProducts, getBrands } from "../services/productService.js";
import { Carousel, Slide, Pagination, Navigation } from "vue3-carousel";
import "@vueform/slider/themes/default.css";
import Slider from "@vueform/slider";
import "vue3-carousel/dist/carousel.css";
import { useRoute, useRouter } from "vue-router";

const route = useRoute();
const router = useRouter();
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
const sortBy = ref("newest");
const clickCount = ref(0);
const isLoading = ref(true);

const brands = ref([]);

const sortOptions = ref([
	{ value: "newest", label: "Mới nhất" },
	{ value: "bestselling", label: "Bán chạy" },
	{ value: "price", label: "Giá tiền" },
]);

const applyPriceFilter = () => {
	fetchProducts();
};

// Reset all filters
const resetFilters = () => {
	selectedBrand.value = null;
	priceRange.value = [0, 60000000];
	sortBy.value = "newest";
	searchKeyword.value = null;
	currentPage.value = 1;
	router.push({ path: "/" });
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

const fetchBrands = async () => {
	const data = await getBrands();
	if (!data) {
		alert("Không thể tải thương hiệu. Vui lòng thử lại sau!");
		return;
	}
	brands.value = data.data;
};

const fetchProducts = async (page = 1) => {
	isLoading.value = true;
	currentPage.value = page;
	const data = await getProducts(currentPage.value, pageSize.value, {
		brand: selectedBrand.value,
		minPrice: priceRange.value[0],
		maxPrice: priceRange.value[1],
		sortBy: sortBy.value,
		search: searchKeyword.value,
	});
	if (!data) {
		alert("Không thể tải sản phẩm. Vui lòng thử lại sau!");
		return;
	}
	products.value = data.data.items;
	totalProducts.value = data.data.totalItems;
	isLoading.value = false;
	window.scrollTo({ top: 0, behavior: "smooth" });
};

const selectBrand = (brandName) => {
	selectedBrand.value = selectedBrand.value === brandName ? null : brandName;
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
.home-container {
	max-width: 1400px;
	margin: 0 auto;
	padding: 0 20px;
}

.promotion-carousel {
	width: 100%;
	margin-bottom: 30px;
}

.carousel-wrapper {
	width: 100%;
	max-width: 1200px;
	margin: 0 auto;
	border-radius: 12px;
	overflow: hidden;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.carousel-image {
	width: 100%;
	height: 380px;
	object-fit: cover;
}

.home-content {
	display: flex;
	gap: 24px;
	margin-bottom: 50px;
}

.sideBar {
	width: 250px;
	background: #fff;
	border-radius: 12px;
	padding: 20px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
	height: fit-content;
	position: sticky;
	top: 20px;
}

.filter-section {
	display: flex;
	flex-direction: column;
}

.filter-section h3 {
	font-size: 18px;
	margin-bottom: 15px;
	color: #333;
	font-weight: 600;
	border-bottom: 2px solid #f5f5f5;
	padding-bottom: 8px;
}

.price-filter-title {
	margin-top: 20px;
}

.brand-list {
	display: flex;
	flex-direction: column;
	gap: 8px;
	margin-bottom: 10px;
	max-height: 220px;
	overflow-y: auto;
	padding-right: 5px;
}

.brand-list::-webkit-scrollbar {
	width: 5px;
}

.brand-list::-webkit-scrollbar-thumb {
	background: #ddd;
	border-radius: 10px;
}

.brand-item {
	padding: 10px 12px;
	border-radius: 8px;
	cursor: pointer;
	transition: all 0.2s;
	font-size: 15px;
	display: flex;
	align-items: center;
}

.brand-item:hover {
	background: #f8f0f4;
	color: var(--primary-color);
}

.brand-item.active {
	background: var(--primary-color);
	color: white;
	font-weight: 500;
}

.main-content {
	flex: 1;
}

.sort-container {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 20px;
}

.sort-options {
	display: flex;
	align-items: center;
	gap: 15px;
	padding: 15px;
	background: #fff;
	border-radius: 10px;
	box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
}

.sort-title {
	font-weight: 600;
	color: #555;
}

.sort-option {
	position: relative;
	padding: 8px 16px;
	border-radius: 6px;
	cursor: pointer;
	transition: all 0.2s;
	display: flex;
	align-items: center;
	gap: 4px;
	font-weight: 500;
}

.sort-option:hover {
	background: #f8f0f4;
	color: var(--primary-color);
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

.result-summary {
	font-size: 14px;
	color: #666;
	padding: 0 10px;
}

.products-container {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
	gap: 24px;
	margin-bottom: 40px;
}

.no-products {
	grid-column: 1 / -1;
	text-align: center;
	padding: 60px 40px;
	color: #666;
	background: white;
	border-radius: 12px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 15px;
}

.no-products i {
	font-size: 40px;
	color: #ccc;
}

.no-products p {
	font-size: 18px;
	margin: 0;
}

.reset-search-btn {
	background: var(--primary-color);
	color: white;
	border: none;
	padding: 10px 20px;
	border-radius: 8px;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
}

.reset-search-btn:hover {
	background: #e358c2;
}

.input-boxes {
	display: flex;
	justify-content: space-between;
	gap: 10px;
	margin: 10px 0 15px;
}

.input-boxes input {
	width: 100%;
	padding: 8px 10px;
	border: 1px solid #eee;
	border-radius: 8px;
	text-align: center;
	font-weight: 500;
	font-size: 14px;
	color: #333;
	background: #f9f9f9;
}

.apply-btn {
	width: 100%;
	padding: 12px 0;
	background-color: var(--primary-color);
	color: white;
	font-weight: 600;
	border: none;
	border-radius: 8px;
	cursor: pointer;
	transition: 0.3s;
	font-size: 15px;
}

.apply-btn:hover {
	background-color: #e358c2;
	transform: translateY(-2px);
}

.filter-actions {
	margin-top: 20px;
	display: flex;
	justify-content: center;
}

.reset-btn {
	background: transparent;
	color: #666;
	border: 1px solid #eee;
	border-radius: 6px;
	padding: 8px 12px;
	font-size: 14px;
	cursor: pointer;
	transition: all 0.2s;
	display: flex;
	align-items: center;
	gap: 6px;
}

.reset-btn:hover {
	background: #f8f0f4;
	color: var(--primary-color);
	border-color: var(--primary-color);
}

/* Custom slider styling */
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

/* Loading state styling */
.loading {
	grid-column: 1 / -1;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 16px;
	height: 300px;
	font-size: 16px;
	color: #666;
	background: white;
	border-radius: 12px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.spinner {
	width: 40px;
	height: 40px;
	border: 3px solid rgba(248, 110, 211, 0.2);
	border-top-color: var(--primary-color);
	border-radius: 50%;
	animation: spin 0.8s linear infinite;
}

@keyframes spin {
	to {
		transform: rotate(360deg);
	}
}

/* Responsive styles */
@media (max-width: 1024px) {
	.products-container {
		grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
		gap: 16px;
	}
}

@media (max-width: 768px) {
	.home-content {
		flex-direction: column;
	}

	.sideBar {
		width: 100%;
		position: static;
	}

	.carousel-image {
		height: 220px;
	}

	.sort-container {
		flex-direction: column;
		align-items: stretch;
		gap: 10px;
	}

	.sort-options {
		justify-content: center;
		flex-wrap: wrap;
	}

	.result-summary {
		text-align: center;
	}

	.products-container {
		grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
		gap: 12px;
	}
}

@media (max-width: 480px) {
	.home-container {
		padding: 0 12px;
	}

	.carousel-image {
		height: 180px;
	}

	.filter-section h3 {
		font-size: 16px;
	}

	.sort-option {
		padding: 6px 10px;
		font-size: 14px;
	}

	.products-container {
		grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
		gap: 10px;
	}
}
</style>
