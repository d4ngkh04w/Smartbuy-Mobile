<template>
    <Loading v-if="isLoading" />
    <div
        v-if="productData && productData.isActive"
        class="product-detail-container"
    >
        <h2 class="product-title">{{ productData.name }}</h2>
        <div class="product-main-content">
            <!-- Phần ảnh sản phẩm -->
            <div class="product-image-section">
                <div class="product-image-slider">
                    <!-- Ảnh chính với nút chuyển ảnh -->
                    <div class="main-image-container">
                        <button
                            class="nav-btn prev-btn"
                            @click="navigateImages('prev')"
                            v-if="getSelectedColorImages.length > 1"
                        >
                            <i class="fas fa-chevron-left"></i>
                        </button>

                        <div class="main-image">
                            <img
                                :src="getCurrentMainImage"
                                :alt="productData.name"
                                class="active-image"
                                @click="openImagePreview(getCurrentMainImage)"
                            />
                            <!-- Hiển thị xem đã đến ảnh nào trong tổng số ảnh - positioned as overlay -->
                            <div
                                class="image-counter"
                                v-if="getSelectedColorImages.length > 1"
                            >
                                {{ getCurrentImageIndex + 1 }}/{{
                                    getSelectedColorImages.length
                                }}
                            </div>
                        </div>

                        <button
                            class="nav-btn next-btn"
                            @click="navigateImages('next')"
                            v-if="getSelectedColorImages.length > 1"
                        >
                            <i class="fas fa-chevron-right"></i>
                        </button>
                    </div>
                    <!-- Danh sách các màu - căn giữa -->
                    <div class="color-thumbnails-wrapper">
                        <div class="color-thumbnails">
                            <div
                                v-for="color in productData.colors"
                                :key="color.id"
                                class="color-thumbnail"
                                :class="{
                                    active: selectedColorId === color.id,
                                }"
                                @click="selectColor(color)"
                            >
                                <div class="color-image">
                                    <img
                                        :src="getMainImageForColor(color)"
                                        :alt="`${color.name}`"
                                    />
                                </div>
                                <span class="color-name">{{ color.name }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Phần thông tin mua hàng (ngang với ảnh) -->
            <div class="product-info-section">
                <div class="product-info-buy">
                    <!-- Thông tin giá -->
                    <div class="price-section">
                        <span class="current-price"
                            >{{ format.formatPrice(productData.price) }}₫</span
                        >
                        <span
                            v-if="productData.discount"
                            class="original-price"
                        >
                            {{ format.formatPrice(productData.salePrice) }}₫
                        </span>
                        <span
                            v-if="productData.discount"
                            class="discount-badge"
                        >
                            {{ productData.discount }}
                        </span>
                    </div>
                    <!-- Hiển thị các màu sắc có sẵn dưới dạng danh sách (3 màu/hàng) -->
                    <div
                        class="color-selection-list"
                        v-if="
                            productData.colors && productData.colors.length > 0
                        "
                    >
                        <label>Lựa chọn màu:</label>
                        <div class="color-list">
                            <div
                                v-for="color in productData.colors"
                                :key="color.id"
                                class="color-option"
                                :class="{
                                    'color-active':
                                        selectedColorId === color.id,
                                }"
                                @click="selectColor(color)"
                            >
                                <div class="color-option-image">
                                    <img
                                        :src="getMainImageForColor(color)"
                                        :alt="color.name"
                                    />
                                </div>
                                <span class="color-option-name">{{
                                    color.name
                                }}</span>
                                <!-- Đặt dấu tick ở ngoài viền ô màu -->
                                <div
                                    v-if="selectedColorId === color.id"
                                    class="color-check"
                                >
                                    <i class="fas fa-check"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- Chọn số lượng -->
                    <div
                        class="quantity-selector"
                        :class="{ 'quantity-disabled': getQuanity <= 0 }"
                    >
                        <div class="quantity-header">
                            <label>Nhập số lượng cần mua:</label>
                            <span
                                v-if="getQuanity > 0"
                                class="stock-info-simple"
                                :class="{
                                    'high-stock': getQuanity >= 20,
                                    'medium-stock':
                                        getQuanity < 20 && getQuanity > 4,
                                    'low-stock': getQuanity <= 4,
                                }"
                            >
                                (Còn {{ getQuanity }} sản phẩm)
                            </span>
                            <span
                                class="out-of-stock-label"
                                v-if="getQuanity <= 0"
                            >
                                <strong>Hết hàng</strong>
                            </span>
                        </div>
                        <div class="quantity-simple-layout">
                            <div class="quantity-control-simple">
                                <button
                                    @click="decreaseQuantity"
                                    :disabled="getQuanity <= 0 || quantity <= 1"
                                    class="quantity-btn-simple"
                                >
                                    −
                                </button>
                                <input
                                    type="text"
                                    v-model="quantity"
                                    min="1"
                                    :max="getQuanity"
                                    @focus="handleQuantityFocus"
                                    @blur="handleQuantityBlur"
                                    @input="validateQuantity"
                                    :disabled="getQuanity <= 0"
                                    class="quantity-input-simple"
                                />
                                <button
                                    @click="increaseQuantity"
                                    :disabled="
                                        getQuanity <= 0 ||
                                        quantity >= getQuanity
                                    "
                                    class="quantity-btn-simple"
                                >
                                    +
                                </button>
                            </div>
                        </div>
                    </div>
                    <!-- Nút hành động -->
                    <div
                        class="action-buttons"
                        :class="{ 'out-of-stock-disable': getQuanity <= 0 }"
                    >
                        <div class="cart-button-container">
                            <button
                                class="add-to-cart-btn"
                                @click="addToCart"
                                :disabled="getQuanity <= 0"
                            >
                                <i class="fas fa-shopping-cart"></i>
                            </button>
                        </div>
                        <div class="buy-button-container">
                            <button
                                class="buy-now-btn"
                                @click="buyNow"
                                :disabled="getQuanity <= 0"
                            >
                                <span class="buy-now-text">MUA NGAY</span>
                                <span class="delivery-note"
                                    >(Giao tận nhà hoặc nhận tại cửa hàng)</span
                                >
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Tabs cho Mô tả, Thông số kỹ thuật và Đánh giá -->
        <div class="product-details-tabs">
            <div class="tabs-header">
                <div
                    class="tab-button"
                    :class="{ 'active-tab': activeTab === 'description' }"
                    @click="activeTab = 'description'"
                >
                    Mô tả sản phẩm
                </div>
                <div
                    class="tab-button"
                    :class="{ 'active-tab': activeTab === 'specs' }"
                    @click="activeTab = 'specs'"
                >
                    Thông số kỹ thuật
                </div>
                <div
                    class="tab-button"
                    :class="{ 'active-tab': activeTab === 'reviews' }"
                    @click="activeTab = 'reviews'"
                >
                    Đánh giá sản phẩm
                </div>
            </div>

            <div class="tab-content">
                <!-- Mô tả sản phẩm tab -->
                <div v-if="activeTab === 'description'" class="tab-pane">
                    <div
                        class="product-description"
                        v-if="productData.description"
                    >
                        <div class="description-content">
                            {{ productData.description }}
                        </div>
                    </div>
                    <div v-else class="no-content">
                        <p>Chưa có thông tin mô tả cho sản phẩm này.</p>
                    </div>
                </div>
                <!-- Thông số kỹ thuật tab -->
                <div v-if="activeTab === 'specs'" class="tab-pane">
                    <div class="specs-grid">
                        <!-- Bộ nhớ trong -->
                        <div
                            v-if="productData.detail.storage"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-hdd"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Bộ nhớ trong</span>
                                <span class="spec-value"
                                    >{{ productData.detail.storage }} GB</span
                                >
                            </div>
                        </div>

                        <!-- Kích thước màn hình -->
                        <div
                            v-if="productData.detail.screenSize"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-mobile-alt"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name"
                                    >Kích thước màn hình</span
                                >
                                <span class="spec-value"
                                    >{{
                                        productData.detail.screenSize
                                    }}
                                    inch</span
                                >
                            </div>
                        </div>

                        <!-- Độ phân giải -->
                        <div
                            v-if="productData.detail.screenResolution"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-expand"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Độ phân giải</span>
                                <span class="spec-value">{{
                                    productData.detail.screenResolution
                                }}</span>
                            </div>
                        </div>

                        <!-- Hệ điều hành -->
                        <div
                            v-if="productData.detail.operatingSystem"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-cog"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Hệ điều hành</span>
                                <span class="spec-value">{{
                                    productData.detail.operatingSystem
                                }}</span>
                            </div>
                        </div>

                        <!-- Bảo hành -->
                        <div
                            v-if="productData.detail.warranty"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-shield-alt"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Bảo hành</span>
                                <span class="spec-value"
                                    >{{
                                        productData.detail.warranty
                                    }}
                                    tháng</span
                                >
                            </div>
                        </div>

                        <!-- Số SIM -->
                        <div
                            v-if="productData.detail.simSlots"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-sim-card"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Số khe SIM</span>
                                <span class="spec-value">{{
                                    productData.detail.simSlots
                                }}</span>
                            </div>
                        </div>

                        <!-- RAM -->
                        <div v-if="productData.detail.ram" class="spec-card">
                            <div class="spec-icon">
                                <i class="fas fa-memory"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">RAM</span>
                                <span class="spec-value"
                                    >{{ productData.detail.ram }} GB</span
                                >
                            </div>
                        </div>

                        <!-- CPU -->
                        <div
                            v-if="productData.detail.processor"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-microchip"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Bộ xử lý</span>
                                <span class="spec-value">{{
                                    productData.detail.processor
                                }}</span>
                            </div>
                        </div>

                        <!-- Dung lượng pin -->
                        <div
                            v-if="productData.detail.battery"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-battery-full"></i>
                            </div>
                            <div class="spec-content">
                                <span class="spec-name">Pin</span>
                                <span class="spec-value"
                                    >{{ productData.detail.battery }} mAh</span
                                >
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Đánh giá sản phẩm tab -->
                <div v-if="activeTab === 'reviews'" class="tab-pane">
                    <ProductComments
                        :productId="Number(productId)"
                        :isLoggedIn="isLoggedIn"
                    />
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { useRoute, useRouter } from "vue-router";
import { ref, onMounted, computed } from "vue";
import productService from "../../services/productService.js";
import authService from "../../services/authService.js";
import emitter from "../../utils/evenBus.js";
import format from "../../utils/format.js";
import Loading from "../common/Loading.vue";
import ProductComments from "./ProductComments.vue";

