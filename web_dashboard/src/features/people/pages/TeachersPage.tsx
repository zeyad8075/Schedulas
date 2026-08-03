import { useState, useMemo } from 'react';
import { Box, Chip } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon, Restore as RestoreIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { useTeachers, useCreateTeacher, useUpdateTeacher, useDeleteTeacher, useRestoreTeacher } from '../hooks/useTeachers';
import { TeacherDto } from '../models';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { TeacherFormDialog } from '../forms/TeacherFormDialog';

export const TeachersPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingTeacher, setEditingTeacher] = useState<TeacherDto | undefined>();

  const { data, isLoading } = useTeachers({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const createMutation = useCreateTeacher();
  const updateMutation = useUpdateTeacher();
  const deleteMutation = useDeleteTeacher();
  const restoreMutation = useRestoreTeacher();

  const handleOpenForm = (teacher?: TeacherDto) => {
    setEditingTeacher(teacher);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingTeacher(undefined);
  };

  const handleSubmit = async (formData: any) => {
    if (editingTeacher) {
      await updateMutation.mutateAsync({ id: editingTeacher.id, data: formData });
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

  const columns = useMemo<GridColDef<TeacherDto>[]>(() => [
    { field: 'fullName', headerName: t('people.fullName', 'Full Name'), flex: 1, minWidth: 200 },
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
        title={t('people.teachers', 'Teachers')}
        actionLabel={t('people.addTeacher', 'Add Teacher')}
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
        <TeacherFormDialog
          open={formOpen}
          onClose={handleCloseForm}
          onSubmit={handleSubmit}
          initialData={editingTeacher}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </Box>
  );
};
