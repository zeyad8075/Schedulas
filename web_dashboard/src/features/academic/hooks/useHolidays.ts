import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { holidayApi, CreateHolidayRequest, UpdateHolidayRequest } from '../api/holidayApi';
import { PaginationParams } from '../../../shared/types/pagination';

const HOLIDAY_KEYS = {
  all: ['holidays'] as const,
  lists: () => [...HOLIDAY_KEYS.all, 'list'] as const,
  list: (params: any) => [...HOLIDAY_KEYS.lists(), params] as const,
  details: () => [...HOLIDAY_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...HOLIDAY_KEYS.details(), id] as const,
};

export const useHolidays = (params?: PaginationParams & { institutionId?: string; academicTermId?: string }) => {
  return useQuery({
    queryKey: HOLIDAY_KEYS.list(params),
    queryFn: () => holidayApi.getHolidays(params),
  });
};

export const useHoliday = (id: string) => {
  return useQuery({
    queryKey: HOLIDAY_KEYS.detail(id),
    queryFn: () => holidayApi.getHolidayById(id),
    enabled: !!id,
  });
};

export const useCreateHoliday = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateHolidayRequest) => holidayApi.createHoliday(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HOLIDAY_KEYS.lists() });
    },
  });
};

export const useUpdateHoliday = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateHolidayRequest }) => holidayApi.updateHoliday(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: HOLIDAY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: HOLIDAY_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteHoliday = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => holidayApi.deleteHoliday(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HOLIDAY_KEYS.lists() });
    },
  });
};

export const useRestoreHoliday = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => holidayApi.restoreHoliday(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HOLIDAY_KEYS.lists() });
    },
  });
};