const route = useRoute();
const router = useRouter();

const productId = route.params.id;
const productData = ref(null);
const selectedColorId = ref(null);
const selectedImgId = ref(null);
const currentImageId = ref(null);
const quantity = ref(1);
const isLoading = ref(true);
const activeTab = ref("description"); // Default active tab

const fetchProduct = async (productId) => {
    isLoading.value = true;
    const data = await productService.getProductById(productId);

    if (data) {
        productData.value = data;
        if (!productData.value.isActive) {
            // Chuyển hướng đến trang 404 hoặc trang thông báo
            router.push({ name: "not-found" });
            return;
        }
        // Chọn màu đầu tiên làm mặc định nếu có
        if (data.colors && data.colors.length > 0) {
            selectedColorId.value = data.colors[0].id;
        }
    } else {
        console.error("Product not found");
    }
    isLoading.value = false;
};

const getAllImages = () => {
    if (!productData.value || !productData.value.colors) return [];

    // Get all images from all colors
    const allImages = productData.value.colors.flatMap((color) => color.images);

    // Sort to prioritize main images first
    return allImages.sort((a, b) => {
        if (a.isMain && !b.isMain) return -1;
        if (!a.isMain && b.isMain) return 1;
        return 0;
    });
};

const findColorIdByImageId = (colorsArray, imageId) => {
    const foundColor = colorsArray.find((color) =>
        color.images.some((img) => img.id === imageId)
    );
    return foundColor ? foundColor.id : null;
};

