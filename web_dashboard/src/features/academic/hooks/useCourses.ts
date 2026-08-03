import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { courseApi, CreateCourseRequest, UpdateCourseRequest } from '../api/courseApi';
import { PaginationParams } from '../../../shared/types/pagination';

const COURSE_KEYS = {
  all: ['courses'] as const,
  lists: () => [...COURSE_KEYS.all, 'list'] as const,
  list: (params: any) => [...COURSE_KEYS.lists(), params] as const,
  details: () => [...COURSE_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...COURSE_KEYS.details(), id] as const,
};

export const useCourses = (params?: PaginationParams & { programId?: string; isActive?: boolean }) => {
  return useQuery({
    queryKey: COURSE_KEYS.list(params),
    queryFn: () => courseApi.getCourses(params),
  });
};

export const useCourse = (id: string) => {
  return useQuery({
    queryKey: COURSE_KEYS.detail(id),
    queryFn: () => courseApi.getCourseById(id),
    enabled: !!id,
  });
};

export const useCreateCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCourseRequest) => courseApi.createCourse(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COURSE_KEYS.lists() });
    },
  });
};

export const useUpdateCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCourseRequest }) => courseApi.updateCourse(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: COURSE_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: COURSE_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => courseApi.deleteCourse(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COURSE_KEYS.lists() });
    },
  });
};

export const useRestoreCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => courseApi.restoreCourse(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COURSE_KEYS.lists() });
    },
  });
};
