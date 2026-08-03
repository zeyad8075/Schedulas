import { Box, Typography, Paper } from '@mui/material';
import { BarChart } from '@mui/x-charts/BarChart';
import { useTranslation } from 'react-i18next';
import { WorkloadReportDto } from '../models';

interface WorkloadBarChartProps {
  report: WorkloadReportDto;
  title: string;
}

export const WorkloadBarChart = ({ report, title }: WorkloadBarChartProps) => {
  const { t } = useTranslation();

  return (
    <Paper sx={{ p: 3, height: '100%' }}>
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      <Box sx={{ width: '100%', height: 400 }}>
        <BarChart
          xAxis={[
            {
              scaleType: 'band',
              data: [t('reports.workload', 'Workload')],
            },
          ]}
          series={[
            {
              data: [report.totalWorkloadMinutes / 60],
              label: t('reports.hours', 'Hours'),
              color: '#1976d2',
            },
          ]}
        />
      </Box>
      <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
        <Typography variant="body2" color="text.secondary">
          {t('reports.period', 'Period')}: {new Date(report.startDate).toLocaleDateString()} - {new Date(report.endDate).toLocaleDateString()}
        </Typography>
        <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
          {t('reports.totalMinutes', 'Total Minutes')}: {report.totalWorkloadMinutes}
        </Typography>
      </Box>
    </Paper>
  );
};
