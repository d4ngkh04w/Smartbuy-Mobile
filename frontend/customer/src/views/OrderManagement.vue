<template>
  <div class="order-management">
    <h2 class="page-title">Đơn hàng của bạn</h2>

    <div v-if="loading" class="loading-state">
      <i class="fas fa-spinner fa-spin"></i> Đang tải đơn hàng...
    </div>

    <div v-else-if="error" class="error-state">
      <i class="fas fa-exclamation-circle"></i> Đã xảy ra lỗi khi tải đơn hàng. Vui lòng thử lại sau.
    </div>

    <div v-else-if="orders.length === 0" class="empty-state">
      <i class="fas fa-box-open"></i> Bạn chưa có đơn hàng nào đang chờ xử lý.
    </div>

    <div v-else class="order-list">
      <div v-for="order in orders" :key="order.id" class="order-card">
        <div class="order-header">
          <span class="order-id">Mã ĐH: #{{ order.id }}</span>
          <span :class="['order-status', getStatusClass(order.status)]">
            {{ getStatusText(order.status) }}
          </span>
        </div>
        <div class="order-body">
          <div class="product-summary">
            <div v-for="item in order.items.slice(0, 2)" :key="item.productId" class="product-item-preview">
              <img :src="item.image" :alt="item.name" class="product-thumbnail" />
              <div class="product-info-preview">
                <p class="product-name">{{ item.name }}</p>
                <p class="product-quantity">x{{ item.quantity }}</p>
              </div>
            </div>
            <p v-if="order.items.length > 2" class="more-items">
              và {{ order.items.length - 2 }} sản phẩm khác...
            </p>
          </div>
          <div class="order-details">
            <p class="order-date">Ngày đặt: {{ formatDate(order.orderDate) }}</p>
            <p class="order-total">Tổng tiền: {{ formatCurrency(order.totalAmount) }}</p>
          </div>
        </div>
        <div class="order-actions">
          <button @click="showPopup(order)" class="btn btn-primary">Xem chi tiết</button>
          <button v-if="canCancel(order.status)" @click="cancelOrder(order.id)" class="btn btn-secondary">Hủy đơn hàng</button>
          <button v-if="canContact(order.status)" @click="contactSupport(order.id)" class="btn btn-info">Liên hệ hỗ trợ</button>
        </div>
      </div>
    </div>

    <!-- Popup chi tiết đơn hàng -->
    <div v-if="popupVisible" class="popup-overlay" @click.self="closePopup">
      <div class="popup-content">
        <h3>Chi tiết đơn hàng #{{ selectedOrder.id }}</h3>
        <p><strong>Ngày đặt:</strong> {{ formatDate(selectedOrder.orderDate) }}</p>
        <p><strong>Trạng thái:</strong> {{ getStatusText(selectedOrder.status) }}</p>
        <p><strong>Tổng tiền:</strong> {{ formatCurrency(selectedOrder.totalAmount) }}</p>

        <h4>Sản phẩm:</h4>
        <ul class="popup-product-list">
          <li v-for="item in selectedOrder.items" :key="item.productId" class="popup-product-item">
            <img :src="item.image" :alt="item.name" class="popup-product-thumbnail" />
            <div>
              <p class="popup-product-name">{{ item.name }}</p>
              <p>Số lượng: {{ item.quantity }}</p>
              <p>Giá: {{ formatCurrency(item.price) }}</p>
            </div>
          </li>
        </ul>

        <button @click="closePopup" class="btn btn-close">Đóng</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const orders = ref([]);
const loading = ref(true);
const error = ref(false);

const popupVisible = ref(false);
const selectedOrder = ref(null);

