import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { teachersApi, CreateTeacherRequest, UpdateTeacherRequest } from '../api/teachersApi';
import { PaginationParams } from '../../../shared/types/pagination';

export const useTeachers = (params: PaginationParams) => {
  return useQuery({
    queryKey: ['teachers', params],
    queryFn: () => teachersApi.getTeachers(params),
  });
};

export const useTeacher = (id: string) => {
  return useQuery({
    queryKey: ['teachers', id],
    queryFn: () => teachersApi.getTeacher(id),
    enabled: !!id,
  });
};

export const useCreateTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTeacherRequest) => teachersApi.createTeacher(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
    },
  });
};

export const useUpdateTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTeacherRequest }) => teachersApi.updateTeacher(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
      queryClient.invalidateQueries({ queryKey: ['teachers', variables.id] });
    },
  });
};

export const useDeleteTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => teachersApi.deleteTeacher(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
      queryClient.invalidateQueries({ queryKey: ['teachers', id] });
    },
  });
};

export const useRestoreTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => teachersApi.restoreTeacher(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
      queryClient.invalidateQueries({ queryKey: ['teachers', id] });
    },
  });
};
