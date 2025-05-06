<template>
    <div class="account-page">
        <div class="container">
            <h1 class="page-title">Tổng quan</h1>

            <div class="account-container">
                <!-- Sidebar menu -->
                <div class="account-menu">
                    <div class="menu-section">
                        <div class="menu-item active">
                            <i class="fas fa-home"></i>
                            <span>Tổng quan</span>
                        </div>
                        <div class="menu-item">
                            <i class="fas fa-shopping-bag"></i>
                            <span>Đơn hàng của bạn</span>
                        </div>
                        <div class="menu-item">
                            <i class="fas fa-ticket-alt"></i>
                            <span>Trung tâm voucher</span>
                        </div>
                        <div class="menu-item">
                            <i class="fas fa-history"></i>
                            <span>Lịch sử mua hàng</span>
                        </div>
                        <div class="menu-item">
                            <i class="fas fa-user-cog"></i>
                            <span>Thông tin cá nhân</span>
                        </div>
                        <div class="menu-item" @click="handleLogout">
                            <i class="fas fa-sign-out-alt"></i>
                            <span>Đăng xuất</span>
                        </div>
                    </div>
                </div>

                <!-- Main content area -->
                <div class="account-content">
                    <!-- Membership card -->
                    <div class="membership-card">
                        <div class="membership-header">
                            <div class="membership-icon">
                                <i class="fas fa-medal"></i>
                            </div>
                            <div class="membership-title">
                                <h2>Thành viên Silver</h2>
                            </div>
                        </div>

                        <div class="membership-progress">
                            <div class="membership-levels">
                                <div class="level edu">Edu</div>
                                <div class="level new">New</div>
                                <div class="level silver active">Silver</div>
                                <div class="level gold">Gold</div>
                                <div class="level diamond">Diamond</div>
                            </div>
                            <div class="progress-bar">
                                <div
                                    class="progress-fill silver"
                                    style="width: 60%"
                                ></div>
                            </div>
                        </div>

                        <div class="membership-details">
                            <p>Tổng chi tiêu: <strong>2,346,000đ</strong></p>
                            <p>
                                Thứ hạng được cập nhật lúc:
                                <strong>17/03/2025</strong>
                            </p>
                        </div>

                        <div class="membership-upgrade-info">
                            <div class="info-box">
                                <i class="fas fa-star"></i>
                                <p>
                                    Bạn là Học sinh - Sinh viên - Giáo viên?
                                    <strong>Đăng ký thành viên Edu ngay</strong>
                                </p>
                                <i class="fas fa-arrow-right"></i>
                            </div>
                        </div>
                    </div>

                    <!-- Membership benefits -->
                    <div class="section-title">
                        Khám phá ưu đãi hạng thành viên
                    </div>
                    <div class="membership-tiers">
                        <div class="tier-card">
                            <div class="tier-icon edu">
                                <img src="" alt="Edu" />
                                <span>Edu</span>
                            </div>
                        </div>
                        <div class="tier-card">
                            <div class="tier-icon new">
                                <img src="" alt="New" />
                                <span>New</span>
                            </div>
                        </div>
                        <div class="tier-card active">
                            <div class="tier-icon silver">
                                <img src="" alt="Silver" />
                                <span>Silver</span>
                                <div class="tier-check">
                                    <i class="fas fa-check-circle"></i>
                                </div>
                            </div>
                        </div>
                        <div class="tier-card">
                            <div class="tier-icon gold">
                                <img src="" alt="Gold" />
                                <span>Gold</span>
                            </div>
                        </div>
                        <div class="tier-card">
                            <div class="tier-icon diamond">
                                <img src="" alt="Diamond" />
                                <span>Diamond</span>
                            </div>
                        </div>
                    </div>

                    <!-- Membership benefits details -->
                    <div class="benefits-detail">
                        <div class="benefit-section">
                            <div class="benefit-icon">
                                <i class="fas fa-award"></i>
                            </div>
                            <div class="benefit-content">
                                <h3>Điều kiện</h3>
                                <p>
                                    Tổng số tiền mua hàng tích lũy trong thời
                                    gian đạt dưới 30 triệu đồng.
                                </p>
                            </div>
                        </div>

                        <div class="benefit-section">
                            <div class="benefit-icon">
                                <i class="fas fa-percentage"></i>
                            </div>
                            <div class="benefit-content">
                                <h3>
                                    Nguyên tắc tích điểm (nguyên tắc chung áp
                                    dụng với tất cả ngành hàng)
                                </h3>
                                <p>0,3% trên giá trị đơn hàng mới</p>
                            </div>
                        </div>

                        <div class="benefit-section">
                            <div class="benefit-icon">
                                <i class="fas fa-tags"></i>
                            </div>
                            <div class="benefit-content">
                                <h3>Nguyên tắc chiết khấu</h3>
                                <p>0,4%</p>
                            </div>
                        </div>
                    </div>
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

