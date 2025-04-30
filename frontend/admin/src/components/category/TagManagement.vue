<template>
    <div class="tag-management">
        <div class="flex justify-between items-center mb-6">
            <div class="flex items-center">
                <span class="material-icons-outlined text-green-600 mr-2"
                    >sell</span
                >
                <h2 class="text-xl font-semibold text-gray-800">Quản lý Tag</h2>
            </div>

            <div class="flex items-center">
                <div class="relative mr-4">
                    <span
                        class="absolute inset-y-0 left-0 flex items-center pl-3"
                    >
                        <span class="material-icons-outlined text-gray-400"
                            >search</span
                        >
                    </span>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm tag..."
                        class="pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:border-green-500 focus:ring-1 focus:ring-green-500 focus:outline-none"
                    />
                </div>
                <button
                    @click="openAddTagModal"
                    class="flex items-center bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md transition-colors shadow-sm"
                >
                    <span class="material-icons-outlined mr-1">add</span>
                    Thêm tag
                </button>
            </div>
        </div>

        <!-- Stats cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            <div
                class="bg-white rounded-lg shadow-sm border border-gray-200 p-4"
            >
                <div class="flex items-center">
                    <div
                        class="flex-shrink-0 bg-green-100 rounded-full p-3 mr-4"
                    >
                        <span class="material-icons-outlined text-green-600"
                            >sell</span
                        >
                    </div>
                    <div>
                        <p class="text-sm font-medium text-gray-500">
                            Tổng số Tag
                        </p>
                        <h3 class="text-xl font-bold text-gray-800">
                            {{ tags.length }}
                        </h3>
                    </div>
                </div>
            </div>

            <div
                class="bg-white rounded-lg shadow-sm border border-gray-200 p-4"
            >
                <div class="flex items-center">
                    <div
                        class="flex-shrink-0 bg-blue-100 rounded-full p-3 mr-4"
                    >
                        <span class="material-icons-outlined text-blue-600"
                            >phone_android</span
                        >
                    </div>
                    <div>
                        <p class="text-sm font-medium text-gray-500">
                            Số sản phẩm có tag
                        </p>
                        <h3 class="text-xl font-bold text-gray-800">
                            {{ totalProductsWithTags }}
                        </h3>
                    </div>
                </div>
            </div>

            <div
                class="bg-white rounded-lg shadow-sm border border-gray-200 p-4"
            >
                <div class="flex items-center">
                    <div
                        class="flex-shrink-0 bg-purple-100 rounded-full p-3 mr-4"
                    >
                        <span class="material-icons-outlined text-purple-600"
                            >trending_up</span
                        >
                    </div>
                    <div>
                        <p class="text-sm font-medium text-gray-500">
                            Tag phổ biến nhất
                        </p>
                        <h3 class="text-xl font-bold text-gray-800">
                            {{ mostPopularTag || "Chưa có" }}
                        </h3>
                    </div>
                </div>
            </div>
        </div>

        <!-- Tags table -->
        <div
            class="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden"
        >
            <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th
                                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            >
                                ID
                            </th>
                            <th
                                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            >
                                Tên tag
                            </th>
                            <th
                                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            >
                                Số sản phẩm
                            </th>
                            <th
                                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            >
                                Trạng thái
                            </th>
                            <th
                                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            >
                                Hành động
                            </th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        <tr v-if="loading">
                            <td colspan="5" class="px-6 py-4 text-center">
                                <div class="flex justify-center">
                                    <div
                                        class="animate-spin rounded-full h-6 w-6 border-b-2 border-green-500"
                                    ></div>
                                </div>
                                <p class="mt-2 text-sm text-gray-500">
                                    Đang tải dữ liệu...
                                </p>
                            </td>
                        </tr>
                        <tr v-else-if="filteredTags.length === 0">
                            <td
                                colspan="5"
                                class="px-6 py-4 text-center text-gray-500"
                            >
                                <div class="flex flex-col items-center py-6">
                                    <span
                                        class="material-icons-outlined text-gray-400 text-4xl mb-2"
                                        >sell</span
                                    >
                                    <p>Không tìm thấy tag nào</p>
                                </div>
                            </td>
                        </tr>
                        <tr
                            v-for="tag in filteredTags"
                            :key="tag.id"
                            class="hover:bg-gray-50 transition-colors"
                        >
                            <td
                                class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900"
                            >
                                {{ tag.id }}
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="flex items-center">
                                    <span
                                        class="px-2 py-1 text-xs rounded-full bg-green-100 text-green-800 font-medium"
                                    >
                                        {{ tag.name }}
                                    </span>
                                </div>
                            </td>
                            <td
                                class="px-6 py-4 whitespace-nowrap text-sm text-gray-700"
                            >
                                <div class="flex items-center">
                                    <span
                                        class="material-icons-outlined text-gray-400 text-sm mr-1"
                                        >phone_android</span
                                    >
                                    <span>{{ tag.productCount || 0 }}</span>
                                </div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <span
                                    :class="[
                                        'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                                        tag.productCount > 0
                                            ? 'bg-green-100 text-green-800'
                                            : 'bg-gray-100 text-gray-800',
                                    ]"
                                >
                                    {{
                                        tag.productCount > 0
                                            ? "Đang sử dụng"
                                            : "Chưa sử dụng"
                                    }}
                                </span>
                            </td>
                            <td
                                class="px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                            >
                                <div class="flex space-x-2">
                                    <button
                                        @click="editTag(tag)"
                                        class="text-blue-600 hover:text-blue-900 bg-blue-100 hover:bg-blue-200 rounded-full p-1 transition-colors"
                                        title="Chỉnh sửa"
                                    >
                                        <span
                                            class="material-icons-outlined text-sm"
                                            >edit</span
                                        >
                                    </button>
                                    <button
                                        @click="confirmDelete(tag)"
                                        class="text-red-600 hover:text-red-900 bg-red-100 hover:bg-red-200 rounded-full p-1 transition-colors"
                                        title="Xóa"
                                    >
                                        <span
                                            class="material-icons-outlined text-sm"
                                            >delete</span
                                        >
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Pagination -->
        <div
            class="flex items-center justify-between mt-4 bg-white p-4 border border-gray-200 rounded-lg shadow-sm"
        >
            <div class="flex items-center">
                <span class="text-sm text-gray-700">
                    Hiển thị
                    <span class="font-medium">{{ filteredTags.length }}</span> /
                    <span class="font-medium">{{ tags.length }}</span> tag
                </span>
            </div>
            <div class="flex items-center space-x-2">
                <button
                    class="px-3 py-1 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-50"
                    :disabled="currentPage === 1"
                >
                    <span class="material-icons-outlined text-sm"
                        >arrow_back_ios</span
                    >
                </button>
                <div class="text-sm text-gray-700">
                    Trang {{ currentPage }} / {{ totalPages }}
                </div>
                <button
                    class="px-3 py-1 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-50"
                    :disabled="currentPage === totalPages"
                >
                    <span class="material-icons-outlined text-sm"
                        >arrow_forward_ios</span
                    >
                </button>
            </div>
        </div>

        <!-- Add/Edit Tag Modal -->
        <div
            v-if="showModal"
            class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 backdrop-blur-sm"
        >
            <div class="bg-white rounded-lg shadow-xl p-6 w-full max-w-md m-4">
                <div class="flex justify-between items-center mb-6">
                    <h3 class="text-xl font-bold text-gray-800">
                        {{ isEditing ? "Chỉnh sửa tag" : "Thêm tag mới" }}
                    </h3>
                    <button
                        @click="closeModal"
                        class="text-gray-500 hover:text-gray-700"
                    >
                        <span class="material-icons-outlined">close</span>
                    </button>
                </div>

                <form @submit.prevent="submitForm">
                    <div class="mb-4">
                        <label
                            class="block text-sm font-medium text-gray-700 mb-2"
                            >Tên tag <span class="text-red-500">*</span></label
                        >
                        <input
                            v-model="formData.name"
                            type="text"
                            class="w-full p-3 border border-gray-300 rounded-md focus:border-green-500 focus:ring-1 focus:ring-green-500 focus:outline-none"
                            placeholder="Nhập tên tag"
                            required
                        />
                        <p class="mt-1 text-xs text-gray-500">
                            Tên tag phải là duy nhất và không được trùng lặp
                        </p>
                    </div>

                    <div class="flex justify-end space-x-3 mt-6">
                        <button
                            type="button"
                            @click="closeModal"
                            class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                        >
                            Hủy
                        </button>
                        <button
                            type="submit"
                            class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors flex items-center"
                            :disabled="loading"
                        >
                            <span
                                v-if="loading"
                                class="animate-spin h-4 w-4 mr-2 border-2 border-white border-t-transparent rounded-full"
                            ></span>
                            {{ isEditing ? "Cập nhật" : "Thêm mới" }}
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <!-- Delete Confirmation Modal -->
        <div
            v-if="showDeleteModal"
            class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 backdrop-blur-sm"
        >
            <div class="bg-white rounded-lg shadow-xl p-6 w-full max-w-md m-4">
                <div class="flex flex-col items-center text-center mb-6">
                    <div
                        class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mb-4"
                    >
                        <span
                            class="material-icons-outlined text-3xl text-red-600"
                            >warning</span
                        >
                    </div>
                    <h3 class="text-xl font-bold text-gray-800 mb-2">
                        Xác nhận xóa tag
                    </h3>
                    <p class="text-gray-600">
                        Bạn có chắc chắn muốn xóa tag "<span
                            class="font-medium"
                            >{{ tagToDelete?.name }}</span
                        >"?
                    </p>
                    <div
                        v-if="hasProducts"
                        class="mt-4 p-3 bg-red-50 border border-red-200 rounded-md text-left"
                    >
                        <div class="flex items-start">
                            <span
                                class="material-icons-outlined text-red-600 mr-2 mt-0.5"
                                >error</span
                            >
                            <p class="text-sm text-red-600">
                                <span class="font-semibold">Cảnh báo:</span> Tag
                                này đang được sử dụng bởi
                                {{ tagToDelete?.productCount }} sản phẩm. Việc
                                xóa có thể ảnh hưởng đến dữ liệu liên quan.
                            </p>
                        </div>
                    </div>
                </div>

                <div class="flex justify-end space-x-3">
                    <button
                        @click="cancelDelete"
                        class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                    >
                        Hủy
                    </button>
                    <button
                        @click="deleteTag"
                        class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors flex items-center"
                        :disabled="loading"
                    >
                        <span
                            v-if="loading"
                            class="animate-spin h-4 w-4 mr-2 border-2 border-white border-t-transparent rounded-full"
                        ></span>
                        Xóa
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import axios from "axios";

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
const currentPage = ref(1);
const itemsPerPage = ref(10);

