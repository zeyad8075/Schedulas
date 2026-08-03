import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { ClassDto } from '../models';

export interface CreateClassRequest {
  courseId: string;
  academicTermId: string;
  name: string;
}

export interface UpdateClassRequest {
  name: string;
}

export const classApi = {
  getClasses: async (params?: PaginationParams & { courseId?: string; academicTermId?: string; isActive?: boolean }): Promise<PaginatedList<ClassDto>> => {
    const response = await apiClient.get<PaginatedList<ClassDto>>('/classes', { params });
    return response.data;
  },

  getClassById: async (id: string): Promise<ClassDto> => {
    const response = await apiClient.get<ClassDto>(`/classes/${id}`);
    return response.data;
  },

  createClass: async (data: CreateClassRequest): Promise<ClassDto> => {
    const response = await apiClient.post<ClassDto>('/classes', data);
    return response.data;
  },

  updateClass: async (id: string, data: UpdateClassRequest): Promise<ClassDto> => {
    const response = await apiClient.put<ClassDto>(`/classes/${id}`, data);
    return response.data;
  },

  deleteClass: async (id: string): Promise<void> => {
    await apiClient.delete(`/classes/${id}`);
  },

  restoreClass: async (id: string): Promise<void> => {
    await apiClient.post(`/classes/${id}/restore`);
  }
};
