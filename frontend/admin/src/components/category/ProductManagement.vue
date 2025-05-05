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
const fileInput = ref(null);

// State for new color input
const newColor = ref({
    name: "",
    images: [], // Actual file objects for upload
    imagePreviews: [], // URLs for previewing images
    mainImageIndex: 0, // Index of the main image for this color
});

// Product to toggle status
const productToToggle = ref(null);

// Form validation
const formErrors = reactive({
    name: "",
    brandId: "",
    productLineId: "",
    quantity: "",
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
    quantity: 1,
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
    colorData: [], // Array of { name, images, mainImageIndex }
    colors: [],
});

// Product to delete reference
const productToDelete = ref(null);

// Format currency function
const formatCurrency = (value) => {
    if (!value) return "0 ₫";
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(value);
};

// Format image URL
const formatImageUrl = (imagePath) => {
    if (!imagePath) return null;

    // If image path is already a full URL or base64 data
    if (imagePath.startsWith("http") || imagePath.startsWith("data:"))
        return imagePath;

    // Get base URL from API config
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Normalize the path (convert \ to /)
    const normalizedPath = imagePath.replace(/\\/g, "/");

    // Check if path starts with /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Computed properties
const filteredProducts = computed(() => {
    if (!searchQuery.value) return products.value;

    let result = products.value;

    // Apply search query
    if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase().trim();
        result = result.filter(
            (product) =>
                product.name.toLowerCase().includes(query) ||
                (product.description &&
                    product.description.toLowerCase().includes(query)) ||
                (product.productLineName &&
                    product.productLineName.toLowerCase().includes(query))
        );
    }

    return result;
});

// Stats for cards
const totalProducts = computed(() => products.value.length);
const activeProducts = computed(
    () => products.value.filter((p) => p.isActive).length
);
const totalValue = computed(() => {
    return products.value.reduce((sum, product) => {
        return sum + product.quantity * product.salePrice;
    }, 0);
});

// Methods
const fetchProducts = async () => {
    loading.value = true;
    try {
        const response = await getProducts();
        if (response.data && response.data.products) {
            products.value = response.data.products;
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
            // Không có brandId thì không làm gì cả, hoặc set mảng rỗng
            productLines.value = [];
            return;
        }

        // Nếu có brandId, gọi API lấy product line theo brand
        // Thêm tham số isActive=true để chỉ lấy các product line đang kích hoạt
        const response = await getProductLinesByBrand(brandId, {
            isActive: true,
        });

        if (response.data && response.data.productLines) {
            productLines.value = [...response.data.productLines];
        } else if (Array.isArray(response.data)) {
            // Trong một số trường hợp API trả về mảng trực tiếp
            productLines.value = [...response.data];
        } else {
            // Nếu không có dữ liệu hợp lệ, gán mảng rỗng
            productLines.value = [];
        }
    } catch (error) {
        console.error("Error fetching product lines:", error);
        productLines.value = [];
    }
};

const fetchBrands = async () => {
    try {
        const response = await getBrands({
            includeProductLines: true,
            isActive: true, // Only fetch active brands
        });
        if (response.data && response.data.brands) {
            brands.value = response.data.brands;
        } else if (Array.isArray(response.data)) {
            brands.value = response.data;
        }
    } catch (error) {
        console.error("Error fetching brands:", error);
        brands.value = [];
    }
};

const getBrandIdForProductLine = (productLineId) => {
    const productLine = productLines.value.find(
        (pl) => pl.id === productLineId
    );
    return productLine ? productLine.brandId : null;
};

// Filter product lines based on selected brand
const filteredProductLines = computed(() => {
    if (!formData.value.brandId) return [];

    // Khi sử dụng API getProductLinesByBrand, danh sách đã được lọc theo brand
    // nên chỉ cần trả về productLines hiện tại
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
            quantity: 1,
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

        // Reset any validation errors
        resetFormErrors();

        // Reset new color form
        resetNewColor();

        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
    } finally {
        loading.value = false;
    }
};

// Reset form errors
const resetFormErrors = () => {
    Object.keys(formErrors).forEach((key) => {
        formErrors[key] = "";
    });
};