const getQuanity = computed(() => {
    const color = productData.value.colors.find(
        (color) => color.id === selectedColorId.value
    );
    if (color) {
        const quantity = color.quantity;
        return quantity ? quantity : 0;
    }
    return 0;
});

const getCurrentMainImage = computed(() => {
    if (!productData.value || !productData.value.colors) return "";

    selectedColorId.value = findColorIdByImageId(
        productData.value.colors,
        selectedImgId.value
    );

    // If no image is selected, find the first main image of the selected color
    if (selectedImgId.value === null) {
        const selectedColor = productData.value.colors.find(
            (color) => color.id === selectedColorId.value
        );

        if (selectedColor) {
            // Look for main image first
            const mainImage = selectedColor.images.find((img) => img.isMain);
            if (mainImage) {
                selectedImgId.value = mainImage.id;
            } else if (selectedColor.images.length > 0) {
                selectedImgId.value = selectedColor.images[0].id;
            }
        } else if (productData.value.colors.length > 0) {
            // If no color is selected, use the first color's main image
            const firstColor = productData.value.colors[0];
            const mainImage = firstColor.images.find((img) => img.isMain);
            selectedColorId.value = firstColor.id;
            if (mainImage) {
                selectedImgId.value = mainImage.id;
            } else if (firstColor.images.length > 0) {
                selectedImgId.value = firstColor.images[0].id;
            }
        }
    }

    const images = getAllImages();
    if (!images?.length) return "";

    const foundImage = images.find((img) => img.id === selectedImgId.value);
    if (!foundImage) return "";

    currentImageId.value = selectedImgId.value;

    return getImageUrl(foundImage.imagePath) || "";
});

// Lấy ảnh chính của một màu cụ thể
const getMainImageForColor = (color) => {
    if (!color || !color.images || color.images.length === 0) return "";

    // Tìm ảnh được đánh dấu là ảnh chính
    const mainImage = color.images.find((img) => img.isMain);
    if (mainImage) {
        return getImageUrl(mainImage.imagePath);
    }

    // Nếu không có ảnh chính, sử dụng ảnh đầu tiên
    return getImageUrl(color.images[0].imagePath);
};

// Lấy danh sách các ảnh của màu được chọn hiện tại
const getSelectedColorImages = computed(() => {
    if (!productData.value || !productData.value.colors) return [];

    const selectedColor = productData.value.colors.find(
        (color) => color.id === selectedColorId.value
    );
    if (!selectedColor) return [];

    return selectedColor.images;
});

// Lấy vị trí hiện tại của ảnh trong danh sách ảnh của màu đã chọn
const getCurrentImageIndex = computed(() => {
    if (!selectedImgId.value || getSelectedColorImages.value.length === 0)
        return 0;

    const index = getSelectedColorImages.value.findIndex(
        (img) => img.id === selectedImgId.value
    );
    return index >= 0 ? index : 0;
});

