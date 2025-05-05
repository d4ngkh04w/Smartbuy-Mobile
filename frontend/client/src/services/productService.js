import axiosInstance from "./axiosConfig";

export const getProducts = async (page, limit) => {
    try{
        const response = await axiosInstance.get(`/product/page?page=${page}&pageSize=${limit}`);
        if(response){
            return response.data;
        }
    } catch(error) {
        console.error("Error fetching products:", error);
        throw error;
    }
}