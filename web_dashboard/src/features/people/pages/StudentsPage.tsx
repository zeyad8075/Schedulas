import { useState, useMemo } from 'react';
import { Box, Chip } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon, Restore as RestoreIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { useStudents, useCreateStudent, useUpdateStudent, useDeleteStudent, useRestoreStudent } from '../hooks/useStudents';
import { StudentDto } from '../models';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { StudentFormDialog } from '../forms/StudentFormDialog';

export const StudentsPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingStudent, setEditingStudent] = useState<StudentDto | undefined>();

  const { data, isLoading } = useStudents({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const createMutation = useCreateStudent();
  const updateMutation = useUpdateStudent();
  const deleteMutation = useDeleteStudent();
  const restoreMutation = useRestoreStudent();

  const handleOpenForm = (student?: StudentDto) => {
    setEditingStudent(student);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingStudent(undefined);
  };

  const handleSubmit = async (formData: any) => {
    if (editingStudent) {
      await updateMutation.mutateAsync({ id: editingStudent.id, data: formData });
    } else {
      await createMutation.mutateAsync({ ...formData, institutionId: 'current-inst-id' });
    }
    handleCloseForm();
  };

  const handleDelete = async (id: string) => {
    if (window.confirm(t('common.confirmDelete', 'Are you sure you want to delete this?'))) {
      await deleteMutation.mutateAsync(id);
    }
  };

  const handleRestore = async (id: string) => {
    await restoreMutation.mutateAsync(id);
  };

  const columns = useMemo<GridColDef<StudentDto>[]>(() => [
    { field: 'fullName', headerName: t('people.fullName', 'Full Name'), flex: 1, minWidth: 200 },
    { field: 'studentNumber', headerName: t('people.studentNumber', 'Student ID'), width: 150 },
    { field: 'email', headerName: t('people.email', 'Email'), width: 200 },
    {
      field: 'status',
      headerName: t('common.status', 'Status'),
      width: 130,
      renderCell: (params) => (
        params.row.isActive ? (
          <Chip label={t('common.active', 'Active')} color="success" size="small" />
        ) : (
          <Chip label={t('common.inactive', 'Inactive')} color="default" size="small" />
        )
      ),
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: t('common.actions', 'Actions'),
      width: 150,
      getActions: (params) => {
        if (!params.row.isActive) {
          return [
            <GridActionsCellItem key={Math.random()}
              icon={<RestoreIcon />}
              label={t('common.restore', 'Restore')}
              onClick={() => handleRestore(params.row.id)}
            />,
          ];
        }
        return [
          <GridActionsCellItem key={Math.random()}
            icon={<EditIcon />}
            label={t('common.edit', 'Edit')}
            onClick={() => handleOpenForm(params.row)}
          />,
          <GridActionsCellItem key={Math.random()}
            icon={<DeleteIcon />}
            label={t('common.delete', 'Delete')}
            onClick={() => handleDelete(params.row.id)}
          />,
        ];
      },
    },
  // oxlint-disable-next-line react-hooks/exhaustive-deps
    ], [t]);

  return (
    <Box>
      <PageHeader
        title={t('people.students', 'Students')}
        actionLabel={t('people.addStudent', 'Add Student')}
        onActionClick={() => handleOpenForm()}
      />

      <DataTable
        columns={columns}
        rows={data?.items || []}
        rowCount={data?.totalCount || 0}
        loading={isLoading}
        page={pagination.page}
        pageSize={pagination.pageSize}
        onPageChange={pagination.setPage}
        onPageSizeChange={pagination.setPageSize}
        onSearch={pagination.setSearchTerm}
        searchTerm={pagination.searchTerm}
      />

      {formOpen && (
        <StudentFormDialog
          open={formOpen}
          onClose={handleCloseForm}
          onSubmit={handleSubmit}
          initialData={editingStudent}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </Box>
  );
};
