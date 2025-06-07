<template>
    <div class="chatbot-container">
        <!-- Chatbot Toggle Button -->
        <div
            v-if="!isOpen"
            class="chatbot-toggle"
            @click="toggleChat"
            :class="{ 'has-notification': hasUnreadMessage }"
        >
            <i class="fas fa-comments"></i>
            <div v-if="hasUnreadMessage" class="notification-dot"></div>
        </div>

        <!-- Chatbot Window -->
        <div v-if="isOpen" class="chatbot-window">
            <!-- Header -->
            <div class="chatbot-header">
                <div class="header-info">
                    <div class="avatar">
                        <i class="fas fa-robot"></i>
                    </div>
                    <div class="info">
                        <h4>SmartBuy Assistant</h4>
                        <span class="status">{{
                            isTyping ? "Đang trả lời..." : "Trực tuyến"
                        }}</span>
                    </div>
                </div>
                <div class="header-actions">
                    <button
                        @click="clearChat"
                        class="clear-btn"
                        title="Xóa lịch sử chat"
                    >
                        <i class="fas fa-trash"></i>
                    </button>
                    <button
                        @click="toggleChat"
                        class="close-btn"
                        title="Đóng chat"
                    >
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>

            <!-- Messages Container -->
            <div class="messages-container" ref="messagesContainer">
                <!-- Welcome Message -->
                <div v-if="messages.length === 0" class="welcome-message">
                    <div class="welcome-content">
                        <i class="fas fa-wave-square"></i>
                        <h3>Xin chào!</h3>
                        <p>Tôi là SmartBuy Assistant. Tôi có thể giúp bạn:</p>
                        <ul>
                            <li>Tư vấn sản phẩm</li>
                            <li>Hướng dẫn mua hàng</li>
                            <li>Giải đáp thắc mắc</li>
                        </ul>
                    </div>
                </div>

                <!-- Messages -->
                <div
                    v-for="message in messages"
                    :key="message.id"
                    class="message-wrapper"
                >
                    <div
                        class="message"
                        :class="{
                            'user-message': message.isUser,
                            'bot-message': !message.isUser,
                            'error-message': message.isError,
                        }"
                    >
                        <div v-if="!message.isUser" class="message-avatar">
                            <i class="fas fa-robot"></i>
                        </div>
                        <div class="message-content">
                            <div
                                class="message-text"
                                v-html="formatMessage(message.content)"
                            ></div>
                            <div class="message-time">
                                {{ formatTime(message.timestamp) }}
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Typing Indicator -->
                <div v-if="isTyping" class="typing-indicator">
                    <div class="typing-avatar">
                        <i class="fas fa-robot"></i>
                    </div>
                    <div class="typing-dots">
                        <span></span>
                        <span></span>
                        <span></span>
                    </div>
                </div>
            </div>
            <!-- Quick Questions -->
            <div
                v-if="showQuickQuestions"
                class="quick-questions"
                :class="{
                    minimized: messages.length > 0 && !isQuickQuestionsExpanded,
                }"
            >
                <div
                    class="quick-questions-header"
                    @click="toggleQuickQuestions"
                >
                    <h5>
                        <i class="fas fa-question-circle"></i>
                        Câu hỏi thường gặp
                    </h5>
                    <i
                        v-if="messages.length > 0"
                        class="fas fa-chevron-down toggle-icon"
                        :class="{ rotated: !isQuickQuestionsExpanded }"
                    ></i>
                </div>
                <div v-show="isQuickQuestionsExpanded" class="questions-list">
                    <button
                        v-for="question in quickQuestions"
                        :key="question"
                        @click="sendQuickQuestion(question)"
                        class="quick-question-btn"
                    >
                        <i class="fas fa-comment-alt"></i>
                        {{ question }}
                    </button>
                </div>
            </div>

            <!-- Input Area -->
            <div class="input-area">
                <div class="input-wrapper">
                    <input
                        v-model="inputMessage"
                        @keypress.enter="sendMessage"
                        @focus="hideQuickQuestions"
                        type="text"
                        placeholder="Nhập tin nhắn của bạn..."
                        :disabled="isTyping"
                        ref="messageInput"
                    />
                    <button
                        @click="sendMessage"
                        :disabled="!inputMessage.trim() || isTyping"
                        class="send-btn"
                    >
                        <i class="fas fa-paper-plane"></i>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, nextTick, onMounted, watch } from "vue";
