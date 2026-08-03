import { useQuery } from '@tanstack/react-query';
import { reportsApi } from '../api/reportsApi';

const REPORTS_KEYS = {
  all: ['reports'] as const,
  institutionWorkload: (params: any) => [...REPORTS_KEYS.all, 'institutionWorkload', params] as const,
  teacherWorkload: (teacherId: string, params: any) => [...REPORTS_KEYS.all, 'teacherWorkload', teacherId, params] as const,
  studentWorkload: (studentId: string, params: any) => [...REPORTS_KEYS.all, 'studentWorkload', studentId, params] as const,
  activityDistribution: (params: any) => [...REPORTS_KEYS.all, 'activityDistribution', params] as const,
};

export const useInstitutionWorkload = (params: { startDate: string; endDate: string }) => {
  return useQuery({
    queryKey: REPORTS_KEYS.institutionWorkload(params),
    queryFn: () => reportsApi.getInstitutionWorkload(params),
  });
};

export const useTeacherWorkload = (teacherId: string, params: { startDate: string; endDate: string }) => {
  return useQuery({
    queryKey: REPORTS_KEYS.teacherWorkload(teacherId, params),
    queryFn: () => reportsApi.getTeacherWorkload(teacherId, params),
    enabled: !!teacherId,
  });
};

export const useStudentWorkload = (studentId: string, params: { startDate: string; endDate: string }) => {
  return useQuery({
    queryKey: REPORTS_KEYS.studentWorkload(studentId, params),
    queryFn: () => reportsApi.getStudentWorkload(studentId, params),
    enabled: !!studentId,
  });
};

export const useActivityDistribution = (params: { startDate: string; endDate: string; institutionId?: string }) => {
  return useQuery({
    queryKey: REPORTS_KEYS.activityDistribution(params),
    queryFn: () => reportsApi.getActivityDistribution(params),
  });
};
