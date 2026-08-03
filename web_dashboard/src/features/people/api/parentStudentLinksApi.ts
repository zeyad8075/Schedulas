import { apiClient } from '../../../core/api/apiClient';
import { StudentDto } from '../models';

export const parentStudentLinksApi = {
  getParentStudents: async (parentId: string): Promise<StudentDto[]> => {
    const response = await apiClient.get<StudentDto[]>(`/api/v1/parent-student-links/parent/${parentId}/students`);
    return response.data;
  },

  linkParentToStudent: async (parentId: string, studentId: string): Promise<void> => {
    await apiClient.post(`/api/v1/parent-student-links/parent/${parentId}/student/${studentId}`);
  },

  unlinkParentFromStudent: async (linkId: string): Promise<void> => {
    await apiClient.delete(`/api/v1/parent-student-links/${linkId}`);
  },
};
