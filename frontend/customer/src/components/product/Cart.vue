<template>
    <div class="cart-container">
        <h2 class="cart-title">Giỏ hàng của bạn</h2>

        <div v-if="items.length > 0" class="cart-grid">
            <!-- Danh sách sản phẩm -->
            <div class="cart-items">
                <!-- Chọn tất cả -->
                <div class="select-all">
                    <input
                        type="checkbox"
                        v-model="selectAll"
                        @change="toggleSelectAll"
                    />
                    <label>Chọn tất cả</label>
                </div>

                <!-- Sản phẩm -->
                <div
                    class="cart-item"
                    v-for="item in items"
                    :key="item.id"
                    :class="{ 'disabled-item': !item.product.isActive }"
                >
                    <input
                        type="checkbox"
                        v-model="selectedItems"
                        :value="item.id"
                        :disabled="!item.product.isActive"
                    />
                    <img
                        :src="productService.getUrlImage(item.colorImage)"
                        alt="product"
                    />
                    <div class="item-info">
                        <h3>{{ item.product.name }}</h3>
                        <button style="cursor: auto">
                            {{ item.colorName }}
                        </button>
                        <!-- Hiển thị giá bán (đã giảm giá) -->
                        <p class="current-price">
                            {{ format.formatPrice(item.product.price) }}₫
                        </p>

                        <!-- Hiển thị giá gốc và phần trăm giảm giá nếu có -->
                        <div
                            v-if="
                                item.product.discount &&
                                item.product.price < item.product.salePrice
                            "
                            class="price-info"
                        >
                            <span class="original-price"
                                >{{
                                    format.formatPrice(item.product.salePrice)
                                }}₫</span
                            >
                            <span class="discount-badge">{{
                                item.product.discount
                            }}</span>
                        </div>

                        <div
                            class="quantity-control"
                            v-if="item.product.isActive"
                        >
                            <button @click="decreaseQuantity(item)">−</button>
                            <span>{{ item.quantity }}</span>
                            <button @click="increaseQuantity(item)">＋</button>
                        </div>
                        <div v-else class="quantity-control">
                            <button disabled>−</button>
                            <span>{{ item.quantity }}</span>
                            <button disabled>＋</button>
                        </div>
                    </div>
                    <button class="remove-btn" @click="removeItem(item.id)">
                        <i class="fa-solid fa-trash-can"></i>
                    </button>
                </div>
            </div>

            <!-- Tóm tắt đơn hàng -->
            <div class="cart-summary">
                <div class="summary-box">
                    <h3>Tóm tắt đơn hàng</h3>
                    <p>
                        <strong>Đã chọn:</strong> {{ selectedItems.length }} sản
                        phẩm
                    </p>
                    <p>
                        <strong>Tổng tiền:</strong>
                        {{ format.formatPrice(totalSelectedPrice) }}₫
                    </p>
                    <button
                        class="checkout-btn"
                        :disabled="selectedItems.length === 0"
                        @click="checkout"
                    >
                        Thanh toán
                    </button>
                </div>
            </div>
        </div>

        <div v-else class="cart-empty">
            <img src="../../../emty-cart.gif" alt="empty" />
            <p>Chưa có gì trong giỏ hàng!</p>
            <router-link to="/" class="shop-now-btn"
                >Mua sắm ngay thôi nào</router-link
            >
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import format from "@/utils/format.js";
import authService from "@/services/authService.js";
import productService from "../../services/productService.js";
import emitter from "../../utils/evenBus.js";

const router = useRouter();
const items = ref([]);
const selectedItems = ref([]);
const selectAll = ref(true);

const getCartItems = async () => {
    const res = await productService.getCarts();
    if (res && res.data && res.data.cartItems) {
        console.log("Cart Items:", res.data.cartItems);

        // Kiểm tra giá của từng sản phẩm
        res.data.cartItems.forEach((item) => {
            console.log(
                `Product: ${item.product.name}, Price: ${item.product.price}, SalePrice: ${item.product.salePrice}, Discount: ${item.product.discount}`
            );
        });

        items.value = res.data.cartItems;
    }
};

