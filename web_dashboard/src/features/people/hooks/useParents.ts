import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { parentsApi, CreateParentRequest, UpdateParentRequest } from '../api/parentsApi';
import { PaginationParams } from '../../../shared/types/pagination';

export const useParents = (params: PaginationParams) => {
  return useQuery({
    queryKey: ['parents', params],
    queryFn: () => parentsApi.getParents(params),
  });
};

export const useParent = (id: string) => {
  return useQuery({
    queryKey: ['parents', id],
    queryFn: () => parentsApi.getParent(id),
    enabled: !!id,
  });
};

export const useCreateParent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateParentRequest) => parentsApi.createParent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['parents'] });
    },
  });
};

export const useUpdateParent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateParentRequest }) => parentsApi.updateParent(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['parents'] });
      queryClient.invalidateQueries({ queryKey: ['parents', variables.id] });
    },
  });
};

export const useDeleteParent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => parentsApi.deleteParent(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['parents'] });
      queryClient.invalidateQueries({ queryKey: ['parents', id] });
    },
  });
};

export const useRestoreParent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => parentsApi.restoreParent(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['parents'] });
      queryClient.invalidateQueries({ queryKey: ['parents', id] });
    },
  });
};
