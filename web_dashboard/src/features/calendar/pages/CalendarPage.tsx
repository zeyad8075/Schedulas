import { useState } from 'react';
import { Box, Tabs, Tab, CircularProgress, Typography, TextField } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { PageHeader } from '../../../shared/components/PageHeader';
import { useDailyCalendar, useWeeklyCalendar, useMonthlyCalendar } from '../hooks/useCalendar';
import { CalendarDailyView } from '../components/CalendarDailyView';
import { CalendarWeeklyView } from '../components/CalendarWeeklyView';
import { CalendarMonthlyView } from '../components/CalendarMonthlyView';

export const CalendarPage = () => {
  const { t } = useTranslation();
  const [tabIndex, setTabIndex] = useState(0);
  
  const today = new Date().toISOString().split('T')[0];
  const [selectedDate, setSelectedDate] = useState(today);

  // Derive year/month for monthly view
  const dateObj = new Date(selectedDate);
  const year = dateObj.getFullYear();
  const month = dateObj.getMonth() + 1;

  const { data: dailyData, isLoading: dailyLoading } = useDailyCalendar(selectedDate);
  const { data: weeklyData, isLoading: weeklyLoading } = useWeeklyCalendar(selectedDate);
  const { data: monthlyData, isLoading: monthlyLoading } = useMonthlyCalendar(year, month);

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabIndex(newValue);
  };

  const isLoading = 
    (tabIndex === 0 && dailyLoading) || 
    (tabIndex === 1 && weeklyLoading) || 
    (tabIndex === 2 && monthlyLoading);

  return (
    <Box>
      <PageHeader title={t('calendar.title', 'Calendar')} />

      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Tabs value={tabIndex} onChange={handleTabChange}>
          <Tab label={t('calendar.daily', 'Daily')} />
          <Tab label={t('calendar.weekly', 'Weekly')} />
          <Tab label={t('calendar.monthly', 'Monthly')} />
        </Tabs>

        <TextField
          type="date"
          size="small"
          value={selectedDate}
          onChange={(e) => setSelectedDate(e.target.value)}
          sx={{ width: 200 }}
        />
      </Box>

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Box>
          {tabIndex === 0 && dailyData && <CalendarDailyView data={dailyData} />}
          {tabIndex === 1 && weeklyData && <CalendarWeeklyView data={weeklyData} />}
          {tabIndex === 2 && monthlyData && <CalendarMonthlyView data={monthlyData} />}
          {!dailyData && !weeklyData && !monthlyData && (
            <Typography align="center" color="textSecondary">
              {t('calendar.noData', 'No data available for the selected period.')}
            </Typography>
          )}
        </Box>
      )}
    </Box>
  );
};