const totalSelectedPrice = computed(() => {
    return items.value
        .filter((item) => selectedItems.value.includes(item.id))
        .filter((item) => item.product.isActive)
        .reduce((sum, item) => sum + item.product.price * item.quantity, 0);
});

async function removeItem(id) {
    await productService.removeCartItem(id);
    emitter.emit("show-notification", {
        status: "success",
        message: "Đã xóa sản phẩm",
    });
    emitter.emit("update-cart-count");
    items.value = items.value.filter((item) => item.id !== id);
    selectedItems.value = selectedItems.value.filter((i) => i !== id);
}

function toggleSelectAll() {
    if (selectAll.value) {
        selectedItems.value = items.value
            .filter((item) => item.product.isActive)
            .map((item) => item.id);
    } else {
        selectedItems.value = [];
    }
}

watch(selectedItems, (val) => {
    selectAll.value = val.length === items.value.length;
});

async function increaseQuantity(item) {
    try {
        const availableQuantity = await productService.getQuantityProduct(
            item.productId,
            item.colorId
        );

        if (availableQuantity <= item.quantity) {
            emitter.emit("show-notification", {
                status: "error",
                message: "Đạt đến số lượng tối đa của sản phẩm này",
            });
            return;
        }
        item.quantity++;
        productService.updateCartItem(item.id, item.quantity);
    } catch (error) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể kiểm tra số lượng tồn kho",
        });
    }
}

function decreaseQuantity(item) {
    if (item.quantity === 1) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Số lượng sản phẩm tối thiểu là 1",
        });
    } else {
        item.quantity--;
        productService.updateCartItem(item.id, item.quantity);
    }
}
async function checkAuth() {
    try {
        const response = await authService.verifyUser();
        if (response?.data?.success) {
            return true;
        }

        console.log("Chưa đăng nhập, chuyển hướng...");
        await router.push({ name: "not-logged-in" });
        return false;
    } catch (error) {
        console.error("Lỗi khi kiểm tra xác thực:", error);
        await router.push({ name: "not-logged-in" });
        return false;
    }
}

async function checkout() {
    const isAuthen = await checkAuth();
    console.log("isAuthen", isAuthen);
    if (!isAuthen) return;
    try {
        // 1. Kiểm tra sản phẩm hợp lệ
        const hasInactiveItems = items.value.some(
            (item) =>
                selectedItems.value.includes(item.id) && !item.product.isActive
        );

        if (hasInactiveItems) {
            emitter.emit("show-notification", {
                status: "error",
                message: "Không thể thanh toán vì có sản phẩm không khả dụng",
            });
            return;
        }

        // 2. Chuẩn bị dữ liệu tối ưu
        const selectedItemsData = items.value
            .filter((item) => selectedItems.value.includes(item.id))
            .map((item) => ({
                productId: item.productId,
                colorId: item.colorId,
                quantity: item.quantity,
                colorName: item.colorName,
                imagePath: item.colorImage,
                productName: item.product.name,
                name: item.product.name,
                price: item.product.price,
                salePrice: item.product.salePrice,
                discount: item.product.discount,
                isActive: item.product.isActive,
            }));

        // 3. Kiểm tra số lượng tồn kho (option)
        const stockCheck = await Promise.all(
            selectedItemsData.map((item) =>
                productService.getQuantityProduct(item.productId, item.colorId)
            )
        );

        const outOfStockItems = selectedItemsData.filter(
            (item, index) => item.quantity > stockCheck[index]
        );

        if (outOfStockItems.length > 0) {
            emitter.emit("show-notification", {
                status: "error",
                message: `Sản phẩm ${outOfStockItems[0].name} vượt quá số lượng tồn kho`,
            });
            return;
        }

        // 4. Truyền dữ liệu
        router.push({
            name: "order",
            query: {
                direct: "true", // Đổi từ fromCart -> direct để đồng bộ với bên order
                products: encodeURIComponent(JSON.stringify(selectedItemsData)),
            },
        });
    } catch (error) {
        console.error("Lỗi khi thanh toán:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra khi xử lý đơn hàng",
        });
    }
}
// Kiểm tra xem giá phản ánh đúng chưa
function checkPriceIntegrity() {
    items.value.forEach((item) => {
        // Xem có tương đồng giữa giá hiển thị và thực tế không
        const hasDiscount = item.product.price < item.product.salePrice;
        console.log(
            `${item.product.name}: hasDiscount=${hasDiscount}, price=${item.product.price}, salePrice=${item.product.salePrice}`
        );

        if (item.product.discount && !hasDiscount) {
            console.warn(
                `Sản phẩm ${item.product.name} có discount=${item.product.discount} nhưng price=${item.product.price} không nhỏ hơn salePrice=${item.product.salePrice}`
            );
        }
    });
}

