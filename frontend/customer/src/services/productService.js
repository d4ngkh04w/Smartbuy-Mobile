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

export const getProductById = async (id) => {
    try{
        const response = await axiosInstance.get(`/product/${id}`);
        console.log("product", response.data.product);
        if(response){
            return response.data.product;
        }
        
    }catch(error) {
        console.error("Error fetching products:", error);
        throw error;
    }
}

export const getUrlImage = (url) => {
    try{
        let newUrlImage = url.startsWith('http') ? url : "http://localhost:5000" + `/${url}`;
        return newUrlImage;
    }catch(error) {
        console.error("Error get Url Image:", error);
        throw error;
    }
}