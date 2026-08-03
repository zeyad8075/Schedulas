import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { termApi, CreateTermRequest, UpdateTermRequest } from '../api/termApi';
import { PaginationParams } from '../../../shared/types/pagination';

const TERM_KEYS = {
  all: ['terms'] as const,
  lists: () => [...TERM_KEYS.all, 'list'] as const,
  list: (params: any) => [...TERM_KEYS.lists(), params] as const,
  details: () => [...TERM_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...TERM_KEYS.details(), id] as const,
};

export const useTerms = (params?: PaginationParams & { institutionId?: string; isActive?: boolean }) => {
  return useQuery({
    queryKey: TERM_KEYS.list(params),
    queryFn: () => termApi.getTerms(params),
  });
};

export const useTerm = (id: string) => {
  return useQuery({
    queryKey: TERM_KEYS.detail(id),
    queryFn: () => termApi.getTermById(id),
    enabled: !!id,
  });
};

export const useCreateTerm = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTermRequest) => termApi.createTerm(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: TERM_KEYS.lists() });
    },
  });
};

export const useUpdateTerm = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTermRequest }) => termApi.updateTerm(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: TERM_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: TERM_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteTerm = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => termApi.deleteTerm(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: TERM_KEYS.lists() });
    },
  });
};

export const useRestoreTerm = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => termApi.restoreTerm(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: TERM_KEYS.lists() });
    },
  });
};
