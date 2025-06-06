<template>
    <div class="account-page">
        <div class="container">
            <h1 class="page-title">Tài khoản của tôi</h1>

            <div class="account-layout">
                <!-- Sidebar menu -->
                <div class="sidebar">
                    <sidebar-menu
                        v-model:active-menu="activeMenu"
                        @logout="handleLogout"
                    />
                </div>

                <!-- Main content area -->
                <div class="main-content">
                    <!-- User Profile Tab -->
                    <profile-info
                        v-if="activeMenu === 'profile'"
                        v-model:user-data="userData"
                        @change-password="activeMenu = 'change-password'"
                    />

                    <!-- Password Change Tab -->
                    <password-change
                        v-else-if="activeMenu === 'change-password'"
                        @back="activeMenu = 'profile'"
                    />

                    <!-- Delete Account Tab -->
                    <delete-account
                        v-else-if="activeMenu === 'delete-account'"
                        @back="activeMenu = 'profile'"
                    />
                    <OrderManagement
                        v-else-if="activeMenu === 'orders'"
                        @back="activeMenu = 'profile'"
                    />
                    <PurchaseHistory
                        v-else-if="activeMenu === 'purchase-history'"
                        @back="activeMenu = 'profile'"
                    />
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import authService from "@/services/authService";
import meService from "@/services/meService";
import emitter from "@/utils/evenBus";
import SidebarMenu from "@/components/account/SidebarMenu.vue";
import ProfileInfo from "@/components/account/ProfileInfo.vue";
import PasswordChange from "@/components/account/PasswordChange.vue";
import DeleteAccount from "@/components/account/DeleteAccount.vue";
import OrderManagement from "./OrderManagement.vue";
import PurchaseHistory from "./PurchaseHistory.vue";

const route = useRoute();
const router = useRouter();
const activeMenu = ref("profile");
const loading = ref(false);

// Single source of truth for user data
const userData = ref({
    name: "",
    email: "",
    emailConfirmed: false,
    phoneNumber: "",
    phoneNumberConfirmed: false,
    createdAt: "",
    avatar: "",
    gender: "",
    address: "",
    dateOfBirth: "",
    avatarFile: null,
});

// Fetch user data
const fetchUserData = async () => {
    loading.value = true;
    try {
        const data = await meService.getMe();

        if (data) {
            // Update user data
            userData.value = {
                ...data,
                avatarFile: null,
            };
        }
    } catch (error) {
        console.error("Error fetching user data:", error);
        if (error.response && error.response.status === 401) {
            router.push("/login");
        }
    } finally {
        loading.value = false;
    }
};

// Hàm xử lý đăng xuất
const handleLogout = async () => {
    try {
        await authService.logout();
        // Phát ra sự kiện đăng xuất để header có thể cập nhật trạng thái
        emitter.emit("user-logged-out");
        router.push("/");
    } catch (error) {
        console.error("Error during logout:", error);
    }
};
watch(activeMenu, () => {
    router.replace({ query: {} }); // Xoá hết query trên URL
});
onMounted(() => {
    fetchUserData();
    if (route.query.section === "orders") {
        activeMenu.value = "orders";
    }
});
</script>

<style scoped>
.account-page {
    padding: 1rem 0;
    min-height: 100vh;
    width: 100%;
}

.page-title {
    font-size: 2rem;
    margin-bottom: 2rem;
    color: #333;
    font-weight: 600;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 15px;
}

.account-layout {
    display: flex;
    gap: 2rem;
}

.sidebar {
    width: 250px;
    flex-shrink: 0;
}

.main-content {
    flex: 1;
    padding: 0;
    background-color: #ffffff;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

@media (max-width: 992px) {
    .account-layout {
        flex-direction: column;
    }

    .sidebar {
        width: 100%;
        margin-bottom: 1.5rem;
    }
}
</style>
