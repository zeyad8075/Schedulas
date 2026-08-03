import { apiClient } from '../../../core/api/apiClient';
import { PaginatedNotificationsResponse } from '../models';
import { PaginationParams } from '../../../shared/types/pagination';

export const notificationsApi = {
  getNotifications: async (params: PaginationParams & { unreadOnly?: boolean; category?: string }): Promise<PaginatedNotificationsResponse> => {
    const response = await apiClient.get<PaginatedNotificationsResponse>('/notifications', { params });
    return response.data;
  },

  getUnreadCount: async (): Promise<number> => {
    const response = await apiClient.get<{ count: number }>('/notifications/unread-count');
    return response.data.count;
  },

  markAsRead: async (id: string): Promise<void> => {
    await apiClient.put(`/notifications/${id}/read`);
  },

  markAllAsRead: async (): Promise<void> => {
    await apiClient.put('/notifications/read-all');
  },
};
