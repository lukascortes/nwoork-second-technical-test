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

// On an expired / invalid token, clear the session and bounce to login.
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    if (status === 401 && !window.location.pathname.includes('/login')) {
      localStorage.removeItem('token');
      localStorage.removeItem('userRole');
      localStorage.removeItem('userId');
      localStorage.removeItem('fullName');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;
