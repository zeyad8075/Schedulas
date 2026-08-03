import { apiClient } from '../../../core/api/apiClient';
import type { ProfileDto } from '../types';
import type { UpdateProfileFormData } from '../schemas/profileSchemas';

export const getMyProfileApi = async (): Promise<ProfileDto> => {
  const response = await apiClient.get<ProfileDto>('/profiles/me');
  return response.data;
};

export const updateProfileApi = async (data: UpdateProfileFormData): Promise<ProfileDto> => {
  const response = await apiClient.put<ProfileDto>('/profiles/me', data);
  return response.data;
};

export const updateThemeApi = async (preferredTheme: number): Promise<void> => {
  await apiClient.put('/profiles/me/theme', { preferredTheme });
};
