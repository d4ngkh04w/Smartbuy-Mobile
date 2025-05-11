import axiosInstance from "./axiosConfig";

export const getProducts = async (page, limit, filters = {}) => {
    try {
        const {
            search = "",
            sortBy = "newest",
            brand = null,
            minPrice = null,
            maxPrice = null,
        } = filters;

        const params = new URLSearchParams({
            page,
            pageSize: limit,
        });

        if (search) params.append("search", search);
        if (sortBy) params.append("sortBy", sortBy);
        if (brand) params.append("brand", brand);
        if (minPrice !== null) params.append("minPrice", minPrice);
        if (maxPrice !== null) params.append("maxPrice", maxPrice);

        const response = await axiosInstance.get(`/product/page?${params.toString()}`);
        if (response) {
            return response.data;
        }
    } catch (error) {
        console.error("Error fetching products:", error);
        throw error;
    }
}


export const getProductById = async (id) => {
    try{
        const response = await axiosInstance.get(`/product/${id}`);
        // console.log("product", response.data.data);
        if(response){
            return response.data.data;
        }
        
    }catch(error) {
        console.error("Error fetching products:", error);
        throw error;
    }
}

export const getBrands = async () => {
    try{
        const response = await axiosInstance.get("/brand");
        if(response){
            return response.data;
        }
    }catch(error) {
        console.error("Error fetching brands:", error);
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

export const getCarts = async () => {
    try{
        const response = await axiosInstance.get("/cart");
        if(response){
            console.log("carts", response.data);
            return response.data;
        }
    }catch(error) {
        console.error("Error fetching carts:", error);
        throw error;
    }
}