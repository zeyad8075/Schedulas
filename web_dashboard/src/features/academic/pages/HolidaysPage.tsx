import { useState, useMemo } from 'react';
import { Box } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useTranslation } from 'react-i18next';
import { useHolidays, useCreateHoliday, useUpdateHoliday, useDeleteHoliday } from '../hooks/useHolidays';
import { HolidayDto } from '../models';
import { PageHeader } from '../../../shared/components/PageHeader';
import { DataTable } from '../../../shared/components/DataTable';
import { useTablePagination } from '../../../shared/hooks/useTablePagination';
import { HolidayFormDialog } from '../forms/HolidayFormDialog';
import dayjs from 'dayjs';

export const HolidaysPage = () => {
  const { t } = useTranslation();
  const pagination = useTablePagination();
  const [formOpen, setFormOpen] = useState(false);
  const [editingHoliday, setEditingHoliday] = useState<HolidayDto | undefined>();

  const { data, isLoading } = useHolidays({
    pageNumber: pagination.page,
    pageSize: pagination.pageSize,
    searchTerm: pagination.searchTerm,
    sortBy: pagination.sortBy,
    sortDescending: pagination.sortDescending,
  });

  const createMutation = useCreateHoliday();
  const updateMutation = useUpdateHoliday();
  const deleteMutation = useDeleteHoliday();

  const handleOpenForm = (holiday?: HolidayDto) => {
    setEditingHoliday(holiday);
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setEditingHoliday(undefined);
  };

  const handleSubmit = async (formData: any) => {
    if (editingHoliday) {
      await updateMutation.mutateAsync({ id: editingHoliday.id, data: formData });
    } else {
      await createMutation.mutateAsync({ ...formData, institutionId: 'current-inst' });
    }
    handleCloseForm();
  };

  const handleDelete = async (id: string) => {
    if (window.confirm(t('common.confirmDelete', 'Are you sure you want to delete this?'))) {
      await deleteMutation.mutateAsync(id);
    }
  };

  const columns = useMemo<GridColDef<HolidayDto>[]>(() => [
    { field: 'name', headerName: t('academic.holidayName', 'Holiday Name'), flex: 1, minWidth: 200 },
    { 
      field: 'holidayDate', 
      headerName: t('academic.holidayDate', 'Date'), 
      width: 150,
      valueFormatter: (value) => dayjs(value).format('ll')
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: t('common.actions', 'Actions'),
      width: 150,
      getActions: (params) => {
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
        title={t('academic.holidays', 'Holidays')}
        actionLabel={t('academic.addHoliday', 'Add Holiday')}
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
        <HolidayFormDialog
          open={formOpen}
          onClose={handleCloseForm}
          onSubmit={handleSubmit}
          initialData={editingHoliday}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </Box>
  );
};
