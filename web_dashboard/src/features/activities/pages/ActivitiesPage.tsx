import { useState, useMemo } from 'react';
import { Box, Chip } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon, Restore as RestoreIcon, Cancel as CancelIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { useActivities, useDeleteActivity, useRestoreActivity, useCancelActivity } from '../hooks/useActivities';
import { ActivityDto, ActivityStatus, ActivityType } from '../models';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { ActivityFormDialog } from '../forms/ActivityFormDialog';

export const ActivitiesPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingActivity, setEditingActivity] = useState<ActivityDto | undefined>();

  const { data, isLoading } = useActivities({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const deleteMutation = useDeleteActivity();
  const restoreMutation = useRestoreActivity();
  const cancelMutation = useCancelActivity();

  const handleOpenForm = (activity?: ActivityDto) => {
    setEditingActivity(activity);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingActivity(undefined);
  };

  const getStatusColor = (status: ActivityStatus) => {
    switch (status) {
      case ActivityStatus.Scheduled:
        return 'primary';
      case ActivityStatus.InProgress:
        return 'info';
      case ActivityStatus.Completed:
        return 'success';
      case ActivityStatus.Cancelled:
        return 'error';
      case ActivityStatus.RequiresOverrideApproved:
        return 'warning';
      default:
        return 'default';
    }
  };

  const columns = useMemo<GridColDef[]>(
    () => [
      {
        field: 'title',
        headerName: t('activities.title', 'Title'),
        flex: 1,
      },
      {
        field: 'activityType',
        headerName: t('activities.type', 'Type'),
        width: 150,
        renderCell: (params) => (
          <Chip
            label={t(`activities.types.${(ActivityType as any)[params.value]}`, (ActivityType as any)[params.value]) as string}
            size="small"
            variant="outlined"
          />
        ),
      },
      {
        field: 'scheduledDate',
        headerName: t('activities.scheduledDate', 'Scheduled Date'),
        width: 150,
      },
      {
        field: 'scheduledTime',
        headerName: t('activities.scheduledTime', 'Time'),
        width: 150,
        valueGetter: (_, row) => row.scheduledTime ? `${row.scheduledTime} - ${row.endTime || ''}` : '-',
      },
      {
        field: 'status',
        headerName: t('activities.status', 'Status'),
        width: 150,
        renderCell: (params) => (
          <Chip
            label={t(`activities.statuses.${(ActivityStatus as any)[params.value]}`, (ActivityStatus as any)[params.value]) as string}
            color={getStatusColor(params.value)}
            size="small"
          />
        ),
      },
      {
        field: 'actions',
        type: 'actions',
        headerName: t('common.actions', 'Actions'),
        width: 150,
        getActions: (params) => {
          const actions = [
            <GridActionsCellItem
              key="edit"
              icon={<EditIcon />}
              label={t('common.edit', 'Edit')}
              onClick={() => handleOpenForm(params.row as ActivityDto)}
            />,
          ];

          if (params.row.status !== ActivityStatus.Cancelled) {
            actions.push(
              <GridActionsCellItem
                key="cancel"
                icon={<CancelIcon />}
                label={t('activities.cancel', 'Cancel')}
                onClick={() => {
                  if (window.confirm(t('activities.confirmCancel', 'Are you sure you want to cancel this activity?') as string)) {
                    cancelMutation.mutate(params.row.id);
                  }
                }}
              />
            );
          }

          actions.push(
            <GridActionsCellItem
              key="delete"
              icon={<DeleteIcon />}
              label={t('common.delete', 'Delete')}
              onClick={() => {
                if (window.confirm(t('common.confirmDelete', 'Are you sure you want to delete this?') as string)) {
                  deleteMutation.mutate(params.row.id);
                }
              }}
            />
          );

          actions.push(
            <GridActionsCellItem
              key="restore"
              icon={<RestoreIcon />}
              label={t('common.restore', 'Restore')}
              onClick={() => restoreMutation.mutate(params.row.id)}
            />
          );

          return actions;
        },
      },
    ],
    [t, cancelMutation, deleteMutation, restoreMutation]
  );

  return (
    <Box>
      <PageHeader
        title={t('activities.title', 'Activities')}
        onActionClick={() => handleOpenForm()}
        actionLabel={t('activities.addActivity', 'Add Activity')}
      />

      <DataTable
        rows={data?.items || []}
        columns={columns}
        loading={isLoading}
        rowCount={data?.totalCount || 0}
        page={pagination.page - 1}
        pageSize={pagination.pageSize}
        onPageChange={(p) => pagination.setPage(p + 1)}
        onPageSizeChange={pagination.setPageSize}
        onSortModelChange={(model) => {
          if (model.length > 0) {
            pagination.setSorting(model[0].field, model[0].sort === 'desc');
          } else {
            pagination.setSorting('', false);
          }
        }}
      />

      <ActivityFormDialog
        open={formOpen}
        onClose={handleCloseForm}
        activity={editingActivity}
      />
    </Box>
  );
};
