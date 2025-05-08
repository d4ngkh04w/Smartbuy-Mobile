 <template>
  <div class="home-container">
    <!-- Slide hình ảnh thu nhỏ -->
    <div class="promotion-carousel">
      <div class="carousel-wrapper">
        <Carousel :items-to-show="1" :autoplay="4000" wrap-around>
          <Slide v-for="(slide, index) in slides" :key="index">
            <img class="carousel-image" :src="slide.image" :alt="'Slide ' + index" />
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
      <div class="sideBar " ref="sidebarRef">
        <div class="filter-section">
          <h3>Thương hiệu</h3>
          <div class="brand-list">
            <div 
              v-for="brand in brands" 
              :key="brand.id"
              class="brand-item"
              :class="{ active: selectedBrand === brand.id }"
              @click="selectBrand(brand.id)"
            >
              {{ brand.name }}
            </div>
          </div>
            <label class="label">Mức giá</label>

            <Slider
              v-model="priceRange"
              :min="0"
              :max="6000000"
              :step="1000000"
              :tooltip="false"
              :lazy="true"
              :format="(val) => val.toLocaleString('vi-VN')"
              class="price-slider flat"
            />

            <div class="input-boxes">
              <input type="text" :value="priceRange[0].toLocaleString('vi-VN')" readonly />
              <input type="text" :value="priceRange[1].toLocaleString('vi-VN')" readonly />
            </div>

            <button class="apply-btn" @click="applyPriceFilter">ÁP DỤNG</button>
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
            :class="{ active: sortBy === option.value }"
            @click="changeSort(option.value)"
          >
            {{ option.label }}
          </div>
        </div>

        <!-- Danh sách sản phẩm -->
        <div class="products-container">
          <template v-if="hasProducts">
            <ProductCard
              v-for="product in filteredProducts"
              :key="product.id"
              :product="product"
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
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import ProductCard from '../components/product/ProductCard.vue'
import Pagi from '../components/Pagination.vue'
import { getProducts } from '../services/productService.js'
import { Carousel, Slide, Pagination, Navigation } from 'vue3-carousel'
import '@vueform/slider/themes/default.css'
import Slider from '@vueform/slider'
import 'vue3-carousel/dist/carousel.css'

const priceRange = ref([0, 60000000])
const sidebarRef = ref(null)

// Dữ liệu slides
const slides = ref([
  { image: 'https://cdn.hoanghamobile.com/i/home/Uploads/2025/04/02/iphone-16-series-w.png' },
  { image: 'https://cdn.hoanghamobile.com/i/home/Uploads/2025/05/05/a06-a16-a26-5g-1200x375.jpg' },
  { image: 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Banner/bb/8d/bb8dfe11adb6e77d6383d7cbea2e12ab.png' }
])

// State quản lý sản phẩm
const products = ref([])
const currentPage = ref(1)
const pageSize = ref(20)
const totalProducts = ref(0)
const selectedBrand = ref('')
const sortBy = ref('relevant')

// Dữ liệu filter
const brands = ref([
  { id: 'samsung', name: 'SAMSUNG' },
  { id: 'apple', name: 'Apple' },
  { id: 'xiaomi', name: 'Xiaomi' },
  { id: 'oppo', name: 'OPPO' }
])

const sortOptions = ref([
  { value: 'relevant', label: 'Liên quan' },
  { value: 'newest', label: 'Mới nhất' },
  { value: 'bestselling', label: 'Bán chạy' },
  { value: 'price', label: 'Giá tiền' }
])
const applyPriceFilter = () => {
  console.log('Khoảng giá đã chọn:', priceRange.value)
  fetchProducts(1) // hoặc gọi API có kèm priceRange.value[0], priceRange.value[1]
}


// Computed properties
const hasProducts = computed(() => products.value?.length > 0)
const filteredProducts = computed(() => {
  let result = [...products.value]
  
  // Lọc theo thương hiệu
  if (selectedBrand.value) {
    result = result.filter(p => p.brand === selectedBrand.value)
  }
  
  // Sắp xếp
  switch (sortBy.value) {
    case 'newest':
      return result.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
    case 'bestselling':
      return result.sort((a, b) => b.sold - a.sold)
    case 'price':
      return result.sort((a, b) => a.price - b.price)
    default:
      return result
  }
})

// Methods
const fetchProducts = async (page = 1) => {
  currentPage.value = page
  const data = await getProducts(currentPage.value, pageSize.value)
  if (!data) {
    alert('Không thể tải sản phẩm. Vui lòng thử lại sau!')
    return
  }
  products.value = data.products.items
  totalProducts.value = data.products.totalItems
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const selectBrand = (brandId) => {
  selectedBrand.value = selectedBrand.value === brandId ? '' : brandId
}

const changeSort = (option) => {
  sortBy.value = option
}

onMounted(fetchProducts)
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})

const handleClickOutside = (event) => {
  if (sidebarRef.value && !sidebarRef.value.contains(event.target)) {
    selectedBrand.value = ''
  }
}

</script>

<style scoped>
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
  width: 200px;
  background: #fff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
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
  flex-direction: column;
  gap: 10px;
}

.brand-item {
  padding: 8px 12px;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.brand-item:hover {
  background: #f5f5f5;
}

.brand-item.active {
  background: var(--primary-color);
  color: white;
}

.main-content {
  flex: 1;
}

.sort-options {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
  padding: 15px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.sort-title {
  font-weight: 600;
  color: #555;
}

.sort-option {
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.sort-option:hover {
  background: #f5f5f5;
}

.sort-option.active {
  background: var(--primary-color);
  color: white;
}

.products-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
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
  }
  
  .sort-options {
    flex-wrap: wrap;
  }
  
  .carousel-image {
    height: 180px;
  }
}
.filter-box {
  background: white;
  border-radius: 10px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
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
  --slider-handle-shadow: 0 0 0 1px rgba(0,0,0,0.1);
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
  box-shadow: 0 1px 2px rgba(0,0,0,0.1) !important;
  top: -6px !important;
}

:deep(.slider-tooltip) {
  display: none !important;
}
</style>