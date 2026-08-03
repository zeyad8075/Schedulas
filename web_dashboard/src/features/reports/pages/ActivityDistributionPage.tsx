import { useState } from 'react';
import { Box, TextField, CircularProgress, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { PageHeader } from '../../../shared/components/PageHeader';
import { useActivityDistribution } from '../hooks/useReports';
import { ActivityDistributionPieChart } from '../components/ActivityDistributionPieChart';

export const ActivityDistributionPage = () => {
  const { t } = useTranslation();
  const [startDate, setStartDate] = useState(
    new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split('T')[0]
  );
  const [endDate, setEndDate] = useState(
    new Date().toISOString().split('T')[0]
  );

  const { data, isLoading, error } = useActivityDistribution({ startDate, endDate });

  return (
    <Box>
      <PageHeader title={t('reports.activityDistribution', 'Activity Distribution')} />
      
      <Box sx={{ display: 'flex', gap: 2, mb: 4 }}>
        <TextField
          label={t('reports.startDate', 'Start Date')}
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
          size="small"
        />
        <TextField
          label={t('reports.endDate', 'End Date')}
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
          size="small"
        />
      </Box>

      {isLoading && <CircularProgress />}
      
      {error && (
        <Typography color="error">{t('common.error', 'An error occurred')}</Typography>
      )}

      {data && (
        <ActivityDistributionPieChart report={data} />
      )}
    </Box>
  );
};