// Điều hướng giữa các ảnh
const navigateImages = (direction) => {
    if (getSelectedColorImages.value.length <= 1) return;

    const currentIndex = getCurrentImageIndex.value;
    let newIndex;

    if (direction === "next") {
        newIndex =
            currentIndex + 1 >= getSelectedColorImages.value.length
                ? 0
                : currentIndex + 1;
    } else {
        newIndex =
            currentIndex - 1 < 0
                ? getSelectedColorImages.value.length - 1
                : currentIndex - 1;
    }

    selectedImgId.value = getSelectedColorImages.value[newIndex].id;
};

// Chọn màu - cập nhật cả phần thumbnail và phần chọn màu
const selectColor = (color) => {
    selectedColorId.value = color.id;

    // Tìm ảnh chính cho màu này
    const mainImage = color.images.find((img) => img.isMain);
    if (mainImage) {
        selectedImgId.value = mainImage.id;
    } else if (color.images.length > 0) {
        // Nếu không có ảnh chính, dùng ảnh đầu tiên
        selectedImgId.value = color.images[0].id;
    }

    // Cuộn màn hình đến phần ảnh chính nếu đang ở mobile
    if (window.innerWidth < 768) {
        const imageSection = document.querySelector(".product-image-section");
        if (imageSection) {
            imageSection.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }
};

// Helper functions
const getImageUrl = (imgPath) => {
    return productService.getUrlImage(imgPath);
};

const handleQuantityFocus = (event) => {
    // Khi focus vào input, chọn toàn bộ nội dung để dễ dàng xóa
    event.target.select();

    // Nếu giá trị là 1 (mặc định), xóa nó để người dùng có thể nhập số mới
    if (quantity.value === 1) {
        quantity.value = "";
    }
};

const handleQuantityBlur = () => {
    // Khi blur khỏi input, nếu giá trị trống hoặc không hợp lệ, đặt lại thành 1
    if (!quantity.value || quantity.value < 1) {
        quantity.value = 1;
    }
};

const validateQuantity = (event) => {
    const value = parseInt(event.target.value);
    const maxQuantity = getQuanity.value;

    // Nếu giá trị không phải là số hoặc nhỏ hơn 1
    if (isNaN(value) || value < 1) {
        // Nếu người dùng đang xóa giá trị (nhập chuỗi rỗng), cho phép
        if (event.target.value === "") {
            quantity.value = "";
            return;
        }
        quantity.value = 1;
    }
    // Nếu giá trị lớn hơn số lượng tồn kho
    else if (value > maxQuantity) {
        quantity.value = maxQuantity;
        emitter.emit("show-notification", {
            status: "error",
            message: "Đạt đến số lượng tối đa",
        });
    }
    // Giá trị hợp lệ
    else {
        quantity.value = value;
    }
};
const increaseQuantity = () => {
    if (quantity.value < getQuanity.value) quantity.value++;
    else {
        emitter.emit("show-notification", {
            status: "error",
            message: "Đạt đến số lượng tối đa",
        });
    }
};

const decreaseQuantity = () => {
    if (quantity.value > 1) quantity.value--;
    else {
        emitter.emit("show-notification", {
            status: "error",
            message: "Đạt đến số lượng tối thiểu",
        });
    }
};

const setQuantity = (value) => {
    // Đảm bảo giá trị trong phạm vi hợp lệ
    if (value >= 1 && value <= getQuanity.value) {
        quantity.value = value;
    }
};

const addToCart = async () => {
    try {
        const isAuthen = await checkAuth();
        if (!isAuthen) {
            return;
        }

        if (checkValidInfor()) {
            const res = await productService.addToCart(
                productId,
                quantity.value,
                selectedColorId.value
            );
            emitter.emit("show-notification", {
                status: "success",
                message: "Đã thêm vào giỏ hàng!",
            });
            emitter.emit("cart-updated");
        }
    } catch (error) {
        console.error("Lỗi khi thêm vào giỏ hàng:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra khi thêm vào giỏ hàng",
        });
    }
};

async function checkAuth() {
    try {
        const response = await authService.verifyUser();
        // Kiểm tra response kỹ hơn
        if (response?.data?.success) {
            return true;
        }

        // Thêm console.log để debug
        console.log("Chưa đăng nhập, chuyển hướng...");
        await router.push({ name: "not-logged-in" });
        return false;
    } catch (error) {
        console.error("Lỗi khi kiểm tra xác thực:", error);
        await router.push({ name: "not-logged-in" });
        return false;
    }
}

const checkValidInfor = () => {
    if (productData.value.quantity <= quantity.value) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Sản phẩm không đáp ứng đủ số lượng",
        });
        return false;
    }
    if (selectedColorId.value === null) {
        emitter.emit("show-notification", {
            status: "warning",
            message: "Vui lòng chọn màu sắc",
        });
        return false;
    }
    return true;
};
console.log("Product:", productService.getProductById(productId));

