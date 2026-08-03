import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { activitiesApi, CreateActivityRequest, UpdateActivityRequest } from '../api/activitiesApi';
import { PaginationParams } from '../../../shared/types/pagination';

export const useActivities = (params: PaginationParams) => {
  return useQuery({
    queryKey: ['activities', params],
    queryFn: () => activitiesApi.getActivities(params),
  });
};

export const useActivity = (id: string) => {
  return useQuery({
    queryKey: ['activities', id],
    queryFn: () => activitiesApi.getActivity(id),
    enabled: !!id,
  });
};

export const useCreateActivity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateActivityRequest) => activitiesApi.createActivity(data),
    onSuccess: (data) => {
      if (data.success) {
        queryClient.invalidateQueries({ queryKey: ['activities'] });
        queryClient.invalidateQueries({ queryKey: ['calendar'] });
      }
    },
  });
};

export const useUpdateActivity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateActivityRequest }) =>
      activitiesApi.updateActivity(id, data),
    onSuccess: (data, variables) => {
      if (data.success) {
        queryClient.invalidateQueries({ queryKey: ['activities'] });
        queryClient.invalidateQueries({ queryKey: ['activities', variables.id] });
        queryClient.invalidateQueries({ queryKey: ['calendar'] });
      }
    },
  });
};

export const useCancelActivity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => activitiesApi.cancelActivity(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['activities'] });
      queryClient.invalidateQueries({ queryKey: ['activities', id] });
      queryClient.invalidateQueries({ queryKey: ['calendar'] });
    },
  });
};

export const useDeleteActivity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => activitiesApi.deleteActivity(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['activities'] });
      queryClient.invalidateQueries({ queryKey: ['activities', id] });
      queryClient.invalidateQueries({ queryKey: ['calendar'] });
    },
  });
};

export const useRestoreActivity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => activitiesApi.restoreActivity(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['activities'] });
      queryClient.invalidateQueries({ queryKey: ['activities', id] });
      queryClient.invalidateQueries({ queryKey: ['calendar'] });
    },
  });
};

export const useOverrideActivity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => activitiesApi.overrideActivity(id),
    onSuccess: (data, id) => {
      if (data.success) {
        queryClient.invalidateQueries({ queryKey: ['activities'] });
        queryClient.invalidateQueries({ queryKey: ['activities', id] });
        queryClient.invalidateQueries({ queryKey: ['calendar'] });
      }
    },
  });
};