// Dữ liệu giả lập đơn hàng (giữ nguyên như bạn có)
const mockOrders = [
  {
    id: 'DH001',
    orderDate: '2025-05-20T10:30:00Z',
    totalAmount: 15000000,
    status: 'processing',
    items: [
      { productId: 'SP001', name: 'iPhone 15 Pro Max 256GB', quantity: 1, price: 15000000, image: 'https://via.placeholder.com/60?text=iPhone' },
    ],
  },
  {
    id: 'DH002',
    orderDate: '2025-05-18T14:45:00Z',
    totalAmount: 2500000,
    status: 'shipping',
    items: [
      { productId: 'SP003', name: 'Tai nghe Bluetooth Sony WF-1000XM5', quantity: 1, price: 2500000, image: 'https://via.placeholder.com/60?text=Headphone' },
    ],
  },
  {
    id: 'DH003',
    orderDate: '2025-05-15T09:00:00Z',
    totalAmount: 1800000,
    status: 'pending',
    items: [
      { productId: 'SP004', name: 'Sạc dự phòng Anker PowerCore', quantity: 2, price: 900000, image: 'https://via.placeholder.com/60?text=Charger' },
      { productId: 'SP004', name: 'Sạc dự phòng Anker PowerCore', quantity: 2, price: 900000, image: 'https://via.placeholder.com/60?text=Charger' },
      { productId: 'SP004', name: 'Sạc dự phòng Anker PowerCore', quantity: 2, price: 900000, image: 'https://via.placeholder.com/60?text=Charger' },
    ],
  },
];

const fetchOrders = async () => {
  loading.value = true;
  error.value = false;
  try {
    await new Promise(resolve => setTimeout(resolve, 1000));
    orders.value = mockOrders.filter(order => order.status !== 'delivered');
  } catch (err) {
    console.error("Failed to fetch orders:", err);
    error.value = true;
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateString) => {
  const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
  return new Date(dateString).toLocaleDateString('vi-VN', options);
};

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const getStatusText = (status) => {
  switch (status) {
    case 'pending': return 'Chờ xác nhận';
    case 'processing': return 'Đang xử lý';
    case 'shipping': return 'Đang vận chuyển';
    case 'delivered': return 'Đã giao hàng';
    case 'cancelled': return 'Đã hủy';
    default: return 'Không rõ';
  }
};

const getStatusClass = (status) => {
  switch (status) {
    case 'pending': return 'status-pending';
    case 'processing': return 'status-processing';
    case 'shipping': return 'status-shipping';
    case 'delivered': return 'status-delivered';
    case 'cancelled': return 'status-cancelled';
    default: return '';
  }
};

const canCancel = (status) => ['pending', 'processing'].includes(status);

const canContact = (status) => ['shipping', 'pending', 'processing'].includes(status);

const showPopup = (order) => {
  selectedOrder.value = order;
  popupVisible.value = true;
};

const closePopup = () => {
  popupVisible.value = false;
  selectedOrder.value = null;
};

const cancelOrder = (orderId) => {
  if (confirm(`Bạn có chắc chắn muốn hủy đơn hàng ${orderId}?`)) {
    orders.value = orders.value.filter(order => order.id !== orderId);
    alert(`Đơn hàng ${orderId} đã được hủy.`);
  }
};

const contactSupport = (orderId) => {
  alert(`Bạn đã gửi yêu cầu hỗ trợ cho đơn hàng ${orderId}. Chúng tôi sẽ liên hệ lại sớm nhất!`);
};

onMounted(() => {
  fetchOrders();
});
</script>

<style scoped>
.order-management {
  padding: 2rem;
  background-color: #fff9fb;
  min-height: calc(100vh - 100px);
}

.page-title {
  font-size: 2.2rem;
  color: #d63384;
  margin-bottom: 2rem;
  text-align: center;
  font-weight: 600;
  text-shadow: 1px 1px 2px rgba(214, 51, 132, 0.1);
  position: relative;
  padding-bottom: 1rem;
}

.page-title::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  width: 5rem;
  height: 2px;
  background-color: #d63384;
  border-radius: 10px;
  transform: translateX(-50%);
}

.loading-state,
.error-state,
.empty-state {
  font-size: 1.2rem;
  text-align: center;
  margin: 2rem 0;
  color: #6c757d;
}

.order-list {
  display: flex;
  flex-direction: column;
  gap: 1.8rem;
}