const buyNow = async () => {
    const isAuthen = await checkAuth();
    if (isAuthen && checkValidInfor()) {
        const selectedColor = productData.value.colors.find(
            (c) => c.id === selectedColorId.value
        );

        // Tạo object sản phẩm
        const product = {
            productId: productId,
            colorId: selectedColorId.value,
            productName: productData.value.name,
            salePrice: productData.value.salePrice,
            quantity: quantity.value,
            colorName: selectedColor?.name || "",
            imagePath: selectedColor?.images[0]?.imagePath || "",
        };

        // Chuyển thành JSON string rồi encode
        const productsStr = encodeURIComponent(JSON.stringify([product]));

        router.push({
            name: "order",
            query: {
                direct: "true",
                products: productsStr,
            },
        });
    }
};

onMounted(async () => {
    await fetchProduct(productId);
    document.title = `${productData.value.name} - SmartBuy Mobile`;

    if (productData.value?.colors?.length > 0 && !selectedColorId.value) {
        const firstColor = productData.value.colors[0];
        selectedColorId.value = firstColor.id;

        // Find main image in first color
        const mainImage = firstColor.images.find((img) => img.isMain);

        if (mainImage) {
            selectedImgId.value = mainImage.id;
        } else if (firstColor.images.length > 0) {
            selectedImgId.value = firstColor.images[0].id;
        }
    }
});
// User authentication status
const isLoggedIn = computed(() => localStorage.getItem("isLogin") !== null);

// Lấy tên màu đã chọn
const getSelectedColorName = computed(() => {
    if (!productData.value || !productData.value.colors) return "";

    const selectedColor = productData.value.colors.find(
        (color) => color.id === selectedColorId.value
    );
    return selectedColor ? selectedColor.name : "";
});

// Xem ảnh lớn
const openImagePreview = (imgUrl) => {
    // Hiển thị ảnh lớn hoặc lightbox khi click vào ảnh
    // Có thể triển khai lightbox ở đây, hiện tại chỉ log
    console.log("Xem ảnh:", imgUrl);

    // Trong tương lai có thể thêm một modal để xem ảnh lớn
    // hoặc sử dụng thư viện như Lightbox, Fancybox, etc.
};

// Cuộn đến form đánh giá
const scrollToReviewForm = () => {
    const element = document.querySelector(".comments-section");
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "start" });
    }
};
</script>

<style scoped>
.product-detail-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

.product-title {
    font-size: 24px;
    margin-bottom: 20px;
    color: #333;
}

.product-main-content {
    display: flex;
    gap: 40px;
    margin-bottom: 20px;
}

.product-image-section {
    flex: 1;
}

.product-info-section {
    flex: 1;
    max-width: 400px;
}

/* Slider styles */
.product-image-slider {
    margin-bottom: 30px;
}

.main-image-container {
    position: relative;
    width: 100%;
    display: flex;
    align-items: center;
    margin-bottom: 15px;
}

.main-image {
    width: 100%;
    height: 420px; /* Giảm chiều cao từ 460px xuống 420px */
    border: 1px solid #eee;
    border-radius: 16px; /* Tăng border-radius để trông mềm mại hơn */
    overflow: hidden;
    display: flex;
    justify-content: center;
    align-items: center;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    position: relative; /* Added position relative for absolute positioned children */
}

.main-image img {
    width: 100%;
    height: 100%;
    object-fit: contain;
    transition: transform 0.3s ease;
}

.main-image img:hover {
    transform: scale(1.02);
}

