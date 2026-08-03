import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { CourseDto } from '../models';

export interface CreateCourseRequest {
  programId: string;
  name: string;
  code?: string;
}

export interface UpdateCourseRequest {
  name: string;
  code?: string;
}

export const courseApi = {
  getCourses: async (params?: PaginationParams & { programId?: string; isActive?: boolean }): Promise<PaginatedList<CourseDto>> => {
    const response = await apiClient.get<PaginatedList<CourseDto>>('/courses', { params });
    return response.data;
  },

  getCourseById: async (id: string): Promise<CourseDto> => {
    const response = await apiClient.get<CourseDto>(`/courses/${id}`);
    return response.data;
  },

  createCourse: async (data: CreateCourseRequest): Promise<CourseDto> => {
    const response = await apiClient.post<CourseDto>('/courses', data);
    return response.data;
  },

  updateCourse: async (id: string, data: UpdateCourseRequest): Promise<CourseDto> => {
    const response = await apiClient.put<CourseDto>(`/courses/${id}`, data);
    return response.data;
  },

  deleteCourse: async (id: string): Promise<void> => {
    await apiClient.delete(`/courses/${id}`);
  },

  restoreCourse: async (id: string): Promise<void> => {
    await apiClient.post(`/courses/${id}/restore`);
  }
};
