import { apiClient } from '../../../core/api/apiClient';
import { ActivityDto, ActivitySubmissionResult } from '../models';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';

export interface CreateActivityRequest {
  classId: string;
  activityType: number;
  title: string;
  description?: string;
  scheduledDate: string;
  scheduledTime?: string;
  endTime?: string;
  duration?: string;
  priority: number;
  estimatedWeight?: number;
  metadataJson?: string;
}

export interface UpdateActivityRequest extends CreateActivityRequest {}

export const activitiesApi = {
  getActivities: async (params: PaginationParams): Promise<PaginatedList<ActivityDto>> => {
    const response = await apiClient.get<PaginatedList<ActivityDto>>('/api/v1/activities', { params });
    return response.data;
  },

  getActivity: async (id: string): Promise<ActivityDto> => {
    const response = await apiClient.get<ActivityDto>(`/api/v1/activities/${id}`);
    return response.data;
  },

  createActivity: async (data: CreateActivityRequest): Promise<ActivitySubmissionResult> => {
    const response = await apiClient.post<ActivitySubmissionResult>('/api/v1/activities', data);
    return response.data;
  },

  updateActivity: async (id: string, data: UpdateActivityRequest): Promise<ActivitySubmissionResult> => {
    const response = await apiClient.put<ActivitySubmissionResult>(`/api/v1/activities/${id}`, data);
    return response.data;
  },

  cancelActivity: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/activities/${id}/cancel`);
  },

  deleteActivity: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/v1/activities/${id}`);
  },

  restoreActivity: async (id: string): Promise<void> => {
    await apiClient.post(`/api/v1/activities/${id}/restore`);
  },

  overrideActivity: async (id: string): Promise<ActivitySubmissionResult> => {
    const response = await apiClient.post<ActivitySubmissionResult>(`/api/v1/activities/${id}/override`);
    return response.data;
  },
};
