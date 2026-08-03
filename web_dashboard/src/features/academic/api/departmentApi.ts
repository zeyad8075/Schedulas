import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { DepartmentDto } from '../models';

export interface CreateDepartmentRequest {
  institutionId: string;
  name: string;
}

export interface UpdateDepartmentRequest {
  name: string;
}

export const departmentApi = {
  getDepartments: async (params?: PaginationParams & { institutionId?: string; isActive?: boolean }): Promise<PaginatedList<DepartmentDto>> => {
    const response = await apiClient.get<PaginatedList<DepartmentDto>>('/departments', { params });
    return response.data;
  },

  getDepartmentById: async (id: string): Promise<DepartmentDto> => {
    const response = await apiClient.get<DepartmentDto>(`/departments/${id}`);
    return response.data;
  },

  createDepartment: async (data: CreateDepartmentRequest): Promise<DepartmentDto> => {
    const response = await apiClient.post<DepartmentDto>('/departments', data);
    return response.data;
  },

  updateDepartment: async (id: string, data: UpdateDepartmentRequest): Promise<DepartmentDto> => {
    const response = await apiClient.put<DepartmentDto>(`/departments/${id}`, data);
    return response.data;
  },

  deleteDepartment: async (id: string): Promise<void> => {
    await apiClient.delete(`/departments/${id}`);
  },

  restoreDepartment: async (id: string): Promise<void> => {
    await apiClient.post(`/departments/${id}/restore`);
  }
};
