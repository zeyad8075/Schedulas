import { useMutation } from '@tanstack/react-query';
import { loginApi, forgotPasswordApi } from '../api/authApi';
import { useAuth } from './useAuth';
import type { LoginFormData, ForgotPasswordFormData } from '../schemas/authSchemas';
import { useNavigate } from 'react-router-dom';

export const useLoginMutation = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: LoginFormData) => loginApi(data),
    onSuccess: (data) => {
      login(data.accessToken, data.refreshToken);
      navigate('/');
    },
  });
};

export const useForgotPasswordMutation = () => {
  return useMutation({
    mutationFn: (data: ForgotPasswordFormData) => forgotPasswordApi(data),
  });
};
