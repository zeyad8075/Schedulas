import { Box, Card, CardContent, Typography, IconButton, Chip } from '@mui/material';
import DoneIcon from '@mui/icons-material/Done';
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord';
import { useTranslation } from 'react-i18next';
import { NotificationDto, NotificationCategory } from '../models';

interface NotificationCardProps {
  notification: NotificationDto;
  onMarkRead: (id: string) => void;
}

export const NotificationCard = ({ notification, onMarkRead }: NotificationCardProps) => {
  const { t } = useTranslation();

  const getCategoryColor = (category: NotificationCategory): "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning" => {
    switch (category) {
      case NotificationCategory.RuleViolation:
        return 'error';
      case NotificationCategory.DeadlineReminder:
        return 'warning';
      case NotificationCategory.RuleOverride:
        return 'info';
      case NotificationCategory.ActivityCancelled:
        return 'secondary';
      default:
        return 'primary';
    }
  };

  return (
    <Card 
      variant="outlined" 
      sx={{ 
        mb: 2, 
        bgcolor: notification.isRead ? 'background.paper' : 'action.hover',
        transition: 'background-color 0.3s'
      }}
    >
      <CardContent sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', pb: '16px !important' }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
          <Box sx={{ mt: 0.5 }}>
            {notification.isRead ? (
              <Box sx={{ width: 24, height: 24 }} /> // Placeholder
            ) : (
              <FiberManualRecordIcon color="primary" fontSize="small" />
            )}
          </Box>
          <Box>
            <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mb: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: notification.isRead ? 'normal' : 'bold' }}>
                {notification.title}
              </Typography>
              <Chip 
                label={t(`notifications.categories.${notification.category}`, notification.category)} 
                size="small" 
                color={getCategoryColor(notification.category)} 
              />
            </Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              {notification.body}
            </Typography>
            <Typography variant="caption" color="text.disabled">
              {new Date(notification.createdAt).toLocaleString()}
            </Typography>
          </Box>
        </Box>
        
        {!notification.isRead && (
          <IconButton 
            onClick={() => onMarkRead(notification.id)}
            title={t('notifications.markRead', 'Mark as read')}
            color="primary"
          >
            <DoneIcon />
          </IconButton>
        )}
      </CardContent>
    </Card>
  );
};
