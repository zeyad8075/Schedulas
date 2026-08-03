import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { classApi, CreateClassRequest, UpdateClassRequest } from '../api/classApi';
import { PaginationParams } from '../../../shared/types/pagination';

const CLASS_KEYS = {
  all: ['classes'] as const,
  lists: () => [...CLASS_KEYS.all, 'list'] as const,
  list: (params: any) => [...CLASS_KEYS.lists(), params] as const,
  details: () => [...CLASS_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...CLASS_KEYS.details(), id] as const,
};

export const useClasses = (params?: PaginationParams & { courseId?: string; academicTermId?: string; isActive?: boolean }) => {
  return useQuery({
    queryKey: CLASS_KEYS.list(params),
    queryFn: () => classApi.getClasses(params),
  });
};

export const useClass = (id: string) => {
  return useQuery({
    queryKey: CLASS_KEYS.detail(id),
    queryFn: () => classApi.getClassById(id),
    enabled: !!id,
  });
};

export const useCreateClass = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateClassRequest) => classApi.createClass(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLASS_KEYS.lists() });
    },
  });
};

export const useUpdateClass = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateClassRequest }) => classApi.updateClass(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: CLASS_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLASS_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteClass = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => classApi.deleteClass(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLASS_KEYS.lists() });
    },
  });
};

export const useRestoreClass = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => classApi.restoreClass(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLASS_KEYS.lists() });
    },
  });
};
