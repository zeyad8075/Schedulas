import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { programApi, CreateProgramRequest, UpdateProgramRequest } from '../api/programApi';
import { PaginationParams } from '../../../shared/types/pagination';

const PROGRAM_KEYS = {
  all: ['programs'] as const,
  lists: () => [...PROGRAM_KEYS.all, 'list'] as const,
  list: (params: any) => [...PROGRAM_KEYS.lists(), params] as const,
  details: () => [...PROGRAM_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...PROGRAM_KEYS.details(), id] as const,
};

export const usePrograms = (params?: PaginationParams & { departmentId?: string; isActive?: boolean }) => {
  return useQuery({
    queryKey: PROGRAM_KEYS.list(params),
    queryFn: () => programApi.getPrograms(params),
  });
};

export const useProgram = (id: string) => {
  return useQuery({
    queryKey: PROGRAM_KEYS.detail(id),
    queryFn: () => programApi.getProgramById(id),
    enabled: !!id,
  });
};

export const useCreateProgram = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateProgramRequest) => programApi.createProgram(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROGRAM_KEYS.lists() });
    },
  });
};

export const useUpdateProgram = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProgramRequest }) => programApi.updateProgram(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: PROGRAM_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: PROGRAM_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteProgram = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => programApi.deleteProgram(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROGRAM_KEYS.lists() });
    },
  });
};

export const useRestoreProgram = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => programApi.restoreProgram(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROGRAM_KEYS.lists() });
    },
  });
};
