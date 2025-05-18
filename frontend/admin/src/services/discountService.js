import instance from "./axiosConfig";

const discountService = {
	// Get all discounts
	getDiscounts() {
		return instance.get("/discount");
	},

	// Get discount by ID
	getDiscountById(id) {
		return instance.get(`/discount/${id}`);
	},

	// Create new discount
	createDiscount(discountData) {
		return instance.post("/discount", discountData);
	},

	// Update discount
	updateDiscount(id, discountData) {
		return instance.put(`/discount/${id}`, discountData);
	},

	// Delete discount
	deleteDiscount(id) {
		return instance.delete(`/discount/${id}`);
	}, // Get products by discount ID
	getProductsByDiscountId(id) {
		return instance.get(`/discount/${id}/products`);
	},

	// Get discounts by product ID
	getDiscountsByProductId(productId) {
		return instance.get(`/discount/product/${productId}`);
	},

	// Add products to discount
	addProductsToDiscount(id, productIds) {
		return instance.post(`/discount/${id}/products`, productIds);
	},

	// Remove product from discount
	removeProductFromDiscount(id, productId) {
		return instance.delete(`/discount/${id}/products/${productId}`);
	},
};

export default discountService;
