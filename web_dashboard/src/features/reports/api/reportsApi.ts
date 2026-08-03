import { apiClient } from '../../../core/api/apiClient';
import { WorkloadReportDto, ActivityDistributionReportDto } from '../models';

export const reportsApi = {
  getInstitutionWorkload: async (params: { startDate: string; endDate: string }): Promise<WorkloadReportDto> => {
    const response = await apiClient.get<WorkloadReportDto>('/reports/institution/workload', { params });
    return response.data;
  },

  getTeacherWorkload: async (teacherId: string, params: { startDate: string; endDate: string }): Promise<WorkloadReportDto> => {
    const response = await apiClient.get<WorkloadReportDto>(`/reports/teachers/${teacherId}/workload`, { params });
    return response.data;
  },

  getStudentWorkload: async (studentId: string, params: { startDate: string; endDate: string }): Promise<WorkloadReportDto> => {
    const response = await apiClient.get<WorkloadReportDto>(`/reports/students/${studentId}/workload`, { params });
    return response.data;
  },

  getActivityDistribution: async (params: { startDate: string; endDate: string; institutionId?: string }): Promise<ActivityDistributionReportDto> => {
    const response = await apiClient.get<ActivityDistributionReportDto>('/reports/activity-distribution', { params });
    return response.data;
  },
};
