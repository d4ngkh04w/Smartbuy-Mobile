<template>
	<div class="discount-management">
		<DiscountManagementHeader
			:loading="loading"
			:searchQuery="searchQuery"
			@search="handleSearch"
			@add="openAddModal"
			@refresh="fetchDiscounts"
		/>
		<DiscountStatCards
			:totalDiscounts="totalDiscounts"
			:activeDiscounts="activeDiscounts"
			:upcomingDiscounts="upcomingDiscounts"
		/>

		<DiscountTable
			:discounts="filteredDiscounts"
			:loading="loading"
			@view="openDetailModal"
			@edit="openEditModal"
			@delete="openDeleteConfirmation"
		/>

		<!-- Modals -->
		<DiscountAddModal
			v-if="showAddModal"
			@close="closeAddModal"
			@add="addDiscount"
		/>

		<DiscountEditModal
			v-if="showEditModal"
			:discount="selectedDiscount"
			@close="closeEditModal"
			@update="updateDiscount"
		/>
		<DiscountDetailModal
			v-if="showDetailModal"
			:discount="selectedDiscount"
			:products="discountProducts"
			:loading="loadingProducts"
			@close="closeDetailModal"
			@edit="openEditModal"
			@add-products="openAddProductsModal"
			@refresh-products="() => fetchDiscountProducts(selectedDiscount.id)"
		/>

		<ConfirmationModal
			v-if="showDeleteConfirmation"
			title="Xác nhận xóa"
			message="Bạn có chắc chắn muốn xóa mã giảm giá này không? Hành động này không thể hoàn tác."
			@confirm="deleteDiscount"
			@cancel="closeDeleteConfirmation"
		/>

		<AddProductsModal
			v-if="showAddProductsModal"
			:discountId="selectedDiscount?.id"
			@close="closeAddProductsModal"
			@add-products="handleAddProducts"
		/>
	</div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import discountService from "../../services/discountService.js";
import productService from "../../services/productService.js";
import emitter from "../../utils/evenBus.js";

// Import component parts
import DiscountManagementHeader from "./components/DiscountManagementHeader.vue";
import DiscountStatCards from "./components/DiscountStatCards.vue";
import DiscountTable from "./components/DiscountTable.vue";
import DiscountDetailModal from "./components/DiscountDetailModal.vue";
import DiscountEditModal from "./components/DiscountEditModal.vue";
import DiscountAddModal from "./components/DiscountAddModal.vue";
import ConfirmationModal from "./components/ConfirmationModal.vue";
import AddProductsModal from "./components/AddProductsModal.vue";

// ======== STATE ========
// Main data
const discounts = ref([]);
const discountProducts = ref([]);

// UI state
const loading = ref(false);
const loadingProducts = ref(false);
const searchQuery = ref("");

// Modal states
const showDetailModal = ref(false);
const showEditModal = ref(false);
const showAddModal = ref(false);
const showDeleteConfirmation = ref(false);
const showAddProductsModal = ref(false);

// Selected data
const selectedDiscount = ref(null);

// ======== COMPUTED PROPERTIES ========
const totalDiscounts = computed(() => discounts.value.length);

const activeDiscounts = computed(() => {
	return discounts.value.filter((discount) => discount.status === "Active")
		.length;
});

const upcomingDiscounts = computed(() => {
	return discounts.value.filter((discount) => discount.status === "Upcoming")
		.length;
});

const filteredDiscounts = computed(() => {
	if (!searchQuery.value) return discounts.value;

	const query = searchQuery.value.toLowerCase();
	return discounts.value.filter(
		(discount) =>
			discount.id.toString().includes(query) ||
			(discount.discountPercentage &&
				discount.discountPercentage.toString().includes(query)) ||
			(discount.discountAmount &&
				discount.discountAmount.toString().includes(query))
	);
});

// ======== METHODS ========
// Notifications
const showErrorNotification = (message) => {
	emitter.emit("show-notification", {
		status: "error",
		message,
	});
};

const showSuccessNotification = (message) => {
	emitter.emit("show-notification", {
		status: "success",
		message,
	});
};

// Fetch data
const fetchDiscounts = async () => {
	loading.value = true;
	try {
		const response = await discountService.getDiscounts();
		if (response.data && response.data.data) {
			discounts.value = response.data.data;
		} else {
			discounts.value = [];
		}
	} catch (error) {
		console.error("Error fetching discounts:", error);
		showErrorNotification("Có lỗi xảy ra khi tải danh sách mã giảm giá");
	} finally {
		loading.value = false;
	}
};