.nav-btn {
    position: absolute;
    width: 36px; /* Giảm từ 40px xuống 36px */
    height: 36px; /* Giảm từ 40px xuống 36px */
    background: rgba(255, 255, 255, 0.9);
    border: 1px solid #eee;
    border-radius: 50%;
    display: flex;
    justify-content: center;
    align-items: center;
    cursor: pointer;
    z-index: 5;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.nav-btn:hover {
    background: white;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    transform: translateY(-2px);
}

.prev-btn {
    left: 10px;
}

.next-btn {
    right: 10px;
}

.image-counter {
    position: absolute;
    bottom: 10px;
    left: 50%;
    transform: translateX(-50%);
    font-size: 14px;
    color: #ffffff;
    background-color: rgba(0, 0, 0, 0.5);
    border-radius: 15px;
    padding: 4px 12px;
    z-index: 5;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
    display: inline-block;
    margin: 0 auto;
}

/* Color thumbnails */
.color-thumbnails-wrapper {
    display: flex;
    justify-content: center;
    margin-top: 20px;
}

.color-thumbnails {
    display: flex;
    flex-wrap: wrap;
    gap: 12px; /* Giảm khoảng cách từ 15px xuống 12px */
    justify-content: center;
}

.color-thumbnail {
    width: 75px; /* Giảm từ 85px xuống 75px */
    display: flex;
    flex-direction: column;
    align-items: center;
    cursor: pointer;
    transition: all 0.3s ease;
    position: relative;
}

.color-thumbnail:hover {
    transform: translateY(-3px);
}

.color-thumbnail.active .color-image {
    border-color: var(--primary-color);
    border-width: 2px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.color-thumbnail.active::after {
    content: "";
    position: absolute;
    top: 3px; /* Giảm từ 5px xuống 3px */
    right: 3px; /* Giảm từ 5px xuống 3px */
    width: 14px; /* Giảm từ 16px xuống 14px */
    height: 14px; /* Giảm từ 16px xuống 14px */
    background-color: var(--primary-color);
    border-radius: 50%;
    display: flex;
    justify-content: center;
    align-items: center;
    color: white;
    font-size: 9px; /* Giảm từ 10px xuống 9px */
    z-index: 2;
}

.color-thumbnail.active::before {
    content: "✓";
    position: absolute;
    top: 3px; /* Giảm từ 5px xuống 3px */
    right: 3px; /* Giảm từ 5px xuống 3px */
    width: 14px; /* Giảm từ 16px xuống 14px */
    height: 14px; /* Giảm từ 16px xuống 14px */
    display: flex;
    justify-content: center;
    align-items: center;
    color: white;
    font-size: 9px; /* Giảm từ 10px xuống 9px */
    z-index: 3;
}

.color-image {
    width: 60px; /* Giảm từ 70px xuống 60px */
    height: 60px; /* Giảm từ 70px xuống 60px */
    border: 1px solid #ddd;
    border-radius: 10px; /* Giảm border-radius từ 12px xuống 10px */
    overflow: hidden;
    margin-bottom: 5px;
    transition: all 0.2s ease;
}

.color-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 10px; /* Cũng tăng border-radius cho ảnh bên trong */
}

.color-name {
    font-size: 11px; /* Giảm từ 12px xuống 11px */
    text-align: center;
    color: #555;
    margin-top: 4px; /* Thêm margin-top để cân đối */
}

/* Product info styles */
.product-info-buy {
    position: sticky;
    top: 20px;
    padding: 20px;
    border: 1px solid #eee;
    border-radius: 8px;
    background: #fff;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.price-section {
    margin-bottom: 20px;
}

.current-price {
    font-size: 28px;
    font-weight: bold;
    color: var(--primary-color);
    margin-right: 10px;
}

.original-price {
    font-size: 18px;
    color: #999;
    text-decoration: line-through;
    margin-right: 10px;
}

.discount-badge {
    background-color: var(--primary-color);
    color: white;
    padding: 3px 8px;
    border-radius: 4px;
    font-size: 14px;
}

/* Color info instead of selector */
.color-info {
    margin-bottom: 20px;
}

.color-info label {
    display: block;
    margin-bottom: 8px;
    font-weight: 500;
}

.selected-color {
    display: flex;
    align-items: center;
}

.color-badge {
    background-color: #f8f8f8;
    border: 1px solid #ddd;
    border-radius: 20px; /* Giữ nguyên border-radius lớn cho badge */
    padding: 5px 15px;
    font-size: 14px;
    color: #333;
    display: inline-block;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

/* Color selector */
.color-selection-list {
    margin-bottom: 20px;
}

.color-selection-list label {
    display: block;
    margin-bottom: 12px;
    font-weight: 500;
    font-size: 15px;
    margin-left: 2px; /* Thêm chút margin bên trái */
}

.color-list {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
    justify-content: flex-start; /* Căn đều các phần tử từ trái sang */
}

.color-option {
    width: calc(33.33% - 8px); /* Thay đổi thành 3 phần tử trên một hàng */
    border: 1.5px solid #e0e0e0; /* Tăng độ dày viền ngoài */
    border-radius: 10px;
    padding: 10px;
    cursor: pointer;
    transition: all 0.3s ease;
    display: flex;
    flex-direction: column; /* Chuyển sang flexbox chiều dọc */
    align-items: center; /* Căn giữa các phần tử */
    position: relative;
    overflow: visible; /* Cho phép dấu tick hiển thị ra ngoài */
    margin-bottom: 10px; /* Thêm khoảng cách giữa các hàng */
}

.color-option:hover {
    border-color: #ccc;
    transform: translateY(-2px);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.05);
}

.color-option.color-active {
    border-color: var(--primary-color);
    border-width: 2px; /* Tăng độ dày của viền khi ô được chọn */
    background-color: rgba(247, 234, 247, 0.3);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.1);
}

.color-option-image {
    width: 38px;
    height: 38px;
    border-radius: 8px;
    overflow: hidden;
    position: relative;
    border: none; /* Bỏ viền ảnh */
    transition: all 0.3s ease;
    margin-bottom: 5px; /* Thêm khoảng cách với tên màu */
}

.color-option.color-active .color-option-image {
    border-color: var(--primary-color);
}

.color-option-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.color-option-name {
    font-size: 12px;
    color: #333;
    text-align: center; /* Căn giữa tên màu */
    white-space: nowrap; /* Không cho phép xuống dòng */
    overflow: hidden; /* Ẩn phần tràn ra */
    text-overflow: ellipsis; /* Thêm dấu ... nếu tên quá dài */
    max-width: 100%; /* Giới hạn độ rộng tối đa */
}

