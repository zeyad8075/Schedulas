import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getMyProfileApi, updateProfileApi, updateThemeApi } from '../api/profileApi';
import type { UpdateProfileFormData } from '../schemas/profileSchemas';

export const profileKeys = {
  all: ['profile'] as const,
  me: () => [...profileKeys.all, 'me'] as const,
};

export const useMyProfile = () => {
  return useQuery({
    queryKey: profileKeys.me(),
    queryFn: getMyProfileApi,
  });
};

export const useUpdateProfileMutation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: UpdateProfileFormData) => updateProfileApi(data),
    onSuccess: (data) => {
      queryClient.setQueryData(profileKeys.me(), data);
    },
  });
};

export const useUpdateThemeMutation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (theme: number) => updateThemeApi(theme),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: profileKeys.me() });
    }
  });
};
