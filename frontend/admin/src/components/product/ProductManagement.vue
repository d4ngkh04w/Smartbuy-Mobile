// filepath:
e:\Doan\pbl3-smartbuy-mobile\frontend\admin\src\components\product\ProductManagement.vue
<script setup>
import { ref, onMounted, computed, reactive } from "vue";
import {
    getProducts,
    getProductById,
    createProduct,
    updateProduct,
    activateProduct,
    deactivateProduct,
    createProductColor,
} from "@/services/productService";
import {
    getProductLinesByBrand,
    getProductLineById,
} from "@/services/productLineService";
import { getBrands } from "@/services/brandService";
import emitter from "../../utils/evenBus.js";

// Import component parts
import ProductManagementHeader from "./components/ProductManagementHeader.vue";
import ProductStatCards from "./components/ProductStatCards.vue";
import ProductTable from "./components/ProductTable.vue";
import ProductAddEditModal from "./components/ProductAddEditModal.vue";
import ProductStatusModal from "./components/ProductStatusModal.vue";
import ProductDetailModal from "./components/ProductDetailModal.vue";

// State
const products = ref([]);
const productLines = ref([]);
const brands = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showStatusModal = ref(false);
const isEditing = ref(false);
const searchQuery = ref("");
const submitLoading = ref(false);

// Note: newColor state moved to ColorManager component

// Product to toggle status
const productToToggle = ref(null);

// Form validation
const formErrors = reactive({
    name: "",
    brandId: "",
    productLineId: "",
    importPrice: "",
    salePrice: "",
    ram: "",
    storage: "",
    processor: "",
    os: "",
    screenSize: "",
    screenResolution: "",
    battery: "",
    warranty: "",
    images: "",
    colors: "",
});

// Product model
const formData = ref({
    name: "",
    brandId: null,
    productLineId: null,
    importPrice: "",
    salePrice: "",
    description: "",
    warranty: "12",
    ram: "",
    storage: "",
    screenSize: "",
    screenResolution: "",
    battery: "",
    processor: "",
    os: "",
    simSlots: 1,
    colorData: [], // Array of { name, quantity, images, mainImageIndex }
    colors: [],
});

// State for product detail modal
const showDetailModal = ref(false);
const selectedProduct = ref(null);

// Stats for cards
const totalProducts = computed(() => products.value.length);
const activeProducts = computed(
    () => products.value.filter((p) => p.isActive).length
);
const totalValue = computed(() => {
    return products.value.reduce((sum, product) => {
        return sum + product.stock * product.salePrice;
    }, 0);
});

// Note: formatCurrency function moved to ProductTable component

// Note: formatImageUrl function moved to ProductTable component

// Note: filteredProducts computed property moved to ProductTable component

// Methods
const fetchProducts = async () => {
    loading.value = true;
    try {
        const response = await getProducts();
        if (response.data && response.data.data) {
            products.value = response.data.data;
        }
    } catch (error) {
        console.error("Error fetching products:", error);
    } finally {
        loading.value = false;
    }
};

const fetchProductLines = async (brandId) => {
    try {
        if (!brandId) {
            productLines.value = [];
            return;
        }

        const response = await getProductLinesByBrand(brandId, {
            isActive: true,
        });
        productLines.value = Array.isArray(response?.data?.data)
            ? [...response.data.data]
            : [];
    } catch (error) {
        console.error("Error fetching product lines:", error);
        productLines.value = [];
    }
};

const fetchBrands = async () => {
    try {
        const response = await getBrands({
            includeProductLines: true,
            isActive: true,
        });

        brands.value = Array.isArray(response?.data?.data)
            ? response.data.data
            : [];
    } catch (error) {
        console.error("Error fetching brands:", error);
        brands.value = [];
    }
};

// Filter product lines based on selected brand
const filteredProductLines = computed(() => {
    if (!formData.value.brandId) return [];
    return productLines.value;
});