.color-check {
    position: absolute;
    top: -6px; /* Di chuyển lên trên viền */
    right: -6px; /* Di chuyển sang phải viền */
    background-color: var(--primary-color);
    color: white;
    width: 16px;
    height: 16px;
    border-radius: 50%; /* Làm tròn đầy đủ */
    display: flex;
    justify-content: center;
    align-items: center;
    font-size: 9px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2); /* Thêm bóng đổ nhẹ */
    border: 1px solid white; /* Thêm viền trắng */
}

/* Quantity selector */
.quantity-selector {
    margin-bottom: 20px;
}

.quantity-header {
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
}

.quantity-header label {
    font-weight: 500;
    font-size: 14px;
    color: #333;
}

.quantity-simple-layout {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.stock-info-simple {
    font-size: 13px;
    margin-left: 8px;
    font-weight: normal;
    opacity: 0.7; /* Làm cho thông tin Còn X sản phẩm mờ đi còn 70% */
}

.stock-info-simple.high-stock {
    color: #34c759; /* Màu xanh lá cho số lượng từ 20 trở lên */
}

.stock-info-simple.medium-stock {
    color: #ff9500; /* Màu vàng/cam cho số lượng từ 5 đến 19 */
}

.stock-info-simple.low-stock {
    color: #ff3b30; /* Màu đỏ cho số lượng dưới hoặc bằng 4 */
}

.out-of-stock-label {
    margin-left: 8px;
    padding: 2px 8px;
    background-color: #ffecec;
    border: 1px solid #ffdbdb;
    border-radius: 4px;
    color: #ff3b30;
    font-size: 13px;
}

.quantity-control-simple {
    display: inline-flex;
    align-items: center;
    border: 1px solid #e0e0e0;
    border-radius: 4px;
    overflow: hidden;
    width: 110px;
    height: 32px;
    background: white;
}

.quantity-btn-simple {
    width: 30px;
    height: 100%;
    border: none;
    background: #fafafa;
    cursor: pointer;
    font-size: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #666;
    transition: background 0.2s;
    padding: 0;
}

.quantity-btn-simple:hover:not(:disabled) {
    background-color: #f0f0f0;
}

.quantity-btn-simple:first-child {
    border-right: 1px solid #e0e0e0;
}

.quantity-btn-simple:last-child {
    border-left: 1px solid #e0e0e0;
}

.quantity-input-simple {
    width: 50px;
    height: 100%;
    text-align: center;
    border: none;
    font-size: 14px;
    font-weight: 500;
    color: #333;
    appearance: textfield;
    -moz-appearance: textfield;
    padding: 0;
}

.quantity-input-simple::-webkit-outer-spin-button,
.quantity-input-simple::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}

.quantity-input:focus {
    outline: none;
}

/* Xóa đoạn CSS này vì đã bỏ phần thông báo hết hàng ở dưới */

/* Action buttons */
.action-buttons {
    display: flex;
    gap: 15px;
    margin-bottom: 30px;
}

.cart-button-container {
    width: 56px;
}

.buy-button-container {
    flex: 1;
    display: flex;
    align-items: center;
}

.add-to-cart-btn,
.buy-now-btn {
    border: none;
    cursor: pointer;
    transition: all 0.2s;
    width: 100%;
    height: 56px;
}

.add-to-cart-btn {
    background-color: var(--primary-color);
    color: white;
    font-size: 20px;
    border-radius: 8px;
    display: flex;
    justify-content: center;
    align-items: center;
}

.add-to-cart-btn:hover:not(:disabled) {
    filter: brightness(0.95);
}

.add-to-cart-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.buy-now-btn {
    background-color: var(--primary-color);
    color: white;
    border-radius: 8px;
    padding: 0;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}

.buy-now-text {
    font-size: 18px;
    font-weight: 700;
    letter-spacing: 0.5px;
    line-height: 1.2;
}

.delivery-note {
    font-size: 12px;
    color: #ffffff;
    opacity: 0.8;
    text-align: center;
    padding-top: 1px;
}

.buy-now-btn:hover:not(:disabled) {
    filter: brightness(0.95);
}

.buy-now-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

/* Product specs */
/* Product details tabs */
.product-details-tabs {
    margin-top: 30px;
    border: 1px solid #eee;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.tabs-header {
    display: flex;
    background-color: #f9f9f9;
    border-bottom: 1px solid #eee;
}

.tab-button {
    padding: 15px 25px;
    font-size: 15px;
    font-weight: 500;
    color: #666;
    cursor: pointer;
    transition: all 0.3s ease;
    border-bottom: 3px solid transparent;
    text-align: center;
    flex: 1;
}

.tab-button:hover {
    color: var(--primary-color);
    background-color: #f5f5f5;
}

.tab-button.active-tab {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
    background-color: white;
}

.tab-content {
    padding: 25px;
    background-color: white;
}

.tab-pane {
    animation: fadeIn 0.5s ease;
}

.description-content {
    line-height: 1.6;
    color: #444;
}

