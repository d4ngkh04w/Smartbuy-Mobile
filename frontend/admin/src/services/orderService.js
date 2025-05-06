import instance from "./axiosConfig";

// Get all orders
export const getOrders = async () => {
	return await instance.get("/order");
};

// Get an order by ID
export const getOrderById = async (id) => {
	return await instance.get(`/order/${id}`);
};

// Update an order status
export const updateOrderStatus = async (id, statusData) => {
	return await instance.put(`/order/${id}/status`, statusData);
};

// Delete an order
export const deleteOrder = async (id) => {
	return await instance.delete(`/order/${id}`);
};

const orderService = {
	getOrders,
	getOrderById,
	updateOrderStatus,
	deleteOrder,
};

export default orderService;
