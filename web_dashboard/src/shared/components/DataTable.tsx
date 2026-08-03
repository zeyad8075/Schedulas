import {
  DataGrid,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
  GridValidRowModel,
} from '@mui/x-data-grid';
import { Box, TextField, InputAdornment, Paper } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useTranslation } from 'react-i18next';
import { useState, useEffect } from 'react';

export interface DataTableProps<R extends GridValidRowModel> {
  columns: GridColDef<R>[];
  rows: R[];
  rowCount: number;
  loading: boolean;
  page: number; // 1-indexed for backend, but DataGrid is 0-indexed
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  onSearch?: (searchTerm: string) => void;
  searchTerm?: string;
  onSortModelChange?: (model: GridSortModel) => void;
}

export const DataTable = <R extends GridValidRowModel>({
  columns,
  rows,
  rowCount,
  loading,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
  onSearch,
  searchTerm,
  onSortModelChange,
}: DataTableProps<R>) => {
  const { t } = useTranslation();
  const [localSearchTerm, setLocalSearchTerm] = useState(searchTerm || '');

  // Debounce search
  useEffect(() => {
    const handler = setTimeout(() => {
      if (onSearch && localSearchTerm !== searchTerm) {
        onSearch(localSearchTerm);
      }
    }, 500);

    return () => {
      clearTimeout(handler);
    };
  }, [localSearchTerm, onSearch, searchTerm]);

  const handlePaginationModelChange = (model: GridPaginationModel) => {
    if (model.page + 1 !== page) {
      onPageChange(model.page + 1);
    }
    if (model.pageSize !== pageSize) {
      onPageSizeChange(model.pageSize);
    }
  };

  return (
    <Paper sx={{ width: '100%', mb: 2, overflow: 'hidden' }}>
      {onSearch && (
        <Box sx={{ p: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <TextField
            size="small"
            placeholder={t('common.search', 'Search...')}
            value={localSearchTerm}
            onChange={(e) => setLocalSearchTerm(e.target.value)}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
              }
            }}
            sx={{ minWidth: 300 }}
          />
        </Box>
      )}
      
      <Box sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={rows}
          columns={columns}
          rowCount={rowCount}
          loading={loading}
          paginationMode="server"
          sortingMode="server"
          filterMode="server"
          paginationModel={{ page: page - 1, pageSize }} // DataGrid is 0-indexed
          onPaginationModelChange={handlePaginationModelChange}
          onSortModelChange={onSortModelChange}
          pageSizeOptions={[10, 20, 50, 100]}
          disableRowSelectionOnClick
          disableColumnMenu
        />
      </Box>
    </Paper>
  );
};
