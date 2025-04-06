import axios from 'axios';

// Gửi username + password để đăng nhập
export const login = (credentials) => {
     return axios.post('/auth/login', credentials);
};

// Gửi thông tin người dùng để đăng ký
export const register = (userInfo) => {
    return axios.post('/auth/register', userInfo);
};