// Handle brand change - Cập nhật để tải product lines mới khi thay đổi brand
const handleBrandChange = async () => {
    // Reset product line selection
    formData.value.productLineId = null;
    productLines.value = [];

    // Fetch product lines for the selected brand
    if (formData.value.brandId) {
        await fetchProductLines(formData.value.brandId);

        // Tự động chọn product line nếu chỉ có một
        if (productLines.value.length === 1) {
            formData.value.productLineId = productLines.value[0].id;
        }
    }
};

// Open modal to add new product
const openAddProductModal = async () => {
    loading.value = true;
    try {
        // Chỉ fetch brands, không fetch product lines ngay từ đầu
        await fetchBrands();

        isEditing.value = false;
        formData.value = {
            name: "",
            brandId: null,
            productLineId: null,
            importPrice: "",
            salePrice: "",
            description: "",
            warranty: "12",
            ram: "",
            storage: "",
            screenSize: "",
            screenResolution: "",
            battery: "",
            processor: "",
            os: "",
            simSlots: 1,
            isActive: true,
            colorData: [],
            colors: [],
        };

        // Form errors and new color are now handled in their respective components

        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
    } finally {
        loading.value = false;
    }
};

// Note: resetFormErrors function moved to ProductAddEditModal component

// Edit product
const editProduct = async (product) => {
    loading.value = true;
    try {
        // Fetch all brands first with their product lines included
        await fetchBrands();

        // Form errors are now handled in the ProductAddEditModal component

        // Get full product details if needed
        let fullProduct = product;
        if (!product.detail || !product.colors) {
            try {
                const response = await getProductById(product.id);
                if (response.data && response.data.data) {
                    fullProduct = response.data.data;
                }
            } catch (error) {
                console.error("Error fetching product details:", error);
            }
        }

        isEditing.value = true;

        // First set all product properties except for brandId
        formData.value = {
            id: fullProduct.id,
            name: fullProduct.name,
            brandId: null, // We'll set this after determining the brand
            productLineId: fullProduct.productLineId,
            importPrice: fullProduct.importPrice,
            salePrice: fullProduct.salePrice,
            description: fullProduct.description || "",
            isActive: fullProduct.isActive,
            warranty: fullProduct.detail?.warranty || "12",
            ram: fullProduct.detail?.ram || "",
            storage: fullProduct.detail?.storage || "",
            screenSize: fullProduct.detail?.screenSize || "",
            screenResolution: fullProduct.detail?.screenResolution || "",
            battery: fullProduct.detail?.battery || "",
            processor: fullProduct.detail?.processor || "",
            os: fullProduct.detail?.operatingSystem || "",
            simSlots: fullProduct.detail?.simSlots || 1,

            // Initialize color data for editing
            colorData: [],
            colors: [],

            // Save reference to existing colors
            existingColors: fullProduct.colors || [],
        };

        // Process colors and their images
        if (fullProduct.colors && fullProduct.colors.length > 0) {
            fullProduct.colors.forEach((color) => {
                // Add to simple colors array for backward compatibility
                formData.value.colors.push(color.name);

                // Create color data structure with images
                const colorData = {
                    id: color.id,
                    name: color.name,
                    quantity: color.quantity || 0, // Add quantity for color
                    existingImages: color.images || [],
                    removedImageIds: [],
                    newImages: [],
                    newImagePreviews: [],
                    mainImageId: null,
                };

                // Find main image if exists
                const mainImage = color.images?.find((img) => img.isMain);
                if (mainImage) {
                    colorData.mainImageId = mainImage.id;
                }

                formData.value.colorData.push(colorData);
            });
        }

        // Get the product line details to find the brand
        if (fullProduct.productLineId) {
            try {
                const response = await getProductLineById(
                    fullProduct.productLineId
                );

                if (response.data && response.data.data) {
                    const productLine = response.data.data;
                    const brandName = productLine.brandName;

                    // Find the matching brand ID from the fetched brands list
                    const matchingBrand = brands.value.find(
                        (brand) => brand.name === brandName
                    );

                    if (matchingBrand) {
                        formData.value.brandId = matchingBrand.id;

                        // Fetch all product lines for this brand
                        await fetchProductLines(matchingBrand.id);
                    } else {
                        console.error(
                            "Could not find matching brand for name:",
                            brandName
                        );
                    }
                }
            } catch (error) {
                console.error("Error fetching product line details:", error);
            }
        }

        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
    } finally {
        loading.value = false;
    }
};