onMounted(async () => {
    await getCartItems();
    if (items.value) {
        selectedItems.value = items.value
            .filter((item) => item.product.isActive)
            .map((item) => item.id);
        checkPriceIntegrity();
    }
});
</script>

<style scoped>
.cart-container {
    max-width: 1200px;
    margin: 20px auto;
    padding: 32px 24px;
    background: #f8fafc;
    border-radius: 16px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
}

.cart-title {
    font-size: 32px;
    font-weight: 700;
    color: var(--primary-color);
    text-align: center;
    margin-bottom: 32px;
    position: relative;
    padding-bottom: 12px;
}

.cart-title::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    width: 120px;
    height: 3px;
    background: linear-gradient(
        90deg,
        var(--primary-color),
        var(--secondary-color)
    );
}

.cart-grid {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 28px;
}

/* Phần sản phẩm */
.cart-items {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.select-all {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 16px 20px;
    background: #ffffff;
    border-radius: 12px;
    font-weight: 600;
    color: #1f2937;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    margin-bottom: 8px;
}

.cart-item {
    display: flex;
    align-items: center;
    background: #ffffff;
    border-radius: 14px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
    padding: 20px;
    gap: 20px;
    transition: all 0.3s ease;
    border-left: 4px solid transparent;
}

.cart-item:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.1);
    border-left-color: var(--primary-color);
}

.cart-item img {
    width: 100px;
    height: 100px;
    object-fit: cover;
    border-radius: 10px;
    border: 1px solid #e5e7eb;
    transition: transform 0.3s ease;
}

.cart-item:hover img {
    transform: scale(1.03);
}

.item-info {
    flex: 1;
    min-width: 0; /* Fix overflow */
}

.item-info h3 {
    font-size: 18px;
    font-weight: 700;
    margin-bottom: 8px;
    color: #111827;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.item-info button {
    /* Style cho button màu sắc */
    padding: 6px 12px;
    border: 1px solid #e2e8f0;
    background-color: #f8fafc;
    border-radius: 20px; /* Pill shape */
    font-size: 13px;
    cursor: pointer;
    margin: 6px 0;
    color: #4b5563;
    transition: all 0.2s ease;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.item-info button:hover {
    background-color: #e2e8f0;
    color: #1f2937;
    transform: translateY(-1px);
}

.item-info p {
    margin: 8px 0;
    font-size: 18px;
    font-weight: 600;
    color: #333;
}

.item-info .current-price {
    margin: 8px 0;
    font-size: 18px;
    font-weight: 600;
    color: #ef4444;
}

.price-info {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-top: -4px;
    margin-bottom: 8px;
}

.original-price {
    font-size: 14px;
    color: #999;
    text-decoration: line-through;
}

.discount-badge {
    background-color: #ef4444;
    color: white;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 500;
}

.quantity-control {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-top: 12px;
}

.quantity-control button {
    width: 32px;
    height: 32px;
    font-size: 16px;
    font-weight: 600;
    background: #f1f5f9;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #4b5563;
}

.quantity-control button:hover {
    background: #e2e8f0;
    color: #1f2937;
    transform: scale(1.05);
}

.quantity-control span {
    min-width: 24px;
    text-align: center;
    font-weight: 600;
}

.remove-btn {
    background: none;
    border: none;
    color: #9ca3af;
    font-size: 20px;
    cursor: pointer;
    transition: all 0.3s ease;
    padding: 8px;
    border-radius: 50%;
}

.remove-btn:hover {
    color: #ef4444;
    background: #fee2e2;
    transform: rotate(15deg);
}

/* Phần thanh toán */
.cart-summary {
    background: #ffffff;
    border-radius: 16px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
    padding: 28px;
    height: fit-content;
    position: sticky;
    top: 100px;
    border: 1px solid #f1f5f9;
}

.summary-box h3 {
    font-size: 22px;
    margin-bottom: 20px;
    font-weight: 700;
    color: #111827;
    position: relative;
    padding-bottom: 12px;
}

.summary-box h3::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 0;
    width: 60px;
    height: 3px;
    background: var(--primary-color);
}

