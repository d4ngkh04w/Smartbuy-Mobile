<script setup>
import { ref, onMounted, computed, watch, reactive } from "vue";
import {
    getProducts,
    getProductById,
    createProduct,
    updateProduct,
    deleteProduct,
} from "@/services/productService";
import {
    getProductLines,
    getProductLinesByBrand,
} from "@/services/productLineService";
import { getBrands } from "@/services/brandService";

// State
const products = ref([]);
const productLines = ref([]);
const brands = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showDeleteModal = ref(false);
const isEditing = ref(false);
const searchQuery = ref("");
const showFilters = ref(false);
const submitLoading = ref(false);
const fileInput = ref(null);

// State for new color input
const newColor = ref({
    name: "",
    images: [], // Actual file objects for upload
    imagePreviews: [], // URLs for previewing images
    mainImageIndex: 0, // Index of the main image for this color
});

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
    isActive: true,

    // Instead of a simple array of colors, now we have color objects with associated images
    colorData: [], // Array of { name, images, mainImageIndex }

    // This is for backward compatibility during editing
    colors: [],
});

// Product to delete reference
const productToDelete = ref(null);

// Filter options
const filters = ref({
    productLineId: "",
    brandId: "",
    priceMin: 0,
    priceMax: 100000000,
    isActive: "all",
    sortBy: "newest",
});

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

    // Nếu đường dẫn đã là URL đầy đủ hoặc là dạng base64
    if (imagePath.startsWith("http") || imagePath.startsWith("data:"))
        return imagePath;

    // Lấy base URL từ cấu hình API
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Chuẩn hóa đường dẫn (chuyển \ thành /)
    const normalizedPath = imagePath.replace(/\\/g, "/");

    // Kiểm tra nếu đường dẫn bắt đầu bằng /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Computed properties
