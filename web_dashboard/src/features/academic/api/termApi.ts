import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { AcademicTermDto } from '../models';

export interface CreateTermRequest {
  institutionId: string;
  name: string;
  startDate: string;
  endDate: string;
}

export interface UpdateTermRequest {
  name: string;
  startDate: string;
  endDate: string;
}

export const termApi = {
  getTerms: async (params?: PaginationParams & { institutionId?: string; isActive?: boolean }): Promise<PaginatedList<AcademicTermDto>> => {
    const response = await apiClient.get<PaginatedList<AcademicTermDto>>('/academic-terms', { params });
    return response.data;
  },

  getTermById: async (id: string): Promise<AcademicTermDto> => {
    const response = await apiClient.get<AcademicTermDto>(`/academic-terms/${id}`);
    return response.data;
  },

  createTerm: async (data: CreateTermRequest): Promise<AcademicTermDto> => {
    const response = await apiClient.post<AcademicTermDto>('/academic-terms', data);
    return response.data;
  },

  updateTerm: async (id: string, data: UpdateTermRequest): Promise<AcademicTermDto> => {
    const response = await apiClient.put<AcademicTermDto>(`/academic-terms/${id}`, data);
    return response.data;
  },

  deleteTerm: async (id: string): Promise<void> => {
    await apiClient.delete(`/academic-terms/${id}`);
  },

  restoreTerm: async (id: string): Promise<void> => {
    await apiClient.post(`/academic-terms/${id}/restore`);
  }
};