// View product detail
const viewProductDetail = async (product) => {
    try {
        // Nếu cần lấy thêm thông tin chi tiết của sản phẩm từ API
        const response = await getProductById(product.id);
        if (response.data && response.data.data) {
            selectedProduct.value = response.data.data;
        } else {
            selectedProduct.value = product;
        }
        const productLine = await getProductLineById(
            selectedProduct.value.productLineId
        );
        selectedProduct.value.brandName = productLine.data.data.brandName;

        showDetailModal.value = true;
    } catch (error) {
        console.error("Error fetching product details:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải chi tiết sản phẩm",
        });
    }
};

// Close product detail modal
const closeDetailModal = () => {
    showDetailModal.value = false;
    selectedProduct.value = null;
};

// Note: handleColorImageUpload function moved to ColorManager component

// Note: setColorMainImage function moved to ColorManager component

// Note: removeColorImage function moved to ColorManager component

// Note: addColorWithImages function moved to ColorManager component

// Note: removeColorData and resetNewColor functions moved to ColorManager component

// Note: handleColorImageUploadForExisting function moved to ColorManager component

// Note: setExistingColorMainImage function moved to ColorManager component

// Note: removeExistingColorImage function moved to ColorManager component

// Note: addNewColorDuringEdit function moved to ColorManager component
// Note: validateForm function moved to ProductAddEditModal component

