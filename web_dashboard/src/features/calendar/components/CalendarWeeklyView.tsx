import { Box, Typography, Paper } from '@mui/material';
import Grid from '@mui/material/Grid';
import { WeeklyCalendarDto, DailyCalendarDto } from '../models';
import { CalendarDailyView } from './CalendarDailyView';

interface CalendarWeeklyViewProps {
  data: WeeklyCalendarDto;
}

export const CalendarWeeklyView = ({ data }: CalendarWeeklyViewProps) => {
  return (
    <Box>
      <Typography variant="h6" gutterBottom align="center">
        {data.startDate} - {data.endDate}
      </Typography>
      <Grid container spacing={2}>
        {data.days.map((day: DailyCalendarDto) => (
          <Grid size={{ xs: 12, md: 6, lg: 4 }} key={day.date}>
            <Paper sx={{ p: 2, height: '100%', minHeight: 200 }}>
              <CalendarDailyView data={day} />
            </Paper>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
