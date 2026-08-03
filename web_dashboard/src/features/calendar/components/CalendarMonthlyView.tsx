import { Box, Typography, Paper } from '@mui/material';
import Grid from '@mui/material/Grid';
import { MonthlyCalendarDto, WeeklyCalendarDto } from '../models';
import { CalendarWeeklyView } from './CalendarWeeklyView';
import { useTranslation } from 'react-i18next';

interface CalendarMonthlyViewProps {
  data: MonthlyCalendarDto;
}

export const CalendarMonthlyView = ({ data }: CalendarMonthlyViewProps) => {
  const { t } = useTranslation();
  const date = new Date(data.year, data.month - 1);
  const monthName = date.toLocaleString(undefined, { month: 'long', year: 'numeric' });

  return (
    <Box>
      <Typography variant="h5" gutterBottom align="center" sx={{ fontWeight: 'bold' }}>
        {monthName}
      </Typography>
      <Grid container spacing={4}>
        {data.weeks.map((week: WeeklyCalendarDto, index: number) => (
          <Grid size={12} key={week.startDate}>
            <Paper sx={{ p: 2, bgcolor: 'background.default' }}>
              <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                {t('calendar.week', 'Week')} {index + 1}
              </Typography>
              <CalendarWeeklyView data={week} />
            </Paper>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