// Edit product
const editProduct = async (product) => {
    loading.value = true;
    try {
        // Fetch all brands first with their product lines included
        await fetchBrands();

        // Reset any validation errors
        resetFormErrors();

        // Get full product details if needed
        let fullProduct = product;
        if (!product.detail || !product.colors) {
            try {
                const response = await getProductById(product.id);
                if (response.data && response.data.product) {
                    fullProduct = response.data.product;
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
            quantity: fullProduct.quantity,
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
                console.log("Product Line API response:", response);

                if (response.data && response.data.productLine) {
                    const productLine = response.data.productLine;
                    const brandName = productLine.brandName;

                    console.log(
                        "Found brand name from product line:",
                        brandName
                    );

                    // Find the matching brand ID from the fetched brands list
                    const matchingBrand = brands.value.find(
                        (brand) => brand.name === brandName
                    );

                    if (matchingBrand) {
                        formData.value.brandId = matchingBrand.id;
                        console.log(
                            "Found matching brand ID:",
                            matchingBrand.id
                        );

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

// Handle color image upload
const handleColorImageUpload = (event) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    // Clear any previous errors
    formErrors.colors = "";

    // Process each file
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Validate file type
        if (!["image/jpeg", "image/png", "image/jpg"].includes(file.type)) {
            formErrors.colors = "Chỉ chấp nhận các định dạng: jpg, jpeg, png";
            continue;
        }

        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            formErrors.colors = "Kích thước tệp tối đa là 5MB";
            continue;
        }

        // Add file to the list
        newColor.value.images.push(file);

        // Create preview
        const reader = new FileReader();
        reader.onload = (e) => {
            newColor.value.imagePreviews.push(e.target.result);
        };
        reader.readAsDataURL(file);
    }

    // Clear the file input for future uploads
    if (event.target) {
        event.target.value = "";
    }
};

// Set main image for a color
const setColorMainImage = (index, colorIndex) => {
    if (colorIndex !== undefined) {
        // Setting main image for an existing color
        formData.value.colorData[colorIndex].mainImageIndex = index;
    } else {
        // Setting main image for a new color
        newColor.value.mainImageIndex = index;
    }
};

// Remove an image from a color
const removeColorImage = (index) => {
    // If removing the main image, select a new main image
    if (index === newColor.value.mainImageIndex) {
        if (newColor.value.imagePreviews.length > 1) {
            const newMainIndex = index === 0 ? 1 : 0;
            newColor.value.mainImageIndex = newMainIndex;
        } else {
            newColor.value.mainImageIndex = 0;
        }
    } else if (index < newColor.value.mainImageIndex) {
        // If removing an image before the main image, adjust the main image index
        newColor.value.mainImageIndex--;
    }

    // Remove from arrays
    newColor.value.images.splice(index, 1);
    newColor.value.imagePreviews.splice(index, 1);
};

// Add a color with its images to the product
const addColorWithImages = () => {
    // Validate color name
    if (!newColor.value.name.trim()) {
        formErrors.colors = "Vui lòng nhập tên màu";
        return;
    }

    // Validate images
    if (newColor.value.images.length === 0) {
        formErrors.colors = "Vui lòng thêm ít nhất một hình ảnh cho màu này";
        return;
    }

    // Check if the color already exists
    const colorExists = formData.value.colorData.some(
        (color) =>
            color.name.toLowerCase() === newColor.value.name.toLowerCase()
    );

    if (colorExists) {
        formErrors.colors = "Màu này đã được thêm";
        return;
    }

    // Add the color data to the form
    formData.value.colorData.push({
        name: newColor.value.name,
        images: [...newColor.value.images],
        imagePreviews: [...newColor.value.imagePreviews],
        mainImageIndex: newColor.value.mainImageIndex,
    });

    // For backward compatibility, also add to simple colors array
    formData.value.colors.push(newColor.value.name);

    // Reset new color form
    resetNewColor();
};

// Remove a color from the product
const removeColorData = (index) => {
    // Also remove from the simple colors array for backward compatibility
    const colorName = formData.value.colorData[index].name;
    const simpleColorIndex = formData.value.colors.findIndex(
        (c) => c === colorName
    );
    if (simpleColorIndex !== -1) {
        formData.value.colors.splice(simpleColorIndex, 1);
    }

    // Remove the color data
    formData.value.colorData.splice(index, 1);
};

// Reset the new color form
const resetNewColor = () => {
    newColor.value = {
        name: "",
        images: [],
        imagePreviews: [],
        mainImageIndex: 0,
    };
    formErrors.colors = "";
};

// Handle color-specific image operations for existing colors during editing
const handleColorImageUploadForExisting = (event, colorIndex) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    const colorData = formData.value.colorData[colorIndex];

    // Process each file
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Validate file type
        if (!["image/jpeg", "image/png", "image/jpg"].includes(file.type)) {
            formErrors.colors = "Chỉ chấp nhận các định dạng: jpg, jpeg, png";
            continue;
        }

        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            formErrors.colors = "Kích thước tệp tối đa là 5MB";
            continue;
        }

        // Initialize arrays if they don't exist
        if (!colorData.newImages) colorData.newImages = [];
        if (!colorData.newImagePreviews) colorData.newImagePreviews = [];

        // Add file to the list
        colorData.newImages.push(file);

        // Create preview
        const reader = new FileReader();
        reader.onload = (e) => {
            colorData.newImagePreviews.push(e.target.result);
        };
        reader.readAsDataURL(file);
    }

    // Clear the file input for future uploads
    if (event.target) {
        event.target.value = "";
    }
};

// Set main image for existing color (either from existing images or new ones)
const setExistingColorMainImage = (
    colorIndex,
    imageIndex,
    isNewImage = false
) => {
    const colorData = formData.value.colorData[colorIndex];

    // Reset the main image ID since we're setting a new one
    colorData.mainImageId = null;

    if (isNewImage) {
        // Set main image from newly uploaded images
        colorData.mainImageIndex = imageIndex;

        // Reset any selected main image from existing images
        for (let i = 0; i < colorData.existingImages?.length || 0; i++) {
            if (colorData.existingImages[i]) {
                colorData.existingImages[i].isMain = false;
            }
        }
    } else {
        // Set main image from existing images
        const image = colorData.existingImages[imageIndex];
        if (image) {
            colorData.mainImageId = image.id;

            // Update isMain flag on all existing images for UI update
            for (let i = 0; i < colorData.existingImages.length; i++) {
                colorData.existingImages[i].isMain = i === imageIndex;
            }

            // Reset any main image from new images
            colorData.mainImageIndex = undefined;
        }
    }
};

// Remove image from existing color
const removeExistingColorImage = (
    colorIndex,
    imageIndex,
    isNewImage = false
) => {
    const colorData = formData.value.colorData[colorIndex];

    if (isNewImage) {
        // Remove from new images
        colorData.newImages.splice(imageIndex, 1);
        colorData.newImagePreviews.splice(imageIndex, 1);

        // If we removed the main image from new images, reset main image index
        if (
            colorData.mainImageIndex !== undefined &&
            imageIndex === colorData.mainImageIndex
        ) {
            colorData.mainImageIndex =
                colorData.newImages.length > 0 ? 0 : undefined;
        } else if (
            colorData.mainImageIndex !== undefined &&
            imageIndex < colorData.mainImageIndex
        ) {
            // If removing an image before the main image, adjust the index
            colorData.mainImageIndex--;
        }
    } else {
        // Mark existing image for removal
        const image = colorData.existingImages[imageIndex];
        if (image) {
            // Initialize removedImageIds if it doesn't exist
            if (!colorData.removedImageIds) colorData.removedImageIds = [];
            colorData.removedImageIds.push(image.id);

            // If we removed the main image, unset mainImageId
            if (colorData.mainImageId === image.id) {
                colorData.mainImageId = null;

                // Try to set another existing image as main
                const remainingImages = colorData.existingImages.filter(
                    (img) => !colorData.removedImageIds.includes(img.id)
                );

                if (remainingImages.length > 0) {
                    colorData.mainImageId = remainingImages[0].id;
                } else if (
                    colorData.newImages &&
                    colorData.newImages.length > 0
                ) {
                    // If no existing images left, set first new image as main
                    colorData.mainImageIndex = 0;
                }
            }
        }
    }
};

// Add new color during edit
const addNewColorDuringEdit = () => {
    // Validate color name
    if (!newColor.value.name.trim()) {
        formErrors.colors = "Vui lòng nhập tên màu";
        return;
    }

    // Validate images
    if (newColor.value.images.length === 0) {
        formErrors.colors = "Vui lòng thêm ít nhất một hình ảnh cho màu này";
        return;
    }

    // Check if the color already exists
    const colorExists = formData.value.colorData.some(
        (color) =>
            color.name.toLowerCase() === newColor.value.name.toLowerCase()
    );

    if (colorExists) {
        formErrors.colors = "Màu này đã được thêm";
        return;
    }

    // Add the color data to the form
    formData.value.colorData.push({
        name: newColor.value.name,
        images: [...newColor.value.images],
        imagePreviews: [...newColor.value.imagePreviews],
        mainImageIndex: newColor.value.mainImageIndex,
        // For new colors during edit, we need to track that this is a new color
        isNewColor: true,
    });

    // Reset new color form
    resetNewColor();
};

// Validate form
const validateForm = () => {
    let isValid = true;
    resetFormErrors();

    // Validate required fields
    if (!formData.value.name.trim()) {
        formErrors.name = "Vui lòng nhập tên sản phẩm";
        isValid = false;
    }

    if (!formData.value.brandId) {
        formErrors.brandId = "Vui lòng chọn thương hiệu";
        isValid = false;
    }

    if (!formData.value.productLineId) {
        formErrors.productLineId = "Vui lòng chọn dòng sản phẩm";
        isValid = false;
    }

    if (!formData.value.quantity || formData.value.quantity < 1) {
        formErrors.quantity = "Số lượng phải lớn hơn 0";
        isValid = false;
    }

    if (!formData.value.importPrice) {
        formErrors.importPrice = "Vui lòng nhập giá nhập";
        isValid = false;
    }

    if (!formData.value.salePrice) {
        formErrors.salePrice = "Vui lòng nhập giá bán";
        isValid = false;
    }

    // Technical specifications, colors and images can be added later
    // Additional validations can be performed if values are provided

    return isValid;
};

// Submit form
const submitForm = async () => {
    // Validate form
    if (!validateForm()) {
        return;
    }

    submitLoading.value = true;

    try {
        const data = new FormData();

        // Add basic fields
        data.append("Name", formData.value.name);
        data.append("Quantity", formData.value.quantity.toString());
        data.append("ImportPrice", formData.value.importPrice.toString());
        data.append("SalePrice", formData.value.salePrice.toString());

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

                if (response.data && response.data.product) {
                    const productId = response.data.product.id;

                    // Add colors one by one
                    for (const colorData of formData.value.colorData) {
                        const colorFormData = new FormData();
                        colorFormData.append("Name", colorData.name);
                        colorFormData.append(
                            "MainImageIndex",
                            (colorData.mainImageIndex ?? 0).toString()
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
    resetFormErrors();
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

// Get product image URL
const getProductMainImage = (product) => {
    if (product.colors && product.colors.length > 0) {
        for (const color of product.colors) {
            if (color.images && color.images.length > 0) {
                const mainImage = color.images.find((img) => img.isMain);
                // Dù không có main image, vẫn lấy ảnh đầu tiên
                const chosenImage = mainImage || color.images[0];
                if (chosenImage) {
                    return formatImageUrl(chosenImage.imagePath);
                }
            }
        }
    }

    return null;
};

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
        <div class="section-header">
            <div class="left-section">
                <h2><i class="fas fa-mobile-alt"></i> Quản lý Sản phẩm</h2>
                <p>Quản lý sản phẩm điện thoại di động</p>
            </div>

            <div class="right-section">
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm sản phẩm..."
                    />
                </div>

                <button @click="openAddProductModal" class="add-button">
                    <i class="fas fa-plus"></i> Thêm sản phẩm
                </button>
            </div>
        </div>

        <!-- Status Cards -->
        <div class="status-cards">
            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-mobile-alt"></i>
                </div>
                <div class="status-content">
                    <h3>{{ totalProducts }}</h3>
                    <p>Tổng số sản phẩm</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-content">
                    <h3>{{ activeProducts }}</h3>
                    <p>Sản phẩm đang bán</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-money-bill-wave"></i>
                </div>
                <div class="status-content">
                    <h3>{{ formatCurrency(totalValue) }}</h3>
                    <p>Tổng giá trị tồn kho</p>
                </div>
            </div>
        </div>

        <!-- Products table -->
        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
            <div v-else-if="filteredProducts.length === 0" class="empty-state">
                <i class="fas fa-box-open"></i>
                <p v-if="searchQuery">Không tìm thấy sản phẩm phù hợp</p>
                <p v-else>Không có sản phẩm nào</p>
                <button @click="openAddProductModal" class="action-button">
                    <i class="fas fa-plus"></i> Thêm sản phẩm mới
                </button>
            </div>
            <table v-else class="data-table">
                <thead>
                    <tr>
                        <th>Sản phẩm</th>
                        <th>Hình ảnh</th>
                        <th>Giá nhập</th>
                        <th>Giá bán</th>
                        <th>Số lượng</th>
                        <th>Dòng sản phẩm</th>
                        <th>Trạng thái</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="product in filteredProducts"
                        :key="product.id"
                        class="data-row"
                    >
                        <td class="name-cell">{{ product.name }}</td>
                        <td class="image-cell">
                            <div class="logo-wrapper">
                                <img
                                    v-if="getProductMainImage(product)"
                                    :src="getProductMainImage(product)"
                                    :alt="product.name"
                                />
                                <div v-else class="no-logo">
                                    <i class="fas fa-mobile-alt"></i>
                                </div>
                            </div>
                        </td>
                        <td>{{ formatCurrency(product.importPrice) }}</td>
                        <td>{{ formatCurrency(product.salePrice) }}</td>
                        <td class="quantity-cell">
                            <span
                                :class="{
                                    'low-stock': product.quantity < 10,
                                    'out-of-stock': product.quantity <= 0,
                                }"
                            >
                                {{ product.quantity }}
                            </span>
                        </td>
                        <td>{{ product.productLineName }}</td>
                        <td class="status-cell">
                            <span
                                :class="[
                                    'status-badge',
                                    product.isActive ? 'active' : 'inactive',
                                ]"
                            >
                                {{
                                    product.isActive ? "Đang bán" : "Ngừng bán"
                                }}
                            </span>
                        </td>
                        <td class="actions-cell">
                            <button
                                @click="editProduct(product)"
                                class="edit-button"
                                title="Chỉnh sửa"
                            >
                                <i class="fas fa-edit"></i>
                            </button>
                            <button
                                @click="toggleStatusModal(product)"
                                :class="[
                                    'status-button',
                                    product.isActive
                                        ? 'deactivate-button'
                                        : 'activate-button',
                                ]"
                                :title="
                                    product.isActive ? 'Ngừng bán' : 'Kích hoạt'
                                "
                            >
                                <i
                                    class="fas"
                                    :class="
                                        product.isActive
                                            ? 'fa-toggle-off'
                                            : 'fa-toggle-on'
                                    "
                                ></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Add/Edit Product Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container product-modal">
                <div class="modal-header">
                    <h3>
                        {{
                            isEditing
                                ? "Chỉnh sửa sản phẩm: " + formData.name
                                : "Thêm sản phẩm mới"
                        }}
                    </h3>
                    <button @click="closeModal" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-form-container">
                    <form @submit.prevent="submitForm" class="modal-form">
                        <!-- Category Selection Section -->
                        <div class="form-section">
                            <h4 class="section-title">Chọn danh mục</h4>

                            <div class="form-row">
                                <div class="form-group">
                                    <label for="brandSelect">
                                        Thương hiệu
                                        <span class="required">*</span>
                                    </label>
                                    <div class="select-wrapper">
                                        <select
                                            id="brandSelect"
                                            v-model="formData.brandId"
                                            @change="handleBrandChange"
                                        >
                                            <option value="" disabled selected>
                                                Chọn thương hiệu
                                            </option>
                                            <option
                                                v-for="brand in brands"
                                                :key="brand.id"
                                                :value="brand.id"
                                            >
                                                {{ brand.name }}
                                            </option>
                                        </select>
                                    </div>
                                    <span
                                        v-if="formErrors.brandId"
                                        class="error-message"
                                    >
                                        {{ formErrors.brandId }}
                                    </span>
                                </div>

                                <div class="form-group">
                                    <label for="productLineSelect">
                                        Dòng sản phẩm
                                        <span class="required">*</span>
                                    </label>
                                    <div class="select-wrapper">
                                        <select
                                            id="productLineSelect"
                                            v-model="formData.productLineId"
                                            :disabled="!formData.brandId"
                                        >
                                            <option value="" disabled selected>
                                                Chọn dòng sản phẩm
                                            </option>
                                            <option
                                                v-for="line in filteredProductLines"
                                                :key="line.id"
                                                :value="line.id"
                                            >
                                                {{ line.name }}
                                            </option>
                                        </select>
                                    </div>
                                    <span
                                        v-if="formErrors.productLineId"
                                        class="error-message"
                                    >
                                        {{ formErrors.productLineId }}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <!-- Basic Information -->
                        <div class="form-section">
                            <h4 class="section-title">Thông tin cơ bản</h4>

                            <!-- Tên sản phẩm - full width -->
                            <div class="form-group">
                                <label for="productName">
                                    Tên sản phẩm <span class="required">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="productName"
                                    v-model="formData.name"
                                    placeholder="Nhập tên sản phẩm"
                                />
                                <span
                                    v-if="formErrors.name"
                                    class="error-message"
                                >
                                    {{ formErrors.name }}
                                </span>
                            </div>

                            <!-- Nhóm thông tin giá và số lượng -->
                            <div class="form-row">
                                <div class="form-group">
                                    <label for="quantity">
                                        Số lượng <span class="required">*</span>
                                    </label>
                                    <input
                                        type="number"
                                        id="quantity"
                                        v-model="formData.quantity"
                                        min="0"
                                        step="1"
                                    />
                                    <span
                                        v-if="formErrors.quantity"
                                        class="error-message"
                                    >
                                        {{ formErrors.quantity }}
                                    </span>
                                </div>

                                <div class="form-group">
                                    <label for="importPrice">
                                        Giá nhập <span class="required">*</span>
                                    </label>
                                    <div class="currency-input">
                                        <input
                                            type="number"
                                            id="importPrice"
                                            v-model="formData.importPrice"
                                            min="0"
                                            step="1000"
                                        />
                                        <span class="currency">VND</span>
                                    </div>
                                    <span
                                        v-if="formErrors.importPrice"
                                        class="error-message"
                                    >
                                        {{ formErrors.importPrice }}
                                    </span>
                                </div>

                                <div class="form-group">
                                    <label for="salePrice">
                                        Giá bán <span class="required">*</span>
                                    </label>
                                    <div class="currency-input">
                                        <input
                                            type="number"
                                            id="salePrice"
                                            v-model="formData.salePrice"
                                            min="0"
                                            step="1000"
                                        />
                                        <span class="currency">VND</span>
                                    </div>
                                    <span
                                        v-if="formErrors.salePrice"
                                        class="error-message"
                                    >
                                        {{ formErrors.salePrice }}
                                    </span>
                                </div>
                            </div>

                            <!-- Nhóm trạng thái -->
                            <div class="form-row">
                                <div class="form-group" v-if="isEditing">
                                    <label>Trạng thái</label>
                                    <div class="toggle-switch-wrapper">
                                        <div class="toggle-switch-container">
                                            <label class="toggle-switch">
                                                <input
                                                    type="checkbox"
                                                    v-model="formData.isActive"
                                                />
                                                <span
                                                    class="toggle-slider"
                                                ></span>
                                            </label>
                                            <span class="toggle-status">
                                                {{
                                                    formData.isActive
                                                        ? "Đang bán"
                                                        : "Ngừng bán"
                                                }}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Mô tả sản phẩm - full width -->
                            <div class="form-group">
                                <label for="description">
                                    Mô tả sản phẩm
                                    <span class="optional-hint"
                                        >(không bắt buộc)</span
                                    >
                                </label>
                                <textarea
                                    id="description"
                                    v-model="formData.description"
                                    rows="4"
                                    placeholder="Nhập mô tả sản phẩm"
                                ></textarea>
                            </div>
                        </div>

                        <!-- Technical Specifications -->
                        <div class="form-section">
                            <h4 class="section-title">
                                Thông số kỹ thuật
                                <span class="optional-badge"
                                    >Không bắt buộc</span
                                >
                            </h4>

                            <div class="form-row">
                                <div class="form-group">
                                    <label for="warranty"
                                        >Bảo hành (tháng)</label
                                    >
                                    <input
                                        type="number"
                                        id="warranty"
                                        v-model="formData.warranty"
                                        min="0"
                                        max="60"
                                        placeholder="12"
                                    />
                                </div>

                                <div class="form-group">
                                    <label for="ram">RAM (GB)</label>
                                    <input
                                        type="number"
                                        id="ram"
                                        v-model="formData.ram"
                                        min="1"
                                        max="32"
                                        placeholder="4"
                                    />
                                </div>

                                <div class="form-group">
                                    <label for="storage"
                                        >Bộ nhớ trong (GB)</label
                                    >
                                    <input
                                        type="number"
                                        id="storage"
                                        v-model="formData.storage"
                                        min="8"
                                        max="2048"
                                        placeholder="64"
                                    />
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group">
                                    <label for="screenSize"
                                        >Kích thước màn hình (inch)</label
                                    >
                                    <input
                                        type="number"
                                        id="screenSize"
                                        v-model="formData.screenSize"
                                        min="3"
                                        max="15"
                                        step="0.1"
                                        placeholder="6.1"
                                    />
                                </div>

                                <div class="form-group">
                                    <label for="screenResolution"
                                        >Độ phân giải màn hình</label
                                    >
                                    <input
                                        type="text"
                                        id="screenResolution"
                                        v-model="formData.screenResolution"
                                        placeholder="1920x1080"
                                    />
                                    <span class="input-hint"
                                        >Định dạng: 1920x1080</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label for="battery"
                                        >Dung lượng pin (mAh)</label
                                    >
                                    <input
                                        type="number"
                                        id="battery"
                                        v-model="formData.battery"
                                        min="1000"
                                        max="10000"
                                        placeholder="4000"
                                    />
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group">
                                    <label for="os">Hệ điều hành</label>
                                    <input
                                        type="text"
                                        id="os"
                                        v-model="formData.os"
                                        placeholder="Android 13"
                                    />
                                </div>

                                <div class="form-group">
                                    <label for="processor">Chip xử lý</label>
                                    <input
                                        type="text"
                                        id="processor"
                                        v-model="formData.processor"
                                        placeholder="Snapdragon 8 Gen 2"
                                    />
                                </div>

                                <div class="form-group">
                                    <label for="simSlots">Số khe SIM</label>
                                    <div class="select-wrapper">
                                        <select
                                            id="simSlots"
                                            v-model="formData.simSlots"
                                        >
                                            <option value="1">1 SIM</option>
                                            <option value="2">2 SIM</option>
                                            <option value="3">3 SIM</option>
                                            <option value="4">4 SIM</option>
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Colors Section -->
                        <div class="form-section">
                            <h4 class="section-title">Màu sắc và Hình ảnh</h4>

                            <div class="color-section-layout">
                                <div class="color-add-form-simple">
                                    <h5>Thêm màu sắc mới</h5>

                                    <div class="color-name-input">
                                        <label for="colorNameInput"
                                            >Tên màu
                                            <span class="required"
                                                >*</span
                                            ></label
                                        >
                                        <input
                                            type="text"
                                            id="colorNameInput"
                                            v-model="newColor.name"
                                            placeholder="Ví dụ: Đen, Trắng, Xanh,..."
                                        />
                                    </div>

                                    <div class="color-upload-wrapper-simple">
                                        <label>
                                            Hình ảnh màu sắc
                                            <span class="required">*</span>
                                        </label>
                                        <div
                                            class="color-upload-area-simple"
                                            @click="
                                                $refs.colorImageInput.click()
                                            "
                                        >
                                            <input
                                                type="file"
                                                ref="colorImageInput"
                                                class="hidden-input"
                                                accept="image/png, image/jpeg, image/jpg"
                                                multiple
                                                @change="handleColorImageUpload"
                                            />
                                            <div
                                                class="upload-placeholder-simple"
                                            >
                                                <i
                                                    class="fas fa-cloud-upload-alt"
                                                ></i>
                                                <p>Nhấn để tải hình ảnh lên</p>
                                            </div>
                                        </div>
                                        <span
                                            v-if="formErrors.colors"
                                            class="error-message"
                                        >
                                            {{ formErrors.colors }}
                                        </span>
                                    </div>

                                    <div
                                        v-if="newColor.imagePreviews.length > 0"
                                        class="color-preview-simple"
                                    >
                                        <div class="preview-label">
                                            Xem trước hình ảnh:
                                        </div>
                                        <div class="image-previews-grid">
                                            <div
                                                v-for="(
                                                    preview, index
                                                ) in newColor.imagePreviews"
                                                :key="`new-preview-${index}`"
                                                class="color-image-preview"
                                                :class="{
                                                    'main-image':
                                                        index ===
                                                        newColor.mainImageIndex,
                                                }"
                                            >
                                                <img
                                                    :src="preview"
                                                    alt="Preview"
                                                />
                                                <div class="image-actions">
                                                    <button
                                                        type="button"
                                                        @click="
                                                            setColorMainImage(
                                                                index
                                                            )
                                                        "
                                                        title="Đặt làm ảnh đại diện"
                                                    >
                                                        <i
                                                            class="fas fa-star"
                                                        ></i>
                                                    </button>
                                                    <button
                                                        type="button"
                                                        @click="
                                                            removeColorImage(
                                                                index
                                                            )
                                                        "
                                                        title="Xóa ảnh"
                                                    >
                                                        <i
                                                            class="fas fa-trash"
                                                        ></i>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="color-actions-simple">
                                        <button
                                            type="button"
                                            class="add-color-btn"
                                            @click="
                                                isEditing
                                                    ? addNewColorDuringEdit()
                                                    : addColorWithImages()
                                            "
                                            :disabled="
                                                !newColor.name ||
                                                newColor.images.length === 0
                                            "
                                        >
                                            <i class="fas fa-plus"></i> Thêm màu
                                            sắc
                                        </button>
                                        <button
                                            type="button"
                                            class="reset-color-btn"
                                            @click="resetNewColor"
                                        >
                                            <i class="fas fa-times"></i> Hủy
                                        </button>
                                    </div>
                                </div>

                                <div class="colors-display-simple">
                                    <div class="colors-display-header">
                                        <h5 class="colors-display-title">
                                            Màu sắc đã thêm
                                        </h5>
                                    </div>

                                    <div
                                        class="colors-list-simple"
                                        v-if="formData.colorData.length > 0"
                                    >
                                        <div
                                            v-for="(
                                                color, colorIndex
                                            ) in formData.colorData"
                                            :key="`color-${colorIndex}`"
                                            class="color-item-simple"
                                        >
                                            <div class="color-item-header">
                                                <h6>{{ color.name }}</h6>
                                                <button
                                                    type="button"
                                                    class="remove-color-btn-simple"
                                                    @click="
                                                        removeColorData(
                                                            colorIndex
                                                        )
                                                    "
                                                    title="Xóa màu sắc"
                                                >
                                                    <i class="fas fa-times"></i>
                                                </button>
                                            </div>

                                            <div v-if="isEditing && color.id">
                                                <!-- Existing images display for editing -->
                                                <div
                                                    v-if="
                                                        color.existingImages &&
                                                        color.existingImages
                                                            .length > 0
                                                    "
                                                >
                                                    <div
                                                        class="color-thumbnails-simple"
                                                    >
                                                        <div
                                                            v-for="(
                                                                image, imgIndex
                                                            ) in color.existingImages"
                                                            :key="`existing-img-${image.id}`"
                                                            class="color-image-preview"
                                                            :class="{
                                                                'main-image':
                                                                    image.isMain ||
                                                                    image.id ===
                                                                        color.mainImageId,
                                                            }"
                                                        >
                                                            <img
                                                                :src="
                                                                    formatImageUrl(
                                                                        image.imagePath
                                                                    )
                                                                "
                                                                alt="Preview"
                                                            />
                                                            <div
                                                                class="image-actions"
                                                            >
                                                                <button
                                                                    type="button"
                                                                    @click="
                                                                        setExistingColorMainImage(
                                                                            colorIndex,
                                                                            imgIndex
                                                                        )
                                                                    "
                                                                    title="Đặt làm ảnh đại diện"
                                                                >
                                                                    <i
                                                                        class="fas fa-star"
                                                                    ></i>
                                                                </button>
                                                                <button
                                                                    type="button"
                                                                    @click="
                                                                        removeExistingColorImage(
                                                                            colorIndex,
                                                                            imgIndex
                                                                        )
                                                                    "
                                                                    title="Xóa ảnh"
                                                                >
                                                                    <i
                                                                        class="fas fa-trash"
                                                                    ></i>
                                                                </button>
                                                            </div>
                                                        </div>

                                                        <!-- Display for new images added to existing color -->
                                                        <div
                                                            v-for="(
                                                                preview,
                                                                newImgIndex
                                                            ) in color.newImagePreviews"
                                                            :key="`new-img-${colorIndex}-${newImgIndex}`"
                                                            class="color-image-preview"
                                                            :class="{
                                                                'main-image':
                                                                    color.mainImageId ===
                                                                        null &&
                                                                    newImgIndex ===
                                                                        color.mainImageIndex,
                                                            }"
                                                        >
                                                            <img
                                                                :src="preview"
                                                                alt="Preview"
                                                            />
                                                            <div
                                                                class="image-actions"
                                                            >
                                                                <button
                                                                    type="button"
                                                                    @click="
                                                                        setExistingColorMainImage(
                                                                            colorIndex,
                                                                            newImgIndex,
                                                                            true
                                                                        )
                                                                    "
                                                                    title="Đặt làm ảnh đại diện"
                                                                >
                                                                    <i
                                                                        class="fas fa-star"
                                                                    ></i>
                                                                </button>
                                                                <button
                                                                    type="button"
                                                                    @click="
                                                                        removeExistingColorImage(
                                                                            colorIndex,
                                                                            newImgIndex,
                                                                            true
                                                                        )
                                                                    "
                                                                    title="Xóa ảnh"
                                                                >
                                                                    <i
                                                                        class="fas fa-trash"
                                                                    ></i>
                                                                </button>
                                                            </div>
                                                        </div>

                                                        <!-- Add more images button -->
                                                        <div
                                                            class="color-image-preview add-more"
                                                            @click="
                                                                $refs[
                                                                    `colorImageInput${colorIndex}`
                                                                ][0].click()
                                                            "
                                                        >
                                                            <input
                                                                type="file"
                                                                :ref="`colorImageInput${colorIndex}`"
                                                                class="hidden-input"
                                                                accept="image/png, image/jpeg, image/jpg"
                                                                multiple
                                                                @change="
                                                                    (e) =>
                                                                        handleColorImageUploadForExisting(
                                                                            e,
                                                                            colorIndex
                                                                        )
                                                                "
                                                            />
                                                            <div
                                                                class="add-icon"
                                                            >
                                                                <i
                                                                    class="fas fa-plus"
                                                                ></i>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>

                                            <!-- New color images display -->
                                            <div
                                                v-if="
                                                    !color.id ||
                                                    (!color.existingImages &&
                                                        !color.id)
                                                "
                                            >
                                                <div
                                                    class="color-thumbnails-simple"
                                                >
                                                    <div
                                                        v-for="(
                                                            preview, imgIndex
                                                        ) in color.imagePreviews ||
                                                        color.images"
                                                        :key="`preview-${colorIndex}-${imgIndex}`"
                                                        class="color-image-preview"
                                                        :class="{
                                                            'main-image':
                                                                imgIndex ===
                                                                color.mainImageIndex,
                                                        }"
                                                    >
                                                        <img
                                                            :src="
                                                                typeof preview ===
                                                                'string'
                                                                    ? preview
                                                                    : URL.createObjectURL(
                                                                          preview
                                                                      )
                                                            "
                                                            alt="Preview"
                                                        />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div v-else class="no-colors-added">
                                        Chưa có màu sắc nào được thêm
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="form-actions">
                            <button
                                type="button"
                                class="cancel-button"
                                @click="closeModal"
                            >
                                <i class="fas fa-times"></i> Hủy
                            </button>
                            <button
                                type="submit"
                                class="submit-button"
                                :disabled="submitLoading"
                            >
                                <span
                                    v-if="submitLoading"
                                    class="spinner small"
                                ></span>
                                <span v-else>
                                    <i class="fas fa-save"></i>
                                    {{
                                        isEditing
                                            ? "Lưu thay đổi"
                                            : "Thêm sản phẩm"
                                    }}
                                </span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Status Confirmation Modal -->
        <div v-if="showStatusModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div
                    class="modal-header"
                    :class="{ warning: productToToggle?.isActive }"
                >
                    <h3>
                        {{
                            productToToggle?.isActive
                                ? "Xác nhận ngừng bán sản phẩm"
                                : "Xác nhận kích hoạt sản phẩm"
                        }}
                    </h3>
                    <button @click="cancelStatusToggle" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div
                        class="warning-icon"
                        :class="{ 'activate-icon': !productToToggle?.isActive }"
                    >
                        <i
                            class="fas"
                            :class="
                                productToToggle?.isActive
                                    ? 'fa-toggle-off'
                                    : 'fa-toggle-on'
                            "
                        ></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn
                        <strong>
                            {{
                                productToToggle?.isActive
                                    ? "ngừng bán"
                                    : "bán lại"
                            }}
                        </strong>
                        sản phẩm
                        <strong>"{{ productToToggle?.name }}"</strong>?
                    </p>
                    <div
                        class="warning-details"
                        :class="{
                            'activate-details': !productToToggle?.isActive,
                        }"
                    >
                        <i class="fas fa-info-circle"></i>
                        <div class="warning-text">
                            <p
                                v-if="productToToggle?.isActive"
                                class="warning-impact"
                            >
                                <strong>Lưu ý:</strong> Ngừng bán sản phẩm sẽ ẩn
                                sản phẩm này khỏi trang người dùng và không thể
                                mua được nữa. Sản phẩm vẫn sẽ xuất hiện trong
                                lịch sử đơn hàng của khách hàng đã mua trước đó.
                            </p>
                            <p v-else class="warning-impact">
                                <strong>Lưu ý:</strong> Kích hoạt sản phẩm sẽ
                                hiển thị lại sản phẩm này trên trang người dùng
                                và có thể được mua bởi khách hàng.
                            </p>
                        </div>
                    </div>
                </div>

                <div class="modal-actions">
                    <button @click="cancelStatusToggle" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="confirmStatusToggle"
                        :class="
                            productToToggle?.isActive
                                ? 'deactivate-confirm-button'
                                : 'activate-confirm-button'
                        "
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner small"></span>
                        <i
                            v-else
                            class="fas"
                            :class="
                                productToToggle?.isActive
                                    ? 'fa-toggle-off'
                                    : 'fa-toggle-on'
                            "
                        ></i>
                        {{
                            productToToggle?.isActive ? "Ngừng bán" : "Bán lại"
                        }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.product-management {
    width: 100%;
}

/* Section Header Styling */
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

/* Search Box Styling */
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

/* Add Button Styling */
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

/* Status Cards Styling */
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

/* Data Table Styling */
.data-card {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    padding: 1.5rem;
    overflow: hidden;
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
    width: 20%;
} /* SẢN PHẨM */
.data-table th:nth-child(2) {
    width: 10%;
} /* HÌNH ẢNH */
.data-table th:nth-child(3) {
    width: 12%;
} /* GIÁ NHẬP */
.data-table th:nth-child(4) {
    width: 12%;
} /* GIÁ BÁN */
.data-table th:nth-child(5) {
    width: 10%;
} /* SỐ LƯỢNG */
.data-table th:nth-child(6) {
    width: 15%;
} /* DÒNG SẢN PHẨM */
.data-table th:nth-child(7) {
    width: 12%;
} /* TRẠNG THÁI */
.data-table th:nth-child(8) {
    width: 9%;
} /* HÀNH ĐỘNG */

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

/* Image cell styling */
.image-cell {
    width: 80px;
}

.logo-wrapper {
    width: 60px;
    height: 60px;
    border-radius: 12px;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f9f9f9;
    border: 1px solid #eee;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.logo-wrapper img {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
}

.name-cell {
    max-width: 200px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.quantity-cell span {
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    font-size: 0.85rem;
}

.low-stock {
    background-color: #fff3e0;
    color: #e65100;
}

.out-of-stock {
    background-color: #ffebee;
    color: #c62828;
}

.status-badge {
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
    display: inline-block;
}

.status-badge.active {
    background-color: #e6f7ea;
    color: #22c55e;
}

.status-badge.inactive {
    background-color: #fee2e2;
    color: #ef4444;
}

.actions-cell {
    display: flex;
    gap: 0.5rem;
}

.edit-button,
.status-button {
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

/* No data states */
.empty-state,
.loading-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem;
    text-align: center;
}

.empty-state i,
.loading-state .spinner {
    font-size: 3rem;
    color: #ddd;
    margin-bottom: 1rem;
}

.empty-state p,
.loading-state p {
    color: #888;
    margin-bottom: 1.5rem;
}

.action-button {
    padding: 0.6rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9rem;
}

.action-button:hover {
    background-color: #e94e9c;
}

/* Modal Styling */
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
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    width: 95%;
    max-width: 1200px;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.warning-modal {
    max-width: 500px;
}

.modal-header.warning {
    border-bottom-color: #fee2e2;
    background-color: #ffebee;
    color: #ef4444;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    color: #333;
}

.modal-header.warning h3 {
    color: #c62828;
}

.modal-header .close-button {
    background: none;
    border: none;
    font-size: 1.25rem;
    cursor: pointer;
    color: #888;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border-radius: 50%;
}

.modal-header .close-button:hover {
    background-color: #f5f5f5;
    color: #555;
}

.modal-form-container {
    flex: 1;
    overflow-y: auto;
}

/* Form Styling */
.modal-form {
    padding: 1.5rem;
}

.form-section {
    margin-bottom: 2rem;
    padding-bottom: 1.5rem;
    border-bottom: 1px solid #f0f0f0;
}

.form-section:last-child {
    margin-bottom: 0;
    padding-bottom: 0;
    border-bottom: none;
}

.section-title {
    font-size: 1.1rem;
    margin: 0 0 1.25rem 0;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.optional-badge {
    font-size: 0.7rem;
    padding: 0.15rem 0.4rem;
    border-radius: 4px;
    background-color: #f5f5f5;
    color: #757575;
}

.optional-hint {
    font-size: 0.8rem;
    color: #888;
    font-weight: normal;
    margin-left: 0.25rem;
}

.form-row {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;
}

@media (max-width: 992px) {
    .form-row {
        grid-template-columns: 1fr 1fr;
    }
}

@media (max-width: 768px) {
    .form-row {
        grid-template-columns: 1fr;
    }
}

.form-group {
    margin-bottom: 1rem;
}

.form-group label {
    display: block;
    font-size: 0.9rem;
    margin-bottom: 0.35rem;
    color: #555;
}

.required {
    color: #f44336;
    margin-left: 0.25rem;
}

.form-group input,
.form-group textarea,
.form-group select {
    width: 100%;
    padding: 0.6rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 0.9rem;
    transition: all 0.2s;
}

.form-group input:focus,
.form-group textarea:focus,
.form-group select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.currency-input {
    position: relative;
}

.currency-input input {
    padding-right: 60px; /* Tăng khoảng cách bên phải để chừa chỗ cho cả nút tăng giảm và chữ VND */
}

.currency {
    position: absolute;
    right: 25px;
    top: 50%;
    transform: translateY(-50%);
    color: #888;
    font-size: 0.85rem;
    z-index: 1;
    background-color: transparent; /* Đổi thành trong suốt để không che mũi tên */
}

.error-message {
    display: block;
    color: #f44336;
    font-size: 0.8rem;
    margin-top: 0.25rem;
}

.input-hint {
    display: block;
    color: #666;
    font-size: 0.8rem;
    margin-top: 0.25rem;
    font-style: italic;
}

.select-wrapper {
    position: relative;
}

.select-icon {
    position: absolute;
    right: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #888;
    pointer-events: none;
}

.toggle-switch-wrapper {
    margin-top: 0.5rem;
}

.toggle-switch-container {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 0.5rem;
}

.toggle-switch {
    position: relative;
    display: inline-block;
    width: 52px;
    height: 26px;
}

.toggle-switch input {
    opacity: 0;
    width: 0;
    height: 0;
}

.toggle-slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #ccc;
    -webkit-transition: 0.4s;
    transition: 0.4s;
    border-radius: 34px;
}

.toggle-slider:before {
    position: absolute;
    content: "";
    height: 18px;
    width: 18px;
    left: 4px;
    bottom: 4px;
    background-color: white;
    -webkit-transition: 0.4s;
    transition: 0.4s;
    border-radius: 50%;
}

input:checked + .toggle-slider {
    background-color: var(--primary-color);
}

input:focus + .toggle-slider {
    box-shadow: 0 0 1px var(--primary-color);
}

input:checked + .toggle-slider:before {
    -webkit-transform: translateX(26px);
    -ms-transform: translateX(26px);
    transform: translateX(26px);
}

.toggle-status {
    font-weight: 500;
}

/* Image upload area */
.upload-area {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 2rem 1rem;
    text-align: center;
    cursor: pointer;
    margin-bottom: 1rem;
    transition: all 0.2s;
}

.upload-area:hover {
    border-color: var(--primary-color);
    background-color: #fff5fc;
}

.upload-placeholder i {
    font-size: 1.5rem;
    color: #ccc;
    margin-bottom: 0.25rem;
}

.upload-placeholder p {
    margin: 0 0 0.15rem;
    font-weight: 500;
    font-size: 0.9rem;
}

.upload-placeholder span {
    font-size: 0.75rem;
    color: #888;
}

.hidden-input {
    display: none;
}

.image-previews {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin: 1rem 0;
}

.image-preview {
    position: relative;
    width: 80px;
    height: 80px;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid #eee;
}

.image-preview img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.image-preview.main-image {
    border: 2px solid var(--primary-color);
}

.image-actions {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.6);
    display: flex;
    justify-content: center;
    padding: 0.25rem;
    gap: 0.5rem;
}

.image-actions button {
    background: none;
    border: none;
    color: white;
    font-size: 0.8rem;
    cursor: pointer;
    padding: 0.25rem;
    border-radius: 4px;
    transition: all 0.2s;
}

.image-actions button:hover {
    background-color: rgba(255, 255, 255, 0.2);
}

/* Color selection styling */
.color-list {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin: 0.5rem 0;
}

.color-tag {
    display: inline-flex;
    align-items: center;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    background-color: #f5f5f5;
    font-size: 0.85rem;
    gap: 0.35rem;
}

.color-tag button {
    background: none;
    border: none;
    color: #999;
    cursor: pointer;
    padding: 0.15rem;
    border-radius: 50%;
    line-height: 1;
    transition: all 0.2s;
}

.color-tag button:hover {
    color: #d32f2f;
    background-color: #ffebee;
}

.color-input-container {
    display: flex;
    gap: 0.5rem;
    margin-top: 0.5rem;
}

.color-input-container input {
    flex: 1;
}

.color-input-container button {
    padding: 0.6rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
}

/* Form actions */
.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1.5rem;
}

