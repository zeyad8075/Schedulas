import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Grid,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { InstitutionDto, InstitutionType } from '../models';
import { useEffect } from 'react';

const institutionSchema = z.object({
  name: z.string().min(2, 'validation.minLength'),
  type: z.nativeEnum(InstitutionType),
  timezone: z.string().min(1, 'validation.required'),
  logoUrl: z.string().url('validation.url').optional().or(z.literal('')),
});

type InstitutionFormData = z.infer<typeof institutionSchema>;

export interface InstitutionFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: InstitutionFormData) => void;
  initialData?: InstitutionDto;
  isLoading?: boolean;
}

export const InstitutionFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: InstitutionFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<InstitutionFormData>({
    resolver: zodResolver(institutionSchema),
    defaultValues: {
      name: '',
      type: InstitutionType.School,
      timezone: 'UTC',
      logoUrl: '',
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          name: initialData.name,
          type: initialData.type,
          timezone: initialData.timezone,
          logoUrl: initialData.logoUrl || '',
        });
      } else {
        reset({
          name: '',
          type: InstitutionType.School,
          timezone: 'UTC',
          logoUrl: '',
        });
      }
    }
  }, [open, initialData, reset]);

  return (
    <Dialog open={open} onClose={!isLoading ? onClose : undefined} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogTitle>
          {isEdit
            ? t('academic.editInstitution', 'Edit Institution')
            : t('academic.addInstitution', 'Add Institution')}
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
                    label={t('academic.institutionName', 'Institution Name')}
                    fullWidth
                    error={!!errors.name}
                    helperText={errors.name?.message ? t(errors.name.message, { count: 2 }) : undefined}
                    disabled={isLoading}
                  />
                )}
              />
            </Grid>
            {!isEdit && (
              <Grid size={12}>
                <Controller
                  name="type"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      select
                      label={t('academic.institutionType', 'Institution Type')}
                      fullWidth
                      error={!!errors.type}
                      helperText={errors.type?.message ? t(errors.type.message) : undefined}
                      disabled={isLoading}
                    >
                      {Object.values(InstitutionType).map((type) => (
                        <MenuItem key={type} value={type}>
                          {t(`academic.institutionType_${type}`, type)}
                        </MenuItem>
                      ))}
                    </TextField>
                  )}
                />
              </Grid>
            )}
            <Grid size={12}>
              <Controller
                name="timezone"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('academic.timezone', 'Timezone')}
                    fullWidth
                    error={!!errors.timezone}
                    helperText={errors.timezone?.message ? t(errors.timezone.message) : undefined}
                    disabled={isLoading}
                    placeholder="e.g. UTC, America/New_York"
                  />
                )}
              />
            </Grid>
            <Grid size={12}>
              <Controller
                name="logoUrl"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('academic.logoUrl', 'Logo URL (Optional)')}
                    fullWidth
                    error={!!errors.logoUrl}
                    helperText={errors.logoUrl?.message ? t(errors.logoUrl.message) : undefined}
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
