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
import { ProgramDto } from '../models';
import { useEffect } from 'react';

const programSchema = z.object({
  name: z.string().min(2, 'validation.minLength'),
});

type ProgramFormData = z.infer<typeof programSchema>;

export interface ProgramFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: ProgramFormData) => void;
  initialData?: ProgramDto;
  isLoading?: boolean;
}

export const ProgramFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: ProgramFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProgramFormData>({
    resolver: zodResolver(programSchema),
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
            ? t('academic.editProgram', 'Edit Program')
            : t('academic.addProgram', 'Add Program')}
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
                    label={t('academic.programName', 'Program Name')}
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
