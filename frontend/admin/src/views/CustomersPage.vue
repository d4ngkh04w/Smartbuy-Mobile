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
const selectedCustomer = ref(null);
const showModal = ref(false);
const showLockModal = ref(false);
const showUnlockModal = ref(false);
const lockReason = ref("");

// Filter options
const statusFilter = ref("all");
const sortByFilter = ref("newest");

// Pagination
const currentPage = ref(1);
const itemsPerPage = ref(10);

// Fetch customers from API
const fetchCustomers = async () => {
	loading.value = true;
	try {
		const response = await userService.getUsers();
		console.log("Fetched customers:", response.data);
		customers.value = response.data.data || [];
	} catch (error) {
		console.error("Error fetching customers:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể tải danh sách khách hàng",
		});
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

	// Apply status filter
	if (statusFilter.value !== "all") {
		const isLocked = statusFilter.value === "locked";
		result = result.filter((customer) => customer.isLocked === isLocked);
	}

	// Apply sorting
	switch (sortByFilter.value) {
		case "newest":
			result.sort(
				(a, b) => new Date(b.createdAt) - new Date(a.createdAt)
			);
			break;
		case "oldest":
			result.sort(
				(a, b) => new Date(a.createdAt) - new Date(b.createdAt)
			);
			break;
		case "name-asc":
			result.sort((a, b) => (a.name || "").localeCompare(b.name || ""));
			break;
		case "name-desc":
			result.sort((a, b) => (b.name || "").localeCompare(a.name || ""));
			break;
	}

	return result;
});

// Paginated customers
const paginatedCustomers = computed(() => {
	const start = (currentPage.value - 1) * itemsPerPage.value;
	const end = start + itemsPerPage.value;
	return filteredCustomers.value.slice(start, end);
});

// Total pages
const totalPages = computed(() => {
	return Math.ceil(filteredCustomers.value.length / itemsPerPage.value) || 1;
});

