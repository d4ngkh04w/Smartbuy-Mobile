<template>
	<div class="tag-management">
		<div class="section-header">
			<div class="left-section">
				<h2><i class="fas fa-tags"></i> Quản lý Tag</h2>
				<p>Quản lý các tag cho sản phẩm</p>
			</div>

			<div class="right-section">
				<div class="search-box">
					<i class="fas fa-search"></i>
					<input
						type="text"
						v-model="searchQuery"
						placeholder="Tìm kiếm tag..."
					/>
				</div>

				<button @click="openAddTagModal" class="add-button">
					<i class="fas fa-plus"></i> Thêm tag
				</button>
			</div>
		</div>

		<!-- Status Cards -->
		<div class="status-cards">
			<div class="status-card">
				<div class="icon-wrapper">
					<i class="fas fa-tags"></i>
				</div>
				<div class="status-content">
					<h3>{{ tags.length }}</h3>
					<p>Tổng số tag</p>
				</div>
			</div>

			<div class="status-card">
				<div class="icon-wrapper">
					<i class="fas fa-mobile-alt"></i>
				</div>
				<div class="status-content">
					<h3>{{ totalProductsWithTags }}</h3>
					<p>Sản phẩm đang gắn tag</p>
				</div>
			</div>

			<div class="status-card">
				<div class="icon-wrapper">
					<i class="fas fa-star"></i>
				</div>
				<div class="status-content">
					<h3>{{ mostPopularTag || "Chưa có" }}</h3>
					<p>Tag phổ biến nhất</p>
				</div>
			</div>
		</div>

		<!-- Tags Table -->
		<div class="data-card">
			<div v-if="loading" class="loading-state">
				<div class="spinner"></div>
				<p>Đang tải dữ liệu...</p>
			</div>
			<div v-else-if="filteredTags.length === 0" class="empty-state">
				<i class="fas fa-tags"></i>
				<p v-if="searchQuery">
					Không tìm thấy tag phù hợp với tìm kiếm
				</p>
				<p v-else>Không có tag nào</p>
				<button @click="openAddTagModal" class="action-button">
					<i class="fas fa-plus"></i> Thêm tag mới
				</button>
			</div>
			<table v-else class="data-table">
				<thead>
					<tr>
						<th>ID</th>
						<th>Tên tag</th>
						<th>Số sản phẩm</th>
						<th>Trạng thái</th>
						<th>Hành động</th>
					</tr>
				</thead>
				<tbody>
					<tr
						v-for="tag in filteredTags"
						:key="tag.id"
						class="data-row"
					>
						<td>{{ tag.id }}</td>
						<td class="name-cell">
							<span class="tag-badge">{{ tag.name }}</span>
						</td>
						<td class="count-cell">
							{{ tag.productCount || 0 }}
						</td>
						<td>
							<span
								:class="[
									'status-badge',
									(tag.productCount || 0) > 0
										? 'active'
										: 'inactive',
								]"
							>
								{{
									(tag.productCount || 0) > 0
										? "Đang sử dụng"
										: "Chưa sử dụng"
								}}
							</span>
						</td>
						<td class="actions-cell">
							<button
								@click="editTag(tag)"
								class="edit-button"
								title="Chỉnh sửa"
							>
								<i class="fas fa-edit"></i>
							</button>
							<button
								@click="confirmDelete(tag)"
								class="delete-button"
								title="Xóa"
							>
								<i class="fas fa-trash-alt"></i>
							</button>
						</td>
					</tr>
				</tbody>
			</table>
		</div>

		<!-- Add/Edit Tag Modal -->
		<div v-if="showModal" class="modal-backdrop">
			<div class="modal-container">
				<div class="modal-header">
					<h3>
						{{ isEditing ? "Chỉnh sửa tag" : "Thêm tag mới" }}
					</h3>
					<button @click="closeModal" class="close-button">
						<i class="fas fa-times"></i>
					</button>
				</div>

				<form @submit.prevent="submitForm" class="modal-form">
					<div class="form-group">
						<label>Tên tag <span class="required">*</span></label>
						<input
							v-model="formData.name"
							type="text"
							placeholder="Nhập tên tag"
							required
						/>
						<p class="form-hint">
							Tên tag phải là duy nhất và không được trùng lặp
						</p>
					</div>

					<div class="form-actions">
						<button
							type="button"
							@click="closeModal"
							class="cancel-button"
						>
							<i class="fas fa-times"></i> Hủy
						</button>
						<button
							type="submit"
							class="submit-button"
							:disabled="loading"
						>
							<span v-if="loading" class="spinner small"></span>
							<i
								v-else
								class="fas"
								:class="isEditing ? 'fa-save' : 'fa-plus'"
							></i>
							{{ isEditing ? "Cập nhật" : "Thêm mới" }}
						</button>
					</div>
				</form>
			</div>
		</div>

		<!-- Delete Confirmation Modal -->
		<div v-if="showDeleteModal" class="modal-backdrop">
			<div class="modal-container warning-modal">
				<div class="modal-header warning">
					<h3>Xác nhận xóa tag</h3>
					<button @click="cancelDelete" class="close-button">
						<i class="fas fa-times"></i>
					</button>
				</div>

				<div class="modal-body text-center">
					<div class="warning-icon">
						<i class="fas fa-exclamation-triangle"></i>
					</div>
					<p class="warning-message">
						Bạn có chắc chắn muốn xóa tag
						<strong>"{{ tagToDelete?.name }}"</strong>?
					</p>
					<div v-if="hasProducts" class="warning-details">
						<i class="fas fa-info-circle"></i>
						<span
							>Tag này đang được gán cho
							{{ tagToDelete?.productCount }}
							sản phẩm. Việc xóa có thể ảnh hưởng đến dữ liệu liên
							quan.</span
						>
					</div>
				</div>

				<div class="modal-actions">
					<button @click="cancelDelete" class="cancel-button">
						<i class="fas fa-arrow-left"></i> Quay lại
					</button>
					<button
						@click="deleteTag"
						class="delete-confirm-button"
						:disabled="loading"
					>
						<span v-if="loading" class="spinner small"></span>
						<i v-else class="fas fa-trash-alt"></i> Xóa
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import {
	getAllTags,
	getTagById,
	createTag,
	updateTag,
	deleteTag as deleteTagAPI,
} from "@/services/tagService";

