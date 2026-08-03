import { Box, Typography, Paper, Chip } from '@mui/material';
import Grid from '@mui/material/Grid';
import { DailyCalendarDto, CalendarEntryDto } from '../models';
import { ActivityType } from '../../activities/models';
import { useTranslation } from 'react-i18next';

interface CalendarDailyViewProps {
  data: DailyCalendarDto;
}

export const CalendarDailyView = ({ data }: CalendarDailyViewProps) => {
  const { t } = useTranslation();

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        {data.date}
      </Typography>
      {data.entries.length === 0 ? (
        <Typography color="textSecondary">{t('calendar.noActivities', 'No activities scheduled.')}</Typography>
      ) : (
        <Grid container spacing={2}>
          {data.entries.map((entry: CalendarEntryDto) => (
            <Grid size={12} key={entry.activityId}>
              <Paper sx={{ p: 2, borderLeft: `6px solid ${entry.colorHex || '#1976d2'}` }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box>
                    <Typography variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                      {entry.title}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      {entry.startTime} - {entry.endTime} | {entry.className}
                    </Typography>
                  </Box>
                  <Box>
                    <Chip
                      label={t(`activities.types.${(ActivityType as any)[entry.activityType]}`, (ActivityType as any)[entry.activityType]) as string}
                      size="small"
                      sx={{ mr: 1 }}
                    />
                  </Box>
                </Box>
              </Paper>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
};
