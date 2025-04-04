<script setup>
    import { ref } from "vue";
    import LoginModal from '@/components/LoginModal.vue';

    const auth = ref({
    isAuthenticated: false, // Đổi thành true nếu muốn test trạng thái đăng nhập
    user: { name: "Người dùng" } // Giá trị giả lập
    });

    const searchQuery = ref('');
    const showLoginModal = ref(false);
    const showDropdown = ref(false);
</script>

<template>
    <header class="header">
        <div class="header-container">
            <!-- Logo -->
            <router-link to="/" class="logo">
                <img src="@/assets/image/logo.png" alt="Logo" />
            </router-link>
        
            <!-- Ô tìm kiếm -->
            <div class="search-bar">
                <input type="text" v-model="searchQuery" placeholder="Tìm kiếm..." />
                <button>
                    <i class="fa-solid fa-magnifying-glass"></i>
                </button>
            </div>
        
            <!-- Các icon điều hướng -->
            <div class="header-actions">
                <div class="action-item">
                    <i class="fa-solid fa-cart-shopping"></i>
                    <span>Giỏ hàng</span>
                </div>
                <div class="action-item">
                    <i class="fa-solid fa-bell"></i>
                    <span>Thông báo</span>
                </div>
                <!-- Nếu đã đăng nhập, hiển thị menu tài khoản -->
                    <div class="action-item user-menu" v-if="auth.isAuthenticated">
                        <i class="fa-solid fa-user"></i>
                        <span @click="toggleDropdown">{{ auth.user.name }}</span>
                        <div class="dropdown-menu" v-if="showDropdown">
                            <router-link to="/profile">Thông tin cá nhân</router-link>
                            <router-link to="/orders">Đơn hàng</router-link>
                            <a @click="logout">Đăng xuất</a>
                        </div>
                    </div>
            
                    <div class="action-item" v-else @click="showLoginModal = true">
                    <i class="fa-solid fa-user"></i>
                    <span>Đăng nhập</span>
                    </div>
                </div>
                <!-- Modal đăng nhập -->
                <LoginModal v-if="showLoginModal" @close="showLoginModal = false" />
        
        </div>
    </header>
    </template>
    

    <style scoped>
    /* Bố cục chính */
    .header {
        background-color: var(--primary-color);
        padding: 5px 10px;
    }
    
    .header-container {
        display: flex;
        align-items: center;
        justify-content: space-between;
        max-width: 100vw;
        margin: 0;
        padding: 0 10px;
    }
    
    /* Logo */
    .logo img {
        width: 208px;
        height: 59px;
        object-fit: cover;
    }
    
    /* Thanh tìm kiếm */
    .search-bar {
        display: flex;
        align-items: center;
        background: white;
        border-radius: 20px;
        padding: 5px 10px;
        width: 50%;
        max-width: 600x;
    }
    
    .search-bar input {
        border: none;
        outline: none;
        width: 100%;
        padding: 5px;
        font-size: 16px;
    }
    
    .search-bar button {
        background: none;
        border: none;
        cursor: pointer;
    }
      
    .search-bar img {
        width: 20px;
        height: 20px;
    }
    
    /* Các icon điều hướng */
    .header-actions {
        display: flex;
        gap: 20px;
    }
    
    .action-item {
        display: flex;
        align-items: center;
        gap: 5px;
        cursor: pointer;
    }
    
    .action-item img {
        width: 24px;
        height: 24px;
    }
    
    /* Responsive */
    @media (max-width: 1200px) {
    .header-container {
        flex-direction: column;
    }
    
    .search-bar {
        width: 80%;
        margin-bottom: 10px;
    }
    
    }
    </style>
    