import { Box, Typography, Paper } from '@mui/material';
import { PieChart } from '@mui/x-charts/PieChart';
import { useTranslation } from 'react-i18next';
import { ActivityDistributionReportDto } from '../models';

interface ActivityDistributionPieChartProps {
  report: ActivityDistributionReportDto;
}

export const ActivityDistributionPieChart = ({ report }: ActivityDistributionPieChartProps) => {
  const { t } = useTranslation();

  const data = report.distribution.map((item, index) => ({
    id: index,
    value: item.count,
    label: t(`activities.types.${item.activityType}`, item.activityType),
  }));

  return (
    <Paper sx={{ p: 3, height: '100%' }}>
      <Typography variant="h6" gutterBottom>
        {t('reports.activityDistribution', 'Activity Distribution')}
      </Typography>
      
      {data.length === 0 ? (
        <Box sx={{ height: 400, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Typography color="text.secondary">
            {t('reports.noData', 'No distribution data available for this period.')}
          </Typography>
        </Box>
      ) : (
        <Box sx={{ width: '100%', height: 400 }}>
          <PieChart
            series={[
              {
                data,
                highlightScope: { fade: 'global', highlight: 'item' },
                faded: { innerRadius: 30, additionalRadius: -30, color: 'gray' },
              },
            ]}
          />
        </Box>
      )}
    </Paper>
  );
};