.cancel-button,
.submit-button {
    padding: 0.6rem 1.5rem;
    border-radius: 8px;
    border: none;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
}

.cancel-button {
    background-color: #f5f5f5;
    color: #666;
}

.cancel-button:hover {
    background-color: #eeeeee;
}

.submit-button {
    background-color: var(--primary-color);
    color: white;
}

.submit-button:hover {
    background-color: #e94e9c;
}

.submit-button:disabled {
    background-color: #ddd;
    cursor: not-allowed;
}

/* Colors and Images Styling */
.color-add-form {
    border: 1px solid #eee;
    border-radius: 8px;
    padding: 1rem;
    margin-bottom: 1rem;
    background-color: #fafafa;
    max-width: 100%;
}

.color-input-group {
    margin-bottom: 0.75rem;
    max-width: 300px;
}

.color-upload-area {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 1rem;
    text-align: center;
    cursor: pointer;
    margin-bottom: 1rem;
    transition: all 0.2s;
    max-width: 400px;
    height: 100px;
}

.color-upload-area:hover {
    border-color: var(--primary-color);
    background-color: #fff5fc;
}

.color-preview-images {
    margin: 1rem 0;
}

.color-preview-images p {
    margin: 0 0 0.5rem 0;
    font-size: 0.9rem;
}