// State
const tags = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showDeleteModal = ref(false);
const isEditing = ref(false);
const formData = ref({
	name: "",
});
const tagToDelete = ref(null);
const hasProducts = ref(false);
const searchQuery = ref("");

// API base URL
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api/v1";

// Computed properties
const filteredTags = computed(() => {
	if (!searchQuery.value) return tags.value;

	const query = searchQuery.value.toLowerCase().trim();
	return tags.value.filter((tag) => tag.name.toLowerCase().includes(query));
});

const totalProductsWithTags = computed(() => {
	return tags.value.reduce(
		(total, tag) => total + (tag.productCount || 0),
		0
	);
});

const mostPopularTag = computed(() => {
	if (tags.value.length === 0) return null;

	const sortedTags = [...tags.value].sort(
		(a, b) => (b.productCount || 0) - (a.productCount || 0)
	);

	return sortedTags[0]?.productCount > 0 ? sortedTags[0].name : null;
});

// Methods
// Fetch all tags
const fetchTags = async () => {
	loading.value = true;
	try {
		const response = await getAllTags();
		console.log("API Response:", response);

		if (response.data && response.data.data) {
			// If response has a 'tags' property (standard format)
			tags.value = response.data.data.map((tag) => ({
				...tag,
				productCount: tag.productCount || 0,
			}));
		} else if (Array.isArray(response.data)) {
			// If response data is directly an array of tags
			tags.value = response.data.map((tag) => ({
				...tag,
				productCount: tag.productCount || 0,
			}));
		} else {
			console.error("Unexpected data format:", response.data);
			tags.value = [];
		}

		console.log("Loaded tags:", tags.value);
	} catch (error) {
		console.error("Error fetching tags:", error);
		tags.value = [];
	} finally {
		loading.value = false;
	}
};

// Open modal to add new tag
const openAddTagModal = () => {
	isEditing.value = false;
	formData.value = {
		name: "",
	};
	showModal.value = true;
};

// Edit tag modal
const editTag = (tag) => {
	isEditing.value = true;
	formData.value = {
		id: tag.id,
		name: tag.name,
	};
	showModal.value = true;
};

// Submit form (create or update)
const submitForm = async () => {
	loading.value = true;

	try {
		if (isEditing.value) {
			await updateTag(formData.value.id, formData.value);
		} else {
			await createTag(formData.value);
		}

		await fetchTags();
		closeModal();
	} catch (error) {
		console.error("Error submitting form:", error);
		alert("Có lỗi xảy ra khi lưu tag. Vui lòng kiểm tra và thử lại.");
	} finally {
		loading.value = false;
	}
};

