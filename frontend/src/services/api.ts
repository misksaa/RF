import axios from 'axios';
import type { LookupDto, RegistrationDto } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5187';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const apiService = {
  getGovernorates: async (): Promise<LookupDto[]> => {
    const response = await apiClient.get<LookupDto[]>('/api/lookups/governorates');
    return response.data;
  },

  getCities: async (governorateId: number): Promise<LookupDto[]> => {
    const response = await apiClient.get<LookupDto[]>(`/api/lookups/cities?governorateId=${governorateId}`);
    return response.data;
  },

  createRegistration: async (data: Omit<RegistrationDto, 'id'>): Promise<{ id: string }> => {
    const response = await apiClient.post<{ id: string }>('/api/registrations', data);
    return response.data;
  },

  getRegistration: async (id: string): Promise<RegistrationDto> => {
    const response = await apiClient.get<RegistrationDto>(`/api/registrations/${id}`);
    return response.data;
  },
};
export default apiClient;