.order-card {
  background-color: #fff;
  padding: 1.5rem;
  border-radius: 16px;
  box-shadow: 0 3px 7px rgba(241, 175, 215, 0.25);
  border: 1px solid #f9c3d7;
  transition: box-shadow 0.3s ease;
  user-select: none;
}

.order-card:hover {
  box-shadow: 0 6px 16px rgba(241, 175, 215, 0.5);
}

.order-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 1rem;
  font-weight: 600;
  font-size: 1.2rem;
}

.order-status {
  padding: 0.3rem 0.7rem;
  border-radius: 12px;
  font-weight: 600;
  font-size: 1rem;
  color: #fff;
  text-transform: uppercase;
  user-select: none;
}

.status-pending {
  background-color: #f59e0b;
}

.status-processing {
  background-color: #3b82f6;
}

.status-shipping {
  background-color: #6366f1;
}

.status-delivered {
  background-color: #22c55e;
}

.status-cancelled {
  background-color: #ef4444;
}

.order-body {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}

.product-summary {
  flex: 1 1 50%;
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.product-item-preview {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  /* background-color: #f7d3e0; */
  padding: 0.3rem 0.6rem;
  border-radius: 10px;
  user-select: none;
}

.product-thumbnail {
  width: 50px;
  height: 50px;
  object-fit: contain;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(220, 38, 127, 0.3);
}

.product-info-preview {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  font-size: 0.9rem;
  color: #6b7280;
  user-select: none;
}

.product-name {
  font-weight: 600;
  font-size: 1rem;
  color: #ec4899;
  text-shadow: 1px 1px 3px #ffe4ef;
  user-select: none;
}

.product-quantity {
  font-weight: 400;
  font-size: 0.85rem;
  user-select: none;
}

.more-items {
  font-size: 0.85rem;
  font-style: italic;
  color: #9ca3af;
  user-select: none;
}

.order-details {
  flex: 1 1 40%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 0.4rem;
  font-weight: 600;
  font-size: 1.1rem;
  color: #4b5563;
  user-select: none;
}

.order-actions {
  margin-top: 1rem;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.btn {
  cursor: pointer;
  border: none;
  padding: 0.5rem 1.2rem;
  border-radius: 20px;
  font-weight: 600;
  font-size: 1rem;
  user-select: none;
  transition: background-color 0.3s ease, color 0.3s ease;
}

.btn-primary {
  background-color: #d63384;
  color: white;
}

.btn-primary:hover {
  background-color: #c0266e;
}

.btn-secondary {
  background-color: #6b7280;
  color: white;
}

.btn-secondary:hover {
  background-color: #4b5563;
}

.btn-info {
  background-color: #3b82f6;
  color: white;
}

.btn-info:hover {
  background-color: #2563eb;
}

/* Popup modal */

.popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0,0,0,0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.popup-content {
  background-color: #fff;
  padding: 2rem 2.5rem;
  border-radius: 16px;
  width: 90%;
  max-width: 600px;
  box-shadow: 0 6px 20px rgba(0,0,0,0.25);
  overflow-y: auto;
  max-height: 80vh;
}

.popup-content h3 {
  margin-bottom: 1rem;
  color: #d63384;
  user-select: none;
}

.popup-content p {
  margin-bottom: 0.8rem;
  font-size: 1.1rem;
  user-select: none;
}

.popup-product-list {
  list-style: none;
  padding: 0;
  margin: 1rem 0 1.5rem;
}

.popup-product-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
  border-bottom: 1px solid #f3f4f6;
  padding-bottom: 1rem;
  user-select: none;
}

.popup-product-thumbnail {
  width: 60px;
  height: 60px;
  object-fit: contain;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(214, 51, 132, 0.2);
}

.popup-product-name {
  font-weight: 600;
  color: #ec4899;
  margin-bottom: 0.2rem;
  user-select: none;
}

.btn-close {
  background-color: #ef4444;
  color: white;
  padding: 0.6rem 1.6rem;
  border-radius: 30px;
  font-weight: 600;
  font-size: 1.1rem;
  cursor: pointer;
  user-select: none;
  transition: background-color 0.3s ease;
}

.btn-close:hover {
  background-color: #b91c1c;
}
</style>
