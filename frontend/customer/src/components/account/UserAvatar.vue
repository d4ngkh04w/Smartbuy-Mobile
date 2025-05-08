<template>
    <div class="profile-avatar-section">
        <div class="avatar-container">
            <img :src="avatarUrl" alt="Avatar" class="avatar-image" />
            <div v-if="editable" class="avatar-overlay">
                <label :for="inputId" class="avatar-upload-label">
                    <i class="fas fa-camera"></i> Cập nhật ảnh
                </label>
                <input
                    :id="inputId"
                    type="file"
                    accept="image/jpeg,image/png,image/jpg"
                    @change="handleFileChange"
                    class="avatar-upload"
                    ref="fileInput"
                />
            </div>
        </div>
        <div class="avatar-info">
            <slot></slot>
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from "vue";
import emitter from "@/utils/evenBus";

const props = defineProps({
    avatar: {
        type: String,
        default: "",
    },
    preview: {
        type: String,
        default: null,
    },
    editable: {
        type: Boolean,
        default: false,
    },
    inputId: {
        type: String,
        default: "avatar-upload",
    },
});

const emit = defineEmits(["file-change"]);
const fileInput = ref(null);

const avatarUrl = computed(() => {
    if (props.preview) return props.preview;
    if (props.avatar) {
        // Nếu avatar đã là URL đầy đủ (bắt đầu bằng http hoặc https)
        if (props.avatar.startsWith("http")) {
            return props.avatar;
        }

        // Lấy base URL từ cấu hình API
        const apiUrl = import.meta.env.VITE_API_URL || "";
        const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

        // Chuẩn hóa đường dẫn file (chuyển \ thành /)
        const normalizedPath = props.avatar.replace(/\\/g, "/");

        // Kiểm tra xem có prefix / hay không
        const avatarPath = normalizedPath.startsWith("/")
            ? normalizedPath
            : `/${normalizedPath}`;

        return `${baseUrl}${avatarPath}`;
    }
    return "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg";
});

const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Kiểm tra định dạng file
    const allowedTypes = ["image/jpeg", "image/png", "image/jpg"];
    if (!allowedTypes.includes(file.type)) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Chỉ chấp nhận file hình ảnh (jpg, jpeg, png)",
        });
        return;
    }

    // Giới hạn kích thước file (15MB)
    if (file.size > 15 * 1024 * 1024) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Kích thước file quá lớn (tối đa 15MB)",
        });
        return;
    }

    // Emit event với file được chọn
    emit("file-change", file);
};

// Method để reset input file
const resetFileInput = () => {
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Expose the reset method to parent components
defineExpose({ resetFileInput });
</script>

<style scoped>
.profile-avatar-section {
    display: flex;
    align-items: center;
    gap: 1.5rem;
    margin-bottom: 1rem;
}

.avatar-container {
    position: relative;
    width: 120px;
    height: 120px;
    border-radius: 50%;
    overflow: hidden;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.avatar-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.avatar-overlay {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.6);
    padding: 0.75rem 0;
    display: flex;
    justify-content: center;
    transition: all 0.3s ease;
}

.avatar-upload-label {
    color: white;
    font-size: 0.85rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.avatar-upload {
    display: none;
}

.avatar-info {
    flex: 1;
}

@media (max-width: 576px) {
    .profile-avatar-section {
        flex-direction: column;
        text-align: center;
    }
}
</style>
