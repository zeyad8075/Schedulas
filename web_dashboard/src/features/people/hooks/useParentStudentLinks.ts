import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { parentStudentLinksApi } from '../api/parentStudentLinksApi';

export const useParentStudents = (parentId: string) => {
  return useQuery({
    queryKey: ['parent-students', parentId],
    queryFn: () => parentStudentLinksApi.getParentStudents(parentId),
    enabled: !!parentId,
  });
};

export const useLinkParentToStudent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ parentId, studentId }: { parentId: string; studentId: string }) =>
      parentStudentLinksApi.linkParentToStudent(parentId, studentId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['parent-students', variables.parentId] });
    },
  });
};

export const useUnlinkParentFromStudent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ linkId }: { linkId: string; parentId: string }) =>
      parentStudentLinksApi.unlinkParentFromStudent(linkId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['parent-students', variables.parentId] });
    },
  });
};
