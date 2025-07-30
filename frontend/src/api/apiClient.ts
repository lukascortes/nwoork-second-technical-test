import axios from 'axios';
const { VITE_API_BASE_URL } = import.meta.env;

const apiClient = axios.create({
  baseURL: VITE_API_BASE_URL,
  timeout: 10000,
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;