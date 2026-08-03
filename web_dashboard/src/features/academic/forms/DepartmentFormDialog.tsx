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
import { DepartmentDto } from '../models';
import { useEffect } from 'react';

const departmentSchema = z.object({
  name: z.string().min(2, 'validation.minLength'),
});

type DepartmentFormData = z.infer<typeof departmentSchema>;

export interface DepartmentFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: DepartmentFormData) => void;
  initialData?: DepartmentDto;
  isLoading?: boolean;
}

export const DepartmentFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: DepartmentFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<DepartmentFormData>({
    resolver: zodResolver(departmentSchema),
    defaultValues: {
      name: '',
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          name: initialData.name,
        });
      } else {
        reset({
          name: '',
        });
      }
    }
  }, [open, initialData, reset]);

  return (
    <Dialog open={open} onClose={!isLoading ? onClose : undefined} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogTitle>
          {isEdit
            ? t('academic.editDepartment', 'Edit Department')
            : t('academic.addDepartment', 'Add Department')}
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
                    label={t('academic.departmentName', 'Department Name')}
                    fullWidth
                    error={!!errors.name}
                    helperText={errors.name?.message ? t(errors.name.message, { count: 2 }) : undefined}
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
