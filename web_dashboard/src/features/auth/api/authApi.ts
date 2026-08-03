import { apiClient } from '../../../core/api/apiClient';
import type { LoginFormData, ForgotPasswordFormData } from '../schemas/authSchemas';

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export const loginApi = async (data: LoginFormData): Promise<LoginResponse> => {
  const response = await apiClient.post<LoginResponse>('/auth/login', data);
  return response.data;
};

export const forgotPasswordApi = async (data: ForgotPasswordFormData): Promise<void> => {
  await apiClient.post('/auth/forgot-password', data);
};
