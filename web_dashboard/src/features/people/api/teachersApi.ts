import { apiClient } from '../../../core/api/apiClient';
import { TeacherDto } from '../models';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';

export interface CreateTeacherRequest {
  fullName: string;
  email: string;
  institutionId: string;
  departmentId?: string;
}

export interface UpdateTeacherRequest {
  fullName: string;
  departmentId?: string;
}

export const teachersApi = {
  getTeachers: async (params: PaginationParams): Promise<PaginatedList<TeacherDto>> => {
    const response = await apiClient.get<PaginatedList<TeacherDto>>('/api/v1/teachers', { params });
    return response.data;
  },

  getTeacher: async (id: string): Promise<TeacherDto> => {
    const response = await apiClient.get<TeacherDto>(`/api/v1/teachers/${id}`);
    return response.data;
  },

  createTeacher: async (data: CreateTeacherRequest): Promise<TeacherDto> => {
    const response = await apiClient.post<TeacherDto>('/api/v1/teachers', data);
    return response.data;
  },

  updateTeacher: async (id: string, data: UpdateTeacherRequest): Promise<void> => {
    await apiClient.put(`/api/v1/teachers/${id}`, data);
  },

  deleteTeacher: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/v1/teachers/${id}`);
  },

  restoreTeacher: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/teachers/${id}/restore`);
  },
};
