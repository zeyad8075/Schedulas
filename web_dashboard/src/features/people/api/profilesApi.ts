import { apiClient } from '../../../core/api/apiClient';
import { ProfileDto, Theme } from '../models';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { UserRole } from '../../auth/types';

export interface CreateProfileRequest {
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: UserRole;
  institutionId?: string;
  departmentId?: string;
  preferredTheme?: Theme;
}

export interface UpdateProfileRequest {
  fullName: string;
  phoneNumber?: string;
  departmentId?: string;
}

export const profilesApi = {
  getProfiles: async (params: PaginationParams): Promise<PaginatedList<ProfileDto>> => {
    const response = await apiClient.get<PaginatedList<ProfileDto>>('/api/v1/profiles', { params });
    return response.data;
  },

  getProfile: async (id: string): Promise<ProfileDto> => {
    const response = await apiClient.get<ProfileDto>(`/api/v1/profiles/${id}`);
    return response.data;
  },

  getMyProfile: async (): Promise<ProfileDto> => {
    const response = await apiClient.get<ProfileDto>('/api/v1/profiles/me');
    return response.data;
  },

  createProfile: async (data: CreateProfileRequest): Promise<ProfileDto> => {
    const response = await apiClient.post<ProfileDto>('/api/v1/profiles', data);
    return response.data;
  },

  updateProfile: async (id: string, data: UpdateProfileRequest): Promise<void> => {
    await apiClient.put(`/api/v1/profiles/${id}`, data);
  },

  updateTheme: async (id: string, theme: Theme): Promise<void> => {
    await apiClient.put(`/api/v1/profiles/${id}/theme`, { preferredTheme: theme });
  },

  suspendProfile: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/profiles/${id}/suspend`);
  },

  activateProfile: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/profiles/${id}/activate`);
  },
};
