<template>
    <footer class="footer">
      <div class="footer-container">
        <!-- Thông tin liên hệ -->
        <div class="footer-section">
          <h3> <i style="color: rgb(236, 87, 172);">SMARTBUY MOBILE</i></h3>
          <ul>
            <li><a href="#">Giới thiệu</a></li>
            <li><a href="#">Chính sách bảo mật</a></li>
            <li><a href="#">Truyền thông</a></li>
          </ul>
        </div>
  
        <!-- Chính sách -->
        <div class="footer-section">
          <h3>HỖ TRỢ KHÁCH HÀNG</h3>
          <ul>
            <li><a href="#">Trung tâm trợ giúp</a></li>
            <li><a href="#">An toàn mua bán</a></li>
            <li><a href="#">Liên hệ hỗ trợ</a></li>
          </ul>
        </div>
  
        <!-- Liên kết nhanh -->
        <div class="footer-section">
          <h3>LIÊN KẾT</h3>
          <ul>
            <li><a href="#"><img src="../assets/image/google.png" alt="Google icon" class="icon-link"></a></li>
            <li><a href="#"><img src="../assets/image/facebook.png" alt="Google icon" class="icon-link"></a></li>
          </ul>
        </div>
      </div>
    </footer>
  </template>
  
  <style scoped>
  .footer {
    background-color: var(--secondary-color);
    color: var(--text-color);
    padding: 20px 0;
  }
  
  .footer-container {
    display: flex;
    justify-content:space-around;
    max-width: 100%;
    margin: 0 auto;
    padding: 0 20px;
    flex-wrap: wrap;
  }
  
  .footer-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 30%;
  }
  
  .footer-section h3 {
    margin-bottom: 10px;
  }
  
  .footer-section ul {
    list-style: none;
    padding: 0;
  }
  
  .footer-section ul li {
    margin-bottom: 5px;
  }
  
  .footer-section ul li a {
    color: var(--text-color);
    text-decoration: none;
  }
  
  .footer-section ul li a:hover {
    text-decoration: underline;
  }
  
  /* Responsive */
  @media (max-width: 768px) {
    .footer-container {
      flex-direction: column;
      text-align: center;
    }
  
    .footer-section {
      width: 100%;
      margin-bottom: 20px;
    }
  }
  .icon-link{
    width: 30px;
  }
  .footer-section ul li a:hover {
    color: var(--primary-color); /* Đổi màu chữ khi hover */
    text-decoration: underline;
    transition: color 0.3s ease-in-out;
  }

  /* Hiệu ứng hover cho icon */
  .icon-link:hover {
    filter: brightness(1.2); /* Tăng độ sáng khi hover */
    transition: filter 0.3s ease-in-out;
  }

  </style>
  