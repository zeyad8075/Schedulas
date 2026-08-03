import { apiClient } from '../../../core/api/apiClient';
import { StudentDto } from '../models';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';

export interface CreateStudentRequest {
  fullName: string;
  email: string;
  institutionId: string;
  studentNumber?: string;
}

export interface UpdateStudentRequest {
  fullName: string;
  studentNumber?: string;
}

export const studentsApi = {
  getStudents: async (params: PaginationParams): Promise<PaginatedList<StudentDto>> => {
    const response = await apiClient.get<PaginatedList<StudentDto>>('/api/v1/students', { params });
    return response.data;
  },

  getStudent: async (id: string): Promise<StudentDto> => {
    const response = await apiClient.get<StudentDto>(`/api/v1/students/${id}`);
    return response.data;
  },

  createStudent: async (data: CreateStudentRequest): Promise<StudentDto> => {
    const response = await apiClient.post<StudentDto>('/api/v1/students', data);
    return response.data;
  },

  updateStudent: async (id: string, data: UpdateStudentRequest): Promise<void> => {
    await apiClient.put(`/api/v1/students/${id}`, data);
  },

  deleteStudent: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/v1/students/${id}`);
  },

  restoreStudent: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/students/${id}/restore`);
  },
};
