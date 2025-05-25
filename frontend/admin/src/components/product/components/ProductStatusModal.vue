<script setup>
import { defineEmits } from "vue";

const props = defineProps({
    showModal: {
        type: Boolean,
        required: true,
    },
    product: {
        type: Object,
        default: null,
    },
    loading: {
        type: Boolean,
        default: false,
    },
});

const emit = defineEmits(["confirm", "cancel"]);

const confirmAction = () => {
    emit("confirm");
};

const cancelAction = () => {
    emit("cancel");
};
</script>

<template>
    <div v-if="showModal" class="modal-backdrop">
        <div class="modal-container warning-modal" style="max-width: 500px">
            <div class="modal-header" :class="{ warning: product?.isActive }">
                <h3>
                    {{
                        product?.isActive
                            ? "Xác nhận ngừng bán sản phẩm"
                            : "Xác nhận kích hoạt sản phẩm"
                    }}
                </h3>
                <button @click="cancelAction" class="close-button">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="modal-body text-center">
                <div
                    class="warning-icon"
                    :class="{ 'activate-icon': !product?.isActive }"
                >
                    <i
                        class="fas"
                        :class="
                            product?.isActive ? 'fa-toggle-off' : 'fa-toggle-on'
                        "
                    ></i>
                </div>
                <p class="warning-message">
                    Bạn có chắc chắn muốn
                    {{ product?.isActive ? "ngừng bán" : "kích hoạt bán lại" }}
                    sản phẩm
                    <strong>"{{ product?.name }}"</strong>?
                </p>
                <div
                    class="warning-details"
                    :class="{ 'activate-details': !product?.isActive }"
                >
                    <i class="fas fa-info-circle"></i>
                    <div class="warning-text">
                        <p v-if="product?.isActive" class="warning-impact">
                            <strong>Lưu ý:</strong> Ngừng bán sản phẩm sẽ ẩn sản
                            phẩm này khỏi trang người dùng và không thể mua được
                            nữa. Sản phẩm vẫn sẽ xuất hiện trong lịch sử đơn
                            hàng của khách hàng đã mua trước đó.
                        </p>
                        <p v-else class="warning-impact">
                            <strong>Lưu ý:</strong> Kích hoạt sản phẩm sẽ hiển
                            thị lại sản phẩm này trên trang người dùng và có thể
                            được mua bởi khách hàng.
                        </p>
                    </div>
                </div>
            </div>
            <div class="modal-actions">
                <button @click="cancelAction" class="cancel-button">
                    <i class="fas fa-arrow-left"></i> Quay lại
                </button>
                <button
                    @click="confirmAction"
                    :class="
                        product?.isActive
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
                            product?.isActive ? 'fa-toggle-off' : 'fa-toggle-on'
                        "
                    ></i>
                    {{ product?.isActive ? "Ngừng bán" : "Kích hoạt" }}
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
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
    max-width: 500px;
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
    border-top-left-radius: 12px;
    border-top-right-radius: 12px;
}

.modal-header.warning {
    background-color: #fee2e2;
    color: #dc2626;
}

.close-button {
    background: none;
    border: none;
    font-size: 1.25rem;
    color: #6b7280;
    cursor: pointer;
}

.modal-body {
    padding: 2rem 1.5rem;
}

.text-center {
    text-align: center;
}

.warning-icon {
    font-size: 3rem;
    color: #dc2626;
    margin-bottom: 1.5rem;
}

.warning-icon.activate-icon {
    color: #22c55e;
}

.warning-message {
    font-size: 1.1rem;
    margin-bottom: 1.5rem;
    color: #374151;
}

.warning-details {
    background-color: #fee2e2;
    border-radius: 8px;
    padding: 1rem;
    display: flex;
    gap: 1rem;
    text-align: left;
    margin-top: 1.5rem;
}

.warning-details.activate-details {
    background-color: #e6f7ea;
}

.warning-details i {
    color: #dc2626;
    font-size: 1.25rem;
}

.warning-details.activate-details i {
    color: #22c55e;
}

.warning-text {
    flex: 1;
}

.warning-impact {
    margin: 0;
    font-size: 0.95rem;
    color: #4b5563;
}

.modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    padding: 1.25rem 1.5rem;
    border-top: 1px solid #eee;
}

.cancel-button {
    padding: 0.75rem 1.25rem;
    background-color: #f3f4f6;
    color: #4b5563;
    border: none;
    border-radius: 8px;
    font-size: 0.95rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
}

.cancel-button:hover {
    background-color: #e5e7eb;
}

.deactivate-confirm-button,
.activate-confirm-button {
    padding: 0.75rem 1.25rem;
    border: none;
    border-radius: 8px;
    font-size: 0.95rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
}

.deactivate-confirm-button {
    background-color: #dc2626;
    color: white;
}

.deactivate-confirm-button:hover {
    background-color: #b91c1c;
}

.activate-confirm-button {
    background-color: #22c55e;
    color: white;
}

.activate-confirm-button:hover {
    background-color: #16a34a;
}

.spinner.small {
    width: 20px;
    height: 20px;
    border: 3px solid rgba(255, 255, 255, 0.3);
    border-radius: 50%;
    border-top: 3px solid white;
    animation: spin 1s linear infinite;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}

button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}
</style>