// Submit form
const submitForm = async () => {
    // Validation is now handled in the ProductAddEditModal component

    submitLoading.value = true;

    try {
        const data = new FormData();

        // Add basic fields
        data.append("Name", formData.value.name);
        data.append("ImportPrice", formData.value.importPrice.toString());
        data.append("SalePrice", formData.value.salePrice.toString());

        // Calculate total quantity from all colors for backward compatibility
        const totalQuantity = formData.value.colorData.reduce(
            (sum, color) => sum + (parseInt(color.quantity) || 0),
            0
        );
        data.append("Quantity", totalQuantity.toString());

        if (formData.value.description) {
            data.append("Description", formData.value.description);
        }

        // For new products, add productLineId
        if (!isEditing.value) {
            data.append(
                "ProductLineId",
                formData.value.productLineId.toString()
            );
        } else {
            // For editing, include ProductLineId and IsActive
            if (formData.value.productLineId) {
                data.append(
                    "ProductLineId",
                    formData.value.productLineId.toString()
                );
            }
            data.append("IsActive", formData.value.isActive.toString());
        }

        // Add technical specifications
        if (formData.value.warranty) {
            data.append("Warranty", formData.value.warranty);
        }

        if (formData.value.ram) {
            data.append("RAM", formData.value.ram);
        }

        if (formData.value.storage) {
            data.append("Storage", formData.value.storage);
        }

        if (formData.value.screenSize) {
            data.append("ScreenSize", formData.value.screenSize);
        }

        if (formData.value.screenResolution) {
            data.append("ScreenResolution", formData.value.screenResolution);
        }

        if (formData.value.battery) {
            data.append("Battery", formData.value.battery);
        }

        if (formData.value.processor) {
            data.append("Processor", formData.value.processor);
        }

        if (formData.value.os) {
            data.append("OS", formData.value.os);
        }

        if (formData.value.simSlots) {
            data.append("SimSlots", formData.value.simSlots.toString());
        }

        if (!isEditing.value) {
            // We're creating a new product

            // Process colors and their images
            if (
                formData.value.colorData &&
                formData.value.colorData.length > 0
            ) {
                // The backend doesn't support ColorData directly in CreateProductDTO anymore
                // Instead, we need to create the product first, then add colors separately
                const response = await createProduct(data);

                if (response.data && response.data.data) {
                    const productId = response.data.data.id;

                    // Add colors one by one
                    for (const colorData of formData.value.colorData) {
                        const colorFormData = new FormData();
                        colorFormData.append("Name", colorData.name);
                        colorFormData.append(
                            "MainImageIndex",
                            (colorData.mainImageIndex ?? 0).toString()
                        );
                        colorFormData.append(
                            "Quantity",
                            colorData.quantity.toString()
                        );

                        // Add color images
                        for (let i = 0; i < colorData.images.length; i++) {
                            colorFormData.append("Images", colorData.images[i]);
                        }
                        const response = await createProductColor(
                            productId,
                            colorFormData
                        );
                    }
                }
            } else {
                // If no colors, just create the product
                await createProduct(data);
            }
        } else {
            // We're updating an existing product

            // Handle existing colors updates
            if (
                formData.value.colorData &&
                formData.value.colorData.length > 0
            ) {
                // Process removed colors
                const existingColorIds =
                    formData.value.existingColors?.map((c) => c.id) || [];
                const remainingColorIds = formData.value.colorData
                    .filter((c) => c.id) // Only consider existing colors
                    .map((c) => c.id);

                // Find colors to remove
                const removeColorIds = existingColorIds.filter(
                    (id) => !remainingColorIds.includes(id)
                );
                removeColorIds.forEach((id) => {
                    data.append("RemoveColorIds", id.toString());
                });

                // Process color updates
                formData.value.colorData.forEach((colorData, index) => {
                    if (colorData.id) {
                        // This is an existing color
                        data.append(
                            `UpdateColorData[${index}].Id`,
                            colorData.id.toString()
                        );
                        data.append(
                            `UpdateColorData[${index}].Name`,
                            colorData.name
                        );
                        data.append(
                            `UpdateColorData[${index}].Quantity`,
                            colorData.quantity.toString()
                        );

                        // Handle removed images
                        if (
                            colorData.removedImageIds &&
                            colorData.removedImageIds.length > 0
                        ) {
                            colorData.removedImageIds.forEach((imageId) => {
                                data.append(
                                    `UpdateColorData[${index}].RemoveImageIds`,
                                    imageId.toString()
                                );
                            });
                        }

                        // Add new images for this existing color
                        if (
                            colorData.newImages &&
                            colorData.newImages.length > 0
                        ) {
                            colorData.newImages.forEach((image, imgIndex) => {
                                data.append(
                                    `UpdateColorData[${index}].AddImages`,
                                    image
                                );
                            });

                            if (colorData.mainImageIndex !== undefined) {
                                data.append(
                                    `UpdateColorData[${index}].MainImageIndex`,
                                    colorData.mainImageIndex.toString()
                                );
                                data.append(
                                    `UpdateColorData[${index}].SetMainImage`,
                                    "true"
                                );
                            }
                        }

                        // Set main image from existing images
                        if (colorData.mainImageId) {
                            data.append(
                                `UpdateColorData[${index}].MainImageId`,
                                colorData.mainImageId.toString()
                            );
                            data.append(
                                `UpdateColorData[${index}].SetMainImage`,
                                "true"
                            );
                        }
                    } else {
                        // This is a new color being added during edit
                        data.append(
                            `UpdateColorData[${index}].Name`,
                            colorData.name
                        );
                        data.append(
                            `UpdateColorData[${index}].Quantity`,
                            colorData.quantity.toString()
                        );

                        // Add images for this new color
                        if (colorData.images && colorData.images.length > 0) {
                            colorData.images.forEach((image, imgIndex) => {
                                data.append(
                                    `UpdateColorData[${index}].AddImages`,
                                    image
                                );
                            });
                        }

                        // Set main image for new color
                        data.append(
                            `UpdateColorData[${index}].MainImageIndex`,
                            colorData.mainImageIndex.toString()
                        );
                        data.append(
                            `UpdateColorData[${index}].SetMainImage`,
                            "true"
                        );
                    }
                });

                await updateProduct(formData.value.id, data);
            } else {
                // If no colors to update, just update the product details
                await updateProduct(formData.value.id, data);
            }
        }

        // Refresh the products list and close the modal
        await fetchProducts();
        closeModal();
    } catch (error) {
        console.error("Error submitting form:", error);
        alert("Có lỗi xảy ra khi lưu sản phẩm. Vui lòng thử lại sau.");
    } finally {
        submitLoading.value = false;
    }
};