.no-content {
    text-align: center;
    padding: 30px;
    color: #888;
    background-color: #f9f9f9;
    border-radius: 8px;
}

/* Specs section */
.specs-title {
    font-size: 18px;
    font-weight: 600;
    color: #333;
    margin-bottom: 20px;
}

.specs-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 16px;
    padding: 5px;
}

.spec-card {
    display: flex;
    padding: 16px;
    border-radius: 8px;
    background-color: #f9f9f9;
    border: 1px solid #eee;
    transition: all 0.2s ease;
}

.spec-card:hover {
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.05);
}

.spec-icon {
    display: flex;
    align-items: flex-start;
    justify-content: center;
    margin-right: 12px;
    color: #666;
    font-size: 18px;
    width: 24px;
}

.spec-content {
    display: flex;
    flex-direction: column;
}

.spec-name {
    font-weight: 400;
    color: #666;
    font-size: 14px;
    margin-bottom: 5px;
}

.spec-value {
    font-size: 16px;
    color: #333;
    font-weight: 500;
}

/* Responsive specs grid */
@media (max-width: 992px) {
    .specs-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 576px) {
    .specs-grid {
        grid-template-columns: 1fr;
    }
}

@keyframes fadeIn {
    from {
        opacity: 0;
    }
    to {
        opacity: 1;
    }
}

/* Khi chưa có đánh giá */
.no-reviews {
    text-align: center;
    padding: 40px 20px;
    background-color: #f9f9f9;
    border-radius: 8px;
    color: #666;
}

.no-reviews i {
    font-size: 40px;
    color: #ccc;
    margin-bottom: 15px;
}

.no-reviews p {
    margin-bottom: 20px;
    font-size: 16px;
}

.btn-write-review {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.3s;
}

.btn-write-review:hover {
    background-color: var(--hover-color);
}

.btn-write-review i {
    font-size: 14px;
    margin-right: 5px;
}

/* Khi có đánh giá */
.reviews-summary {
    display: flex;
    gap: 40px;
    margin-bottom: 30px;
    padding: 20px;
    background-color: #f9f9f9;
    border-radius: 8px;
}

.average-rating {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 150px;
}

.rating-number {
    font-size: 36px;
    font-weight: bold;
    color: var(--primary-color);
}

.stars {
    margin: 5px 0;
    color: #ff8fcf; /* Lighter pink color that still stands out for stars */
}

.total-reviews {
    font-size: 14px;
    color: #666;
}

.rating-bars {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.rating-bar {
    display: flex;
    align-items: center;
    gap: 10px;
}

.star-label {
    width: 60px;
    font-size: 14px;
    color: #666;
}

.bar-container {
    flex: 1;
    height: 8px;
    background-color: #e0e0e0;
    border-radius: 4px;
    overflow: hidden;
}

.bar-fill {
    height: 100%;
    background-color: var(--primary-color);
    border-radius: 4px;
}

.percentage {
    width: 40px;
    font-size: 14px;
    color: #666;
}

/* Danh sách đánh giá */
.reviews-list {
    display: flex;
    flex-direction: column;
    gap: 25px;
}

.review-item {
    padding: 20px;
    border: 1px solid #eee;
    border-radius: 8px;
}

.review-header {
    display: flex;
    align-items: center;
    gap: 15px;
    margin-bottom: 15px;
}

.review-avatar {
    width: 50px;
    height: 50px;
    border-radius: 50%;
    object-fit: cover;
}

.review-user {
    flex: 1;
}

.user-name {
    font-weight: 500;
    display: block;
    margin-bottom: 3px;
}

.review-rating {
    color: #ff8fcf; /* Lighter pink color for stars, matching the other star ratings */
}

.review-date {
    font-size: 14px;
    color: #999;
}

.review-content {
    margin-bottom: 15px;
    line-height: 1.6;
}

.review-images {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
}

.review-images img {
    width: 80px;
    height: 80px;
    object-fit: cover;
    border-radius: 4px;
    cursor: pointer;
    transition: transform 0.3s;
}

.review-images img:hover {
    transform: scale(1.05);
}

/* Responsive */
@media (max-width: 768px) {
    .product-main-content {
        flex-direction: column;
    }

    .product-info-section {
        max-width: 100%;
    }

    .main-image {
        height: 300px;
    }

    .specs-grid {
        grid-template-columns: 1fr;
    }

    .product-info-buy {
        position: static;
    }

    .reviews-summary {
        flex-direction: column;
        gap: 20px;
    }

    .average-rating {
        flex-direction: row;
        justify-content: center;
        gap: 15px;
        align-items: center;
    }

    .rating-number {
        font-size: 28px;
    }

    .review-header {
        flex-wrap: wrap;
    }

    .review-date {
        width: 100%;
        margin-top: 5px;
        padding-left: 65px;
    }
}

.out-of-stock-disable {
    opacity: 0.7;
}

.quantity-disabled input,
.quantity-disabled button {
    background-color: #f5f5f5;
    cursor: not-allowed;
}

.quantity-disabled .quantity-control {
    opacity: 0.7;
}
</style>