.image-previews-list {
    display: flex;
    flex-wrap: wrap;
    gap: 0.75rem;
}

.color-image-preview {
    position: relative;
    width: 80px;
    height: 80px;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid #eee;
}

.color-image-preview img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.color-image-preview.main-image {
    border: 2px solid var(--primary-color);
}

.color-actions {
    display: flex;
    gap: 0.5rem;
    margin-top: 1rem;
}

.add-color-btn,
.reset-color-btn {
    padding: 0.5rem 1rem;
    border-radius: 6px;
    border: none;
    cursor: pointer;
    font-size: 0.85rem;
    display: flex;
    align-items: center;
    gap: 0.35rem;
}

.add-color-btn {
    background-color: var(--primary-color);
    color: white;
    flex: 1;
}

.add-color-btn:hover {
    background-color: #e94e9c;
}

.add-color-btn:disabled {
    background-color: #ddd;
    cursor: not-allowed;
}

.reset-color-btn {
    background-color: #f5f5f5;
    color: #666;
}

.reset-color-btn:hover {
    background-color: #eeeeee;
}

.added-colors {
    margin-top: 1.5rem;
    border-top: 1px solid #eee;
    padding-top: 1.5rem;
}

.color-section-title {
    margin: 0 0 1rem 0;
    font-size: 0.95rem;
    color: #555;
}