.summary-box p {
    font-size: 16px;
    margin-bottom: 12px;
    color: #4b5563;
    display: flex;
    justify-content: space-between;
}

.summary-box p strong {
    color: #1f2937;
}

.checkout-btn {
    margin-top: 24px;
    width: 100%;
    padding: 14px;
    background: linear-gradient(
        to right,
        var(--primary-color),
        var(--secondary-color)
    );
    color: white;
    border: none;
    border-radius: 12px;
    font-size: 17px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.checkout-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15);
    opacity: 0.9;
}

/* Giỏ hàng trống */
.cart-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 60px 20px;
    background-color: #ffffff;
    border-radius: 16px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
    color: #4b5563;
    text-align: center;
}

.cart-empty img {
    width: 280px;
    max-width: 90%;
    margin-bottom: 24px;
    display: block;
}

.cart-empty p {
    margin: 8px 0;
    font-size: 18px;
    color: #6b7280;
}

.cart-empty p:first-of-type {
    font-weight: 700;
    color: #1f2937;
    font-size: 22px;
    margin-bottom: 12px;
}

.shop-now-btn {
    margin-top: 24px;
    padding: 12px 28px;
    background: linear-gradient(
        to right,
        var(--primary-color),
        var(--secondary-color)
    );
    color: white;
    border-radius: 12px;
    font-weight: 600;
    text-decoration: none;
    transition: all 0.3s ease;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    font-size: 16px;
}

.shop-now-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15);
    opacity: 0.9;
}

/* Checkbox */
input[type="checkbox"] {
    appearance: none;
    width: 20px;
    height: 20px;
    border: 2px solid #d1d5db;
    border-radius: 6px;
    position: relative;
    cursor: pointer;
    transition: all 0.2s ease;
}

input[type="checkbox"]:checked {
    background-color: var(--primary-color);
    border-color: var(--primary-color);
}

input[type="checkbox"]:checked::before {
    content: "\2713";
    position: absolute;
    top: 1px;
    left: 5px;
    font-size: 14px;
    color: white;
    font-weight: bold;
}

/* Responsive */
@media (max-width: 768px) {
    .cart-grid {
        grid-template-columns: 1fr;
    }

    .cart-item {
        flex-direction: column;
        align-items: flex-start;
    }

    .cart-item img {
        width: 100%;
        height: auto;
        max-height: 200px;
    }

    .cart-summary {
        position: relative;
        top: unset;
    }

    .cart-title {
        font-size: 26px;
    }
}

@media (max-width: 480px) {
    .cart-container {
        padding: 24px 16px;
        margin: 20px auto;
    }

    .cart-title {
        font-size: 24px;
    }

    .select-all {
        padding: 12px 16px;
    }

    .cart-item {
        padding: 16px;
    }
}
/* Thêm vào phần style */
.disabled-item {
    position: relative;
    opacity: 0.5;
    pointer-events: none;
}

.disabled-item::after {
    content: "Sản phẩm không khả dụng";
    position: absolute;
    top: 80%;
    left: 70%;
    background-color: rgba(255, 0, 0, 0.8);
    color: white;
    padding: 5px 10px;
    border-radius: 4px;
    font-size: 14px;
    font-weight: bold;
    z-index: 20;
}

/* Cho phép nút xóa hoạt động ngay cả khi sản phẩm bị vô hiệu hóa */
.disabled-item .remove-btn {
    pointer-events: auto;
    position: relative;
    z-index: 20;
}
.checkout-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}
</style>
