<template>
    <Loading v-if="isLoading" />
    <div
        v-if="productData && productData.isActive"
        class="product-detail-container"
        :id="`product-detail-${productId}`"
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
                            <!-- Hiển thị xem đã đến ảnh nào trong tổng số ảnh - horizontal bars -->
                            <div
                                class="image-position-indicators"
                                v-if="getSelectedColorImages.length > 1"
                            >
                                <div
                                    v-for="(
                                        image, index
                                    ) in getSelectedColorImages"
                                    :key="image.id"
                                    class="image-dot"
                                    :class="{
                                        'active-dot':
                                            index === getCurrentImageIndex,
                                    }"
                                    @click="selectImageByIndex(index)"
                                ></div>
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
                        <label>Màu:</label>
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
                            <div class="quantity-label-container">
                                <label>Số lượng:</label>
                                <div class="stock-status-container">
                                    <span
                                        v-if="getQuanity > 0"
                                        class="stock-info"
                                        :class="{
                                            'stock-low': getQuanity <= 5,
                                            'stock-medium':
                                                getQuanity > 5 &&
                                                getQuanity <= 15,
                                            'stock-high': getQuanity > 15,
                                        }"
                                    >
                                        (Còn {{ getQuanity }} sản phẩm)
                                    </span>
                                    <span v-else class="out-of-stock-label">
                                        HẾT HÀNG
                                    </span>
                                </div>
                            </div>
                        </div>
                        <div class="quantity-controls-container">
                            <div class="quantity-control">
                                <button
                                    @click="decreaseQuantity"
                                    :disabled="getQuanity <= 0 || quantity <= 1"
                                    class="quantity-btn minus-btn"
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
                                    class="quantity-input"
                                />
                                <button
                                    @click="increaseQuantity"
                                    :disabled="
                                        getQuanity <= 0 ||
                                        quantity >= getQuanity
                                    "
                                    class="quantity-btn plus-btn"
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

        <!-- Tabbed Interface for product info -->
        <div class="product-tabs">
            <div class="tabs-header">
                <div
                    class="tab-button"
                    :class="{ 'active-tab': activeTab === 'description' }"
                    @click="activeTab = 'description'"
                >
                    <i class="fas fa-file-alt"></i> Mô tả sản phẩm
                </div>
                <div
                    class="tab-button"
                    :class="{ 'active-tab': activeTab === 'specs' }"
                    @click="activeTab = 'specs'"
                >
                    <i class="fas fa-cogs"></i> Thông số kỹ thuật
                </div>
                <div
                    class="tab-button"
                    :class="{ 'active-tab': activeTab === 'reviews' }"
                    @click="activeTab = 'reviews'"
                >
                    <i class="fas fa-star"></i> Đánh giá
                </div>
            </div>

            <!-- Tab Content -->
            <div class="tab-content">
                <!-- Description Tab -->
                <div
                    v-if="activeTab === 'description'"
                    class="product-description tab-pane"
                >
                    <div
                        v-if="productData.description"
                        class="description-content"
                    >
                        {{ productData.description }}
                    </div>
                    <div v-else class="no-content">
                        <i class="fas fa-info-circle"></i>
                        <p>Chưa có thông tin mô tả cho sản phẩm này</p>
                    </div>
                </div>

                <!-- Specifications Tab -->
                <div
                    v-if="activeTab === 'specs'"
                    class="product-specs tab-pane"
                >
                    <div class="specs-cards">
                        <!-- Bảo hành -->
                        <div
                            v-if="productData.detail.warranty"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-shield-alt"></i>
                            </div>
                            <div class="spec-info">
                                <span class="spec-name">Bảo hành</span>
                                <span class="spec-value"
                                    >{{
                                        productData.detail.warranty
                                    }}
                                    tháng</span
                                >
                            </div>
                        </div>

                        <!-- RAM -->
                        <div v-if="productData.detail.ram" class="spec-card">
                            <div class="spec-icon">
                                <i class="fas fa-memory"></i>
                            </div>
                            <div class="spec-info">
                                <span class="spec-name">RAM</span>
                                <span class="spec-value"
                                    >{{ productData.detail.ram }} GB</span
                                >
                            </div>
                        </div>

                        <!-- Bộ nhớ trong -->
                        <div
                            v-if="productData.detail.storage"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-hdd"></i>
                            </div>
                            <div class="spec-info">
                                <span class="spec-name">Bộ nhớ trong</span>
                                <span class="spec-value"
                                    >{{ productData.detail.storage }} GB</span
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
                            <div class="spec-info">
                                <span class="spec-name">Bộ xử lý</span>
                                <span class="spec-value">{{
                                    productData.detail.processor
                                }}</span>
                            </div>
                        </div>

                        <!-- Hệ điều hành -->
                        <div
                            v-if="productData.detail.operatingSystem"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-mobile-alt"></i>
                            </div>
                            <div class="spec-info">
                                <span class="spec-name">Hệ điều hành</span>
                                <span class="spec-value">{{
                                    productData.detail.operatingSystem
                                }}</span>
                            </div>
                        </div>

                        <!-- Màn hình -->
                        <div
                            v-if="productData.detail.screenSize"
                            class="spec-card"
                        >
                            <div class="spec-icon">
                                <i class="fas fa-tv"></i>
                            </div>
                            <div class="spec-info">
                                <span class="spec-name">Màn hình</span>
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
                            <div class="spec-info">
                                <span class="spec-name">Độ phân giải</span>
                                <span class="spec-value">{{
                                    productData.detail.screenResolution
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
                            <div class="spec-info">
                                <span class="spec-name">Pin</span>
                                <span class="spec-value"
                                    >{{ productData.detail.battery }} mAh</span
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
                            <div class="spec-info">
                                <span class="spec-name">Số khe SIM</span>
                                <span class="spec-value">{{
                                    productData.detail.simSlots
                                }}</span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Reviews Tab -->
                <div
                    v-if="activeTab === 'reviews'"
                    class="product-reviews tab-pane"
                >
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

// Define props to accept the id passed from the router
const props = defineProps({
    id: {
        type: [String, Number],
        required: true,
    },
});

const route = useRoute();
const router = useRouter();

const productId = props.id || route.params.id;
const productData = ref(null);
const selectedColorId = ref(null);
const selectedImgId = ref(null);
const currentImageId = ref(null);
const quantity = ref(1);
const isLoading = ref(true);
const activeTab = ref("description"); // For the tabbed interface

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

// Chọn ảnh bằng cách bấm vào điểm (dot)
const selectImageByIndex = (index) => {
    if (index >= 0 && index < getSelectedColorImages.value.length) {
        selectedImgId.value = getSelectedColorImages.value[index].id;
    }
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

const calculateDiscountPercentage = () => {
    if (!productData.value) return 0;
    return Math.round(
        ((productData.value.salePrice - productData.value.importPrice) /
            productData.value.importPrice) *
            100
    );
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
    margin-bottom: 40px;
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
    position: relative; /* Added for absolute positioning of dots */
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
    text-align: center;
    margin-bottom: 20px;
    font-size: 14px;
    color: #666;
    background-color: rgba(255, 255, 255, 0.7);
    border-radius: 15px;
    padding: 4px 12px;
    display: inline-block;
    margin: 0 auto 20px;
}

.image-position-indicators {
    position: absolute;
    bottom: 15px;
    left: 0;
    right: 0;
    display: flex;
    justify-content: center;
    gap: 6px;
    z-index: 5;
}

.image-dot {
    width: 20px;
    height: 4px;
    border-radius: 2px;
    background-color: rgba(255, 255, 255, 0.5);
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
    cursor: pointer;
    transition: all 0.3s ease;
}

.active-dot {
    background-color: var(--primary-color);
    width: 24px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
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
    color: #ef4444;
    margin-right: 10px;
}

.original-price {
    font-size: 18px;
    color: #999;
    text-decoration: line-through;
    margin-right: 10px;
}

.discount-badge {
    background-color: #ef4444;
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
    margin-bottom: 12px;
}

.quantity-label-container {
    display: flex;
    align-items: center;
}

.quantity-header label {
    font-weight: 500;
    font-size: 15px;
    color: #333;
    margin-right: 8px;
}

.stock-status-container {
    display: inline-flex;
}

.stock-info {
    font-size: 13px;
    opacity: 0.7;
}

.stock-high {
    color: #2ecc71;
}

.stock-medium {
    color: #f39c12;
}

.stock-low {
    color: #e74c3c;
    font-weight: 500;
}

.out-of-stock-label {
    font-size: 13px;
    font-weight: 600;
    color: #e74c3c;
    padding: 2px 8px;
    border-radius: 3px;
    background-color: rgba(231, 76, 60, 0.1);
    border: 1px solid rgba(231, 76, 60, 0.3);
    letter-spacing: 0.5px;
}

.quantity-controls-container {
    display: flex;
    align-items: flex-start;
}

.quantity-control {
    display: flex;
    align-items: center;
    border: 1px solid #eee;
    border-radius: 8px;
    overflow: hidden;
    width: 130px;
    height: 40px;
    background: white;
}

.quantity-btn {
    width: 42px;
    height: 100%;
    border: none;
    background: #f5f7fa;
    cursor: pointer;
    font-size: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #666;
    transition: all 0.15s ease;
}

.quantity-btn:hover:not(:disabled) {
    background-color: #eef1f6;
}

.minus-btn {
    border-right: 1px solid #eee;
}

.plus-btn {
    border-left: 1px solid #eee;
}

.quantity-input {
    width: 46px;
    height: 100%;
    text-align: center;
    border: none;
    font-size: 16px;
    font-weight: 400;
    color: #333;
    appearance: textfield;
    -moz-appearance: textfield;
}

.quantity-input::-webkit-outer-spin-button,
.quantity-input::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}

.quantity-input:focus {
    outline: none;
}

/* Action buttons */
.action-buttons {
    display: flex;
    gap: 15px;
    margin-bottom: 30px;
}

.cart-button-container {
    width: 60px;
}

.buy-button-container {
    flex: 1;
}

.add-to-cart-btn {
    width: 60px;
    height: 60px;
    background-color: white;
    border: 1px solid #fc7caf;
    color: var(--primary-color);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.3s;
}

.add-to-cart-btn i {
    font-size: 22px;
}

.add-to-cart-btn:hover:not(:disabled) {
    background-color: #ffefff;
}

.add-to-cart-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.buy-now-btn {
    width: 100%;
    height: 60px;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.3s;
}

.buy-now-text {
    font-size: 18px;
    font-weight: 600;
    letter-spacing: 1px;
}

.delivery-note {
    font-size: 12px;
    font-weight: normal;
    opacity: 0.9;
    margin-top: 2px;
}

.buy-now-btn:hover:not(:disabled) {
    background-color: var(--hover-color);
}

.buy-now-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

/* Product Tabs */
.product-tabs {
    margin-top: 30px;
    border: 1px solid #eee;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.tabs-header {
    display: flex;
    background-color: #f8f8f8;
    border-bottom: 1px solid #eee;
}

.tab-button {
    padding: 15px 20px;
    font-weight: 500;
    color: #666;
    cursor: pointer;
    transition: all 0.3s;
    display: flex;
    align-items: center;
    gap: 8px;
    position: relative;
}

.tab-button i {
    font-size: 14px;
}

.tab-button:hover {
    color: var(--primary-color);
    background-color: #f2f2f2;
}

.active-tab {
    color: var(--primary-color);
    font-weight: 600;
    border-bottom: 3px solid var(--primary-color);
}

.tab-content {
    padding: 20px;
}

.tab-pane {
    animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
    from {
        opacity: 0;
    }
    to {
        opacity: 1;
    }
}

.no-content {
    text-align: center;
    padding: 30px;
    color: #999;
    background-color: #f9f9f9;
    border-radius: 6px;
}

.no-content i {
    font-size: 24px;
    margin-bottom: 10px;
    color: #ccc;
}

/* Card-based Specs */
.specs-cards {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 15px;
}

.spec-card {
    display: flex;
    align-items: center;
    padding: 12px;
    border-radius: 8px;
    background-color: #f9f9f9;
    transition: all 0.3s;
    border: 1px solid #eee;
}

.spec-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.05);
    border-color: #ddd;
}

