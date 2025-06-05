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
 * Hàm định dạng ngày theo định dạng Việt Nam (dd/mm/yyyy)
 * @param {string} dateString - Chuỗi ngày
 * @returns {string} Chuỗi đã định dạng (dd/mm/yyyy)
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
 * Lấy định dạng thời gian hiển thị dựa vào bộ lọc thời gian
 * @param {string} timeFilter - Loại bộ lọc thời gian ('today', 'week', 'month', 'quarter', 'year', 'custom')
 * @param {string} startDate - Ngày bắt đầu (chỉ dùng khi timeFilter là 'custom')
 * @param {string} endDate - Ngày kết thúc (chỉ dùng khi timeFilter là 'custom')
 * @returns {string} Chuỗi thời gian đã định dạng
 */
export function getFormattedTimePeriod(
    timeFilter,
    startDate = null,
    endDate = null
) {
    const today = new Date();

    switch (timeFilter) {
        case "today":
            return `Hôm nay (${formatDate(today)})`;
        case "week": {
            // Lấy ngày đầu tuần (thứ 2)
            const firstDayOfWeek = new Date(today);
            const day = today.getDay();
            const diff = day === 0 ? 6 : day - 1; // adjust when day is Sunday
            firstDayOfWeek.setDate(today.getDate() - diff);

            return `Tuần này (${formatDate(firstDayOfWeek)} - ${formatDate(
                today
            )})`;
        }
        case "month": {
            // Lấy ngày đầu tháng
            const firstDayOfMonth = new Date(
                today.getFullYear(),
                today.getMonth(),
                1
            );
            return `Tháng này (${formatDate(firstDayOfMonth)} - ${formatDate(
                today
            )})`;
        }
        case "quarter": {
            // Lấy quý hiện tại
            const quarter = Math.floor(today.getMonth() / 3) + 1;
            const firstMonthOfQuarter = (quarter - 1) * 3;
            const firstDayOfQuarter = new Date(
                today.getFullYear(),
                firstMonthOfQuarter,
                1
            );

            return `Quý ${quarter} (${formatDate(
                firstDayOfQuarter
            )} - ${formatDate(today)})`;
        }
        case "year": {
            // Lấy ngày đầu năm
            const firstDayOfYear = new Date(today.getFullYear(), 0, 1);
            return `Năm nay (${formatDate(firstDayOfYear)} - ${formatDate(
                today
            )})`;
        }
        case "custom":
            if (startDate && endDate) {
                return `${formatDate(startDate)} - ${formatDate(endDate)}`;
            }
            return "Khoảng thời gian tùy chỉnh";
        default:
            return "";
    }
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