// Reset filters
const resetFilters = () => {
	searchQuery.value = "";
	statusFilter.value = "all";
	sortByFilter.value = "newest";
	currentPage.value = 1;
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
				<button class="refresh-button" @click="refreshData">
					<i class="fas fa-sync-alt"></i> Làm mới
				</button>
			</div>
		</div>

		<!-- Filter and search controls -->
		<div class="control-panel">
			<div class="search-section">
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

			<div class="filter-section">
				<div class="filter-group">
					<label>Trạng thái:</label>
					<select v-model="statusFilter">
						<option value="all">Tất cả</option>
						<option value="active">Hoạt động</option>
						<option value="locked">Bị khóa</option>
					</select>
				</div>

				<div class="filter-group">
					<label>Sắp xếp:</label>
					<select v-model="sortByFilter">
						<option value="newest">Mới nhất</option>
						<option value="oldest">Cũ nhất</option>
						<option value="name-asc">Tên A-Z</option>
						<option value="name-desc">Tên Z-A</option>
					</select>
				</div>

				<button @click="resetFilters" class="reset-button">
					<i class="fas fa-sync"></i> Đặt lại
				</button>
			</div>
		</div>

		<!-- Customer statistics cards -->
		<div class="stats-cards">
			<div class="stats-card">
				<div class="stats-icon">
					<i class="fas fa-users"></i>
				</div>
				<div class="stats-content">
					<h3>{{ customers.length }}</h3>
					<p>Tổng số khách hàng</p>
				</div>
			</div>

			<div class="stats-card">
				<div class="stats-icon active">
					<i class="fas fa-user-check"></i>
				</div>
				<div class="stats-content">
					<h3>{{ customers.filter((c) => !c.isLocked).length }}</h3>
					<p>Khách hàng đang hoạt động</p>
				</div>
			</div>

			<div class="stats-card">
				<div class="stats-icon locked">
					<i class="fas fa-user-lock"></i>
				</div>
				<div class="stats-content">
					<h3>{{ customers.filter((c) => c.isLocked).length }}</h3>
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

			<div v-else-if="filteredCustomers.length === 0" class="empty-state">
				<i class="fas fa-users"></i>
				<p v-if="searchQuery || statusFilter !== 'all'">
					Không tìm thấy khách hàng phù hợp với bộ lọc
				</p>
				<p v-else>Chưa có khách hàng nào</p>
				<button @click="resetFilters" class="action-button">
					<i class="fas fa-sync"></i> Đặt lại bộ lọc
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
						v-for="customer in paginatedCustomers"
						:key="customer.id"
						class="customer-row"
					>
						<td class="customer-info">
							<div class="avatar">
								<img
									:src="
										customer.avatar
											? formatAvatarUrl(customer.avatar)
											: getDefaultAvatar(customer.name)
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
						<td>{{ customer.phoneNumber || "Chưa cập nhật" }}</td>
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
									!customer.isLocked ? "Hoạt động" : "Bị khóa"
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

			<!-- Pagination -->
			<div v-if="filteredCustomers.length > 0" class="pagination">
				<button
					:disabled="currentPage === 1"
					@click="currentPage--"
					class="page-button"
				>
					<i class="fas fa-chevron-left"></i>
				</button>
				<span class="page-info">
					Trang {{ currentPage }} / {{ totalPages }}
				</span>
				<button
					:disabled="currentPage === totalPages"
					@click="currentPage++"
					class="page-button"
				>
					<i class="fas fa-chevron-right"></i>
				</button>
			</div>
		</div>

		<!-- Customer Details Modal -->
		<div v-if="showModal" class="modal-backdrop">
			<div class="modal-container">
				<div class="modal-header">
					<h3>Thông tin chi tiết khách hàng</h3>
					<button @click="showModal = false" class="close-button">
						<i class="fas fa-times"></i>
					</button>
				</div>

				<div class="modal-body">
					<div class="customer-profile">
						<div class="profile-header">
							<div class="profile-avatar">
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
							<div class="profile-title">
								<h4>
									{{
										selectedCustomer.name ||
										"Chưa cập nhật tên"
									}}
								</h4>
								<p class="join-date">
									Lần đăng nhập cuối:
									{{
										selectedCustomer.lastLogin
											? formatRegistrationDate(
													selectedCustomer.lastLogin
											  )
											: "Chưa cập nhật"
									}}
								</p>
							</div>
						</div>

						<div class="profile-details">
							<div class="detail-group">
								<label>Email:</label>
								<div class="detail-value">
									{{
										selectedCustomer.email ||
										"Chưa cập nhật"
									}}
								</div>
							</div>

							<div class="detail-group">
								<label>Số điện thoại:</label>
								<div class="detail-value">
									{{
										selectedCustomer.phoneNumber ||
										"Chưa cập nhật"
									}}
								</div>
							</div>

							<div class="detail-group">
								<label>Trạng thái:</label>
								<div class="detail-value">
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

							<div
								v-if="selectedCustomer.isLocked"
								class="detail-group"
							>
								<label>Lý do khóa:</label>
								<div class="detail-value lock-reason">
									{{
										selectedCustomer.lockReason ||
										"Không có lý do"
									}}
								</div>
							</div>

							<div class="detail-group">
								<label>Giới tính:</label>
								<div class="detail-value">
									{{
										selectedCustomer.gender ||
										"Chưa cập nhật"
									}}
								</div>
							</div>

							<div class="detail-group">
								<label>Địa chỉ:</label>
								<div class="detail-value">
									{{
										selectedCustomer.address ||
										"Chưa cập nhật"
									}}
								</div>
							</div>
						</div>

						<div class="profile-actions">
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

.refresh-button {
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

.refresh-button i {
	color: var(--primary-color);
}

.refresh-button:hover {
	background-color: #f8f0f4;
	color: var(--primary-color);
}

.page-header h1 {
	margin: 0;
	font-size: 1.8rem;
	font-weight: 600;
	color: #333;
}

/* Control Panel */
.control-panel {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 2rem;
	flex-wrap: wrap;
	gap: 1rem;
}

.search-section {
	flex: 1;
	min-width: 250px;
}

.search-box {
	position: relative;
	width: 100%;
	max-width: 500px;
}

.search-box i {
	position: absolute;
	left: 12px;
	top: 50%;
	transform: translateY(-50%);
	color: #6e6e6e;
}

.search-box input {
	width: 100%;
	padding: 0.75rem 0.75rem 0.75rem 2.5rem;
	border: 1px solid #e2e8f0;
	border-radius: 8px;
	font-size: 1rem;
	transition: all 0.3s;
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
}

.filter-section {
	display: flex;
	gap: 1rem;
	align-items: center;
	flex-wrap: wrap;
}

.filter-group {
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.filter-group label {
	color: #666;
	font-size: 0.9rem;
}

.filter-group select {
	padding: 0.5rem 2rem 0.5rem 0.75rem;
	border: 1px solid #e2e8f0;
	border-radius: 8px;
	background-color: white;
	font-size: 0.9rem;
	appearance: none;
	background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%236e6e6e' viewBox='0 0 16 16'%3E%3Cpath d='M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z'/%3E%3C/svg%3E");
	background-repeat: no-repeat;
	background-position: right 8px center;
	transition: all 0.3s;
}

.filter-group select:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(232, 93, 185, 0.25);
	outline: none;
}

.reset-button {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	padding: 0.5rem 1rem;
	background-color: #f3f4f6;
	border: 1px solid #e2e8f0;
	border-radius: 8px;
	color: #4b5563;
	font-size: 0.9rem;
	cursor: pointer;
	transition: all 0.3s;
}

.reset-button:hover {
	background-color: #e5e7eb;
}

/* Stats Cards */
.stats-cards {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
	gap: 1.5rem;
	margin-bottom: 2rem;
}

.stats-card {
	background-color: white;
	border-radius: 12px;
	padding: 1.5rem;
	display: flex;
	align-items: center;
	gap: 1.5rem;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
	transition: transform 0.3s ease;
}

.stats-card:hover {
	transform: translateY(-5px);
}

.stats-icon {
	width: 56px;
	height: 56px;
	border-radius: 12px;
	background-color: #f0f9ff;
	color: #0284c7;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 1.5rem;
}

.stats-icon.active {
	background-color: #ecfdf5;
	color: #10b981;
}

.stats-icon.locked {
	background-color: #fef2f2;
	color: #ef4444;
}

.stats-content h3 {
	font-size: 1.5rem;
	font-weight: 600;
	margin: 0 0 0.25rem 0;
	color: #111827;
}

.stats-content p {
	margin: 0;
	color: #6b7280;
	font-size: 0.9rem;
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

/* Pagination */
.pagination {
	display: flex;
	justify-content: center;
	align-items: center;
	margin-top: 1.5rem;
	gap: 1rem;
}

.page-info {
	color: #6b7280;
	font-size: 0.9rem;
}

.page-button {
	width: 32px;
	height: 32px;
	border-radius: 6px;
	background-color: white;
	border: 1px solid #e5e7eb;
	display: flex;
	align-items: center;
	justify-content: center;
	color: #4b5563;
	cursor: pointer;
	transition: all 0.2s;
}

.page-button:hover:not(:disabled) {
	background-color: #f3f4f6;
}

.page-button:disabled {
	opacity: 0.5;
	cursor: not-allowed;
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

/* Customer Profile */
.customer-profile {
	display: flex;
	flex-direction: column;
	gap: 1.5rem;
}

.profile-header {
	display: flex;
	align-items: center;
	gap: 1.5rem;
}

.profile-avatar {
	position: relative;
	width: 96px;
	height: 96px;
	border-radius: 50%;
	overflow: hidden;
}

.profile-avatar img {
	width: 100%;
	height: 100%;
	object-fit: cover;
}

.profile-title h4 {
	margin: 0 0 0.25rem 0;
	font-size: 1.25rem;
	color: #111827;
}

.join-date {
	margin: 0;
	color: #6b7280;
	font-size: 0.875rem;
}

.profile-details {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
	gap: 1.5rem;
}

.detail-group {
	display: flex;
	flex-direction: column;
	gap: 0.375rem;
}

.detail-group label {
	font-size: 0.875rem;
	font-weight: 500;
	color: #6b7280;
}

.detail-value {
	font-size: 1rem;
	color: #111827;
}

.lock-reason {
	padding: 0.75rem;
	background-color: #fee2e2;
	border-radius: 6px;
	color: #b91c1c;
	font-size: 0.875rem;
}

.profile-actions {
	display: flex;
	justify-content: flex-end;
	margin-top: 1rem;
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

	.control-panel {
		flex-direction: column;
		align-items: stretch;
	}

	.search-section {
		width: 100%;
	}

	.filter-section {
		flex-direction: column;
		align-items: flex-start;
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
