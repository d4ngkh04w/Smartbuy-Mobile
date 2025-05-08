<template>
  <div class="home-page">
      <section class="hero-section">
          <div class="container">
              <h1 class="hero-title">Chào mừng đến với SmartBuy Mobile</h1>
              <p class="hero-subtitle">
                  Cửa hàng điện thoại và phụ kiện chính hãng
              </p>
          </div>
      </section>

      <section class="featured-products">
          <div class="container">
              <h2 class="section-title">Sản phẩm nổi bật</h2>
              <div class="products-placeholder">
                  <p>Đang tải sản phẩm...</p>
              </div>
          </div>
      </section>

      <section class="categories">
          <div class="container">
              <h2 class="section-title">Danh mục sản phẩm</h2>
              <div class="category-grid">
                  <div class="category-card">
                      <div class="category-icon">📱</div>
                      <h3>Điện thoại</h3>
                  </div>
                  <div class="category-card">
                      <div class="category-icon">🎧</div>
                      <h3>Phụ kiện</h3>
                  </div>
                  <div class="category-card">
                      <div class="category-icon">⌚</div>
                      <h3>Đồng hồ thông minh</h3>
                  </div>
                  <div class="category-card">
                      <div class="category-icon">🔋</div>
                      <h3>Pin dự phòng</h3>
                  </div>
              </div>
          </div>
      </section>

      <section class="promotion">
          <div class="container">
              <h2 class="section-title">Ưu đãi đặc biệt</h2>
              <div class="promotion-banner">
                  <div class="promotion-content">
                      <h3>Giảm giá 15%</h3>
                      <p>Cho tất cả sản phẩm Apple</p>
                      <button class="btn-primary">Xem ngay</button>
                  </div>
              </div>
          </div>
      </section>
  </div>
</template>

<script setup>
// Component logic here
</script>

<style scoped>
.home-page {
  padding-bottom: 3rem;
}

.hero-section {
  background-color: var(--primary-color);
  color: white;
  padding: 4rem 0;
  text-align: center;
  margin-bottom: 3rem;
}

.hero-title {
  font-size: 2.5rem;
  margin-bottom: 1rem;
}

.hero-subtitle {
  font-size: 1.2rem;
  opacity: 0.9;
}

.section-title {
  font-size: 1.8rem;
  margin-bottom: 2rem;
  text-align: center;
  position: relative;
}

.section-title:after {
  content: "";
  position: absolute;
  bottom: -10px;
  left: 50%;
  transform: translateX(-50%);
  width: 60px;
  height: 3px;
  background-color: var(--primary-color);
}

.featured-products,
.categories,
.promotion {
  margin-bottom: 3rem;
}

.products-placeholder {
  background-color: #f0f0f0;
  padding: 3rem;
  text-align: center;
  border-radius: 8px;
}

.category-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 2rem;
}

.category-card {
  background-color: white;
  border-radius: 8px;
  padding: 2rem;
  text-align: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s;
}

.category-card:hover {
  transform: translateY(-5px);
}

.category-icon {
  font-size: 2.5rem;
  margin-bottom: 1rem;
}

.promotion-banner {
  background-color: #f9f0ff;
  background-image: linear-gradient(135deg, #f9f0ff 0%, #e0c3fc 100%);
  padding: 3rem;
  border-radius: 8px;
  text-align: center;
}

.btn-primary {
  background-color: var(--primary-color);
  color: white;
  border: none;
  padding: 0.8rem 1.5rem;
  border-radius: 4px;
  font-weight: 500;
  margin-top: 1rem;
  transition: background-color 0.3s;
}

.btn-primary:hover {
  background-color: var(--primary-hover);
}

@media (max-width: 768px) {
  .hero-section {
      padding: 3rem 0;
  }

  .hero-title {
      font-size: 2rem;
  }

  .category-grid {
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  }
}
</style>