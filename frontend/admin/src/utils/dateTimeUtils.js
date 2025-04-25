/**
 * Hàm định dạng tiền tệ Việt Nam
 * @param {number} value - Giá trị cần định dạng
 * @returns {string} Chuỗi đã định dạng
 */
export function formatCurrency(value) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        maximumFractionDigits: 0,
    }).format(value);
}

/**
 * Hàm định dạng ngày theo định dạng Việt Nam
 * @param {string} dateString - Chuỗi ngày
 * @returns {string} Chuỗi đã định dạng
 */
export function formatDate(dateString) {
    if (!dateString) return "";

    const date = new Date(dateString);
    return new Intl.DateTimeFormat("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
    }).format(date);
}

/**
 * Trả về ngày hiện tại dưới dạng chuỗi YYYY-MM-DD
 * @returns {string} Chuỗi ngày
 */
export function getCurrentDate() {
    const date = new Date();
    return date.toISOString().split("T")[0];
}

/**
 * Trả về ngày cách đây 1 tháng dưới dạng chuỗi YYYY-MM-DD
 * @returns {string} Chuỗi ngày
 */
export function getLastMonthDate() {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().split("T")[0];
}