// Close modal
const closeModal = () => {
    showModal.value = false;
    // Form errors are now handled in the ProductAddEditModal component
};

// Toggle product status modal
const toggleStatusModal = (product) => {
    productToToggle.value = product;
    showStatusModal.value = true;
};

// Cancel status toggle
const cancelStatusToggle = () => {
    showStatusModal.value = false;
    productToToggle.value = null;
};

// Confirm status toggle and call the API
const confirmStatusToggle = async () => {
    if (!productToToggle.value) return;

    loading.value = true;
    try {
        if (productToToggle.value.isActive) {
            // If currently active, deactivate it
            await deactivateProduct(productToToggle.value.id);
            emitter.emit("show-notification", {
                status: "success",
                message: "Ngừng bán sản phẩm thành công!",
            });
        } else {
            // If currently inactive, activate it
            await activateProduct(productToToggle.value.id);
            emitter.emit("show-notification", {
                status: "success",
                message: "Kích hoạt bán lại sản phẩm thành công!",
            });
        }

        // Refresh products list
        await fetchProducts();
        showStatusModal.value = false;
        productToToggle.value = null;
    } catch (error) {
        console.error("Error toggling product status:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra khi cập nhật trạng thái sản phẩm.",
        });
    } finally {
        loading.value = false;
    }
};

// Note: getProductMainImage function moved to ProductTable component

// Load data when component mounts
onMounted(async () => {
    await fetchProducts();
    await fetchProductLines();
    await fetchBrands();
});
</script>

<template>
    <div class="product-management">
        <!-- Header Section -->
        <ProductManagementHeader
            v-model:searchQuery="searchQuery"
            @add-product="openAddProductModal"
        />

        <!-- Status Cards -->
        <ProductStatCards
            :totalProducts="totalProducts"
            :activeProducts="activeProducts"
            :totalValue="totalValue"
        />

        <!-- Products table -->
        <ProductTable
            :products="products"
            :loading="loading"
            :searchQuery="searchQuery"
            @edit-product="editProduct"
            @view-product-detail="viewProductDetail"
            @toggle-status="toggleStatusModal"
            @add-product="openAddProductModal"
        />

        <!-- Add/Edit Product Modal -->
        <ProductAddEditModal
            :showModal="showModal"
            v-model:formData="formData"
            v-model:formErrors="formErrors"
            :isEditing="isEditing"
            :brands="brands"
            :productLines="productLines"
            :submitLoading="submitLoading"
            @close="closeModal"
            @submit="submitForm"
            @brand-change="handleBrandChange"
        />

        <!-- Status Toggle Confirmation Modal -->
        <ProductStatusModal
            :showModal="showStatusModal"
            :product="productToToggle"
            :loading="loading"
            @confirm="confirmStatusToggle"
            @cancel="cancelStatusToggle"
        />

        <!-- Product Detail Modal -->
        <ProductDetailModal
            :showModal="showDetailModal"
            :product="selectedProduct"
            @close="closeDetailModal"
        />
    </div>
</template>

<style scoped>
.product-management {
    padding: 2rem;
    width: 100%;
}

/* Note: All component-specific styles have been moved to their respective component files */
</style>
