import { useState } from 'react';
import { Box, Select, MenuItem, FormControl, InputLabel, CircularProgress, Pagination } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { PageHeader } from '../../../shared/components/PageHeader';
import { useNotifications, useMarkNotificationRead, useMarkAllNotificationsRead } from '../hooks/useNotifications';
import { NotificationList } from '../components/NotificationList';
import { NotificationCategory } from '../models';
import DoneAllIcon from '@mui/icons-material/DoneAll';

export const NotificationsPage = () => {
  const { t } = useTranslation();
  
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [unreadOnly, setUnreadOnly] = useState<boolean | undefined>(undefined);
  const [category, setCategory] = useState<string>('');

  const { data, isLoading } = useNotifications({
    pageNumber: page,
    pageSize,
    unreadOnly,
    category: category || undefined,
  });

  const markReadMutation = useMarkNotificationRead();
  const markAllReadMutation = useMarkAllNotificationsRead();

  const handleMarkRead = (id: string) => {
    markReadMutation.mutate(id);
  };

  const handleMarkAllRead = () => {
    markAllReadMutation.mutate();
  };

  return (
    <Box>
      <PageHeader 
        title={t('notifications.title', 'Notifications')} 
        actionLabel={t('notifications.markAllRead', 'Mark All Read')}
        actionIcon={<DoneAllIcon />}
        onActionClick={handleMarkAllRead}
      />

      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>{t('notifications.filterStatus', 'Status')}</InputLabel>
          <Select
            value={unreadOnly === undefined ? 'all' : unreadOnly ? 'unread' : 'read'}
            label={t('notifications.filterStatus', 'Status')}
            onChange={(e) => {
              const val = e.target.value;
              setUnreadOnly(val === 'all' ? undefined : val === 'unread');
              setPage(1);
            }}
          >
            <MenuItem value="all">{t('notifications.all', 'All')}</MenuItem>
            <MenuItem value="unread">{t('notifications.unread', 'Unread')}</MenuItem>
            <MenuItem value="read">{t('notifications.read', 'Read')}</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>{t('notifications.filterCategory', 'Category')}</InputLabel>
          <Select
            value={category}
            label={t('notifications.filterCategory', 'Category')}
            onChange={(e) => {
              setCategory(e.target.value);
              setPage(1);
            }}
          >
            <MenuItem value=""><em>{t('common.none', 'None')}</em></MenuItem>
            {Object.values(NotificationCategory).map((cat) => (
              <MenuItem key={cat} value={cat}>
                {t(`notifications.categories.${cat}`, cat)}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          <NotificationList 
            notifications={data?.items || []} 
            onMarkRead={handleMarkRead} 
          />
          
          {data && data.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
              <Pagination 
                count={data.totalPages} 
                page={page} 
                onChange={(_, newPage) => setPage(newPage)} 
                color="primary"
              />
            </Box>
          )}
        </>
      )}
    </Box>
  );
};
