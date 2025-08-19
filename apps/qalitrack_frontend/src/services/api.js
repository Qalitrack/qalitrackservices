import axios from 'axios';
const api = axios.create({
  baseURL: '/api', // configure per environment
  timeout: 10000
});
export default api;
