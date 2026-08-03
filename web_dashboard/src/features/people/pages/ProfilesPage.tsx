import { useState, useMemo } from 'react';
import { Box, Chip } from '@mui/material';
import { Edit as EditIcon, PlayArrow as ActivateIcon, Pause as SuspendIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { useProfiles, useCreateProfile, useUpdateProfile, useSuspendProfile, useActivateProfile } from '../hooks/useProfiles';
import { ProfileDto } from '../models';
import { UserRole } from '../../auth/types';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { ProfileFormDialog } from '../forms/ProfileFormDialog';

export const ProfilesPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingProfile, setEditingProfile] = useState<ProfileDto | undefined>();

  const { data, isLoading } = useProfiles({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const createMutation = useCreateProfile();
  const updateMutation = useUpdateProfile();
  const suspendMutation = useSuspendProfile();
  const activateMutation = useActivateProfile();

  const handleOpenForm = (profile?: ProfileDto) => {
    setEditingProfile(profile);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingProfile(undefined);
  };

  const handleSubmit = async (formData: any) => {
    if (editingProfile) {
      await updateMutation.mutateAsync({ id: editingProfile.id, data: formData });
    } else {
      await createMutation.mutateAsync(formData);
    }
    handleCloseForm();
  };

  const handleSuspend = async (id: string) => {
    if (window.confirm(t('people.confirmSuspend', 'Are you sure you want to suspend this user?'))) {
      await suspendMutation.mutateAsync(id);
    }
  };

  const handleActivate = async (id: string) => {
    await activateMutation.mutateAsync(id);
  };

  const columns = useMemo<GridColDef<ProfileDto>[]>(() => [
    { field: 'fullName', headerName: t('people.fullName', 'Full Name'), flex: 1, minWidth: 200 },
    { field: 'email', headerName: t('people.email', 'Email'), width: 200 },
    { field: 'phoneNumber', headerName: t('people.phoneNumber', 'Phone'), width: 150 },
    { 
      field: 'role', 
      headerName: t('people.role', 'Role'), 
      width: 150,
      valueFormatter: (value) => t(`roles.${UserRole[value]}`, UserRole[value])
    },
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
        const actions = [
          <GridActionsCellItem key={Math.random()}
            icon={<EditIcon />}
            label={t('common.edit', 'Edit')}
            onClick={() => handleOpenForm(params.row)}
          />,
        ];
        
        if (params.row.isActive) {
          actions.push(
            <GridActionsCellItem key={Math.random()}
              icon={<SuspendIcon />}
              label={t('people.suspend', 'Suspend')}
              onClick={() => handleSuspend(params.row.id)}
            />
          );
        } else {
          actions.push(
            <GridActionsCellItem key={Math.random()}
              icon={<ActivateIcon />}
              label={t('people.activate', 'Activate')}
              onClick={() => handleActivate(params.row.id)}
            />
          );
        }
        
        return actions;
      },
    },
  // oxlint-disable-next-line react-hooks/exhaustive-deps
    ], [t]);

  return (
    <Box>
      <PageHeader
        title={t('people.profiles', 'Profiles')}
        actionLabel={t('people.addProfile', 'Add Profile')}
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
        <ProfileFormDialog
          open={formOpen}
          onClose={handleCloseForm}
          onSubmit={handleSubmit}
          initialData={editingProfile}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </Box>
  );
};
