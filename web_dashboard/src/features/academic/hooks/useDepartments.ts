import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { departmentApi, CreateDepartmentRequest, UpdateDepartmentRequest } from '../api/departmentApi';
import { PaginationParams } from '../../../shared/types/pagination';

const DEPARTMENT_KEYS = {
  all: ['departments'] as const,
  lists: () => [...DEPARTMENT_KEYS.all, 'list'] as const,
  list: (params: any) => [...DEPARTMENT_KEYS.lists(), params] as const,
  details: () => [...DEPARTMENT_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...DEPARTMENT_KEYS.details(), id] as const,
};

export const useDepartments = (params?: PaginationParams & { institutionId?: string; isActive?: boolean }) => {
  return useQuery({
    queryKey: DEPARTMENT_KEYS.list(params),
    queryFn: () => departmentApi.getDepartments(params),
  });
};

export const useDepartment = (id: string) => {
  return useQuery({
    queryKey: DEPARTMENT_KEYS.detail(id),
    queryFn: () => departmentApi.getDepartmentById(id),
    enabled: !!id,
  });
};

export const useCreateDepartment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDepartmentRequest) => departmentApi.createDepartment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEPARTMENT_KEYS.lists() });
    },
  });
};

export const useUpdateDepartment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDepartmentRequest }) => departmentApi.updateDepartment(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: DEPARTMENT_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: DEPARTMENT_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteDepartment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => departmentApi.deleteDepartment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEPARTMENT_KEYS.lists() });
    },
  });
};

export const useRestoreDepartment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => departmentApi.restoreDepartment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEPARTMENT_KEYS.lists() });
    },
  });
};