const filteredProducts = computed(() => {
    if (!searchQuery.value && !isFiltering.value) return products.value;

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

    // Apply filters if active
    if (isFiltering.value) {
        // Filter by product line
        if (filters.value.productLineId) {
            result = result.filter(
                (p) => p.productLineId === parseInt(filters.value.productLineId)
            );
        }

        // Filter by brand through product line
        if (filters.value.brandId) {
            const filteredProductLineIds = productLines.value
                .filter((pl) => pl.brandId === parseInt(filters.value.brandId))
                .map((pl) => pl.id);

            result = result.filter((p) =>
                filteredProductLineIds.includes(p.productLineId)
            );
        }

        // Filter by price range
        result = result.filter(
            (p) =>
                p.salePrice >= filters.value.priceMin &&
                p.salePrice <= filters.value.priceMax
        );

        // Filter by active status
        if (filters.value.isActive !== "all") {
            const isActive = filters.value.isActive === "active";
            result = result.filter((p) => p.isActive === isActive);
        }
    }

    // Apply sorting
    switch (filters.value.sortBy) {
        case "newest":
            result = [...result].sort(
                (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
            );
            break;
        case "oldest":
            result = [...result].sort(
                (a, b) => new Date(a.createdAt) - new Date(b.createdAt)
            );
            break;
        case "price_asc":
            result = [...result].sort((a, b) => a.salePrice - b.salePrice);
            break;
        case "price_desc":
            result = [...result].sort((a, b) => b.salePrice - a.salePrice);
            break;
        case "name_asc":
            result = [...result].sort((a, b) => a.name.localeCompare(b.name));
            break;
        case "name_desc":
            result = [...result].sort((a, b) => b.name.localeCompare(a.name));
            break;
    }

    return result;
});

// Check if filters are active
const isFiltering = computed(() => {
    return (
        filters.value.productLineId !== "" ||
        filters.value.brandId !== "" ||
        filters.value.isActive !== "all" ||
        filters.value.priceMin > 0 ||
        filters.value.priceMax < 100000000 ||
        filters.value.sortBy !== "newest"
    );
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

// Computed property để lấy danh sách dòng sản phẩm cho bộ lọc
// Tách thành một computed riêng để tránh tính toán nhiều lần trong template
const getProductLineOptions = computed(() => {
    // Nếu đã chọn brand trong filter, chỉ hiển thị product lines của brand đó
    if (filters.value.brandId) {
        const brandId = parseInt(filters.value.brandId);
        return productLines.value.filter((pl) => pl.brandId === brandId);
    }
    // Nếu không chọn brand, hiển thị tất cả product lines
    return productLines.value;
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

const fetchProductLines = async (brandId = null) => {
    try {
        let response;
        if (brandId) {
            // Nếu có brandId, sử dụng API lấy product line theo brand cụ thể
            response = await getProductLinesByBrand(brandId);

            // Khi lấy theo brand, dữ liệu nằm trong response.data.productLines
            if (response.data && response.data.productLines) {
                // Cập nhật với cách gán trực tiếp mảng mới thay vì thay đổi nội dung mảng hiện có
                productLines.value = [...response.data.productLines];
            }
        } else {
            // Nếu không có brandId, lấy tất cả product line
            response = await getProductLines();

            // Khi lấy tất cả, dữ liệu có thể nằm trong response.data.productLines
            if (response.data && response.data.productLines) {
                productLines.value = [...response.data.productLines];
            }
        }

        // Nếu API trả về một mảng trực tiếp (không phải là .productLines)
        if (Array.isArray(response.data)) {
            productLines.value = [...response.data];
        }
    } catch (error) {
        console.error("Error fetching product lines:", error);
        // Nếu có lỗi, set mảng rỗng để tránh hiển thị dữ liệu cũ
        productLines.value = [];
    }
};

const fetchBrands = async () => {
    try {
        const response = await getBrands({ includeProductLines: true });
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

// Reset filter values
const resetFilters = () => {
    filters.value = {
        productLineId: "",
        brandId: "",
        priceMin: 0,
        priceMax: 100000000,
        isActive: "all",
        sortBy: "newest",
    };
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
        // Chỉ lấy danh sách brands trước, không lấy product lines
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
        const brandId = getBrandIdForProductLine(fullProduct.productLineId);

        // Prepare form data from product
        formData.value = {
            id: fullProduct.id,
            name: fullProduct.name,
            brandId: brandId,
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

        // Lấy dữ liệu product lines dựa trên brandId đã xác định
        if (brandId) {
            await fetchProductLines(brandId);
        }

        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
    } finally {
        loading.value = false;
    }
};

// Handle image uploads
const handleImageUpload = (event) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    // Clear previous error
    formErrors.images = "";

    // Check file count
    if (formData.value.images.length + files.length > 5) {
        formErrors.images = "Tối đa 5 hình ảnh được phép";
        return;
    }

    // Process each file
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Validate file type
        if (!["image/jpeg", "image/png", "image/jpg"].includes(file.type)) {
            formErrors.images = "Chỉ chấp nhận các định dạng: jpg, jpeg, png";
            continue;
        }

        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            formErrors.images = "Kích thước tệp tối đa là 5MB";
            continue;
        }

        // Add file to the list
        formData.value.images.push(file);

        // Create preview
        const reader = new FileReader();
        reader.onload = (e) => {
            formData.value.imagesPreviews.push({
                id: null, // null for new images
                url: e.target.result,
                isMain: formData.value.imagesPreviews.length === 0, // First image is main by default
            });
        };
        reader.readAsDataURL(file);
    }

    // Clear the file input for future uploads
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Remove an image
const removeImage = (index) => {
    // If removing the main image, set a new main image
    if (index === formData.value.mainImageIndex) {
        // If there are other images, set the first one as main
        if (formData.value.imagesPreviews.length > 1) {
            const newMainIndex = index === 0 ? 1 : 0;
            formData.value.mainImageIndex = newMainIndex;
        } else {
            formData.value.mainImageIndex = 0;
        }
    } else if (index < formData.value.mainImageIndex) {
        // If removing an image before the main image, adjust the main image index
        formData.value.mainImageIndex--;
    }

    // For new images
    if (!formData.value.imagesPreviews[index].id) {
        const actualIndex = formData.value.imagesPreviews
            .slice(0, index)
            .filter((img) => !img.id).length;
        formData.value.images.splice(actualIndex, 1);
    }

    // Remove from previews
    formData.value.imagesPreviews.splice(index, 1);
};

// Set main image
const setMainImage = (index) => {
    formData.value.mainImageIndex = index;
};

// Handle color image upload
const handleColorImageUpload = (event) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    // Clear any previous errors
    formErrors.colors = "";

    // Check if this color can accept more images (max 5 per color)
    if (newColor.value.images.length + files.length > 5) {
        formErrors.colors = "Tối đa 5 hình ảnh cho mỗi màu sắc";
        return;
    }

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

// Add function to handle removing images from color data
const removeColorImageFromData = (colorIndex, imageIndex) => {
    const colorData = formData.value.colorData[colorIndex];

    // If removing the main image, set a new main image
    if (imageIndex === colorData.mainImageIndex) {
        if (colorData.imagePreviews.length > 1) {
            const newMainIndex = imageIndex === 0 ? 1 : 0;
            colorData.mainImageIndex = newMainIndex;
        } else {
            colorData.mainImageIndex = 0;
        }
    } else if (imageIndex < colorData.mainImageIndex) {
        // If removing an image before the main image, adjust the main image index
        colorData.mainImageIndex--;
    }

    // Remove from arrays
    colorData.images.splice(imageIndex, 1);
    colorData.imagePreviews.splice(imageIndex, 1);
};

// Handle color-specific image operations for existing colors during editing
const handleColorImageUploadForExisting = (event, colorIndex) => {
    const files = event.target.files;
    if (!files || !files.length) return;

    const colorData = formData.value.colorData[colorIndex];

    // Check if this color can accept more images (max 5 per color including existing ones)
    const currentImageCount =
        (colorData.existingImages?.length || 0) -
        (colorData.removedImageIds?.length || 0) +
        (colorData.newImages?.length || 0);
    if (currentImageCount + files.length > 5) {
        formErrors.colors = "Tối đa 5 hình ảnh cho mỗi màu sắc";
        return;
    }

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
    } else {
        // Set main image from existing images
        const image = colorData.existingImages[imageIndex];
        if (image) {
            colorData.mainImageId = image.id;
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

// Add new color during editing
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

// Add a product color
const addColor = () => {
    const color = document.getElementById("colorInput").value.trim();
    if (!color) return;

    // Check if color already exists
    if (
        formData.value.colors.some(
            (c) => c.toLowerCase() === color.toLowerCase()
        )
    ) {
        return;
    }

    formData.value.colors.push(color);
    document.getElementById("colorInput").value = ""; // Clear input
};

// Remove a product color
const removeColor = (index) => {
    formData.value.colors.splice(index, 1);
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
        data.append("Quantity", formData.value.quantity);
        data.append("ImportPrice", formData.value.importPrice);
        data.append("SalePrice", formData.value.salePrice);

        if (formData.value.description) {
            data.append("Description", formData.value.description);
        }

        // When editing, need to include ProductLineId and IsActive
        if (isEditing.value) {
            data.append("ProductLineId", formData.value.productLineId);
            data.append("IsActive", formData.value.isActive.toString());
        } else {
            // For new products
            data.append("BrandId", formData.value.brandId);
            data.append("ProductLineId", formData.value.productLineId);
        }

        // Add product details
        if (formData.value.warranty)
            data.append("Warranty", formData.value.warranty);
        if (formData.value.ram) data.append("RAM", formData.value.ram);
        if (formData.value.storage)
            data.append("Storage", formData.value.storage);
        if (formData.value.screenSize)
            data.append("ScreenSize", formData.value.screenSize);
        if (formData.value.screenResolution)
            data.append("ScreenResolution", formData.value.screenResolution);
        if (formData.value.battery)
            data.append("Battery", formData.value.battery);
        if (formData.value.processor)
            data.append("Processor", formData.value.processor);
        if (formData.value.os) data.append("OS", formData.value.os);
        if (formData.value.simSlots)
            data.append("SimSlots", formData.value.simSlots);

        // For creating a new product
        if (!isEditing.value) {
            // Process colors and their images for new product
            formData.value.colorData.forEach((colorData, colorIndex) => {
                // Add color name
                data.append(`ColorData[${colorIndex}].Name`, colorData.name);

                // Add which image is the main image for this color
                data.append(
                    `ColorData[${colorIndex}].MainImageIndex`,
                    colorData.mainImageIndex.toString()
                );

                // Add all images for this color
                colorData.images.forEach((image, imageIndex) => {
                    data.append(
                        `ColorData[${colorIndex}].Images[${imageIndex}]`,
                        image
                    );
                });
            });
        }
        // For editing an existing product
        else {
            // Handle existing colors updates and new colors
            const updateColorData = [];
            const removeColorIds = [];

            // First, identify colors that should be removed
            if (formData.value.existingColors) {
                const existingColorIds = formData.value.existingColors.map(
                    (c) => c.id
                );
                const remainingColorIds = formData.value.colorData
                    .filter((c) => c.id) // Only consider colors with IDs (existing colors)
                    .map((c) => c.id);

                // Find colors that exist in existingColors but not in colorData
                existingColorIds.forEach((id) => {
                    if (!remainingColorIds.includes(id)) {
                        removeColorIds.push(id);
                    }
                });
            }

            // Add color IDs that should be removed
            removeColorIds.forEach((id) => {
                data.append("RemoveColorIds", id);
            });

            // Process existing colors that need updates
            formData.value.colorData.forEach((colorData, index) => {
                if (colorData.id) {
                    // This is an existing color being updated
                    data.append(`UpdateColorData[${index}].Id`, colorData.id);
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
                                imageId
                            );
                        });
                    }

                    // Add new images for this existing color
                    if (colorData.newImages && colorData.newImages.length > 0) {
                        colorData.newImages.forEach((image, imgIndex) => {
                            data.append(
                                `UpdateColorData[${index}].AddImages[${imgIndex}]`,
                                image
                            );
                        });

                        // If there's a main image to set from the new images
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

                    // If there's a main image to set from existing images
                    if (colorData.mainImageId) {
                        data.append(
                            `UpdateColorData[${index}].MainImageId`,
                            colorData.mainImageId
                        );
                        data.append(
                            `UpdateColorData[${index}].SetMainImage`,
                            "true"
                        );
                    }
                } else {
                    // This is a new color being added
                    data.append(
                        `UpdateColorData[${index}].Name`,
                        colorData.name
                    );

                    // Add images for this new color
                    if (colorData.images && colorData.images.length > 0) {
                        colorData.images.forEach((image, imgIndex) => {
                            data.append(
                                `UpdateColorData[${index}].AddImages[${imgIndex}]`,
                                image
                            );
                        });
                    }

                    // Set main image for this new color
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
        }

        let response;
        if (isEditing.value) {
            response = await updateProduct(formData.value.id, data);
        } else {
            response = await createProduct(data);
        }

        // Refresh the products list
        await fetchProducts();

        // Close the modal
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

// Confirm delete
const confirmDelete = (product) => {
    productToDelete.value = product;
    showDeleteModal.value = true;
};

// Cancel delete
const cancelDelete = () => {
    showDeleteModal.value = false;
    productToDelete.value = null;
};

// Delete product
const deleteConfirmed = async () => {
    if (!productToDelete.value) return;

    loading.value = true;
    try {
        await deleteProduct(productToDelete.value.id);
        await fetchProducts();
        showDeleteModal.value = false;
        productToDelete.value = null;
    } catch (error) {
        console.error("Error deleting product:", error);
        alert("Có lỗi xảy ra khi xóa sản phẩm. Vui lòng thử lại sau.");
    } finally {
        loading.value = false;
    }
};

// Get product image URL
const getProductImage = (product) => {
    if (!product.images || product.images.length === 0) return null;

    // Find main image first
    const mainImage = product.images.find((img) => img.isMain);
    if (mainImage) {
        return formatImageUrl(mainImage.imagePath);
    }

    // If no main image, return the first one
    return formatImageUrl(product.images[0].imagePath);
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

                <div class="filter-button" @click="showFilters = !showFilters">
                    <i class="fas fa-filter"></i>
                    <span>Bộ lọc</span>
                    <span v-if="isFiltering" class="filter-badge">!</span>
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

        <!-- Filters panel -->
        <div v-if="showFilters" class="filter-panel">
            <div class="filter-header">
                <h3>Bộ lọc</h3>
                <div class="filter-actions">
                    <button @click="resetFilters" class="reset-button">
                        <i class="fas fa-undo"></i> Đặt lại
                    </button>
                    <button @click="showFilters = false" class="close-filter">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>

            <div class="filter-content">
                <div class="filter-group">
                    <label>Thương hiệu</label>
                    <select v-model="filters.brandId">
                        <option value="">Tất cả thương hiệu</option>
                        <option
                            v-for="brand in brands"
                            :key="brand.id"
                            :value="brand.id.toString()"
                        >
                            {{ brand.name }}
                        </option>
                    </select>
                </div>

                <div class="filter-group">
                    <label>Dòng sản phẩm</label>
                    <select v-model="filters.productLineId">
                        <option value="">Tất cả dòng sản phẩm</option>
                        <option
                            v-for="pl in getProductLineOptions"
                            :key="pl.id"
                            :value="pl.id.toString()"
                        >
                            {{ pl.name }}
                        </option>
                    </select>
                </div>

                <div class="filter-group">
                    <label>Trạng thái</label>
                    <select v-model="filters.isActive">
                        <option value="all">Tất cả trạng thái</option>
                        <option value="active">Đang bán</option>
                        <option value="inactive">Ngừng bán</option>
                    </select>
                </div>

                <div class="filter-group">
                    <label>Sắp xếp</label>
                    <select v-model="filters.sortBy">
                        <option value="newest">Mới nhất</option>
                        <option value="oldest">Cũ nhất</option>
                        <option value="price_asc">Giá tăng dần</option>
                        <option value="price_desc">Giá giảm dần</option>
                        <option value="name_asc">Tên A-Z</option>
                        <option value="name_desc">Tên Z-A</option>
                    </select>
                </div>

                <div class="filter-group">
                    <label>Khoảng giá</label>
                    <div class="price-range">
                        <input
                            type="number"
                            v-model="filters.priceMin"
                            placeholder="Giá tối thiểu"
                        />
                        <span>-</span>
                        <input
                            type="number"
                            v-model="filters.priceMax"
                            placeholder="Giá tối đa"
                        />
                    </div>
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
                <p v-if="searchQuery || isFiltering">
                    Không tìm thấy sản phẩm phù hợp
                </p>
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
                            <div class="image-wrapper">
                                <img
                                    v-if="getProductImage(product)"
                                    :src="getProductImage(product)"
                                    :alt="product.name"
                                />
                                <div v-else class="no-image">
                                    <i class="fas fa-image"></i>
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
                                @click="confirmDelete(product)"
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

        <!-- Add/Edit Product Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container product-modal">
                <div class="modal-header">
                    <h3>
                        {{
                            isEditing
                                ? "Chỉnh sửa sản phẩm"
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
                                    <label
                                        >Thương hiệu
                                        <span class="required">*</span></label
                                    >
                                    <select
                                        v-model="formData.brandId"
                                        @change="handleBrandChange"
                                        class="form-select"
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
                                    <span class="input-hint"
                                        >Chọn thương hiệu từ danh sách</span
                                    >
                                    <span
                                        v-if="formErrors.brandId"
                                        class="error-message"
                                    >
                                        {{ formErrors.brandId }}
                                    </span>
                                </div>

                                <div class="form-group">
                                    <label
                                        >Dòng sản phẩm
                                        <span class="required">*</span></label
                                    >
                                    <select
                                        v-model="formData.productLineId"
                                        class="form-select"
                                        :disabled="!formData.brandId"
                                    >
                                        <option value="" disabled selected>
                                            {{
                                                formData.brandId
                                                    ? "Chọn dòng sản phẩm"
                                                    : "Vui lòng chọn thương hiệu trước"
                                            }}
                                        </option>
                                        <option
                                            v-for="pl in filteredProductLines"
                                            :key="pl.id"
                                            :value="pl.id"
                                        >
                                            {{ pl.name }}
                                        </option>
                                    </select>
                                    <span class="input-hint"
                                        >Chọn dòng sản phẩm từ danh sách sau khi
                                        chọn thương hiệu</span
                                    >
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
                                <label
                                    >Tên sản phẩm
                                    <span class="required">*</span></label
                                >
                                <input
                                    v-model="formData.name"
                                    type="text"
                                    placeholder="Ví dụ: iPhone 16 Pro - Chính hãng VN/A"
                                    maxlength="100"
                                />
                                <span class="input-hint"
                                    >Tối đa 100 ký tự, có thể bao gồm chữ, số,
                                    dấu gạch ngang và gạch chéo</span
                                >
                                <span
                                    v-if="formErrors.name"
                                    class="error-message"
                                    >{{ formErrors.name }}</span
                                >
                            </div>

                            <!-- Nhóm thông tin giá và số lượng -->
                            <div class="form-row">
                                <div class="form-group">
                                    <label
                                        >Giá nhập
                                        <span class="required">*</span></label
                                    >
                                    <div class="currency-input">
                                        <input
                                            v-model.number="
                                                formData.importPrice
                                            "
                                            type="number"
                                            min="1000"
                                            step="1000"
                                            placeholder="Ví dụ: 20000000"
                                        />
                                        <span class="currency">VND</span>
                                    </div>
                                    <span class="input-hint"
                                        >Nhập số nguyên, đơn vị VND</span
                                    >
                                    <span
                                        v-if="formErrors.importPrice"
                                        class="error-message"
                                        >{{ formErrors.importPrice }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label
                                        >Giá bán
                                        <span class="required">*</span></label
                                    >
                                    <div class="currency-input">
                                        <input
                                            v-model.number="formData.salePrice"
                                            type="number"
                                            min="1000"
                                            step="1000"
                                            placeholder="Ví dụ: 25000000"
                                        />
                                        <span class="currency">VND</span>
                                    </div>
                                    <span class="input-hint"
                                        >Nhập số nguyên, đơn vị VND</span
                                    >
                                    <span
                                        v-if="formErrors.salePrice"
                                        class="error-message"
                                        >{{ formErrors.salePrice }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label
                                        >Số lượng
                                        <span class="required">*</span></label
                                    >
                                    <input
                                        v-model.number="formData.quantity"
                                        type="number"
                                        min="1"
                                        placeholder="Ví dụ: 10"
                                    />
                                    <span class="input-hint"
                                        >Nhập số nguyên dương</span
                                    >
                                    <span
                                        v-if="formErrors.quantity"
                                        class="error-message"
                                        >{{ formErrors.quantity }}</span
                                    >
                                </div>
                            </div>

                            <!-- Nhóm trạng thái -->
                            <div class="form-row">
                                <div class="form-group">
                                    <label>Trạng thái sản phẩm</label>
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
                                <label>Mô tả sản phẩm</label>
                                <textarea
                                    v-model="formData.description"
                                    placeholder="Nhập mô tả chi tiết về sản phẩm"
                                    rows="4"
                                ></textarea>
                                <span class="input-hint"
                                    >Mô tả chi tiết về sản phẩm, tính năng và
                                    đặc điểm nổi bật</span
                                >
                            </div>
                        </div>

                        <!-- Technical Specifications -->
                        <div class="form-section">
                            <h4 class="section-title">Thông số kỹ thuật</h4>

                            <div class="form-row">
                                <div class="form-group">
                                    <label>RAM (GB)</label>
                                    <input
                                        v-model.number="formData.ram"
                                        type="number"
                                        min="1"
                                        max="32"
                                        placeholder="Ví dụ: 8"
                                    />
                                    <span class="input-hint"
                                        >Nhập số nguyên (1-32) đơn vị GB</span
                                    >
                                    <span
                                        v-if="formErrors.ram"
                                        class="error-message"
                                        >{{ formErrors.ram }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label>Bộ nhớ (GB)</label>
                                    <input
                                        v-model.number="formData.storage"
                                        type="number"
                                        min="8"
                                        max="2048"
                                        placeholder="Ví dụ: 128"
                                    />
                                    <span class="input-hint"
                                        >Nhập số nguyên (8-2048) đơn vị GB</span
                                    >
                                    <span
                                        v-if="formErrors.storage"
                                        class="error-message"
                                        >{{ formErrors.storage }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label>Số khe SIM</label>
                                    <select
                                        v-model.number="formData.simSlots"
                                        class="form-select"
                                    >
                                        <option :value="1">1 SIM</option>
                                        <option :value="2">2 SIM</option>
                                        <option :value="3">3 SIM</option>
                                        <option :value="4">4 SIM</option>
                                    </select>
                                    <span class="input-hint"
                                        >Chọn số khe SIM của thiết bị</span
                                    >
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group">
                                    <label>Thời gian bảo hành (tháng)</label>
                                    <input
                                        v-model.number="formData.warranty"
                                        type="number"
                                        min="1"
                                        max="60"
                                        placeholder="Ví dụ: 12"
                                    />
                                    <span class="input-hint"
                                        >Nhập số tháng bảo hành (1-60)</span
                                    >
                                    <span
                                        v-if="formErrors.warranty"
                                        class="error-message"
                                        >{{ formErrors.warranty }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label>Kích thước màn hình (inch)</label>
                                    <input
                                        v-model.number="formData.screenSize"
                                        type="number"
                                        min="3.0"
                                        max="15.0"
                                        step="0.1"
                                        placeholder="Ví dụ: 6.1"
                                    />
                                    <span class="input-hint"
                                        >Nhập số thập phân (3.0-15.0) đơn vị
                                        inch</span
                                    >
                                    <span
                                        v-if="formErrors.screenSize"
                                        class="error-message"
                                        >{{ formErrors.screenSize }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label>Độ phân giải màn hình</label>
                                    <input
                                        v-model="formData.screenResolution"
                                        type="text"
                                        placeholder="Ví dụ: 1170x2532"
                                        pattern="\d{3,4}x\d{3,4}"
                                    />
                                    <span class="input-hint"
                                        >Nhập theo định dạng: 1234x5678</span
                                    >
                                    <span
                                        v-if="formErrors.screenResolution"
                                        class="error-message"
                                        >{{ formErrors.screenResolution }}</span
                                    >
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group">
                                    <label>Dung lượng Pin (mAh)</label>
                                    <input
                                        v-model.number="formData.battery"
                                        type="number"
                                        min="1000"
                                        max="10000"
                                        step="100"
                                        placeholder="Ví dụ: 3500"
                                    />
                                    <span class="input-hint"
                                        >Nhập số nguyên (1000-10000) đơn vị
                                        mAh</span
                                    >
                                    <span
                                        v-if="formErrors.battery"
                                        class="error-message"
                                        >{{ formErrors.battery }}</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label>Vi xử lý</label>
                                    <input
                                        v-model="formData.processor"
                                        type="text"
                                        placeholder="Ví dụ: Apple A15 Bionic"
                                        maxlength="100"
                                    />
                                    <span class="input-hint"
                                        >Tối đa 100 ký tự</span
                                    >
                                </div>

                                <div class="form-group">
                                    <label>Hệ điều hành</label>
                                    <input
                                        v-model="formData.os"
                                        type="text"
                                        placeholder="Ví dụ: iOS 16"
                                        maxlength="50"
                                    />
                                    <span class="input-hint"
                                        >Tối đa 50 ký tự</span
                                    >
                                </div>
                            </div>
                        </div>

                        <!-- Colors Section -->
                        <div class="form-section">
                            <h4 class="section-title">Màu sắc và Hình ảnh</h4>

                            <div class="color-section-layout">
                                <!-- Phần nhập màu - bên trái -->
                                <div class="color-add-form-simple">
                                    <div class="color-name-input">
                                        <label>Tên màu sắc</label>
                                        <input
                                            v-model="newColor.name"
                                            placeholder="Nhập tên màu sắc"
                                        />
                                    </div>

                                    <div class="color-upload-wrapper-simple">
                                        <label>Hình ảnh màu</label>
                                        <div
                                            class="color-upload-area-simple"
                                            @click="
                                                $refs.colorFileInput.click()
                                            "
                                        >
                                            <div
                                                class="upload-placeholder-simple"
                                            >
                                                <i
                                                    class="fas fa-cloud-upload-alt"
                                                ></i>
                                                <p>Nhấp để tải lên hình ảnh</p>
                                            </div>
                                        </div>
                                        <input
                                            ref="colorFileInput"
                                            type="file"
                                            accept="image/jpeg, image/png, image/jpg"
                                            @change="handleColorImageUpload"
                                            multiple
                                            class="hidden-input"
                                        />
                                    </div>

                                    <div class="color-actions-simple">
                                        <button
                                            @click="
                                                isEditing
                                                    ? addNewColorDuringEdit()
                                                    : addColorWithImages()
                                            "
                                            class="add-color-btn"
                                            :disabled="
                                                !newColor.name ||
                                                newColor.images.length === 0
                                            "
                                        >
                                            <i class="fas fa-plus"></i> Thêm màu
                                        </button>
                                        <button
                                            @click="resetNewColor"
                                            class="reset-color-btn"
                                        >
                                            <i class="fas fa-redo"></i> Làm mới
                                        </button>
                                    </div>

                                    <!-- Preview của màu đang thêm mới -->
                                    <div
                                        class="color-preview-simple"
                                        v-if="newColor.imagePreviews.length > 0"
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
                                                        @click="
                                                            setColorMainImage(
                                                                index
                                                            )
                                                        "
                                                        title="Đặt làm ảnh chính"
                                                    >
                                                        <i
                                                            class="fas fa-star"
                                                        ></i>
                                                    </button>
                                                    <button
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

                                    <span
                                        v-if="formErrors.colors"
                                        class="error-message"
                                    >
                                        {{ formErrors.colors }}
                                    </span>
                                </div>

                                <!-- Phần hiển thị màu đã thêm - bên phải -->
                                <div class="colors-display-simple">
                                    <div class="colors-display-header">
                                        <h5 class="colors-display-title">
                                            Màu đã thêm ({{
                                                formData.colorData.length
                                            }})
                                        </h5>
                                    </div>

                                    <!-- Hiển thị màu hiện tại khi thêm mới sản phẩm -->
                                    <div
                                        v-if="
                                            !isEditing &&
                                            formData.colorData.length > 0
                                        "
                                        class="colors-list-simple"
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
                                                    @click="
                                                        removeColorData(
                                                            colorIndex
                                                        )
                                                    "
                                                    class="remove-color-btn-simple"
                                                    title="Xóa màu"
                                                >
                                                    <i class="fas fa-times"></i>
                                                </button>
                                            </div>

                                            <div class="preview-label">
                                                Hình ảnh:
                                            </div>
                                            <div class="image-previews-grid">
                                                <div
                                                    v-for="(
                                                        preview, imageIndex
                                                    ) in color.imagePreviews"
                                                    :key="`preview-${colorIndex}-${imageIndex}`"
                                                    class="color-image-preview"
                                                    :class="{
                                                        'main-image':
                                                            imageIndex ===
                                                            color.mainImageIndex,
                                                    }"
                                                >
                                                    <img
                                                        :src="preview"
                                                        alt="Product Image"
                                                    />
                                                    <div class="image-actions">
                                                        <button
                                                            @click="
                                                                setColorMainImage(
                                                                    imageIndex,
                                                                    colorIndex
                                                                )
                                                            "
                                                            title="Đặt làm ảnh chính"
                                                        >
                                                            <i
                                                                class="fas fa-star"
                                                            ></i>
                                                        </button>
                                                        <button
                                                            @click="
                                                                removeColorImageFromData(
                                                                    colorIndex,
                                                                    imageIndex
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
                                    </div>

                                    <!-- Hiển thị màu hiện tại khi cập nhật sản phẩm -->
                                    <div
                                        v-if="
                                            isEditing &&
                                            formData.colorData.length > 0
                                        "
                                        class="colors-list-simple"
                                    >
                                        <div
                                            v-for="(
                                                color, colorIndex
                                            ) in formData.colorData"
                                            :key="`color-${
                                                color.id || colorIndex
                                            }`"
                                            class="color-item-simple"
                                        >
                                            <div class="color-item-header">
                                                <h6>{{ color.name }}</h6>
                                                <button
                                                    @click="
                                                        removeColorData(
                                                            colorIndex
                                                        )
                                                    "
                                                    class="remove-color-btn-simple"
                                                    title="Xóa màu"
                                                >
                                                    <i class="fas fa-times"></i>
                                                </button>
                                            </div>

                                            <!-- Existing images for this color -->
                                            <div
                                                v-if="
                                                    color.existingImages &&
                                                    color.existingImages
                                                        .length > 0
                                                "
                                            >
                                                <div class="preview-label">
                                                    Hình ảnh hiện có:
                                                </div>
                                                <div
                                                    class="image-previews-grid"
                                                >
                                                    <div
                                                        v-for="(
                                                            image, imageIndex
                                                        ) in color.existingImages"
                                                        :key="`existing-img-${image.id}`"
                                                        class="color-image-preview"
                                                        :class="{
                                                            'main-image':
                                                                color.mainImageId ===
                                                                image.id,
                                                        }"
                                                        v-show="
                                                            !color.removedImageIds ||
                                                            !color.removedImageIds.includes(
                                                                image.id
                                                            )
                                                        "
                                                    >
                                                        <img
                                                            :src="
                                                                formatImageUrl(
                                                                    image.imagePath
                                                                )
                                                            "
                                                            alt="Product Image"
                                                        />
                                                        <div
                                                            class="image-actions"
                                                        >
                                                            <button
                                                                @click="
                                                                    setExistingColorMainImage(
                                                                        colorIndex,
                                                                        imageIndex
                                                                    )
                                                                "
                                                                title="Đặt làm ảnh chính"
                                                            >
                                                                <i
                                                                    class="fas fa-star"
                                                                ></i>
                                                            </button>
                                                            <button
                                                                @click="
                                                                    removeExistingColorImage(
                                                                        colorIndex,
                                                                        imageIndex
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

                                            <!-- New images for this color -->
                                            <div
                                                v-if="
                                                    color.newImagePreviews &&
                                                    color.newImagePreviews
                                                        .length > 0
                                                "
                                            >
                                                <div class="preview-label">
                                                    Hình ảnh mới thêm:
                                                </div>
                                                <div
                                                    class="image-previews-grid"
                                                >
                                                    <div
                                                        v-for="(
                                                            preview, imageIndex
                                                        ) in color.newImagePreviews"
                                                        :key="`new-image-${colorIndex}-${imageIndex}`"
                                                        class="color-image-preview"
                                                        :class="{
                                                            'main-image':
                                                                color.mainImageIndex ===
                                                                imageIndex,
                                                        }"
                                                    >
                                                        <img
                                                            :src="preview"
                                                            alt="New Product Image"
                                                        />
                                                        <div
                                                            class="image-actions"
                                                        >
                                                            <button
                                                                @click="
                                                                    setExistingColorMainImage(
                                                                        colorIndex,
                                                                        imageIndex,
                                                                        true
                                                                    )
                                                                "
                                                                title="Đặt làm ảnh chính"
                                                            >
                                                                <i
                                                                    class="fas fa-star"
                                                                ></i>
                                                            </button>
                                                            <button
                                                                @click="
                                                                    removeExistingColorImage(
                                                                        colorIndex,
                                                                        imageIndex,
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
                                                </div>
                                            </div>

                                            <!-- Button to add more images to this color -->
                                            <div
                                                class="color-upload-wrapper-simple mt-2"
                                            >
                                                <button
                                                    class="add-image-btn"
                                                    @click="
                                                        $refs[
                                                            `colorFileInput${colorIndex}`
                                                        ][0].click()
                                                    "
                                                    style="
                                                        padding: 6px 10px;
                                                        background-color: #f5f5f5;
                                                        border: 1px solid #ddd;
                                                        border-radius: 6px;
                                                        cursor: pointer;
                                                        width: 100%;
                                                    "
                                                >
                                                    <i class="fas fa-plus"></i>
                                                    Thêm hình ảnh cho màu này
                                                </button>
                                                <input
                                                    :ref="`colorFileInput${colorIndex}`"
                                                    type="file"
                                                    accept="image/jpeg, image/png, image/jpg"
                                                    @change="
                                                        (e) =>
                                                            handleColorImageUploadForExisting(
                                                                e,
                                                                colorIndex
                                                            )
                                                    "
                                                    multiple
                                                    class="hidden-input"
                                                />
                                            </div>
                                        </div>
                                    </div>

                                    <div
                                        v-else-if="
                                            formData.colorData.length === 0
                                        "
                                        class="no-colors-added"
                                    >
                                        Chưa có màu sắc nào được thêm
                                    </div>
                                </div>
                            </div>
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
                                :disabled="submitLoading"
                            >
                                <i
                                    v-if="submitLoading"
                                    class="spinner small"
                                ></i>
                                <i v-else class="fas fa-save"></i>
                                {{ isEditing ? "Cập nhật" : "Thêm mới" }}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Delete Confirmation Modal -->
        <div v-if="showDeleteModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div class="modal-header warning">
                    <h3>Xác nhận xóa sản phẩm</h3>
                    <button @click="cancelDelete" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div class="warning-icon">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn xóa sản phẩm
                        <strong>"{{ productToDelete?.name }}"</strong>?
                    </p>
                    <p class="warning-details">
                        Hành động này không thể hoàn tác và sẽ xóa vĩnh viễn sản
                        phẩm khỏi hệ thống.
                    </p>
                </div>

                <div class="modal-actions">
                    <button @click="cancelDelete" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="deleteConfirmed"
                        class="delete-confirm-button"
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner small"></span>
                        <i v-else class="fas fa-trash-alt"></i>
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

/* Filter Button Styling */
.filter-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.6rem 1rem;
    background-color: #f9f9f9;
    border: 1px solid #ddd;
    border-radius: 8px;
    cursor: pointer;
    position: relative;
}

.filter-button:hover {
    background-color: #f0f0f0;
}

.filter-badge {
    position: absolute;
    top: -5px;
    right: -5px;
    background-color: var(--primary-color);
    color: white;
    width: 16px;
    height: 16px;
    border-radius: 50%;
    font-size: 0.6rem;
    display: flex;
    align-items: center;
    justify-content: center;
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

/* Filter Panel Styling */
.filter-panel {
    background-color: white;
    border-radius: 12px;
    padding: 1.5rem;
    margin-bottom: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.filter-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
}

.filter-header h3 {
    font-size: 1.1rem;
    margin: 0;
    color: #333;
}

.filter-actions {
    display: flex;
    gap: 0.5rem;
}

.reset-button,
.close-filter {
    padding: 0.4rem 0.75rem;
    border-radius: 6px;
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.25rem;
    font-size: 0.85rem;
}

.reset-button {
    background-color: #f9f9f9;
    border: 1px solid #ddd;
    color: #666;
}

.reset-button:hover {
    background-color: #f0f0f0;
}

.close-filter {
    background: none;
    color: #666;
}

.close-filter:hover {
    color: #333;
}

.filter-content {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1rem;
}

.filter-group {
    margin-bottom: 0.5rem;
}

.filter-group label {
    display: block;
    font-size: 0.9rem;
    margin-bottom: 0.35rem;
    color: #555;
}

.filter-group select,
.filter-group input {
    width: 100%;
    padding: 0.6rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 0.9rem;
}

.filter-group select:focus,
.filter-group input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.price-range {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.price-range input {
    flex: 1;
}

.price-range span {
    font-size: 1.25rem;
    font-weight: 500;
    color: #666;
}

/* Data Table Styling */
.data-card {
    background-color: white;
    border-radius: 12px;
    padding: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    position: relative;
}

.data-table {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0;
}

.data-table th {
    text-align: left;
    padding: 1rem;
    font-weight: 500;
    color: #555;
    border-bottom: 1px solid #eee;
    background-color: #fafafa;
}

.data-table th:first-child {
    border-top-left-radius: 8px;
}

.data-table th:last-child {
    border-top-right-radius: 8px;
    text-align: center;
}

.data-table td {
    padding: 1rem;
    border-bottom: 1px solid #eee;
    vertical-align: middle;
}

.data-row:hover {
    background-color: #f9f9f9;
}

.data-table td:last-child {
    text-align: center;
}

/* Image cell styling */
.image-cell {
    width: 80px;
}

.image-wrapper {
    width: 60px;
    height: 60px;
    border-radius: 8px;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f9f9f9;
}

.image-wrapper img {
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
    border-radius: 4px;
    font-size: 0.85rem;
    display: inline-block;
}

.status-badge.active {
    background-color: #e8f5e9;
    color: #2e7d32;
}

.status-badge.inactive {
    background-color: #f5f5f5;
    color: #757575;
}

.actions-cell {
    white-space: nowrap;
}

.actions-cell button {
    padding: 0.4rem;
    margin: 0 0.25rem;
    border: none;
    background: none;
    cursor: pointer;
    border-radius: 4px;
    transition: all 0.2s;
}

.edit-button {
    color: #5c6bc0;
}

.edit-button:hover {
    background-color: #ede7f6;
}

.delete-button {
    color: #e57373;
}

.delete-button:hover {
    background-color: #ffebee;
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
    background-color: #ffebee;
    border-bottom: none;
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
    font-size: 3rem;
    color: #f44336;
    margin-bottom: 1rem;
}

.warning-message {
    font-size: 1.1rem;
    margin-bottom: 0.75rem;
    color: #333;
}

.warning-details {
    color: #666;
    margin-bottom: 0;
    font-size: 0.9rem;
}

.modal-actions {
    padding: 1rem 1.5rem;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    border-top: 1px solid #eee;
}

.delete-confirm-button {
    padding: 0.6rem 1.5rem;
    background-color: #f44336;
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.delete-confirm-button:hover {
    background-color: #d32f2f;
}

.delete-confirm-button:disabled {
    background-color: #e57373;
    cursor: not-allowed;
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
.no-image {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
    font-size: 1.5rem;
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
