import { apiClient } from '../../../core/api/apiClient';
import { ParentDto } from '../models';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';

export interface CreateParentRequest {
  fullName: string;
  email: string;
}

export interface UpdateParentRequest {
  fullName: string;
}

export const parentsApi = {
  getParents: async (params: PaginationParams): Promise<PaginatedList<ParentDto>> => {
    const response = await apiClient.get<PaginatedList<ParentDto>>('/api/v1/parents', { params });
    return response.data;
  },

  getParent: async (id: string): Promise<ParentDto> => {
    const response = await apiClient.get<ParentDto>(`/api/v1/parents/${id}`);
    return response.data;
  },

  createParent: async (data: CreateParentRequest): Promise<ParentDto> => {
    const response = await apiClient.post<ParentDto>('/api/v1/parents', data);
    return response.data;
  },

  updateParent: async (id: string, data: UpdateParentRequest): Promise<void> => {
    await apiClient.put(`/api/v1/parents/${id}`, data);
  },

  deleteParent: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/v1/parents/${id}`);
  },

  restoreParent: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/parents/${id}/restore`);
  },
};
