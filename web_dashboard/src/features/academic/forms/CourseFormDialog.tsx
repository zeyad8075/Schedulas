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
import { CourseDto } from '../models';
import { useEffect } from 'react';

const courseSchema = z.object({
  name: z.string().min(2, 'validation.minLength'),
  code: z.string().optional().or(z.literal('')),
});

type CourseFormData = z.infer<typeof courseSchema>;

export interface CourseFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CourseFormData) => void;
  initialData?: CourseDto;
  isLoading?: boolean;
}

export const CourseFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: CourseFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CourseFormData>({
    resolver: zodResolver(courseSchema),
    defaultValues: {
      name: '',
      code: '',
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          name: initialData.name,
          code: initialData.code || '',
        });
      } else {
        reset({
          name: '',
          code: '',
        });
      }
    }
  }, [open, initialData, reset]);

  return (
    <Dialog open={open} onClose={!isLoading ? onClose : undefined} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogTitle>
          {isEdit
            ? t('academic.editCourse', 'Edit Course')
            : t('academic.addCourse', 'Add Course')}
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
                    label={t('academic.courseName', 'Course Name')}
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
                name="code"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('academic.courseCode', 'Course Code (Optional)')}
                    fullWidth
                    error={!!errors.code}
                    helperText={errors.code?.message ? t(errors.code.message) : undefined}
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
