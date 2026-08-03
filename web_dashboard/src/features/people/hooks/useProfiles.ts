import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { profilesApi, CreateProfileRequest, UpdateProfileRequest } from '../api/profilesApi';
import { PaginationParams } from '../../../shared/types/pagination';

export const useProfiles = (params: PaginationParams) => {
  return useQuery({
    queryKey: ['profiles', params],
    queryFn: () => profilesApi.getProfiles(params),
  });
};

export const useProfile = (id: string) => {
  return useQuery({
    queryKey: ['profiles', id],
    queryFn: () => profilesApi.getProfile(id),
    enabled: !!id,
  });
};

export const useCreateProfile = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateProfileRequest) => profilesApi.createProfile(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
    },
  });
};

export const useUpdateProfile = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProfileRequest }) => profilesApi.updateProfile(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      queryClient.invalidateQueries({ queryKey: ['profiles', variables.id] });
    },
  });
};

export const useSuspendProfile = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => profilesApi.suspendProfile(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      queryClient.invalidateQueries({ queryKey: ['profiles', id] });
    },
  });
};

export const useActivateProfile = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => profilesApi.activateProfile(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      queryClient.invalidateQueries({ queryKey: ['profiles', id] });
    },
  });
};
