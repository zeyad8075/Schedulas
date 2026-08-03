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
import { HolidayDto } from '../models';
import { useEffect } from 'react';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs from 'dayjs';

const holidaySchema = z.object({
  name: z.string().min(2, 'validation.minLength'),
  holidayDate: z.string().min(1, 'validation.required'),
});

type HolidayFormData = z.infer<typeof holidaySchema>;

export interface HolidayFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: HolidayFormData) => void;
  initialData?: HolidayDto;
  isLoading?: boolean;
}

export const HolidayFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: HolidayFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<HolidayFormData>({
    resolver: zodResolver(holidaySchema),
    defaultValues: {
      name: '',
      holidayDate: '',
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          name: initialData.name,
          holidayDate: initialData.holidayDate,
        });
      } else {
        reset({
          name: '',
          holidayDate: '',
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
              ? t('academic.editHoliday', 'Edit Holiday')
              : t('academic.addHoliday', 'Add Holiday')}
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
                      label={t('academic.holidayName', 'Holiday Name')}
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
                  name="holidayDate"
                  control={control}
                  render={({ field }) => (
                    <DatePicker
                      label={t('academic.holidayDate', 'Holiday Date')}
                      value={field.value ? dayjs(field.value) : null}
                      onChange={(date) => field.onChange(date ? date.format('YYYY-MM-DD') : '')}
                      disabled={isLoading}
                      slotProps={{
                        textField: {
                          fullWidth: true,
                          error: !!errors.holidayDate,
                          helperText: errors.holidayDate?.message ? t(errors.holidayDate.message) : undefined,
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