import { marked } from "marked";
import chatbotService from "@/services/chatbotService";
import productService from "@/services/productService";

// Props
const props = defineProps({
    productContext: {
        type: Object,
        default: null,
    },
});

// Reactive Data
const isOpen = ref(false);
const messages = ref([]);
const inputMessage = ref("");
const isTyping = ref(false);
const hasUnreadMessage = ref(false);
const showQuickQuestions = ref(true);
const isQuickQuestionsExpanded = ref(true);
const messagesContainer = ref(null);
const messageInput = ref(null);

// Quick questions
const quickQuestions = ref([]);

// Methods
const loadQuickQuestions = async () => {
    try {
        const questions = await chatbotService.getQuickQuestions();
        quickQuestions.value = questions;
    } catch (error) {
        console.error("Error loading quick questions:", error); // Fallback questions
        quickQuestions.value = [
            "Làm sao để đặt hàng?",
            "Chính sách đổi trả như thế nào?",
            "Thời gian giao hàng bao lâu?",
            "Có những phương thức thanh toán nào?",
            "Làm sao để theo dõi đơn hàng?",
            "Tư vấn sản phẩm phù hợp với tôi",
            "Có chương trình khuyến mãi nào không?",
            "Cách liên hệ hỗ trợ khách hàng?",
        ];
    }
};

// Methods
const toggleChat = () => {
    isOpen.value = !isOpen.value;
    hasUnreadMessage.value = false;

    if (isOpen.value) {
        nextTick(() => {
            scrollToBottom();
            if (messageInput.value) {
                messageInput.value.focus();
            }
        });
    }
};

const sendMessage = async () => {
    if (!inputMessage.value.trim() || isTyping.value) return;
    const messageText = inputMessage.value.trim();
    inputMessage.value = "";

    // Auto-collapse quick questions after first message
    if (messages.value.length === 0) {
        isQuickQuestionsExpanded.value = false;
    }

    // Add user message
    const userMessage = {
        id: Date.now(),
        content: messageText,
        timestamp: new Date(),
        isUser: true,
    };
    messages.value.push(userMessage);

    // Show typing indicator
    isTyping.value = true;
    scrollToBottom();

    try {
        // Get product context if available
        let context = props.productContext;
        if (!context) {
            context = await getProductContext();
        }

        // Send to chatbot service
        const botResponse = await chatbotService.sendMessage(
            messageText,
            context
        );
        messages.value.push(botResponse);

        if (!isOpen.value) {
            hasUnreadMessage.value = true;
        }
    } catch (error) {
        console.error("Error sending message:", error);
        const errorMessage = {
            id: Date.now(),
            content: "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.",
            timestamp: new Date(),
            isUser: false,
            isError: true,
        };
        messages.value.push(errorMessage);
    } finally {
        isTyping.value = false;
        scrollToBottom();
    }
};

const sendQuickQuestion = (question) => {
    inputMessage.value = question;
    sendMessage();
};

const toggleQuickQuestions = () => {
    isQuickQuestionsExpanded.value = !isQuickQuestionsExpanded.value;
};

const clearChat = () => {
    messages.value = [];
    chatbotService.clearHistory();
    showQuickQuestions.value = true;
    isQuickQuestionsExpanded.value = true;

    // Reset input
    inputMessage.value = "";

    // Scroll to bottom to show welcome message and quick questions
    nextTick(() => {
        scrollToBottom();
        if (messageInput.value) {
            messageInput.value.focus();
        }
    });
};

