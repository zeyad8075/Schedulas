import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { AcademicTermDto } from '../models';
import { useEffect } from 'react';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs from 'dayjs';

const termSchema = z.object({
  name: z.string().min(2, 'validation.minLength'),
  startDate: z.string().min(1, 'validation.required'),
  endDate: z.string().min(1, 'validation.required'),
});

type TermFormData = z.infer<typeof termSchema>;

export interface TermFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: TermFormData) => void;
  initialData?: AcademicTermDto;
  isLoading?: boolean;
}

export const TermFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: TermFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TermFormData>({
    resolver: zodResolver(termSchema),
    defaultValues: {
      name: '',
      startDate: '',
      endDate: '',
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          name: initialData.name,
          startDate: initialData.startDate,
          endDate: initialData.endDate,
        });
      } else {
        reset({
          name: '',
          startDate: '',
          endDate: '',
        });
      }
    }
  }, [open, initialData, reset]);

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Dialog open={open} onClose={!isLoading ? onClose : undefined} maxWidth="sm" fullWidth>
        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogTitle>
            {isEdit
              ? t('academic.editTerm', 'Edit Term')
              : t('academic.addTerm', 'Add Term')}
          </DialogTitle>
          <DialogContent dividers>
            <Grid container spacing={3}>
              <Grid size={12}>
                <Controller
                  name="name"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label={t('academic.termName', 'Term Name')}
                      fullWidth
                      error={!!errors.name}
                      helperText={errors.name?.message ? t(errors.name.message, { count: 2 }) : undefined}
                      disabled={isLoading}
                    />
                  )}
                />
              </Grid>
              <Grid size={12}>
                <Controller
                  name="startDate"
                  control={control}
                  render={({ field }) => (
                    <DatePicker
                      label={t('academic.startDate', 'Start Date')}
                      value={field.value ? dayjs(field.value) : null}
                      onChange={(date) => field.onChange(date ? date.format('YYYY-MM-DD') : '')}
                      disabled={isLoading}
                      slotProps={{
                        textField: {
                          fullWidth: true,
                          error: !!errors.startDate,
                          helperText: errors.startDate?.message ? t(errors.startDate.message) : undefined,
                        },
                      }}
                    />
                  )}
                />
              </Grid>
              <Grid size={12}>
                <Controller
                  name="endDate"
                  control={control}
                  render={({ field }) => (
                    <DatePicker
                      label={t('academic.endDate', 'End Date')}
                      value={field.value ? dayjs(field.value) : null}
                      onChange={(date) => field.onChange(date ? date.format('YYYY-MM-DD') : '')}
                      disabled={isLoading}
                      slotProps={{
                        textField: {
                          fullWidth: true,
                          error: !!errors.endDate,
                          helperText: errors.endDate?.message ? t(errors.endDate.message) : undefined,
                        },
                      }}
                    />
                  )}
                />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={onClose} disabled={isLoading}>
              {t('common.cancel', 'Cancel')}
            </Button>
            <Button type="submit" variant="contained" disabled={isLoading}>
              {t('common.save', 'Save')}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  );
};
