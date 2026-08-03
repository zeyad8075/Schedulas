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
import { StudentDto } from '../models';
import { useEffect } from 'react';

const studentSchema = z.object({
  fullName: z.string().min(2, 'validation.minLength'),
  email: z.string().email('validation.email'),
  studentNumber: z.string().optional().or(z.literal('')),
});

type StudentFormData = z.infer<typeof studentSchema>;

export interface StudentFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: StudentFormData) => void;
  initialData?: StudentDto;
  isLoading?: boolean;
}

export const StudentFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: StudentFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<StudentFormData>({
    resolver: zodResolver(studentSchema),
    defaultValues: {
      fullName: '',
      email: '',
      studentNumber: '',
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          fullName: initialData.fullName,
          email: initialData.email,
          studentNumber: initialData.studentNumber || '',
        });
      } else {
        reset({
          fullName: '',
          email: '',
          studentNumber: '',
        });
      }
    }
  }, [open, initialData, reset]);

  return (
    <Dialog open={open} onClose={!isLoading ? onClose : undefined} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogTitle>
          {isEdit
            ? t('people.editStudent', 'Edit Student')
            : t('people.addStudent', 'Add Student')}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={3}>
            <Grid size={12}>
              <Controller
                name="fullName"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('people.fullName', 'Full Name')}
                    fullWidth
                    error={!!errors.fullName}
                    helperText={errors.fullName?.message ? t(errors.fullName.message, { count: 2 }) : undefined}
                    disabled={isLoading}
                  />
                )}
              />
            </Grid>
            <Grid size={12}>
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    type="email"
                    label={t('people.email', 'Email Address')}
                    fullWidth
                    error={!!errors.email}
                    helperText={errors.email?.message ? t(errors.email.message) : undefined}
                    disabled={isEdit || isLoading} // email is usually readonly on edit
                  />
                )}
              />
            </Grid>
            <Grid size={12}>
              <Controller
                name="studentNumber"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('people.studentNumber', 'Student Number (Optional)')}
                    fullWidth
                    error={!!errors.studentNumber}
                    helperText={errors.studentNumber?.message ? t(errors.studentNumber.message) : undefined}
                    disabled={isLoading}
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
  );
};
