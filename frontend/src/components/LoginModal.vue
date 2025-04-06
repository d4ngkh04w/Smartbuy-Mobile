<script setup>
    import { ref } from "vue";
    import { login, register } from "../services/authService.js"; 
    const phoneNumber = ref('');
    const password = ref('');
    const password_confirm = ref('');
    const email = ref('');

    const errorMessage = ref('');
    const successMessage = ref('');

    const handleLogin = async () => {
      errorMessage.value = ''
      try {
        // Gửi thông tin đăng nhập tới backend
        const response = await login({
          username: phoneNumber.value,
          password: password.value
        })
        
        // Lưu token JWT vào localStorage
        const token = response.data.token
        localStorage.setItem('jwt', token)

        successMessage.value = 'Đăng nhập thành công!'
        
        // Có thể redirect hoặc thực hiện hành động sau khi đăng nhập thành công
        // ví dụ: router.push('/dashboard') hoặc emit sự kiện đóng modal
      } catch (err) {
        errorMessage.value = 'Sai tài khoản hoặc mật khẩu.'
        console.error(err)
      }
    }
    const handleRegister = async () => {
      // Reset lỗi cũ
      errorMessage.value = ''
      successMessage.value = ''

      // Kiểm tra nếu mật khẩu và xác nhận mật khẩu khớp
      if (password.value !== passwordConfirm.value) {
        errorMessage.value = 'Mật khẩu không khớp.'
        return
      }

      try {
        // Gửi dữ liệu đăng ký tới backend
        const response = await register({
          username: phoneNumber.value,
          password: password.value,
          email: email.value
        })

        // Nếu đăng ký thành công, bạn có thể đăng nhập người dùng ngay (nếu cần)
        const token = response.data.token
        localStorage.setItem('jwt', token)

        successMessage.value = 'Đăng ký thành công!'

        // Có thể chuyển hướng người dùng đến trang khác sau khi đăng ký thành công
        // router.push('/dashboard') hoặc emit('close') để đóng modal
      } catch (err) {
        errorMessage.value = 'Đã có lỗi xảy ra khi đăng ký.'
        console.error(err)
      }
    }

    const isRightPanelActive = ref(false);

    const showRegister = () => {
    isRightPanelActive.value = true;
    };

    const showLogin = () => {
    isRightPanelActive.value = false;
};
</script>