const router = useRouter();
const currentUser = ref(null);

// Hàm xử lý đăng xuất
const handleLogout = async () => {
    try {
        await authService.logout();
        router.push("/login");
    } catch (error) {
        console.error("Error during logout:", error);
    }
};

// Fetch user data on component mount
const fetchUserData = async () => {
    try {
        const userData = await meService.getMe();
        currentUser.value = userData;
    } catch (error) {
        console.error("Error fetching user data:", error);
        if (error.response && error.response.status === 401) {
            router.push("/login");
        }
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

/* Sidebar menu */
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

/* Main content area */
.account-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

/* Membership card */
.membership-card {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
}

.membership-header {
    display: flex;
    align-items: center;
    margin-bottom: 1.5rem;
}

.membership-icon {
    width: 50px;
    height: 50px;
    background-color: #f9f0ff;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 1rem;
}

.membership-icon i {
    color: var(--primary-color);
    font-size: 1.5rem;
}

.membership-title h2 {
    font-size: 1.5rem;
    color: #333;
    margin: 0;
}

.membership-progress {
    margin-bottom: 1.5rem;
}

.membership-levels {
    display: flex;
    justify-content: space-between;
    margin-bottom: 0.5rem;
}

.level {
    font-size: 0.9rem;
    color: #888;
    position: relative;
    padding: 0.25rem 0.5rem;
    background-color: #f5f5f5;
    border-radius: 4px;
    text-align: center;
}

.level.active {
    background-color: #e6ccf5;
    color: #9c27b0;
    font-weight: 500;
}

.level.edu {
    background-color: #e3f2fd;
    color: #2196f3;
}

.level.new {
    background-color: #f0f4c3;
    color: #afb42b;
}

.level.silver {
    background-color: #e0e0e0;
    color: #757575;
}

.level.gold {
    background-color: #fff8e1;
    color: #ffc107;
}

.level.diamond {
    background-color: #e8f5e9;
    color: #4caf50;
}

.progress-bar {
    height: 10px;
    background-color: #f5f5f5;
    border-radius: 5px;
    overflow: hidden;
}

.progress-fill {
    height: 100%;
    border-radius: 5px;
}

.progress-fill.silver {
    background-color: #9e9e9e;
}

.membership-details {
    margin-bottom: 1.5rem;
}

.membership-details p {
    margin: 0.5rem 0;
    color: #666;
}

.membership-details strong {
    color: #333;
}

.membership-upgrade-info {
    border-top: 1px solid #eee;
    padding-top: 1.5rem;
}

.info-box {
    background-color: #f0f8ff;
    padding: 1rem;
    border-radius: 8px;
    border: 1px solid #e3f2fd;
    display: flex;
    align-items: center;
    cursor: pointer;
}

.info-box i:first-child {
    color: #2196f3;
    margin-right: 0.5rem;
}

.info-box p {
    flex: 1;
    margin: 0;
    color: #666;
}

.info-box i:last-child {
    color: #2196f3;
}

/* Membership tiers */
.section-title {
    font-size: 1.25rem;
    font-weight: 500;
    color: #333;
    margin-bottom: 1rem;
}

.membership-tiers {
    display: flex;
    gap: 1rem;
    margin-bottom: 1.5rem;
    flex-wrap: wrap;
}

.tier-card {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1rem;
    flex: 1;
    min-width: 120px;
    text-align: center;
    transition: all 0.3s ease;
    cursor: pointer;
}

.tier-card:hover {
    transform: translateY(-5px);
}

.tier-card.active {
    border: 2px solid var(--primary-color);
}

.tier-icon {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    position: relative;
}

.tier-icon img {
    width: 50px;
    height: 50px;
    object-fit: contain;
}

.tier-check {
    position: absolute;
    top: -5px;
    right: -5px;
    color: var(--primary-color);
}

/* Benefits detail */
.benefits-detail {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
}

.benefit-section {
    display: flex;
    gap: 1rem;
    padding: 1rem 0;
    border-bottom: 1px solid #eee;
}

.benefit-section:last-child {
    border-bottom: none;
}

.benefit-icon {
    width: 40px;
    height: 40px;
    background-color: #f5f5f5;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.benefit-content h3 {
    font-size: 1rem;
    color: #333;
    margin: 0 0 0.5rem 0;
}

.benefit-content p {
    color: #666;
    margin: 0;
}

@media (max-width: 992px) {
    .account-container {
        flex-direction: column;
    }

    .account-menu {
        width: 100%;
    }
}

@media (max-width: 768px) {
    .membership-tiers {
        overflow-x: auto;
        flex-wrap: nowrap;
        padding-bottom: 1rem;
    }

    .tier-card {
        min-width: 100px;
    }
}

@media (max-width: 576px) {
    .page-title {
        font-size: 1.5rem;
    }

    .membership-levels {
        font-size: 0.75rem;
    }
}
</style>
