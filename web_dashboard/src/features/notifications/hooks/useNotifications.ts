import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsApi } from '../api/notificationsApi';
import { PaginationParams } from '../../../shared/types/pagination';

const NOTIFICATIONS_KEYS = {
  all: ['notifications'] as const,
  lists: () => [...NOTIFICATIONS_KEYS.all, 'list'] as const,
  list: (params: any) => [...NOTIFICATIONS_KEYS.lists(), params] as const,
  unreadCount: () => [...NOTIFICATIONS_KEYS.all, 'unreadCount'] as const,
};

export const useNotifications = (params: PaginationParams & { unreadOnly?: boolean; category?: string }) => {
  return useQuery({
    queryKey: NOTIFICATIONS_KEYS.list(params),
    queryFn: () => notificationsApi.getNotifications(params),
  });
};

export const useUnreadNotificationsCount = () => {
  return useQuery({
    queryKey: NOTIFICATIONS_KEYS.unreadCount(),
    queryFn: () => notificationsApi.getUnreadCount(),
    refetchInterval: 60000, // Poll every minute
  });
};

export const useMarkNotificationRead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.all });
    },
  });
};

export const useMarkAllNotificationsRead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.all });
    },
  });
};