// Close modal
const closeModal = () => {
	showModal.value = false;
	formData.value = {
		name: "",
	};
};

// Confirm delete
const confirmDelete = (tag) => {
	tagToDelete.value = tag;
	hasProducts.value = (tag.productCount || 0) > 0;
	showDeleteModal.value = true;
};

// Cancel delete
const cancelDelete = () => {
	showDeleteModal.value = false;
	tagToDelete.value = null;
	hasProducts.value = false;
};

// Delete tag
const deleteTag = async () => {
	if (!tagToDelete.value) return;

	loading.value = true;
	try {
		await deleteTagAPI(tagToDelete.value.id);
		await fetchTags();
		showDeleteModal.value = false;
		tagToDelete.value = null;
	} catch (error) {
		console.error("Error deleting tag:", error);
		alert("Có lỗi xảy ra khi xóa tag. Vui lòng thử lại sau.");
	} finally {
		loading.value = false;
	}
};

// Load data when component mounts
onMounted(async () => {
	await fetchTags();
});
</script>

<style scoped>
.tag-management {
	width: 100%;
}

.section-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 1.5rem;
}

.left-section h2 {
	font-size: 1.25rem;
	color: #333;
	margin: 0 0 0.25rem 0;
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.left-section p {
	color: #666;
	margin: 0;
	font-size: 0.9rem;
}

.right-section {
	display: flex;
	gap: 1rem;
	align-items: center;
}

.search-box {
	position: relative;
	width: 250px;
}

.search-box i {
	position: absolute;
	left: 10px;
	top: 50%;
	transform: translateY(-50%);
	color: #666;
}

.search-box input {
	width: 100%;
	padding: 0.6rem 0.6rem 0.6rem 2rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.9rem;
	transition: all 0.3s;
}

.search-box input:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
	outline: none;
}

.add-button {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	padding: 0.6rem 1rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 8px;
	cursor: pointer;
	transition: all 0.3s;
}

.add-button:hover {
	background-color: #e94e9c;
}

.status-cards {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
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

.data-card {
	background-color: white;
	border-radius: 12px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
	padding: 1.5rem;
	overflow: hidden;
}

.loading-state,
.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 3rem 0;
	color: #666;
}

.spinner {
	width: 40px;
	height: 40px;
	border: 3px solid rgba(248, 110, 211, 0.1);
	border-top: 3px solid var(--primary-color);
	border-radius: 50%;
	animation: spin 1s linear infinite;
	margin-bottom: 1rem;
}

.spinner.small {
	width: 16px;
	height: 16px;
	border-width: 2px;
	margin: 0;
	margin-right: 0.5rem;
}

@keyframes spin {
	0% {
		transform: rotate(0deg);
	}
	100% {
		transform: rotate(360deg);
	}
}

.empty-state i {
	font-size: 3rem;
	color: #ddd;
	margin-bottom: 1rem;
}

.empty-state p {
	margin-bottom: 1rem;
}

.action-button {
	padding: 0.5rem 1rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 8px;
	cursor: pointer;
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.data-table {
	width: 100%;
	border-collapse: collapse;
	table-layout: fixed;
}

.data-table th {
	text-align: left;
	padding: 1rem;
	color: #666;
	border-bottom: 1px solid #eee;
	font-weight: 600;
	font-size: 0.85rem;
	text-transform: uppercase;
}

.data-table th:nth-child(1) {
	width: 10%;
} /* ID */
.data-table th:nth-child(2) {
	width: 40%;
} /* Tên tag */
.data-table th:nth-child(3) {
	width: 15%;
} /* Số sản phẩm */
.data-table th:nth-child(4) {
	width: 20%;
} /* Trạng thái */
.data-table th:nth-child(5) {
	width: 15%;
} /* Hành động */

.data-row {
	border-bottom: 1px solid #eee;
	transition: background-color 0.2s;
}

.data-row:hover {
	background-color: #f9f9f9;
}

.data-row td {
	padding: 1rem;
	color: #333;
	vertical-align: middle;
}

.name-cell {
	font-weight: 500;
}

.tag-badge {
	display: inline-block;
	padding: 0.35rem 0.75rem;
	background-color: #f0f9ff;
	color: #0369a1;
	border-radius: 16px;
	font-size: 0.85rem;
	font-weight: 500;
}

.count-cell {
	text-align: center;
	font-weight: 500;
}

.status-badge {
	padding: 0.25rem 0.5rem;
	border-radius: 12px;
	font-size: 0.75rem;
	font-weight: 500;
}

.status-badge.active {
	background-color: #e6f7ea;
	color: #22c55e;
}

.status-badge.inactive {
	background-color: #f9f9f9;
	color: #6b7280;
}

.actions-cell {
	display: flex;
	gap: 0.5rem;
}

.edit-button,
.delete-button {
	width: 32px;
	height: 32px;
	border-radius: 8px;
	display: flex;
	align-items: center;
	justify-content: center;
	border: none;
	cursor: pointer;
	transition: all 0.2s;
}

.edit-button {
	background-color: #e6f7ea;
	color: #22c55e;
}

.edit-button:hover {
	background-color: #22c55e;
	color: white;
}

.delete-button {
	background-color: #fee2e2;
	color: #ef4444;
}

.delete-button:hover {
	background-color: #ef4444;
	color: white;
}

/* Modal Styles */
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
	z-index: 1000;
	backdrop-filter: blur(3px);
}

