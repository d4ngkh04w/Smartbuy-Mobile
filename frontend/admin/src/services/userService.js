import instance from "./axiosConfig";

// Get all users (customers)
export const getUsers = async (params = {}) => {
	return await instance.get("/user");
};

// Get a specific user by ID
export const getUserById = async (id) => {
	return await instance.get(`/user/${id}`);
};

// Lock a user account
export const lockUser = async (id, reasonData) => {
	return await instance.put(`/user/${id}/lock`, reasonData);
};

// Unlock a user account
export const unlockUser = async (id) => {
	return await instance.put(`/user/${id}/unlock`);
};

// User service object with named functions
const userService = {
	getUsers,
	getUserById,
	lockUser,
	unlockUser,
};

export default userService;
