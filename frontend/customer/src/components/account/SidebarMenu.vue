<template>
    <div class="account-menu">
        <div class="menu-section">
            <div
                class="menu-item"
                :class="{ active: activeMenu === 'profile' }"
                @click="$emit('update:activeMenu', 'profile')"
            >
                <i class="fas fa-user-cog"></i>
                <span>Thông tin cá nhân</span>
            </div>
            <div 
                class="menu-item" 
                @click="$emit('update:activeMenu', 'orders')"
                :class="{ active: activeMenu === 'orders' }"
            >
                <i class="fas fa-shopping-bag"></i>
                <span>Đơn hàng của bạn</span>
            </div>
            <div 
                class="menu-item" 
                @click="$emit('update:activeMenu', 'purchase-history')"
                :class="{ active: activeMenu === 'purchase-history' }"
            >
                <i class="fas fa-history"></i>
                <span>Lịch sử mua hàng</span>
            </div>
            
            <div
                class="menu-item"
                :class="{ active: activeMenu === 'delete-account' }"
                @click="$emit('update:activeMenu', 'delete-account')"
            >
                <i class="fas fa-user-slash danger-icon"></i>
                <span class="danger-text">Xóa tài khoản</span>
            </div>
            <div class="menu-item" @click="$emit('logout')">
                <i class="fas fa-sign-out-alt"></i>
                <span>Đăng xuất</span>
            </div>
        </div>
    </div>
</template>

<script setup>

defineProps({
    activeMenu: {
        type: String,
        required: true,
    },
});

defineEmits(["update:activeMenu", "logout"]);
</script>

<style scoped>
.account-menu {
    width: 250px;
    flex-shrink: 0;
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.menu-section {
    padding: 1rem 0;
}

.menu-item {
    display: flex;
    align-items: center;
    padding: 1rem 1.5rem;
    cursor: pointer;
    transition: all 0.3s ease;
    color: #666;
}

.menu-item i {
    margin-right: 1rem;
    width: 20px;
    text-align: center;
}

.menu-item:hover {
    background-color: #f9f0ff;
    color: var(--primary-color);
}

.menu-item.active {
    background-color: #f9f0ff;
    color: var(--primary-color);
    font-weight: 500;
    border-left: 3px solid var(--primary-color);
}

.danger-icon {
    color: #e53935;
}

.danger-text {
    color: #e53935;
}

.menu-item:hover .danger-icon,
.menu-item:hover .danger-text,
.menu-item.active .danger-icon,
.menu-item.active .danger-text {
    color: #c62828;
}

@media (max-width: 992px) {
    .account-menu {
        width: 100%;
    }
}
</style>
