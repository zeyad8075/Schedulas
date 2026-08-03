import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { ProgramDto } from '../models';

export interface CreateProgramRequest {
  departmentId: string;
  name: string;
}

export interface UpdateProgramRequest {
  name: string;
}

export const programApi = {
  getPrograms: async (params?: PaginationParams & { departmentId?: string; isActive?: boolean }): Promise<PaginatedList<ProgramDto>> => {
    const response = await apiClient.get<PaginatedList<ProgramDto>>('/programs', { params });
    return response.data;
  },

  getProgramById: async (id: string): Promise<ProgramDto> => {
    const response = await apiClient.get<ProgramDto>(`/programs/${id}`);
    return response.data;
  },

  createProgram: async (data: CreateProgramRequest): Promise<ProgramDto> => {
    const response = await apiClient.post<ProgramDto>('/programs', data);
    return response.data;
  },

  updateProgram: async (id: string, data: UpdateProgramRequest): Promise<ProgramDto> => {
    const response = await apiClient.put<ProgramDto>(`/programs/${id}`, data);
    return response.data;
  },

  deleteProgram: async (id: string): Promise<void> => {
    await apiClient.delete(`/programs/${id}`);
  },

  restoreProgram: async (id: string): Promise<void> => {
    await apiClient.post(`/programs/${id}/restore`);
  }
};
