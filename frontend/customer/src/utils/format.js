class format {
	formatPrice = (price) => {
		return new Intl.NumberFormat("vi-VN").format(price);
	};
	// Định dạng ngày tháng
	formatDate = (dateString) => {
		const options = { year: "numeric", month: "long", day: "numeric" };
		return new Date(dateString).toLocaleDateString("vi-VN", options);
	};
}
export default new format();
