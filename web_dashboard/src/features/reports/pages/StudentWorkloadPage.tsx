import { useState } from 'react';
import { Box, TextField, CircularProgress, Typography, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { PageHeader } from '../../../shared/components/PageHeader';
import { useStudentWorkload } from '../hooks/useReports';
import { WorkloadBarChart } from '../components/WorkloadBarChart';
import { useStudents } from '../../people/hooks/useStudents';

export const StudentWorkloadPage = () => {
  const { t } = useTranslation();
  const [startDate, setStartDate] = useState(
    new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split('T')[0]
  );
  const [endDate, setEndDate] = useState(
    new Date().toISOString().split('T')[0]
  );
  
  const { data: studentsData } = useStudents({ pageNumber: 1, pageSize: 100 });
  
  const [selectedStudentId, setSelectedStudentId] = useState('');

  const { data, isLoading, error } = useStudentWorkload(selectedStudentId, { startDate, endDate });

  return (
    <Box>
      <PageHeader title={t('reports.studentWorkload', 'Student Workload')} />
      
      <Box sx={{ display: 'flex', gap: 2, mb: 4, flexWrap: 'wrap' }}>
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>{t('people.student', 'Student')}</InputLabel>
          <Select
            value={selectedStudentId}
            label={t('people.student', 'Student')}
            onChange={(e) => setSelectedStudentId(e.target.value)}
          >
            <MenuItem value=""><em>{t('common.select', 'Select...')}</em></MenuItem>
            {studentsData?.items.map(student => (
              <MenuItem key={student.id} value={student.id}>
                {student.fullName}
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

      {data && selectedStudentId && (
        <WorkloadBarChart 
          report={data} 
          title={t('reports.studentWorkload', 'Student Workload')} 
        />
      )}
      {!selectedStudentId && (
        <Typography color="text.secondary">
          {t('reports.selectStudentPrompt', 'Please select a student to view their workload.')}
        </Typography>
      )}
    </Box>
  );
};