.color-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
    gap: 1rem;
}

.color-add-section {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.color-add-container {
    border: 1px solid #eee;
    border-radius: 8px;
    padding: 1rem;
    background-color: #fafafa;
}

.color-add-header {
    margin-bottom: 1rem;
}

.color-add-title {
    font-size: 1rem;
    margin: 0;
    color: #333;
}

.color-add-content {
    display: grid;
    grid-template-columns: 1fr 2fr;
    gap: 1rem;
}

.color-name-input label,
.color-upload-wrapper label {
    display: block;
    font-size: 0.9rem;
    margin-bottom: 0.5rem;
    color: #555;
}

.color-upload-area {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 1rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.2s;
    height: 100px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.color-upload-area:hover {
    border-color: var(--primary-color);
    background-color: rgba(248, 110, 211, 0.05);
}

.upload-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.upload-placeholder i {
    font-size: 1.5rem;
    color: #bbb;
    margin-bottom: 0.5rem;
}

.upload-placeholder p {
    margin: 0 0 0.25rem;
    font-size: 0.9rem;
    font-weight: 500;
}

.upload-placeholder span {
    font-size: 0.75rem;
    color: #888;
}

.color-preview-container {
    margin-top: 1rem;
}

.preview-title {
    font-size: 0.9rem;
    margin: 0 0 0.5rem 0;
    color: #555;
}

.image-previews-list {
    display: flex;
    flex-wrap: wrap;
    gap: 0.75rem;
}

.color-actions {
    display: flex;
    gap: 0.5rem;
    margin-top: 1rem;
}

.added-colors-container {
    border-top: 1px solid #eee;
    padding-top: 1.5rem;
}

.color-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
    gap: 1rem;
}

/* Warning modal styling */
.modal-body {
    padding: 1.5rem;
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

.warning-icon.activate-icon {
    background-color: #e6f7ea;
    color: #22c55e;
}

.warning-message {
    font-size: 1.1rem;
    color: #333;
    margin-bottom: 1.5rem;
}

.warning-details {
    display: flex;
    align-items: flex-start;
    gap: 0.8rem;
    padding: 1.25rem;
    background-color: #ffeeee;
    border: 1px solid #ffcccc;
    border-radius: 8px;
    color: #d32f2f;
    text-align: left;
    font-size: 1rem;
    margin-top: 1.5rem;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.warning-details.activate-details {
    background-color: #e8f5e9;
    border: 1px solid #c8e6c9;
    color: #2e7d32;
}

.warning-text {
    flex: 1;
}

.warning-count {
    margin: 0 0 0.75rem 0;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.35rem;
}

.warning-impact {
    margin: 0;
    font-size: 1rem;
    line-height: 1.5;
}

.warning-details i {
    font-size: 1.5rem;
    margin-top: 0.2rem;
}

.activate-confirm-button,
.deactivate-confirm-button {
    min-width: 140px;
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
}

.activate-confirm-button {
    background-color: #22c55e;
    border: none;
    color: white;
    box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);
}

.activate-confirm-button:hover {
    background-color: #16a34a;
    box-shadow: 0 4px 6px rgba(34, 197, 94, 0.3);
}

.deactivate-confirm-button {
    background-color: #ef4444;
    border: none;
    color: white;
    box-shadow: 0 2px 4px rgba(239, 68, 68, 0.2);
}

.deactivate-confirm-button:hover {
    background-color: #dc2626;
    box-shadow: 0 4px 6px rgba(239, 68, 68, 0.3);
}

/* Status Toggle Buttons */
.status-button {
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

.activate-button {
    background-color: #e6f7ea;
    color: #22c55e;
}

.activate-button:hover {
    background-color: #22c55e;
    color: white;
}

.deactivate-button {
    background-color: #fee2e2;
    color: #ef4444;
}

.deactivate-button:hover {
    background-color: #ef4444;
    color: white;
}

/* Spinner */
.spinner {
    display: inline-block;
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-radius: 50%;
    border-top-color: #fff;
    width: 1.5rem;
    height: 1.5rem;
    animation: spin 1s ease-in-out infinite;
}

.spinner.small {
    width: 1rem;
    height: 1rem;
    border-width: 2px;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

/* No image placeholder */
.no-logo {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ddd;
    font-size: 1.5rem;
    background-color: #f9f9f9;
}

/* Text center utility */
.text-center {
    text-align: center;
}

/* Simple Color Section Styling */
.color-section-layout {
    display: flex;
    gap: 20px;
    margin-top: 1rem;
}

.color-add-form-simple {
    flex: 1;
    padding: 15px;
    border: 1px solid #eee;
    border-radius: 8px;
    background-color: #ffffff;
}

.colors-display-simple {
    flex: 1;
    padding: 15px;
    border: 1px solid #eee;
    border-radius: 8px;
    background-color: #ffffff;
    max-height: 400px;
    overflow-y: auto;
}

.color-name-input {
    margin-bottom: 15px;
}

.color-name-input label,
.color-upload-wrapper-simple label {
    display: block;
    font-size: 0.9rem;
    margin-bottom: 0.35rem;
    color: #555;
}

.color-name-input input {
    width: 100%;
    padding: 0.6rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 0.9rem;
    transition: all 0.2s;
}

.color-name-input input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.color-upload-wrapper-simple {
    margin-bottom: 15px;
}

.color-upload-area-simple {
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 20px 10px;
    text-align: center;
    cursor: pointer;
    transition: all 0.2s;
}

.color-upload-area-simple:hover {
    border-color: var(--primary-color);
    background-color: rgba(248, 110, 211, 0.05);
}

.upload-placeholder-simple {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}

.upload-placeholder-simple i {
    font-size: 1.2rem;
    margin-bottom: 5px;
    color: #888;
}

.upload-placeholder-simple p {
    margin: 0;
    font-size: 0.9rem;
    color: #666;
}

.color-actions-simple {
    display: flex;
    gap: 10px;
    margin-bottom: 15px;
}

.add-color-btn {
    flex: 1;
    padding: 8px 15px;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
    transition: all 0.2s;
}

.add-color-btn:hover {
    background-color: #e94e9c;
}

.add-color-btn:disabled {
    background-color: #ddd;
    cursor: not-allowed;
}

.reset-color-btn {
    padding: 8px 15px;
    background-color: #f5f5f5;
    color: #666;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
    transition: all 0.2s;
}

.reset-color-btn:hover {
    background-color: #eeeeee;
}

.preview-label {
    font-size: 0.9rem;
    color: #555;
    margin-bottom: 8px;
}

.color-preview-simple {
    margin-top: 15px;
    padding-top: 15px;
    border-top: 1px solid #eee;
}

.image-previews-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
}

.color-image-preview {
    position: relative;
    width: 80px;
    height: 80px;
    border-radius: 6px;
    overflow: hidden;
    border: 1px solid #eee;
}

.color-image-preview img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.color-image-preview.main-image {
    border: 2px solid var(--primary-color);
}

.image-actions {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.6);
    display: flex;
    justify-content: center;
    padding: 5px;
    gap: 8px;
}

.image-actions button {
    background: none;
    border: none;
    color: white;
    font-size: 0.8rem;
    cursor: pointer;
    padding: 3px;
    border-radius: 3px;
    transition: all 0.2s;
}

.image-actions button:hover {
    background-color: rgba(255, 255, 255, 0.2);
}

.colors-display-header {
    margin-bottom: 15px;
    padding-bottom: 10px;
    border-bottom: 1px solid #eee;
}

.colors-display-title {
    margin: 0;
    font-size: 1rem;
    color: #333;
    font-weight: 500;
}

.colors-list-simple {
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.color-item-simple {
    padding: 10px;
    border: 1px solid #eee;
    border-radius: 6px;
    background-color: #ffffff;
    transition: all 0.2s;
}

.color-item-simple:hover {
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.color-item-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
}

.color-item-header h6 {
    margin: 0;
    font-size: 0.9rem;
    color: #333;
}

.remove-color-btn-simple {
    border: none;
    background: none;
    color: #999;
    cursor: pointer;
    font-size: 1rem;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.remove-color-btn-simple:hover {
    background-color: #ffebee;
    color: #f44336;
}

.color-thumbnails-simple {
    display: flex;
    flex-wrap: wrap;
    gap: 5px;
}

.color-thumbnail-simple {
    width: 30px;
    height: 30px;
    border-radius: 4px;
    overflow: hidden;
    border: 1px solid #eee;
}

.color-thumbnail-simple img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.color-thumbnail-simple.main-thumbnail {
    border: 2px solid var(--primary-color);
}

.no-colors-added {
    color: #888;
    font-style: italic;
    text-align: center;
    padding: 20px;
}
</style>