const hideQuickQuestions = () => {
    isQuickQuestionsExpanded.value = false;
};

const scrollToBottom = () => {
    nextTick(() => {
        if (messagesContainer.value) {
            messagesContainer.value.scrollTop =
                messagesContainer.value.scrollHeight;
        }
    });
};

const formatMessage = (content) => {
    return marked(content);
};

const formatTime = (timestamp) => {
    return new Date(timestamp).toLocaleTimeString("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
    });
};

const getProductContext = async () => {
    try {
        const [productsResponse, brandsResponse] = await Promise.all([
            productService.getProducts(1, 10),
            productService.getBrands(),
        ]);

        return chatbotService.createProductContext(
            productsResponse?.data?.data,
            brandsResponse?.data
        );
    } catch (error) {
        console.error("Error getting product context:", error);
        return null;
    }
};

// Watch for new messages to auto-scroll
watch(
    messages,
    () => {
        scrollToBottom();
    },
    { deep: true }
);

onMounted(() => {
    // Load quick questions
    loadQuickQuestions();
});
</script>

<style scoped>
.chatbot-container {
    position: fixed;
    bottom: 10px;
    right: 10px;
    z-index: 1000;
    font-family: "Inter", -apple-system, BlinkMacSystemFont, sans-serif;
}

/* Toggle Button */
.chatbot-toggle {
    width: 60px;
    height: 60px;
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    box-shadow: 0 8px 25px rgba(248, 110, 211, 0.3);
    transition: all 0.3s ease;
    position: relative;
    animation: pulse 2s infinite;
}

.chatbot-toggle:hover {
    transform: translateY(-2px) scale(1.05);
    box-shadow: 0 15px 40px rgba(248, 110, 211, 0.4);
    animation: none;
}

.chatbot-toggle i {
    color: white;
    font-size: 24px;
}

.notification-dot {
    position: absolute;
    top: -2px;
    right: -2px;
    width: 12px;
    height: 12px;
    background: #ff4757;
    border-radius: 50%;
    border: 2px solid white;
    animation: bounce 1s infinite;
}

@keyframes pulse {
    0% {
        box-shadow: 0 8px 25px rgba(248, 110, 211, 0.3);
    }
    50% {
        box-shadow: 0 8px 25px rgba(248, 110, 211, 0.5);
    }
    100% {
        box-shadow: 0 8px 25px rgba(248, 110, 211, 0.3);
    }
}

@keyframes bounce {
    0%,
    20%,
    50%,
    80%,
    100% {
        transform: translateY(0);
    }
    40% {
        transform: translateY(-3px);
    }
    60% {
        transform: translateY(-2px);
    }
}

/* Chatbot Window */
.chatbot-window {
    width: 380px;
    height: 600px;
    background: white;
    border-radius: 20px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.1);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    animation: slideUp 0.3s ease-out;
}