const fetchDiscountProducts = async (discountId) => {
	loadingProducts.value = true;
	try {
		const response = await discountService.getProductsByDiscountId(
			discountId
		);
		if (response.data && response.data.data) {
			discountProducts.value = response.data.data;
		} else {
			discountProducts.value = [];
		}
	} catch (error) {
		console.error("Error fetching discount products:", error);
		showErrorNotification(
			"Có lỗi xảy ra khi tải danh sách sản phẩm giảm giá"
		);
	} finally {
		loadingProducts.value = false;
	}
};

// Modal handlers
const openDetailModal = async (discount) => {
	selectedDiscount.value = discount;
	await fetchDiscountProducts(discount.id);
	showDetailModal.value = true;
};

const closeDetailModal = () => {
	showDetailModal.value = false;
	selectedDiscount.value = null;
	discountProducts.value = [];
};

const openEditModal = (discount) => {
	selectedDiscount.value = discount;
	showEditModal.value = true;
	// Close the detail modal if it's open
	if (showDetailModal.value) {
		showDetailModal.value = false;
		// Don't reset selected discount or products as we need them for the edit modal
	}
};

const closeEditModal = () => {
	showEditModal.value = false;
	selectedDiscount.value = null;
};

const openAddModal = () => {
	showAddModal.value = true;
};

const closeAddModal = () => {
	showAddModal.value = false;
};

const openDeleteConfirmation = (discount) => {
	selectedDiscount.value = discount;
	showDeleteConfirmation.value = true;
};

const closeDeleteConfirmation = () => {
	showDeleteConfirmation.value = false;
	selectedDiscount.value = null;
};

const openAddProductsModal = (discount) => {
	selectedDiscount.value = discount;
	showAddProductsModal.value = true;
};

const closeAddProductsModal = () => {
	showAddProductsModal.value = false;
};

// Search handler
const handleSearch = (query) => {
	searchQuery.value = query;
};

// CRUD operations
const addDiscount = async (discountData) => {
	try {
		await discountService.createDiscount(discountData);
		await fetchDiscounts();
		showSuccessNotification("Thêm mã giảm giá thành công");
		closeAddModal();
	} catch (error) {
		console.error("Error adding discount:", error);
		showErrorNotification("Có lỗi xảy ra khi thêm mã giảm giá");
	}
};

const updateDiscount = async (id, discountData) => {
	try {
		await discountService.updateDiscount(id, discountData);
		await fetchDiscounts();
		showSuccessNotification("Cập nhật mã giảm giá thành công");
		closeEditModal();
	} catch (error) {
		console.error("Error updating discount:", error);
		showErrorNotification("Có lỗi xảy ra khi cập nhật mã giảm giá");
	}
};

const deleteDiscount = async () => {
	if (!selectedDiscount.value) return;

	try {
		await discountService.deleteDiscount(selectedDiscount.value.id);
		await fetchDiscounts();
		showSuccessNotification("Xóa mã giảm giá thành công");
		closeDeleteConfirmation();
	} catch (error) {
		console.error("Error deleting discount:", error);
		showErrorNotification("Có lỗi xảy ra khi xóa mã giảm giá");
	}
};

const handleAddProducts = async (discountId, productIds) => {
	try {
		console.log("Adding products to discount:", discountId, productIds);
		// Sửa lại tên thuộc tính "ProductIds" đúng với DTO ở backend (chú ý P và s viết hoa)
		await discountService.addProductsToDiscount(discountId, {
			ProductIds: productIds,
		});
		if (selectedDiscount.value?.id === discountId) {
			await fetchDiscountProducts(discountId);
		}
		showSuccessNotification(
			"Thêm sản phẩm vào chương trình giảm giá thành công"
		);
		closeAddProductsModal();
	} catch (error) {
		console.error("Error adding products to discount:", error);
		showErrorNotification(
			"Có lỗi xảy ra khi thêm sản phẩm vào chương trình giảm giá"
		);
	}
};

const removeProductFromDiscount = async (discountId, productId) => {
	try {
		await discountService.removeProductFromDiscount(discountId, productId);
		await fetchDiscountProducts(discountId);
		showSuccessNotification(
			"Xóa sản phẩm khỏi chương trình giảm giá thành công"
		);
	} catch (error) {
		console.error("Error removing product from discount:", error);
		showErrorNotification(
			"Có lỗi xảy ra khi xóa sản phẩm khỏi chương trình giảm giá"
		);
	}
};

// Lifecycle hooks
onMounted(() => {
	fetchDiscounts();
});
</script>

<style scoped>
.discount-management {
	width: 100%;
	padding: 1rem 0;
}
</style>
