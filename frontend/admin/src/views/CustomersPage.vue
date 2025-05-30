<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import userService from "../services/userService.js";
import emitter from "../utils/evenBus.js";
import { formatDate } from "../utils/dateTimeUtils.js";

const router = useRouter();
const customers = ref([]);
const loading = ref(false);
const searchQuery = ref("");
const statusFilter = ref("all"); // Thêm filter trạng thái
const selectedCustomer = ref(null);
const showModal = ref(false);
const showLockModal = ref(false);
const showUnlockModal = ref(false);
const lockReason = ref("");

// No filters or pagination needed

// Fetch customers from API
const fetchCustomers = async () => {
    loading.value = true;
    try {
        const response = await userService.getUsers();
        console.log("Fetched customers:", response.data);
        customers.value = response.data.data || [];
    } catch (error) {
        console.error("Error fetching customers:", error);
        if (error.response && error.response.status === 404) {
            customers.value = [];
        } else {
            emitter.emit("show-notification", {
                status: "error",
                message: "Không thể tải danh sách khách hàng",
            });
        }
    } finally {
        loading.value = false;
    }
};

// Refresh data
const refreshData = async () => {
    try {
        loading.value = true;
        await fetchCustomers();
        emitter.emit("show-notification", {
            status: "success",
            message: "Dữ liệu khách hàng đã được cập nhật",
        });
    } catch (error) {
        console.error("Error refreshing data:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể làm mới dữ liệu khách hàng",
        });
    } finally {
        loading.value = false;
    }
};

// Apply filters to customers
const filteredCustomers = computed(() => {
    let result = [...customers.value];

    // Apply status filter
    if (statusFilter.value !== "all") {
        const isActive = statusFilter.value === "active";
        result = result.filter((customer) => !customer.isLocked === isActive);
    }

    // Apply search filter
    if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase();
        result = result.filter(
            (customer) =>
                (customer.name &&
                    customer.name.toLowerCase().includes(query)) ||
                (customer.email &&
                    customer.email.toLowerCase().includes(query)) ||
                (customer.phoneNumber && customer.phoneNumber.includes(query))
        );
    }

    return result;
});

// Reset filters
const resetFilters = () => {
    searchQuery.value = "";
    statusFilter.value = "all";
};

// Format customer registration date
const formatRegistrationDate = (dateString) => {
    if (!dateString) return "N/A";
    return formatDate(dateString);
};

// View customer details
const viewCustomerDetails = (customer) => {
    selectedCustomer.value = customer;
    showModal.value = true;
};

