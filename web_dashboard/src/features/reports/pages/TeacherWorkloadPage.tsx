import { useState } from 'react';
import { Box, TextField, CircularProgress, Typography, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { PageHeader } from '../../../shared/components/PageHeader';
import { useTeacherWorkload } from '../hooks/useReports';
import { WorkloadBarChart } from '../components/WorkloadBarChart';
import { useTeachers } from '../../people/hooks/useTeachers';

export const TeacherWorkloadPage = () => {
  const { t } = useTranslation();
  const [startDate, setStartDate] = useState(
    new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split('T')[0]
  );
  const [endDate, setEndDate] = useState(
    new Date().toISOString().split('T')[0]
  );
  
  const { data: teachersData } = useTeachers({ pageNumber: 1, pageSize: 100 });
  
  const [selectedTeacherId, setSelectedTeacherId] = useState('');

  const { data, isLoading, error } = useTeacherWorkload(selectedTeacherId, { startDate, endDate });

  return (
    <Box>
      <PageHeader title={t('reports.teacherWorkload', 'Teacher Workload')} />
      
      <Box sx={{ display: 'flex', gap: 2, mb: 4, flexWrap: 'wrap' }}>
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>{t('people.teacher', 'Teacher')}</InputLabel>
          <Select
            value={selectedTeacherId}
            label={t('people.teacher', 'Teacher')}
            onChange={(e) => setSelectedTeacherId(e.target.value)}
          >
            <MenuItem value=""><em>{t('common.select', 'Select...')}</em></MenuItem>
            {teachersData?.items.map(teacher => (
              <MenuItem key={teacher.id} value={teacher.id}>
                {teacher.fullName}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

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

      {data && selectedTeacherId && (
        <WorkloadBarChart 
          report={data} 
          title={t('reports.teacherWorkload', 'Teacher Workload')} 
        />
      )}
      {!selectedTeacherId && (
        <Typography color="text.secondary">
          {t('reports.selectTeacherPrompt', 'Please select a teacher to view their workload.')}
        </Typography>
      )}
    </Box>
  );
};
