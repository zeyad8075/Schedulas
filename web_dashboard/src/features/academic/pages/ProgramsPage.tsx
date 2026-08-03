import { useState, useMemo } from 'react';
import { Box, Chip } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon, Restore as RestoreIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { usePrograms, useCreateProgram, useUpdateProgram, useDeleteProgram, useRestoreProgram } from '../hooks/usePrograms';
import { ProgramDto } from '../models';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { ProgramFormDialog } from '../forms/ProgramFormDialog';

export const ProgramsPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingProgram, setEditingProgram] = useState<ProgramDto | undefined>();

  const { data, isLoading } = usePrograms({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const createMutation = useCreateProgram();
  const updateMutation = useUpdateProgram();
  const deleteMutation = useDeleteProgram();
  const restoreMutation = useRestoreProgram();

  const handleOpenForm = (program?: ProgramDto) => {
    setEditingProgram(program);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingProgram(undefined);
  };

  const handleSubmit = async (formData: any) => {
    if (editingProgram) {
      await updateMutation.mutateAsync({ id: editingProgram.id, data: formData });
    } else {
      await createMutation.mutateAsync({ ...formData, departmentId: 'current-dept-id' });
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

  const columns = useMemo<GridColDef<ProgramDto>[]>(() => [
    { field: 'name', headerName: t('academic.programName', 'Program Name'), flex: 1, minWidth: 200 },
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
        title={t('academic.programs', 'Programs')}
        actionLabel={t('academic.addProgram', 'Add Program')}
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
        <ProgramFormDialog
          open={formOpen}
          onClose={handleCloseForm}
          onSubmit={handleSubmit}
          initialData={editingProgram}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </Box>
  );
};