// Format avatar URL
const formatAvatarUrl = (avatarPath) => {
    if (!avatarPath) return null;

    // If avatar is already a full URL
    if (avatarPath.startsWith("http")) return avatarPath;

    // Get base URL from API config
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Normalize the path (convert \ to /)
    const normalizedPath = avatarPath.replace(/\\/g, "/");

    // Check if path starts with /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Default avatar
const getDefaultAvatar = (name) => {
    if (!name)
        return "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg";
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(
        name
    )}&background=random&color=fff`;
};

// Show lock modal
const showLockUserModal = (customer) => {
    selectedCustomer.value = customer;
    lockReason.value = "";
    showLockModal.value = true;
};

// Show unlock modal
const showUnlockUserModal = (customer) => {
    selectedCustomer.value = customer;
    showUnlockModal.value = true;
};

// Process locking a user
const lockCustomer = async () => {
    if (!selectedCustomer.value || !lockReason.value) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Vui lòng nhập lý do khóa tài khoản",
        });
        return;
    }

    loading.value = true;
    try {
        await userService.lockUser(selectedCustomer.value.id, {
            reason: lockReason.value,
        });
        await fetchCustomers();
        showLockModal.value = false;
        emitter.emit("show-notification", {
            status: "success",
            message: "Đã khóa tài khoản khách hàng thành công",
        });
    } catch (error) {
        console.error("Error locking customer:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể khóa tài khoản khách hàng",
        });
    } finally {
        loading.value = false;
    }
};

// Process unlocking a user
const unlockCustomer = async () => {
    if (!selectedCustomer.value) {
        return;
    }

    loading.value = true;
    try {
        await userService.unlockUser(selectedCustomer.value.id);
        await fetchCustomers();
        showUnlockModal.value = false;
        emitter.emit("show-notification", {
            status: "success",
            message: "Đã mở khóa tài khoản khách hàng thành công",
        });
    } catch (error) {
        console.error("Error unlocking customer:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể mở khóa tài khoản khách hàng",
        });
    } finally {
        loading.value = false;
    }
};

// Navigation
const goBack = () => {
    router.push("/dashboard");
};

// Load data when component mounts
onMounted(async () => {
    await fetchCustomers();
});
</script>

<template>
    <div class="customer-management">
        <div class="page-header">
            <div class="header-top">
                <button class="back-button" @click="goBack">
                    <i class="fas fa-arrow-left"></i> Quay lại Dashboard
                </button>
                <h1>Quản lý Khách hàng</h1>
                <button class="invisible-button" style="visibility: hidden">
                    <i class="fas fa-sync-alt"></i> Nút ẩn
                </button>
            </div>
        </div>

        <!-- Customer Management Content -->
        <div class="customer-content">
            <!-- Customer Management Header -->
            <div class="section-header">
                <div class="left-section">
                    <h2><i class="fas fa-users"></i> Quản lý Khách hàng</h2>
                    <p>Quản lý thông tin khách hàng và trạng thái tài khoản</p>
                </div>
                <div class="right-section">
                    <!-- Status Filter -->
                    <div class="status-filter">
                        <i class="fas fa-filter"></i>
                        <select v-model="statusFilter">
                            <option value="all">Tất cả trạng thái</option>
                            <option value="active">Hoạt động</option>
                            <option value="locked">Bị khóa</option>
                        </select>
                    </div>

                    <!-- Search Box -->
                    <div class="search-box">
                        <i class="fas fa-search"></i>
                        <input
                            type="text"
                            v-model="searchQuery"
                            placeholder="Tìm kiếm theo tên, email, SĐT..."
                        />
                        <button
                            v-if="searchQuery"
                            @click="searchQuery = ''"
                            class="clear-search"
                        >
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            </div>

            <!-- Customer statistics cards -->
            <div class="status-cards">
                <div class="status-card">
                    <div class="icon-wrapper">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="status-content">
                        <h3>{{ customers.length }}</h3>
                        <p>Tổng số khách hàng</p>
                    </div>
                </div>

                <div class="status-card">
                    <div class="icon-wrapper">
                        <i class="fas fa-user-check"></i>
                    </div>
                    <div class="status-content">
                        <h3>
                            {{ customers.filter((c) => !c.isLocked).length }}
                        </h3>
                        <p>Khách hàng hoạt động</p>
                    </div>
                </div>

                <div class="status-card">
                    <div class="icon-wrapper">
                        <i class="fas fa-user-lock"></i>
                    </div>
                    <div class="status-content">
                        <h3>
                            {{ customers.filter((c) => c.isLocked).length }}
                        </h3>
                        <p>Khách hàng bị khóa</p>
                    </div>
                </div>
            </div>

            <!-- Customer table -->
            <div class="table-container">
                <div v-if="loading" class="loading-indicator">
                    <div class="spinner"></div>
                    <p>Đang tải dữ liệu khách hàng...</p>
                </div>
                <div
                    v-else-if="filteredCustomers.length === 0"
                    class="empty-state"
                >
                    <i class="fas fa-users"></i>
                    <p v-if="searchQuery">
                        Không tìm thấy khách hàng phù hợp với từ khóa tìm kiếm
                    </p>
                    <p v-else>Chưa có khách hàng nào</p>
                    <button @click="resetFilters" class="action-button">
                        <i class="fas fa-sync"></i> Xóa từ khóa tìm kiếm
                    </button>
                </div>

                <table v-else class="customers-table">
                    <thead>
                        <tr>
                            <th>Khách hàng</th>
                            <th>Email</th>
                            <th>Số điện thoại</th>
                            <th>Ngày tham gia</th>
                            <th>Trạng thái</th>
                            <th>Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="customer in filteredCustomers"
                            :key="customer.id"
                            class="customer-row"
                        >
                            <td class="customer-info">
                                <div class="avatar">
                                    <img
                                        :src="
                                            customer.avatar
                                                ? formatAvatarUrl(
                                                      customer.avatar
                                                  )
                                                : getDefaultAvatar(
                                                      customer.name
                                                  )
                                        "
                                        :alt="customer.name || 'User avatar'"
                                    />
                                </div>
                                <div class="name">
                                    <span>{{
                                        customer.name || "Chưa cập nhật"
                                    }}</span>
                                </div>
                            </td>
                            <td>{{ customer.email || "Chưa cập nhật" }}</td>
                            <td>
                                {{ customer.phoneNumber || "Chưa cập nhật" }}
                            </td>
                            <td>
                                {{ formatRegistrationDate(customer.createdAt) }}
                            </td>
                            <td>
                                <span
                                    class="status-badge"
                                    :class="{
                                        active: !customer.isLocked,
                                        locked: customer.isLocked,
                                    }"
                                >
                                    {{
                                        !customer.isLocked
                                            ? "Hoạt động"
                                            : "Bị khóa"
                                    }}
                                </span>
                            </td>
                            <td class="actions">
                                <button
                                    @click="viewCustomerDetails(customer)"
                                    class="view-button"
                                    title="Xem thông tin"
                                >
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button
                                    v-if="!customer.isLocked"
                                    @click="showLockUserModal(customer)"
                                    class="lock-button"
                                    title="Khóa tài khoản"
                                >
                                    <i class="fas fa-lock"></i>
                                </button>
                                <button
                                    v-else
                                    @click="showUnlockUserModal(customer)"
                                    class="unlock-button"
                                    title="Mở khóa tài khoản"
                                >
                                    <i class="fas fa-unlock"></i>
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Customer Details Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container detail-modal">
                <div class="modal-header">
                    <h3>Chi tiết khách hàng</h3>
                    <button @click="showModal = false" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body">
                    <div
                        v-if="selectedCustomer"
                        class="customer-detail-content"
                    >
                        <!-- Thông tin cơ bản -->
                        <div class="customer-detail-header">
                            <div class="customer-detail-avatar">
                                <img
                                    :src="
                                        selectedCustomer.avatar
                                            ? formatAvatarUrl(
                                                  selectedCustomer.avatar
                                              )
                                            : getDefaultAvatar(
                                                  selectedCustomer.name
                                              )
                                    "
                                    :alt="
                                        selectedCustomer.name || 'User avatar'
                                    "
                                />
                            </div>
                            <div class="customer-detail-basic">
                                <h4 class="customer-detail-name">
                                    {{
                                        selectedCustomer.name ||
                                        "Chưa cập nhật tên"
                                    }}
                                </h4>
                                <div class="customer-detail-info-row">
                                    <span class="detail-label"
                                        >ID khách hàng:</span
                                    >
                                    <span class="detail-value">{{
                                        selectedCustomer.id
                                    }}</span>
                                </div>
                                <div class="customer-detail-info-row">
                                    <span class="detail-label">Email:</span>
                                    <span class="detail-value">{{
                                        selectedCustomer.email ||
                                        "Chưa cập nhật"
                                    }}</span>
                                </div>
                                <div class="customer-detail-info-row">
                                    <span class="detail-label"
                                        >Số điện thoại:</span
                                    >
                                    <span class="detail-value">{{
                                        selectedCustomer.phoneNumber ||
                                        "Chưa cập nhật"
                                    }}</span>
                                </div>
                                <div class="customer-detail-info-row">
                                    <span class="detail-label"
                                        >Ngày tham gia:</span
                                    >
                                    <span class="detail-value">{{
                                        formatRegistrationDate(
                                            selectedCustomer.createdAt
                                        )
                                    }}</span>
                                </div>
                                <div class="customer-detail-info-row">
                                    <span class="detail-label"
                                        >Lần đăng nhập cuối:</span
                                    >
                                    <span class="detail-value">{{
                                        selectedCustomer.lastLogin
                                            ? formatRegistrationDate(
                                                  selectedCustomer.lastLogin
                                              )
                                            : "Chưa cập nhật"
                                    }}</span>
                                </div>
                                <div class="customer-detail-info-row">
                                    <span class="detail-label"
                                        >Trạng thái:</span
                                    >
                                    <span
                                        class="status-badge"
                                        :class="{
                                            active: !selectedCustomer.isLocked,
                                            locked: selectedCustomer.isLocked,
                                        }"
                                    >
                                        {{
                                            !selectedCustomer.isLocked
                                                ? "Hoạt động"
                                                : "Bị khóa"
                                        }}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <!-- Thông tin chi tiết -->
                        <div class="customer-detail-section">
                            <h5 class="detail-section-title">
                                Thông tin cá nhân
                            </h5>
                            <div class="customer-info-grid">
                                <div class="info-item">
                                    <div class="info-label">
                                        <i class="fas fa-venus-mars"></i> Giới
                                        tính
                                    </div>
                                    <div class="info-value">
                                        {{
                                            selectedCustomer.gender ||
                                            "Chưa cập nhật"
                                        }}
                                    </div>
                                </div>
                                <div class="info-item">
                                    <div class="info-label">
                                        <i class="fas fa-map-marker-alt"></i>
                                        Địa chỉ
                                    </div>
                                    <div class="info-value">
                                        {{
                                            selectedCustomer.address ||
                                            "Chưa cập nhật"
                                        }}
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Lý do khóa nếu có -->
                        <div
                            v-if="selectedCustomer.isLocked"
                            class="customer-detail-section"
                        >
                            <h5 class="detail-section-title">
                                Lý do khóa tài khoản
                            </h5>
                            <div class="lock-reason-content">
                                {{
                                    selectedCustomer.lockReason ||
                                    "Không có lý do được cung cấp"
                                }}
                            </div>
                        </div>

                        <!-- Hành động -->
                        <div class="customer-detail-actions">
                            <button
                                v-if="!selectedCustomer.isLocked"
                                @click="
                                    showModal = false;
                                    showLockUserModal(selectedCustomer);
                                "
                                class="lock-account-button"
                            >
                                <i class="fas fa-lock"></i> Khóa tài khoản
                            </button>
                            <button
                                v-else
                                @click="
                                    showUnlockUserModal(selectedCustomer);
                                    showModal = false;
                                "
                                class="unlock-account-button"
                            >
                                <i class="fas fa-unlock"></i> Mở khóa tài khoản
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Lock User Modal -->
        <div v-if="showLockModal" class="modal-backdrop">
            <div class="modal-container lock-modal">
                <div class="modal-header warning">
                    <h3>Xác nhận khóa tài khoản</h3>
                    <button @click="showLockModal = false" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body">
                    <div class="lock-warning">
                        <div class="warning-icon">
                            <i class="fas fa-exclamation-triangle"></i>
                        </div>
                        <p>
                            Bạn đang khóa tài khoản của
                            <strong>{{
                                selectedCustomer?.name || "người dùng này"
                            }}</strong
                            >. Họ sẽ không thể đăng nhập vào hệ thống cho đến
                            khi tài khoản được mở khóa.
                        </p>
                    </div>

                    <div class="form-group">
                        <label for="lock-reason">Lý do khóa tài khoản:</label>
                        <textarea
                            id="lock-reason"
                            v-model="lockReason"
                            placeholder="Vui lòng nhập lý do khóa tài khoản..."
                            rows="3"
                        ></textarea>
                        <p class="form-hint">
                            Lý do này sẽ được hiển thị cho người dùng khi họ cố
                            gắng đăng nhập.
                        </p>
                    </div>
                </div>

                <div class="modal-footer">
                    <button
                        @click="showLockModal = false"
                        class="cancel-button"
                    >
                        <i class="fas fa-times"></i> Hủy
                    </button>
                    <button
                        @click="lockCustomer"
                        class="lock-confirm-button"
                        :disabled="!lockReason"
                    >
                        <i class="fas fa-lock"></i> Khóa tài khoản
                    </button>
                </div>
            </div>
        </div>

        <!-- Unlock User Modal -->
        <div v-if="showUnlockModal" class="modal-backdrop">
            <div class="modal-container lock-modal">
                <div class="modal-header warning">
                    <h3>Xác nhận mở khóa tài khoản</h3>
                    <button
                        @click="showUnlockModal = false"
                        class="close-button"
                    >
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body">
                    <div class="lock-warning">
                        <div class="warning-icon">
                            <i class="fas fa-exclamation-triangle"></i>
                        </div>
                        <p>
                            Bạn đang mở khóa tài khoản của
                            <strong>{{
                                selectedCustomer?.name || "người dùng này"
                            }}</strong
                            >. Họ sẽ có thể đăng nhập vào hệ thống.
                        </p>
                    </div>
                </div>

                <div class="modal-footer">
                    <button
                        @click="showUnlockModal = false"
                        class="cancel-button"
                    >
                        <i class="fas fa-times"></i> Hủy
                    </button>
                    <button @click="unlockCustomer" class="lock-confirm-button">
                        <i class="fas fa-unlock"></i> Mở khóa tài khoản
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.customer-management {
    padding: 2rem;
    background-color: #f9f9f9;
    min-height: 100vh;
}

.page-header {
    display: flex;
    flex-direction: column;
    margin-bottom: 2rem;
    background-color: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border-left: 4px solid var(--primary-color);
}

.customer-content {
    background-color: white;
    border-radius: 12px;
    padding: 1rem;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.header-top {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
}

.back-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: white;
    color: #666;
    border: 1px solid #e9ecef;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.back-button i {
    color: var(--primary-color);
}

.back-button:hover {
    background-color: #f8f0f4;
    color: var(--primary-color);
}

.invisible-button {
    background-color: transparent;
    border: none;
    cursor: default;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.page-header h1 {
    margin: 0;
    font-size: 1.8rem;
    font-weight: 600;
    color: #333;
}

/* Section Header */
.section-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 2rem;
    padding: 0;
}

.left-section h2 {
    color: #1a202c;
    font-size: 1.5rem;
    font-weight: 600;
    margin: 0 0 0.5rem 0;
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.left-section h2 i {
    color: var(--primary-color);
    font-size: 1.25rem;
}

.left-section p {
    color: #64748b;
    margin: 0;
    font-size: 0.95rem;
    line-height: 1.4;
}

.right-section {
    display: flex;
    align-items: center;
    gap: 1rem;
}

/* Status Filter */
.status-filter {
    position: relative;
    width: 200px;
}

.status-filter i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.status-filter select {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    background-color: white;
    cursor: pointer;
    transition: all 0.3s;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23666' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'%3E%3C/polyline%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 10px center;
    background-size: 16px;
}

.status-filter select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

/* Search Box */
.search-box {
    position: relative;
    width: 350px;
}

.search-box i {
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #6e6e6e;
    font-size: 0.9rem;
}

.search-box input {
    width: 100%;
    padding: 0.75rem 0.75rem 0.75rem 2.5rem;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    font-size: 0.95rem;
    transition: all 0.3s;
    background: white;
}

.search-box input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(232, 93, 185, 0.25);
    outline: none;
}

.clear-search {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #6e6e6e;
    cursor: pointer;
    padding: 0.25rem;
    border-radius: 4px;
    transition: all 0.2s;
}

.clear-search:hover {
    background-color: #f1f5f9;
    color: #374151;
}

/* Status Cards */
.status-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.status-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.25rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.status-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.icon-wrapper {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background-color: #fff5fc;
    color: var(--primary-color);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
}

.status-content h3 {
    font-size: 1.5rem;
    margin: 0 0 0.25rem 0;
    color: #333;
}

.status-content p {
    margin: 0;
    color: #666;
    font-size: 0.85rem;
}

/* Table Container */
.table-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    padding: 1.5rem;
}

.loading-indicator {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 0;
}

.spinner {
    width: 48px;
    height: 48px;
    border: 4px solid rgba(232, 93, 185, 0.1);
    border-radius: 50%;
    border-top: 4px solid var(--primary-color);
    animation: spin 1s linear infinite;
    margin-bottom: 1rem;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}

.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 0;
    text-align: center;
}

.empty-state i {
    font-size: 3rem;
    color: #d1d5db;
    margin-bottom: 1rem;
}

.empty-state p {
    color: #6b7280;
    margin-bottom: 1.5rem;
}

.action-button {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem 1.5rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s;
}

.action-button:hover {
    background-color: #e856b1;
}

/* Customers Table */
.customers-table {
    width: 100%;
    border-collapse: collapse;
}

.customers-table th {
    text-align: left;
    padding: 1rem;
    color: #6b7280;
    font-weight: 600;
    font-size: 0.9rem;
    border-bottom: 1px solid #e5e7eb;
}

.customers-table th:last-child {
    text-align: center; /* Căn giữa tiêu đề cột "Hành động" */
}

.customer-row {
    border-bottom: 1px solid #e5e7eb;
    transition: background-color 0.2s;
}

.customer-row:hover {
    background-color: #f9fafb;
}

.customer-row td {
    padding: 1rem;
    color: #374151;
    vertical-align: middle;
}

.customer-info {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    overflow: hidden;
    background-color: #f3f4f6;
}

.avatar img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.name {
    font-weight: 500;
    color: #111827;
}

.status-badge {
    display: inline-block;
    padding: 0.35rem 0.65rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 500;
}

.status-badge.active {
    background-color: #d1fae5;
    color: #065f46;
}

.status-badge.locked {
    background-color: #fee2e2;
    color: #b91c1c;
}

.actions {
    display: flex;
    gap: 0.5rem;
    justify-content: center;
    align-items: center;
}

.view-button,
.lock-button,
.unlock-button {
    width: 32px;
    height: 32px;
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    cursor: pointer;
    transition: all 0.2s;
    line-height: 1; /* Đảm bảo biểu tượng không bị lệch do line-height */
}

.view-button i,
lock-button i,
.unlock-button i {
    display: flex; /* Căn chỉnh biểu tượng */
    align-items: center;
    justify-content: center;
    font-size: 14px; /* Đồng nhất kích thước biểu tượng */
}

.view-button {
    background-color: #e0f2fe;
    color: var(--primary-color);
}

.view-button:hover {
    background-color: #bae6fd;
}

.lock-button {
    background-color: #fee2e2;
    color: #b91c1c;
}

.lock-button:hover {
    background-color: #fecaca;
}

.unlock-button {
    background-color: #d1fae5;
    color: #065f46;
}

.unlock-button:hover {
    background-color: #a7f3d0;
}

/* Modal */
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 50;
    backdrop-filter: blur(4px);
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1),
        0 10px 10px -5px rgba(0, 0, 0, 0.04);
    width: 100%;
    max-width: 600px;
    max-height: 90vh;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
}

.modal-header {
    padding: 1.5rem;
    border-bottom: 1px solid #e5e7eb;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.modal-header.warning {
    border-bottom-color: #fee2e2;
    color: #b91c1c;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    color: #111827;
}

.close-button {
    background: none;
    border: none;
    color: #6b7280;
    font-size: 1.25rem;
    cursor: pointer;
    transition: color 0.2s;
}

.close-button:hover {
    color: #111827;
}

.modal-body {
    padding: 1.5rem;
    flex: 1;
    overflow-y: auto;
}

.modal-footer {
    padding: 1.5rem;
    border-top: 1px solid #e5e7eb;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 1rem;
}

/* Customer Detail Modal */
.detail-modal {
    width: 95%;
    max-width: 800px;
    max-height: 90vh;
}

.customer-detail-content {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.customer-detail-header {
    display: grid;
    grid-template-columns: 200px 1fr;
    gap: 2rem;
}

@media (max-width: 768px) {
    .customer-detail-header {
        grid-template-columns: 1fr;
    }
}

.customer-detail-avatar {
    width: 200px;
    height: 200px;
    border-radius: 12px;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f9f9f9;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.customer-detail-avatar img {
    max-width: 100%;
    max-height: 100%;
    object-fit: cover;
}

.customer-detail-basic {
    display: flex;
    flex-direction: column;
}

.customer-detail-name {
    font-size: 1.5rem;
    margin: 0 0 1rem 0;
    color: #333;
    font-weight: 600;
    line-height: 1.3;
    border-left: 3px solid var(--primary-color);
    padding-left: 0.75rem;
}

.customer-detail-info-row {
    display: flex;
    gap: 1rem;
    margin-bottom: 0.5rem;
    align-items: center;
}

.customer-detail-section {
    border-top: 1px solid #eee;
    padding-top: 1.5rem;
}

.detail-section-title {
    font-size: 1.1rem;
    margin: 0 0 1rem 0;
    color: #333;
}

.customer-info-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1rem;
}

.info-item {
    background-color: #f9fafb;
    padding: 1rem;
    border-radius: 8px;
}

.info-label {
    color: #6b7280;
    font-size: 0.9rem;
    margin-bottom: 0.5rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.info-value {
    font-weight: 500;
    color: #111827;
}

.lock-reason-content {
    background-color: #fef2f2;
    border: 1px solid #fecaca;
    border-radius: 8px;
    padding: 1rem;
    color: #b91c1c;
    line-height: 1.6;
}

.customer-detail-actions {
    display: flex;
    justify-content: center;
    padding-top: 1.5rem;
    border-top: 1px solid #eee;
}

.lock-account-button,
.unlock-account-button {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
}

.lock-account-button {
    background-color: #fee2e2;
    color: #b91c1c;
    border: 1px solid #fecaca;
}

.lock-account-button:hover {
    background-color: #fecaca;
}

.unlock-account-button {
    background-color: #d1fae5;
    color: #065f46;
    border: 1px solid #a7f3d0;
}

.unlock-account-button:hover {
    background-color: #a7f3d0;
}

/* Lock Modal */
.lock-modal {
    max-width: 500px;
}

.lock-warning {
    display: flex;
    align-items: flex-start;
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.warning-icon {
    padding: 0.75rem;
    border-radius: 50%;
    background-color: #fee2e2;
    color: #b91c1c;
    font-size: 1.25rem;
    display: flex;
    align-items: center;
    justify-content: center;
}

.lock-warning p {
    margin: 0;
    color: #4b5563;
    font-size: 0.95rem;
    line-height: 1.5;
}

.form-group {
    margin-bottom: 1.5rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #111827;
}

.form-group textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    resize: vertical;
    min-height: 80px;
    font-size: 0.95rem;
    transition: all 0.3s;
}

.form-group textarea:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(232, 93, 185, 0.25);
    outline: none;
}

.form-hint {
    margin-top: 0.5rem;
    font-size: 0.8rem;
    color: #6b7280;
}

.cancel-button {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem 1.25rem;
    background-color: white;
    border: 1px solid #d1d5db;
    border-radius: 8px;
    color: #4b5563;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
}

.cancel-button:hover {
    background-color: #f3f4f6;
}

.lock-confirm-button {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem 1.25rem;
    background-color: #ef4444;
    border: none;
    border-radius: 8px;
    color: white;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
}

.lock-confirm-button:hover {
    background-color: #dc2626;
}

.lock-confirm-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

/* Responsive */
@media (max-width: 768px) {
    .customer-management {
        padding: 1rem;
    }

    .page-header {
        padding: 1.5rem;
    }
    .section-header {
        flex-direction: column;
        align-items: stretch;
        gap: 1.5rem;
    }

    .right-section {
        flex-direction: column;
        gap: 1rem;
    }

    .status-filter,
    .search-box {
        width: 100%;
    }

    .customers-table {
        display: block;
        overflow-x: auto;
    }

    .modal-container {
        width: 90%;
    }
}
</style>
