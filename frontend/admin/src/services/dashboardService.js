import instance from "./axiosConfig";

class DashboardService {
    /**
     * Lấy dữ liệu sản phẩm bán chạy
     * @param {Object} params - Tham số query
     * @param {Date} params.startDate - Ngày bắt đầu
     * @param {Date} params.endDate - Ngày kết thúc
     * @param {string} params.sortBy - Sắp xếp theo (quantity, revenue)
     * @returns {Promise<Array>} Danh sách sản phẩm bán chạy
     */
    async getTopProducts(params = {}) {
        try {
            const queryParams = new URLSearchParams();

            if (params.startDate) {
                queryParams.append("startDate", params.startDate);
            }
            if (params.endDate) {
                queryParams.append("endDate", params.endDate);
            }
            if (params.sortBy) {
                queryParams.append("sortBy", params.sortBy);
            }

            const url = `/dashboard/top-products${
                queryParams.toString() ? `?${queryParams.toString()}` : ""
            }`;
            const response = await instance.get(url);
            return response.data.data;
        } catch (error) {
            console.error("Error fetching top products:", error);
            throw error;
        }
    }

    /**
     * Lấy danh sách khách hàng thân thiết (từ API user)
     * @returns {Promise<Array>} Danh sách khách hàng
     */
    async getFrequentCustomers() {
        try {
            const response = await instance.get("/user");
            const users = response.data.data;

            // Lọc và sắp xếp khách hàng theo số đơn hàng và tổng chi tiêu
            // Trong thực tế, backend sẽ có API riêng để lấy thống kê này
            return users.filter((user) => user.role === "user").slice(0, 10);
        } catch (error) {
            console.error("Error fetching frequent customers:", error);
            throw error;
        }
    }

    /**
     * Chuyển đổi timeFilter thành startDate và endDate
     * @param {string} timeFilter - Bộ lọc thời gian (today, week, month, quarter, year, custom)
     * @param {string} customStartDate - Ngày bắt đầu tùy chỉnh
     * @param {string} customEndDate - Ngày kết thúc tùy chỉnh
     * @returns {Object} Object chứa startDate và endDate
     */
    getDateRange(timeFilter, customStartDate = null, customEndDate = null) {
        const now = new Date();
        const today = new Date(
            now.getFullYear(),
            now.getMonth(),
            now.getDate()
        );

        switch (timeFilter) {
            case "today":
                return {
                    startDate: today.toISOString().split("T")[0],
                    endDate: today.toISOString().split("T")[0],
                };

            case "week":
                const startOfWeek = new Date(today);
                startOfWeek.setDate(today.getDate() - today.getDay());
                const endOfWeek = new Date(today);
                endOfWeek.setDate(startOfWeek.getDate() + 6);
                return {
                    startDate: startOfWeek.toISOString().split("T")[0],
                    endDate: endOfWeek.toISOString().split("T")[0],
                };

            case "month":
                const startOfMonth = new Date(
                    now.getFullYear(),
                    now.getMonth(),
                    1
                );
                const endOfMonth = new Date(
                    now.getFullYear(),
                    now.getMonth() + 1,
                    0
                );
                return {
                    startDate: startOfMonth.toISOString().split("T")[0],
                    endDate: endOfMonth.toISOString().split("T")[0],
                };

            case "quarter":
                const quarter = Math.floor(now.getMonth() / 3);
                const startOfQuarter = new Date(
                    now.getFullYear(),
                    quarter * 3,
                    1
                );
                const endOfQuarter = new Date(
                    now.getFullYear(),
                    quarter * 3 + 3,
                    0
                );
                return {
                    startDate: startOfQuarter.toISOString().split("T")[0],
                    endDate: endOfQuarter.toISOString().split("T")[0],
                };

            case "year":
                const startOfYear = new Date(now.getFullYear(), 0, 1);
                const endOfYear = new Date(now.getFullYear(), 11, 31);
                return {
                    startDate: startOfYear.toISOString().split("T")[0],
                    endDate: endOfYear.toISOString().split("T")[0],
                };

            case "custom":
                return {
                    startDate: customStartDate,
                    endDate: customEndDate,
                };

            default:
                return {
                    startDate: null,
                    endDate: null,
                };
        }
    }

    /**
     * Chuyển đổi timeFilter thành period cho API
     * @param {string} timeFilter - Bộ lọc thời gian
     * @returns {string} Period cho API (day, month, year)
     */
    getPeriodFromTimeFilter(timeFilter) {
        switch (timeFilter) {
            case "today":
            case "week":
                return "day";
            case "month":
            case "quarter":
                return "month";
            case "year":
                return "year";
            case "custom":
                return "day"; // Mặc định là day cho custom
            default:
                return "month";
        }
    }
    /**
     * Lấy thống kê đơn hàng
     * @param {Object} params - Tham số query
     * @param {Date} params.startDate - Ngày bắt đầu
     * @param {Date} params.endDate - Ngày kết thúc
     * @param {string} params.timeFilter - Bộ lọc thời gian (sử dụng để xác định period)
     * @returns {Promise<Object>} Dữ liệu thống kê đơn hàng
     */
    async getOrderStatistics(params = {}) {
        try {
            const queryParams = new URLSearchParams();

            if (params.startDate) {
                queryParams.append("startDate", params.startDate);
            }
            if (params.endDate) {
                queryParams.append("endDate", params.endDate);
            }

            // Thêm tham số period dựa trên timeFilter
            if (params.timeFilter) {
                const period = this.getPeriodFromTimeFilter(params.timeFilter);
                queryParams.append("period", period);
            } else {
                // Mặc định là month nếu không có timeFilter
                queryParams.append("period", "month");
            }

            const url = `/dashboard/order-statistics${
                queryParams.toString() ? `?${queryParams.toString()}` : ""
            }`;
            const response = await instance.get(url);
            return response.data.data;
        } catch (error) {
            console.error("Error fetching order statistics:", error);
            throw error;
        }
    }
}

export default new DashboardService();
