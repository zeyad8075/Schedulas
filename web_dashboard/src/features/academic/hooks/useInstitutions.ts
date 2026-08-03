import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { institutionApi, CreateInstitutionRequest, UpdateInstitutionRequest } from '../api/institutionApi';
import { PaginationParams } from '../../../shared/types/pagination';

const INSTITUTION_KEYS = {
  all: ['institutions'] as const,
  lists: () => [...INSTITUTION_KEYS.all, 'list'] as const,
  list: (params: any) => [...INSTITUTION_KEYS.lists(), params] as const,
  details: () => [...INSTITUTION_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...INSTITUTION_KEYS.details(), id] as const,
};

export const useInstitutions = (params?: PaginationParams & { isSuspended?: boolean }) => {
  return useQuery({
    queryKey: INSTITUTION_KEYS.list(params),
    queryFn: () => institutionApi.getInstitutions(params),
  });
};

export const useInstitution = (id: string) => {
  return useQuery({
    queryKey: INSTITUTION_KEYS.detail(id),
    queryFn: () => institutionApi.getInstitutionById(id),
    enabled: !!id,
  });
};

export const useCreateInstitution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateInstitutionRequest) => institutionApi.createInstitution(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.lists() });
    },
  });
};

export const useUpdateInstitution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateInstitutionRequest }) => institutionApi.updateInstitution(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.detail(variables.id) });
    },
  });
};

export const useSuspendInstitution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => institutionApi.suspendInstitution(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.detail(id) });
    },
  });
};

export const useReactivateInstitution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => institutionApi.reactivateInstitution(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.detail(id) });
    },
  });
};

export const useDeleteInstitution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => institutionApi.deleteInstitution(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.lists() });
    },
  });
};

export const useRestoreInstitution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => institutionApi.restoreInstitution(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: INSTITUTION_KEYS.lists() });
    },
  });
};