.modal-container {
	background-color: white;
	border-radius: 12px;
	box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
	width: 100%;
	max-width: 500px;
	max-height: 90vh;
	overflow-y: auto;
}

.modal-header {
	padding: 1.25rem 1.5rem;
	border-bottom: 1px solid #eee;
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.modal-header.warning {
	border-bottom-color: #fee2e2;
	color: #ef4444;
}

.modal-header h3 {
	margin: 0;
	font-size: 1.25rem;
	color: #333;
}

.close-button {
	background: none;
	border: none;
	color: #666;
	font-size: 1rem;
	cursor: pointer;
	width: 32px;
	height: 32px;
	border-radius: 8px;
	display: flex;
	align-items: center;
	justify-content: center;
	transition: all 0.2s;
}

.close-button:hover {
	background-color: #f9f9f9;
	color: #333;
}

.modal-form {
	padding: 1.5rem;
}

.form-group {
	margin-bottom: 1.25rem;
}

.form-group label {
	display: block;
	margin-bottom: 0.5rem;
	font-weight: 500;
	color: #333;
}

.form-hint {
	margin-top: 0.5rem;
	font-size: 0.8rem;
	color: #666;
}

.form-group input[type="text"],
.form-group textarea {
	width: 100%;
	padding: 0.75rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.95rem;
	transition: all 0.3s;
}

.form-group input[type="text"]:focus,
.form-group textarea:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
	outline: none;
}

.required {
	color: #ef4444;
}

.form-actions {
	display: flex;
	justify-content: flex-end;
	gap: 1rem;
}

.cancel-button,
.submit-button,
.delete-confirm-button {
	padding: 0.75rem 1.25rem;
	border-radius: 8px;
	font-weight: 500;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	cursor: pointer;
	transition: all 0.2s;
}

.cancel-button {
	background-color: #f9f9f9;
	border: 1px solid #ddd;
	color: #666;
}

.cancel-button:hover {
	background-color: #eee;
}

.submit-button {
	background-color: var(--primary-color);
	border: none;
	color: white;
}

.submit-button:hover {
	background-color: #e94e9c;
}

.submit-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

.modal-body {
	padding: 1.5rem;
}

.text-center {
	text-align: center;
}

.warning-icon {
	width: 64px;
	height: 64px;
	border-radius: 50%;
	background-color: #fee2e2;
	color: #ef4444;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 2rem;
	margin: 0 auto 1.5rem auto;
}

.warning-message {
	font-size: 1.1rem;
	color: #333;
	margin-bottom: 1.5rem;
}

.warning-details {
	display: flex;
	align-items: flex-start;
	gap: 0.5rem;
	padding: 1rem;
	background-color: #fee2e2;
	border-radius: 8px;
	color: #ef4444;
	text-align: left;
	font-size: 0.9rem;
}

.modal-actions {
	padding: 1.25rem 1.5rem;
	border-top: 1px solid #eee;
	display: flex;
	justify-content: flex-end;
	gap: 1rem;
}

.delete-confirm-button {
	background-color: #ef4444;
	border: none;
	color: white;
}

.delete-confirm-button:hover {
	background-color: #dc2626;
}

.delete-confirm-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

/* Responsive tweaks */
@media (max-width: 768px) {
	.section-header {
		flex-direction: column;
		align-items: flex-start;
		gap: 1rem;
	}

	.right-section {
		width: 100%;
		flex-direction: column;
	}

	.search-box {
		width: 100%;
	}

	.add-button {
		width: 100%;
		justify-content: center;
	}

	.status-cards {
		grid-template-columns: 1fr;
	}

	.data-table {
		display: block;
		overflow-x: auto;
	}
}
</style>