// API base URL
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api/v1";

// Computed properties
const filteredTags = computed(() => {
    if (!searchQuery.value) return tags.value;

    return tags.value.filter((tag) =>
        tag.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    );
});

const totalPages = computed(() => {
    return Math.ceil(filteredTags.value.length / itemsPerPage.value) || 1;
});

const totalProductsWithTags = computed(() => {
    return tags.value.reduce(
        (total, tag) => total + (tag.productCount || 0),
        0
    );
});

const mostPopularTag = computed(() => {
    if (tags.value.length === 0) return null;

    const mostUsedTag = [...tags.value].sort(
        (a, b) => (b.productCount || 0) - (a.productCount || 0)
    )[0];
    return mostUsedTag.productCount > 0 ? mostUsedTag.name : null;
});

// Fetch all tags
const fetchTags = async () => {
    loading.value = true;
    try {
        const response = await axios.get(`${API_URL}/tags`);

        // Fetch product counts for each tag
        const tagsWithCounts = await Promise.all(
            response.data.tags.map(async (tag) => {
                try {
                    // This endpoint needs to be implemented in the backend
                    // to return count of products associated with this tag
                    const countResponse = await axios.get(
                        `${API_URL}/product?tagId=${tag.id}&count=true`
                    );
                    return {
                        ...tag,
                        productCount: countResponse.data.count || 0,
                    };
                } catch (error) {
                    console.error(
                        `Error fetching count for tag ${tag.id}:`,
                        error
                    );
                    return {
                        ...tag,
                        productCount: 0,
                    };
                }
            })
        );

        tags.value = tagsWithCounts;
    } catch (error) {
        console.error("Error fetching tags:", error);
        // Handle error (show notification, etc.)
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

// Open modal to edit tag
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
            await axios.put(
                `${API_URL}/tags/${formData.value.id}`,
                formData.value
            );
        } else {
            await axios.post(`${API_URL}/tags`, formData.value);
        }

        closeModal();
        fetchTags();
    } catch (error) {
        console.error("Error submitting form:", error);
        // Handle error (show notification, etc.)
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
    hasProducts.value = tag.productCount > 0;
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
        await axios.delete(`${API_URL}/tags/${tagToDelete.value.id}`);
        fetchTags();
        showDeleteModal.value = false;
        tagToDelete.value = null;
    } catch (error) {
        console.error("Error deleting tag:", error);
        // Handle error (show notification, etc.)
    } finally {
        loading.value = false;
    }
};

// Load tags when component mounts
onMounted(() => {
    fetchTags();
});
</script>

<style scoped>
/* Additional styling if needed */
</style>