@keyframes slideUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* Header */
.chatbot-header {
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    color: white;
    padding: 12px 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.header-info {
    display: flex;
    align-items: center;
    gap: 8px;
}

.avatar {
    width: 32px;
    height: 32px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.avatar i {
    font-size: 14px;
}

.info h4 {
    margin: 0;
    font-size: 14px;
    font-weight: 600;
}

.status {
    font-size: 11px;
    opacity: 0.8;
}

.header-actions {
    display: flex;
    gap: 6px;
}

.clear-btn,
.close-btn {
    width: 28px;
    height: 28px;
    background: rgba(255, 255, 255, 0.2);
    border: none;
    border-radius: 6px;
    color: white;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background 0.2s;
}

.clear-btn:hover,
.close-btn:hover {
    background: rgba(255, 255, 255, 0.3);
}

.clear-btn i,
.close-btn i {
    font-size: 12px;
}

/* Messages Container */
.messages-container {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.messages-container::-webkit-scrollbar {
    width: 4px;
}

.messages-container::-webkit-scrollbar-track {
    background: #f1f1f1;
}

.messages-container::-webkit-scrollbar-thumb {
    background: #f86ed3;
    border-radius: 2px;
}

.messages-container::-webkit-scrollbar-thumb:hover {
    background: #f045c2;
}

/* Welcome Message */
.welcome-message {
    text-align: center;
    padding: 20px;
    color: #6b7280;
}

.welcome-content i {
    font-size: 32px;
    color: #f86ed3;
    margin-bottom: 12px;
}

.welcome-content h3 {
    margin: 0 0 8px 0;
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    font-size: 18px;
    font-weight: 600;
}

.welcome-content p {
    margin: 0 0 12px 0;
    font-size: 14px;
}

.welcome-content ul {
    text-align: left;
    display: inline-block;
    margin: 0;
    padding-left: 20px;
}

.welcome-content li {
    font-size: 14px;
    margin-bottom: 4px;
}

/* Messages */
.message-wrapper {
    display: flex;
    flex-direction: column;
}

.message {
    display: flex;
    gap: 8px;
    margin-bottom: 4px;
    animation: messageSlide 0.3s ease-out;
}

.user-message {
    justify-content: flex-end;
    animation: messageSlideRight 0.3s ease-out;
}

.bot-message {
    animation: messageSlideLeft 0.3s ease-out;
}

@keyframes messageSlide {
    from {
        opacity: 0;
        transform: translateY(10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes messageSlideRight {
    from {
        opacity: 0;
        transform: translateX(20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

@keyframes messageSlideLeft {
    from {
        opacity: 0;
        transform: translateX(-20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

.user-message .message-content {
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    color: white;
    margin-left: 40px;
}

.bot-message .message-content {
    background: #f8f9ff;
    color: #374151;
    margin-right: 40px;
    border-left: 3px solid #f86ed3;
}

.error-message .message-content {
    background: #fef2f2;
    color: #dc2626;
    border-left: 3px solid #ef4444;
}

.message-avatar {
    width: 32px;
    height: 32px;
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 14px;
    flex-shrink: 0;
}

.message-content {
    max-width: 280px;
    padding: 12px 16px;
    border-radius: 18px;
    position: relative;
}

.message-text {
    font-size: 14px;
    line-height: 1.4;
    word-wrap: break-word;
}

.message-text :deep(p) {
    margin: 0;
}

.message-text :deep(ul),
.message-text :deep(ol) {
    margin: 8px 0;
    padding-left: 20px;
}

.message-text :deep(li) {
    margin-bottom: 4px;
}

.message-time {
    font-size: 11px;
    opacity: 0.6;
    margin-top: 4px;
}

/* Typing Indicator */
.typing-indicator {
    display: flex;
    align-items: center;
    gap: 8px;
}

.typing-avatar {
    width: 32px;
    height: 32px;
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 14px;
}

.typing-dots {
    display: flex;
    gap: 4px;
    padding: 12px 16px;
    background: #f3f4f6;
    border-radius: 18px;
    background: linear-gradient(90deg, #f3f4f6 25%, #e5e7eb 50%, #f3f4f6 75%);
    background-size: 200% 100%;
    animation: shimmer 1.5s infinite;
}

.typing-dots span {
    width: 6px;
    height: 6px;
    background: #9ca3af;
    border-radius: 50%;
    animation: typing 1.4s infinite ease-in-out both;
}

.typing-dots span:nth-child(1) {
    animation-delay: -0.32s;
}
.typing-dots span:nth-child(2) {
    animation-delay: -0.16s;
}

@keyframes typing {
    0%,
    80%,
    100% {
        transform: scale(0);
        opacity: 0.5;
    }
    40% {
        transform: scale(1);
        opacity: 1;
    }
}

@keyframes shimmer {
    0% {
        background-position: -200% 0;
    }
    100% {
        background-position: 200% 0;
    }
}

/* Quick Questions */
.quick-questions {
    border-top: 1px solid #e5e7eb;
    background: #f9fafb;
    transition: all 0.3s ease;
}

.quick-questions.minimized {
    background: #ffffff;
}

.quick-questions-header {
    padding: 16px 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    cursor: pointer;
    transition: background 0.2s ease;
}

.quick-questions-header:hover {
    background: rgba(248, 110, 211, 0.05);
}

.quick-questions h5 {
    margin: 0;
    font-size: 14px;
    color: #6b7280;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 8px;
}

.quick-questions h5 i {
    color: #f86ed3;
    font-size: 16px;
}

.toggle-icon {
    color: #9ca3af;
    font-size: 12px;
    transition: transform 0.3s ease;
}

.toggle-icon.rotated {
    transform: rotate(-90deg);
}

.questions-list {
    padding: 0 20px 16px 20px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    animation: slideDown 0.3s ease-out;
}

@keyframes slideDown {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.quick-question-btn {
    background: white;
    border: 1px solid #d1d5db;
    border-radius: 12px;
    padding: 12px 16px;
    text-align: left;
    font-size: 13px;
    color: #374151;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 10px;
    position: relative;
    overflow: hidden;
}

.quick-question-btn::before {
    content: "";
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(
        90deg,
        transparent,
        rgba(248, 110, 211, 0.1),
        transparent
    );
    transition: left 0.5s ease;
}

.quick-question-btn:hover::before {
    left: 100%;
}

.quick-question-btn i {
    color: #f86ed3;
    font-size: 12px;
    flex-shrink: 0;
}

.quick-question-btn:hover {
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    color: white;
    border-color: #f86ed3;
    transform: translateY(-1px);
    box-shadow: 0 4px 15px rgba(248, 110, 211, 0.2);
}

.quick-question-btn:hover i {
    color: white;
}

.quick-question-btn:active {
    transform: translateY(0);
    box-shadow: 0 2px 8px rgba(248, 110, 211, 0.3);
}

/* Input Area */
.input-area {
    padding: 20px;
    border-top: 1px solid #e5e7eb;
    background: white;
}

.input-wrapper {
    display: flex;
    gap: 12px;
    align-items: center;
}

.input-wrapper input {
    flex: 1;
    padding: 12px 16px;
    border: 1px solid #d1d5db;
    border-radius: 12px;
    font-size: 14px;
    outline: none;
    transition: border-color 0.2s;
}

.input-wrapper input:focus {
    border-color: #f86ed3;
}

.input-wrapper input:disabled {
    background: #f9fafb;
    color: #9ca3af;
}

.send-btn {
    width: 44px;
    height: 44px;
    background: linear-gradient(135deg, #f86ed3 0%, #f045c2 100%);
    border: none;
    border-radius: 12px;
    color: white;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.send-btn:hover:not(:disabled) {
    transform: translateY(-1px) scale(1.05);
    box-shadow: 0 6px 20px rgba(248, 110, 211, 0.3);
}

.send-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
}

/* Responsive */
@media (max-width: 480px) {
    .chatbot-window {
        width: calc(100vw - 20px);
        height: calc(100vh - 100px);
        max-width: none;
        max-height: none;
        border-radius: 15px;
    }

    .chatbot-toggle {
        width: 56px;
        height: 56px;
        bottom: 20px;
        right: 20px;
    }

    .chatbot-toggle i {
        font-size: 22px;
    }

    .messages-container {
        padding: 15px;
    }

    .input-area {
        padding: 15px;
    }
    .quick-questions-header {
        padding: 12px 15px;
    }

    .questions-list {
        padding: 0 15px 12px 15px;
    }

    .quick-question-btn {
        padding: 10px 14px;
        font-size: 12px;
    }
}

@media (max-width: 360px) {
    .chatbot-window {
        width: calc(100vw - 10px);
        height: calc(100vh - 80px);
        border-radius: 12px;
    }

    .message-content {
        max-width: 240px;
    }
}
</style>