.spec-icon {
    width: 36px;
    height: 36px;
    background-color: var(--primary-color);
    color: white;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 12px;
    flex-shrink: 0;
}

.spec-icon i {
    font-size: 18px;
}

.spec-info {
    display: flex;
    flex-direction: column;
    flex: 1;
    overflow: hidden;
}

.spec-name {
    font-weight: 500;
    color: #666;
    font-size: 13px;
    margin-bottom: 3px;
}

.spec-value {
    font-size: 15px;
    color: #333;
}

/* Description Tab */
.description-content {
    line-height: 1.6;
    color: #444;
}

/* Product Reviews */
.product-reviews {
    margin-top: 0;
    padding-top: 0;
    border-top: none;
}

/* Responsive */
@media (max-width: 992px) {
    .specs-cards {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 768px) {
    .tabs-header {
        flex-wrap: wrap;
    }

    .tab-button {
        flex: 1;
        justify-content: center;
        padding: 12px 10px;
        font-size: 14px;
    }

    .specs-cards {
        grid-template-columns: 1fr;
    }

    .spec-card {
        padding: 12px;
    }

    .spec-icon {
        width: 36px;
        height: 36px;
    }
}

.product-reviews h3 {
    font-size: 20px;
    margin-bottom: 20px;
    color: #333;
    padding-bottom: 10px;
    border-bottom: 1px solid #eee;
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
    color: #ffc107;
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
    color: #ffc107;
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

.quantity-disabled .out-of-stock-label {
    opacity: 1;
}
</style>
