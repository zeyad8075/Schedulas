import { apiClient } from '../../../core/api/apiClient';
import type { LoginFormData, ForgotPasswordFormData } from '../schemas/authSchemas';

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: any;
  meta?: any;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export const loginApi = async (data: LoginFormData): Promise<LoginResponse> => {
  const response = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', data);
  return response.data.data;
};

export const forgotPasswordApi = async (data: ForgotPasswordFormData): Promise<void> => {
  await apiClient.post('/auth/forgot-password', data);
};
