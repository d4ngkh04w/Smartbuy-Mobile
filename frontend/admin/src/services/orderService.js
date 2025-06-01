import instance from "./axiosConfig";

class OrderService {
    getUrlImage(url) {
        const baseUrl =
            import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
        return url?.startsWith("http") ? url : `${baseUrl}${url}`;
    }
    // ===== Date/Time Utilities =====
    formatOrderDate(dateString) {
        if (!dateString) return "N/A";
        const date = new Date(dateString);
        return date.toLocaleDateString("vi-VN", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
        });
    }

    // ===== Status Utilities =====
    getStatusClass(status) {
        const statusMap = {
            "Chờ xác nhận": "pending",
            "Đã xác nhận": "confirmed",
            "Đang giao hàng": "shipping",
            "Đã giao hàng": "delivered",
            "Hoàn thành": "completed",
            "Đã huỷ": "canceled",
            "Đã hoàn tiền": "refunded",
            "Đã trả hàng": "returned",
        };
        return statusMap[status] || "default";
    }

    getAvailableStatusOptions(currentStatus) {
        const statusTransitions = {
            "Chờ xác nhận": ["Đã xác nhận", "Đã huỷ"],
            "Đã xác nhận": ["Đang giao hàng", "Đã huỷ"],
            "Đang giao hàng": ["Đã giao hàng"],
            "Đã giao hàng": ["Hoàn thành", "Đã trả hàng"],
            "Hoàn thành": [],
            "Đã huỷ": [],
            "Đã trả hàng": ["Đã hoàn tiền"],
            "Đã hoàn tiền": [],
        };
        return statusTransitions[currentStatus] || [];
    }

    // ===== Filter Utilities =====
    applyFilters(orders, filters = {}) {
        let result = [...orders];
        const { searchQuery, statusFilter, sortBy } = filters;

        // Apply search filter
        if (searchQuery) {
            const query = searchQuery.toLowerCase();
            result = result.filter(
                (order) =>
                    (order.id && order.id.toString().includes(query)) ||
                    order.user?.name?.toLowerCase().includes(query) ||
                    order.user?.email?.toLowerCase().includes(query) ||
                    order.user?.phoneNumber?.includes(query)
            );
        }

        // Apply status filter
        if (statusFilter && statusFilter !== "all") {
            result = result.filter((order) => order.status === statusFilter);
        }

        // Apply sorting
        switch (sortBy) {
            case "newest":
                result.sort(
                    (a, b) => new Date(b.orderDate) - new Date(a.orderDate)
                );
                break;
            case "oldest":
                result.sort(
                    (a, b) => new Date(a.orderDate) - new Date(b.orderDate)
                );
                break;
            case "total_desc":
                result.sort((a, b) => b.totalAmount - a.totalAmount);
                break;
            case "total_asc":
                result.sort((a, b) => a.totalAmount - b.totalAmount);
                break;
        }

        return result;
    }

    // ===== Statistics Utilities =====
    calculateOrderStats(orders) {
        return {
            total: orders.length,
            pending: orders.filter((o) => o.status === "Chờ xác nhận").length,
            processing: orders.filter(
                (o) =>
                    o.status === "Đã xác nhận" || o.status === "Đang giao hàng"
            ).length,
            delivered: orders.filter((o) => o.status === "Đã giao hàng").length,
            completed: orders.filter((o) => o.status === "Hoàn thành").length,
            cancelled: orders.filter(
                (o) =>
                    o.status === "Đã huỷ" ||
                    o.status === "Đã hoàn tiền" ||
                    o.status === "Đã trả hàng"
            ).length,
        };
    }

    // ===== Pagination Utilities =====
    getPaginatedOrders(orders, currentPage, itemsPerPage) {
        const start = (currentPage - 1) * itemsPerPage;
        const end = start + itemsPerPage;
        return orders.slice(start, end);
    }

    getPageCount(totalItems, itemsPerPage) {
        return Math.ceil(totalItems / itemsPerPage);
    }

    getDisplayedPageNumbers(currentPage, pageCount, maxVisiblePages = 5) {
        const pages = [];

        if (pageCount <= maxVisiblePages) {
            for (let i = 1; i <= pageCount; i++) {
                pages.push(i);
            }
        } else {
            pages.push(1);

            let start = Math.max(2, currentPage - 1);
            let end = Math.min(pageCount - 1, currentPage + 1);

            if (currentPage <= 3) {
                start = 2;
                end = Math.min(start + 2, pageCount - 1);
            } else if (currentPage >= pageCount - 2) {
                end = pageCount - 1;
                start = Math.max(2, end - 2);
            }

            if (start > 2) pages.push("...");

            for (let i = start; i <= end; i++) {
                pages.push(i);
            }

            if (end < pageCount - 1) pages.push("...");

            if (pageCount > 1) {
                pages.push(pageCount);
            }
        }

        return pages;
    } // ===== Status Update Utilities =====
    prepareStatusUpdateData(status) {
        const statusData = { status };
        return statusData;
    }

    // ===== Order API =====
    async getOrders(filters = {}) {
        try {
            const response = await instance.get("/order");
            return response.data;
        } catch (error) {
            console.error("Error fetching orders:", error);
            throw error;
        }
    }

    async getOrderById(id) {
        try {
            const response = await instance.get(`/order/${id}`);
            return response.data;
        } catch (error) {
            console.error("Error fetching order by ID:", error);
            throw error;
        }
    }

    async updateOrderStatus(id, statusData) {
        try {
            const response = await instance.put(
                `/order/${id}/status`,
                statusData
            );
            return response.data;
        } catch (error) {
            console.error("Error updating order status:", error);
            throw error;
        }
    }
}

export default new OrderService();
