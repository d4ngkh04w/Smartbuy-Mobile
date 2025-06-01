<script>
import { ref } from "vue";
import productService from "@/services/productService";
import productLineService from "@/services/productLineService";
import { emitter } from "@/utils/emitter";

export default {
    setup() {
        const productToEdit = ref(null);
        const showEditModal = ref(false);
        const brands = ref([]);
        const selectedProduct = ref(null);
        const showDetailModal = ref(false);

        const fetchProductLinesForBrand = async (brandId) => {
            // Implementation for fetching product lines for a specific brand
        };

        // Open product edit modal
        const editProduct = async (product) => {
            try {
                // Fetch detailed product information
                const response = await productService.getProductById(
                    product.id
                );
                if (response.data && response.data.data) {
                    productToEdit.value = response.data.data;
                } else {
                    productToEdit.value = product;
                }

                // Find the brand ID for this product line
                const productLineId = productToEdit.value.productLineId;
                let brandId = null;

                // Look through all brands to find which one contains this product line
                for (const brand of brands.value) {
                    if (
                        brand.productLines &&
                        brand.productLines.some((pl) => pl.id === productLineId)
                    ) {
                        brandId = brand.id;
                        break;
                    }
                }

                // If we found a brand, fetch its product lines
                if (brandId) {
                    await fetchProductLinesForBrand(brandId);
                } else {
                    // If brand is not found in the cached brands data, we need to fetch it from the API
                    try {
                        const productLineResponse =
                            await productLineService.getProductLineById(
                                productLineId
                            );
                        if (productLineResponse.data) {
                            const brandIdFromApi =
                                productLineResponse.data.brandId;
                            if (brandIdFromApi) {
                                await fetchProductLinesForBrand(brandIdFromApi);
                            }
                        }
                    } catch (error) {
                        console.error(
                            "Error fetching product line details:",
                            error
                        );
                    }
                }

                // Show the edit modal
                showEditModal.value = true;
            } catch (error) {
                console.error(
                    "Error fetching product details for editing:",
                    error
                );
                emitter.emit("show-notification", {
                    status: "error",
                    message: "Không thể tải thông tin sản phẩm để chỉnh sửa",
                });
            }
        };

        // View product detail
        const viewProductDetail = async (product) => {
            try {
                // Fetch detailed information if needed
                const response = await productService.getProductById(
                    product.id
                );
                if (response.data && response.data.data) {
                    selectedProduct.value = response.data.data;
                } else {
                    selectedProduct.value = product;
                }

                // Get brand name from product line
                try {
                    const productLine =
                        await productLineService.getProductLineById(
                            selectedProduct.value.productLineId
                        );
                    if (productLine.data) {
                        selectedProduct.value.brandName =
                            productLine.data.brandName;

                        // If brandName is not provided, try to find it from our brands list
                        if (!selectedProduct.value.brandName) {
                            const brandId = productLine.data.brandId;
                            const brand = brands.value.find(
                                (b) => b.id === brandId
                            );
                            if (brand) {
                                selectedProduct.value.brandName = brand.name;
                            }
                        }
                    }
                } catch (error) {
                    console.error(
                        "Error fetching product line details:",
                        error
                    );
                }

                showDetailModal.value = true;
            } catch (error) {
                console.error("Error fetching product details:", error);
                emitter.emit("show-notification", {
                    status: "error",
                    message: "Không thể tải chi tiết sản phẩm",
                });
            }
        };

        return {
            editProduct,
            viewProductDetail,
            productToEdit,
            showEditModal,
            brands,
            selectedProduct,
            showDetailModal,
        };
    },
};
</script>
