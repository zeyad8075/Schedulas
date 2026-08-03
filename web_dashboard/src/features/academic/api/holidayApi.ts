import { apiClient } from '../../../core/api/apiClient';
import { PaginatedList, PaginationParams } from '../../../shared/types/pagination';
import { HolidayDto } from '../models';

export interface CreateHolidayRequest {
  institutionId: string;
  academicTermId?: string;
  name: string;
  holidayDate: string;
}

export interface UpdateHolidayRequest {
  academicTermId?: string;
  name: string;
  holidayDate: string;
}

export const holidayApi = {
  getHolidays: async (params?: PaginationParams & { institutionId?: string; academicTermId?: string }): Promise<PaginatedList<HolidayDto>> => {
    const response = await apiClient.get<PaginatedList<HolidayDto>>('/holidays', { params });
    return response.data;
  },

  getHolidayById: async (id: string): Promise<HolidayDto> => {
    const response = await apiClient.get<HolidayDto>(`/holidays/${id}`);
    return response.data;
  },

  createHoliday: async (data: CreateHolidayRequest): Promise<HolidayDto> => {
    const response = await apiClient.post<HolidayDto>('/holidays', data);
    return response.data;
  },

  updateHoliday: async (id: string, data: UpdateHolidayRequest): Promise<HolidayDto> => {
    const response = await apiClient.put<HolidayDto>(`/holidays/${id}`, data);
    return response.data;
  },

  deleteHoliday: async (id: string): Promise<void> => {
    await apiClient.delete(`/holidays/${id}`);
  },

  restoreHoliday: async (id: string): Promise<void> => {
    await apiClient.post(`/holidays/${id}/restore`);
  }
};
