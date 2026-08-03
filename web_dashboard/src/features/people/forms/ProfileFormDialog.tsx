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
import { ProfileDto, Theme } from '../models';
import { UserRole } from '../../auth/types';
import { useEffect } from 'react';

const profileSchema = z.object({
  fullName: z.string().min(2, 'validation.minLength'),
  email: z.string().email('validation.email'),
  phoneNumber: z.string().optional().or(z.literal('')),
  role: z.nativeEnum(UserRole).optional(),
  preferredTheme: z.nativeEnum(Theme).optional(),
});

type ProfileFormData = z.infer<typeof profileSchema>;

export interface ProfileFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: ProfileFormData) => void;
  initialData?: ProfileDto;
  isLoading?: boolean;
}

export const ProfileFormDialog = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isLoading,
}: ProfileFormDialogProps) => {
  const { t } = useTranslation();
  const isEdit = !!initialData;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      fullName: '',
      email: '',
      phoneNumber: '',
      role: UserRole.Student,
      preferredTheme: Theme.System,
    },
  });

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          fullName: initialData.fullName,
          email: initialData.email,
          phoneNumber: initialData.phoneNumber || '',
          role: initialData.role,
          preferredTheme: initialData.preferredTheme,
        });
      } else {
        reset({
          fullName: '',
          email: '',
          phoneNumber: '',
          role: UserRole.Student,
          preferredTheme: Theme.System,
        });
      }
    }
  }, [open, initialData, reset]);

  return (
    <Dialog open={open} onClose={!isLoading ? onClose : undefined} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogTitle>
          {isEdit
            ? t('people.editProfile', 'Edit Profile')
            : t('people.addProfile', 'Add Profile')}
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
                    disabled={isEdit || isLoading}
                  />
                )}
              />
            </Grid>
            <Grid size={12}>
              <Controller
                name="phoneNumber"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('people.phoneNumber', 'Phone Number (Optional)')}
                    fullWidth
                    error={!!errors.phoneNumber}
                    helperText={errors.phoneNumber?.message ? t(errors.phoneNumber.message) : undefined}
                    disabled={isLoading}
                  />
                )}
              />
            </Grid>
            {!isEdit && (
              <Grid size={12}>
                <Controller
                  name="role"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      select
                      label={t('people.role', 'Role')}
                      fullWidth
                      error={!!errors.role}
                      helperText={errors.role?.message ? t(errors.role.message) : undefined}
                      disabled={isLoading}
                    >
                      {Object.values(UserRole)
                        .filter((r) => typeof r === 'number')
                        .map((role) => (
                          <MenuItem key={role} value={role}>
                            {t(`roles.${(UserRole as any)[role]}`, (UserRole as any)[role]) as string}
                          </MenuItem>
                        ))}
                    </TextField>
                  )}
                />
              </Grid>
            )}
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
