import { useState } from 'react';
import { useMyProfile, useUpdateProfileMutation } from '../hooks/useProfile';
import { updateProfileSchema, type UpdateProfileFormData } from '../schemas/profileSchemas';
import { UserRole } from '../../auth/types';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Box, Card, CardContent, Typography, Avatar, Grid, Button, TextField, Divider, Alert, CircularProgress, Chip } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { LoadingView } from '../../../shared/components/LoadingView';
import { ErrorView } from '../../../shared/components/ErrorView';

const RoleLabel = ({ role }: { role: UserRole }) => {
  const { t } = useTranslation();
  switch (role) {
    case UserRole.PlatformAdmin: return <Chip label={t('roles.platformAdmin', 'Platform Admin')} color="secondary" />;
    case UserRole.Admin: return <Chip label={t('roles.admin', 'Institution Admin')} color="primary" />;
    case UserRole.Teacher: return <Chip label={t('roles.teacher', 'Teacher')} color="info" />;
    case UserRole.Student: return <Chip label={t('roles.student', 'Student')} color="default" />;
    default: return <Chip label={t('roles.unknown', 'Unknown')} />;
  }
};

const ProfilePage = () => {
  const { t } = useTranslation();
  const { data: profile, isLoading, isError, refetch } = useMyProfile();
  const updateMutation = useUpdateProfileMutation();
  const [isEditing, setIsEditing] = useState(false);

  const { register, handleSubmit, formState: { errors }, reset } = useForm<UpdateProfileFormData>({
    resolver: zodResolver(updateProfileSchema),
  });

  if (isLoading) return <LoadingView message={t('profile.loading', 'Loading profile...')} />;
  if (isError || !profile) return <ErrorView message={t('profile.error', 'Failed to load profile')} onRetry={() => refetch()} />;

  const onEditClick = () => {
    reset({
      fullName: profile.fullName,
      phoneNumber: profile.phoneNumber || '',
    });
    setIsEditing(true);
  };

  const onCancelClick = () => {
    setIsEditing(false);
  };

  const onSubmit = (data: UpdateProfileFormData) => {
    updateMutation.mutate(data, {
      onSuccess: () => {
        setIsEditing(false);
      }
    });
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto' }}>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 'bold' }}>
        {t('profile.title', 'My Profile')}
      </Typography>

      {updateMutation.isSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {t('profile.updateSuccess', 'Profile updated successfully')}
        </Alert>
      )}

      {updateMutation.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {t('profile.updateError', 'Failed to update profile')}
        </Alert>
      )}

      <Card sx={{ mb: 4, boxShadow: 2 }}>
        <CardContent sx={{ p: 4 }}>
          <Grid container spacing={4} sx={{ alignItems: 'center', mb: 4 }}>
            <Grid size="auto">
              <Avatar sx={{ width: 80, height: 80, bgcolor: 'primary.main', fontSize: 32 }}>
                {profile.fullName.charAt(0).toUpperCase()}
              </Avatar>
            </Grid>
            <Grid size="grow">
              <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{profile.fullName}</Typography>
              <Typography color="text.secondary">{profile.email}</Typography>
              <Box sx={{ mt: 1 }}>
                <RoleLabel role={profile.role} />
                {profile.isActive ? (
                   <Chip label={t('common.active', 'Active')} color="success" size="small" sx={{ ml: 1 }} />
                ) : (
                   <Chip label={t('common.inactive', 'Inactive')} color="error" size="small" sx={{ ml: 1 }} />
                )}
              </Box>
            </Grid>
            <Grid size="auto">
              {!isEditing && (
                <Button variant="outlined" onClick={onEditClick}>
                  {t('common.edit', 'Edit Profile')}
                </Button>
              )}
            </Grid>
          </Grid>

          <Divider sx={{ mb: 4 }} />

          {isEditing ? (
            <form onSubmit={handleSubmit(onSubmit)} noValidate>
              <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    label={t('profile.fullName', 'Full Name')}
                    {...register('fullName')}
                    error={!!errors.fullName}
                    helperText={errors.fullName?.message ? t(errors.fullName.message, errors.fullName.message) : undefined}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    label={t('profile.phoneNumber', 'Phone Number')}
                    {...register('phoneNumber')}
                    error={!!errors.phoneNumber}
                    helperText={errors.phoneNumber?.message ? t(errors.phoneNumber.message, errors.phoneNumber.message) : undefined}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                    <Button variant="text" onClick={onCancelClick} disabled={updateMutation.isPending}>
                      {t('common.cancel', 'Cancel')}
                    </Button>
                    <Button 
                      type="submit" 
                      variant="contained" 
                      disabled={updateMutation.isPending}
                      startIcon={updateMutation.isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
                    >
                      {t('common.save', 'Save Changes')}
                    </Button>
                  </Box>
                </Grid>
              </Grid>
            </form>
          ) : (
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">{t('profile.fullName', 'Full Name')}</Typography>
                <Typography variant="body1">{profile.fullName}</Typography>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">{t('profile.phoneNumber', 'Phone Number')}</Typography>
                <Typography variant="body1">{profile.phoneNumber || '-'}</Typography>
              </Grid>
              {profile.institutionName && (
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">{t('profile.institution', 'Institution')}</Typography>
                  <Typography variant="body1">{profile.institutionName}</Typography>
                </Grid>
              )}
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">{t('profile.joined', 'Joined')}</Typography>
                <Typography variant="body1">{new Date(profile.createdAt).toLocaleDateString()}</Typography>
              </Grid>
            </Grid>
          )}
        </CardContent>
      </Card>
      
      {profile.role === UserRole.PlatformAdmin && (
        <Card sx={{ boxShadow: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>{t('profile.platformAdminInfo', 'Platform Administration')}</Typography>
            <Typography variant="body2" color="text.secondary">
              {t('profile.platformAdminDesc', 'You have full access to manage all institutions and platform settings.')}
            </Typography>
          </CardContent>
        </Card>
      )}
      
      {profile.role === UserRole.Admin && (
        <Card sx={{ boxShadow: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>{t('profile.institutionAdminInfo', 'Institution Administration')}</Typography>
            <Typography variant="body2" color="text.secondary">
              {t('profile.institutionAdminDesc', 'You have access to manage all aspects of your institution.')}
            </Typography>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default ProfilePage;
