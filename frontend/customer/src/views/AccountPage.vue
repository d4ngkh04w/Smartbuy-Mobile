<template>
    <div class="account-page">
        <div class="container">
            <h1 class="page-title">Tài khoản của tôi</h1>

            <div class="account-container">
                <!-- Sidebar menu -->
                <sidebar-menu
                    v-model:active-menu="activeMenu"
                    @logout="handleLogout"
                />

                <!-- Main content area -->
                <div class="account-content">
                    <!-- Overview tab -->
                    <account-overview v-if="activeMenu === 'overview'" />

                    <!-- User Profile Tab -->
                    <profile-info
                        v-else-if="activeMenu === 'profile'"
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
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import authService from "@/services/authService";
import meService from "@/services/meService";
import SidebarMenu from "@/components/account/SidebarMenu.vue";
import AccountOverview from "@/components/account/AccountOverview.vue";
import ProfileInfo from "@/components/account/ProfileInfo.vue";
import PasswordChange from "@/components/account/PasswordChange.vue";
import DeleteAccount from "@/components/account/DeleteAccount.vue";

const router = useRouter();
const activeMenu = ref("overview");
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
        router.push("/login");
    } catch (error) {
        console.error("Error during logout:", error);
    }
};

onMounted(() => {
    fetchUserData();
});
</script>

<style scoped>
.account-page {
    padding: 3rem 0;
    background-color: #f5f5f5;
    min-height: 100vh;
}

.page-title {
    font-size: 2rem;
    margin-bottom: 2rem;
    color: #333;
    font-weight: 600;
}

.account-container {
    display: flex;
    gap: 2rem;
}

.account-content {
    flex: 1;
    display: flex;
    flex-direction: column;
}

@media (max-width: 992px) {
    .account-container {
        flex-direction: column;
    }
}
</style>
