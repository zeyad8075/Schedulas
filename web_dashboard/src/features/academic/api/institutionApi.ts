import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { InstitutionDto, InstitutionType } from '../models';

export interface CreateInstitutionRequest {
  name: string;
  type: InstitutionType;
  timezone: string;
}

export interface UpdateInstitutionRequest {
  name: string;
  logoUrl?: string;
  timezone: string;
}

export const institutionApi = {
  getInstitutions: async (params?: PaginationParams & { isSuspended?: boolean }): Promise<PaginatedList<InstitutionDto>> => {
    const response = await apiClient.get<PaginatedList<InstitutionDto>>('/institutions', { params });
    return response.data;
  },

  getInstitutionById: async (id: string): Promise<InstitutionDto> => {
    const response = await apiClient.get<InstitutionDto>(`/institutions/${id}`);
    return response.data;
  },

  createInstitution: async (data: CreateInstitutionRequest): Promise<InstitutionDto> => {
    const response = await apiClient.post<InstitutionDto>('/institutions', data);
    return response.data;
  },

  updateInstitution: async (id: string, data: UpdateInstitutionRequest): Promise<InstitutionDto> => {
    const response = await apiClient.put<InstitutionDto>(`/institutions/${id}`, data);
    return response.data;
  },

  suspendInstitution: async (id: string): Promise<void> => {
    await apiClient.post(`/institutions/${id}/suspend`);
  },

  reactivateInstitution: async (id: string): Promise<void> => {
    await apiClient.post(`/institutions/${id}/reactivate`);
  },

  deleteInstitution: async (id: string): Promise<void> => {
    await apiClient.delete(`/institutions/${id}`);
  },

  restoreInstitution: async (id: string): Promise<void> => {
    await apiClient.post(`/institutions/${id}/restore`);
  }
};