<template>
    <div class="modal-overlay" @click.self="$emit('close')">
      <div :class="['container', { 'right-panel-active': isRightPanelActive }]">
          <!-- Đăng kí -->
          <div class="form-container register-container">
            <form action="#">
              <h1>Đăng ký</h1>
              <input type="text" placeholder="Số điện thoại" v-model="phoneNumber" />
              <input type="password" placeholder="Mật khẩu" v-model="password" />
              <input type="password" placeholder="Xác nhận mật khẩu" v-model="password_confirm" />
              <input type="email" placeholder="Email" v-model="email" />            
              
              <div class="content">
                <div class="checkbox">
                  <input type="checkbox" id="checkbox" name="checkbox"/>
                  <label for="checkbox" class="terms-label">
                    Đồng ý với <a href="#">Điều khoản Sử dụng</a> và <a href="#">Chính Sách Bảo mật</a>
                  </label>
                </div>
              </div>
              <button @click.prevent="handleRegister">Đăng ký</button>
             
              <div class="separator">
                <span>Hoặc</span>
              </div>
  
              <div class="social-container">
                <a href="#" class="social"><img src="../assets/image/google.png" alt="Google"></a>
                <a href="#" class="social"><img src="../assets/image/facebook.png" alt="Facebook"></a>
              </div>
            </form>
          </div>  
   
          <!-- Đăng nhập -->        
          <div class="form-container login-container">
            <form acction="#">
              <h1>Đăng nhập</h1>
              <input type="text" placeholder="Số điện thoại" v-model="phoneNumber" />
              <input type="password" placeholder="Mật khẩu" v-model="password" />
              <div class="content">
                <div class="checkbox">
                  <input type="checkbox" id="checkbox" name="checkbox"/>
                  <label>Ghi nhớ đăng nhập</label>
                </div>
                
              
                <div class="pass-link">
                  <a href="#">Quên mật khẩu?</a>
                </div>
              </div>
              <button @click.prevent="handleLogin">Tiếp tục</button>
              <div class="separator">
                <span>Hoặc</span>
              </div>
  
              <div class="social-container">
                <a href="#" class="social"><img src="../assets/image/google.png" alt="Google"></a>
                <a href="#" class="social"><img src="../assets/image/facebook.png" alt="Facebook"></a>
              </div>
  
            </form>
          </div>
  
          <div class="overlay-container">
            <div class="overlay">
              <div class="overlay-panel overlay-left">
                <h1 class="title">Xin chào</h1>
                <p>Bạn đã có tài khoản? Đăng nhập ngay.</p>
                <button class="ghost" @click="showLogin">Đăng nhập</button>
              </div>
              <div class="overlay-panel overlay-right">
                <h1 class="title">Đến với chúng tôi</h1>
                <p>Bạn đã sẵn sàn khám phá? Hãy tạo tài khoản ngay!</p>
                <button class="ghost" @click="showRegister">Đăng ký</button>
            </div>
            
          </div>
          
  
         
        </div>
      </div>  
    </div>
  </template>
  
  
  
  <style scoped>
  *{
    box-sizing: border-box;
  }
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
    font-family: "Poppíns", sans-serif;
    overflow: hidden;
  }
   
   h1{
    color: #333;
    font-weight: 600;
    letter-spacing: -1.5px;
    margin: 0;
    margin-bottom: 15px;
  }
  h1.title{
    color: #fff;
    font-size: 47px;
    line-height: 47px;
    margin: 0;
    text-shadow: 0 0 10px rgba(16, 64, 74, 0.5);
  }
  p{
    font-size: 14px;
    font-weight: 100;
    line-height: 20px;
    letter-spacing: 0.5px;
    margin: 20px 0 30px;
    text-shadow: 0 0 10px rgba(16, 64, 74, 0.5);
  }
  
  span{
    font-size: 14px;
    margin-top: 25px;
  }
  
  a{
    color: #333;
    font-size: 14px;
    text-decoration: none;
    margin: 15px 0;
    transition: 0.3 ease-in-out;
  }
  
  a:hover{
    color: #f86ed3;
  }
  
  .terms-label {
    font-size: 11px !important;
    color: #333;
  }
  
  .terms-label a {
    font-size: 11px !important;
    color: blue;
    text-decoration: none;
  }
  
  .terms-label a:hover {
    text-decoration: underline;
  }
  
  .pass-link a{  
    color: blue;
    text-decoration: none;
  }
  .pass-link a:hover{ 
    text-decoration: underline;
  }
  
  .content{
    display: flex;
    width: 100%;
    height: 50px;
    align-items: center;
    justify-content: space-between;
  }
  
  .content .checkbox{
    display: flex;
    align-items: center;
    justify-content: center;
    white-space: nowrap;
  }
  
  .content input{
    accent-color: #333;
    width: 12px;
    height: 12px;
  }
  
  .content label{
    font-size: 14px;
    user-select: none;
    padding-left: 5px;
  }
  
  button{
    position: relative;
    border-radius: 20px;
    border: 2px solid rgba(255, 148, 226, 1);;
    background: rgba(255, 148, 226, 0.8);
    color: #333;
    font-size: 15px;
    font-weight: bold;
    margin: 5px;
    padding: 12px 50px;
    letter-spacing: 0px;
    text-transform: capitalize;
    transition: 0.3 ease-in-out;
  }
  button:hover {
    letter-spacing: 0.5px;
  }
  
  button:active {
    transform: scale(0.95);
  }
  
  button:focus {
    outline: none;
  }
  
  button.ghost {
    background-color: rgba(225, 225, 225, 0.2);
    border: 2px solid #fff;
    color: #fff;
  }
  
  button.ghost i {
    position: absolute;
    opacity: 1;
    transition: 0.3s ease-in-out;
  }
  button.ghost i.register {
    right: 70px;    
  }
  
  button.ghost i.login {
    left: 70px;    
  }
  
  button.ghost:hover i.register {
    right: 40px;    
    opacity: 1;
  }
  
  button.ghost:hover i.login {
    left: 40px;    
    opacity: 1;
  }
  
  .separator {
    padding: 10px 0 0 0;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    position: relative;
  }
  .separator span {
    background: white;
    padding: 0 10px;
    font-size: 16px;
    color: #555;
    
    text-align: center;
    margin: 10px 0;
  }
  
  .separator::before,
  .separator::after {
    content: "";
    flex: 1;
    height: 1px;
    background: #ccc;
  }
  
  
  .social-container {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 15px;
    transition: 0.3s ease-in-out;
  }
  
  .social-container a {
    margin: 0;
  }
  
  .social img {
    width: 32px; 
    height: 32px;
    border: 1px solid #dddddd;
    border-radius: 50%; /* Làm viền hình tròn */
    padding: 3px; /* Giúp viền không dính sát vào logo */
    background-color: white; /* Đảm bảo nền ảnh không bị thay đổi */
  }
  
  form{
    background-color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    padding: 0 50px;
    height: 100%;
    text-align: center;
  }
  
  input{
    background-color: rgba(255, 148, 226, 0.5);
    border-radius: 10px;
    border: none;
    padding: 12px 15px;
    margin: 8px 0;
    width: 100%;
  }
  
  .container{
    margin-top: -100px;
    background-color: #fff;
    border-radius: 25px;
    box-shadow: 0 14px 28px rgba(0, 0, 0, 0.25),
                0 10px 10px rgba(0, 0, 0, 0.22);
    position: relative;
    overflow: hidden;
    width: 768px;
    max-width: 100%;
    min-height: 500px;
  }
  
  .form-container{
    position: absolute;
    top: 0;
    height: 100%;
    transition: all 0.6s ease-in-out; 
  }
  
  .login-container{
    left: 0;
    width: 50%;
    z-index: 2; 
  }
  
  .container.right-panel-active .login-container{
    transform: translateX(100%);
  }
  
  .register-container{
    left: 0;
    width: 50%;
    opacity: 0;
    z-index: 1;
  }
  
  .container.right-panel-active .register-container{
    transform: translateX(100%);
    opacity: 1;
    z-index: 5;
    animation: show 0.6s;
  }
  
  @keyframes show {
    0%
    ,49.99%{
      opacity: 0;
      z-index: 1;
    }
    50%,
    100%{
      opacity: 1;
      z-index: 5;
    }
  }
  
  .overlay-container{
    position: absolute;
    top: 0;
    left: 50%;
    width: 50%;
    height: 100%;
    overflow: hidden;
    transition: transform 0.6s ease-in-out;
    z-index: 100;
  }
  
  .container.right-panel-active .overlay-container{
    transform: translateX(-100%);
  }
  
  .overlay{
    background-image: url("../assets/gif/login1.gif");
    background-repeat: no-repeat;
    background-size: cover;
    background-position: 0 0;
    color: #fff;
    position: relative;
    left: -100%;
    width: 200%;
    height: 100%;
    transform: translateX(0);
    transition: transform 0.6s ease-in-out;
  }
  
  .overlay::before{
    content: "";
    position: absolute;
    left: 0;
    top: 0;
    right: 0;
    bottom: 0;
    background: linear-gradient(
      to top, 
      rgba(46, 94, 109, 0.4) 40%,
      rgba(46, 94, 109, 0)
    );
  }
  
  .container.container.right-panel-active .overlay{
    transform: translateX(50%);
  }
  
  .overlay-panel{
    position: absolute;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    text-align: center;
    padding: 0 40px;
    top: 0;
    height: 100%;
    width: 50%;
    transform: translateX(0);
    transition: transform 0.6s ease-in-out;
  }
  
  .overlay-left{
    transform: translateX(-20%);
  }
  
  .container.right-panel-active .overlay-left{
    transform: translateX(0);
  }
  
  .overlay-right{
    right: 0;
    transform: translateX(0);
  }
  
  .container.right-panel-active .overlay-right{
    transform: translateX(20%);
  }
  
  
  
  
  
  </style>

  