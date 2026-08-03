import { Box, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { NotificationDto } from '../models';
import { NotificationCard } from './NotificationCard';

interface NotificationListProps {
  notifications: NotificationDto[];
  onMarkRead: (id: string) => void;
}

export const NotificationList = ({ notifications, onMarkRead }: NotificationListProps) => {
  const { t } = useTranslation();

  if (notifications.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', p: 4, bgcolor: 'background.paper', borderRadius: 1 }}>
        <Typography color="text.secondary">
          {t('notifications.empty', 'No notifications to display.')}
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {notifications.map((notification) => (
        <NotificationCard
          key={notification.id}
          notification={notification}
          onMarkRead={onMarkRead}
        />
      ))}
    </Box>
  );
};
