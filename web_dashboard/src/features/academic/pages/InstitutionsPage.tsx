import { useState, useMemo } from 'react';
import { Box, Chip } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon, Restore as RestoreIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { useInstitutions, useCreateInstitution, useUpdateInstitution, useDeleteInstitution, useRestoreInstitution } from '../hooks/useInstitutions';
import { InstitutionDto} from '../models';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { InstitutionFormDialog } from '../forms/InstitutionFormDialog';

export const InstitutionsPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingInstitution, setEditingInstitution] = useState<InstitutionDto | undefined>();

  const { data, isLoading } = useInstitutions({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const createMutation = useCreateInstitution();
  const updateMutation = useUpdateInstitution();
  const deleteMutation = useDeleteInstitution();
  const restoreMutation = useRestoreInstitution();

  const handleOpenForm = (institution?: InstitutionDto) => {
    setEditingInstitution(institution);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingInstitution(undefined);
  };

  const handleSubmit = async (formData: any) => {
    if (editingInstitution) {
      await updateMutation.mutateAsync({ id: editingInstitution.id, data: formData });
    } else {
      await createMutation.mutateAsync(formData);
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

  const columns = useMemo<GridColDef<InstitutionDto>[]>(() => [
    { field: 'name', headerName: t('academic.institutionName', 'Institution Name'), flex: 1, minWidth: 200 },
    { field: 'type', headerName: t('academic.institutionType', 'Type'), width: 150, valueFormatter: (value) => t(`academic.institutionType_${value}`, value) },
    { field: 'timezone', headerName: t('academic.timezone', 'Timezone'), width: 150 },
    {
      field: 'status',
      headerName: t('common.status', 'Status'),
      width: 130,
      renderCell: (params) => (
        params.row.isSuspended ? (
          <Chip label={t('common.suspended', 'Suspended')} color="error" size="small" />
        ) : (
          <Chip label={t('common.active', 'Active')} color="success" size="small" />
        )
      ),
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: t('common.actions', 'Actions'),
      width: 150,
      getActions: (params) => {
        if (params.row.isSuspended) {
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
        title={t('academic.institutions', 'Institutions')}
        actionLabel={t('academic.addInstitution', 'Add Institution')}
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
        <InstitutionFormDialog
          open={formOpen}
          onClose={handleCloseForm}
          onSubmit={handleSubmit}
          initialData={editingInstitution}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </Box>
  );
};
