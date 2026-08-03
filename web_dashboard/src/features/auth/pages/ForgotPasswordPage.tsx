import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { forgotPasswordSchema, type ForgotPasswordFormData } from '../schemas/authSchemas';
import { useForgotPasswordMutation } from '../hooks/useAuthMutations';
import { Box, Button, TextField, Typography, Alert, CircularProgress, Link } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

const ForgotPasswordPage = () => {
  const { t } = useTranslation();
  const forgotPasswordMutation = useForgotPasswordMutation();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = (data: ForgotPasswordFormData) => {
    forgotPasswordMutation.mutate(data);
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 400, mx: 'auto', p: 3, boxShadow: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
      <Typography variant="h5" component="h1" gutterBottom align="center" sx={{ fontWeight: 'bold' }}>
        {t('auth.forgotPasswordTitle', 'Reset Password')}
      </Typography>
      
      <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 3 }}>
        {t('auth.forgotPasswordDesc', 'Enter your email address and we will send you instructions to reset your password.')}
      </Typography>

      {forgotPasswordMutation.isSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {t('auth.forgotPasswordSuccess', 'If an account exists, you will receive an email with instructions.')}
        </Alert>
      )}

      {forgotPasswordMutation.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {/* @ts-ignore */}
          {forgotPasswordMutation.error?.response?.data?.message || t('auth.forgotPasswordError', 'Something went wrong. Please try again.')}
        </Alert>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <TextField
          fullWidth
          label={t('auth.email', 'Email Address')}
          margin="normal"
          {...register('email')}
          error={!!errors.email}
          helperText={errors.email?.message ? t(errors.email.message, errors.email.message) : undefined}
          autoComplete="email"
          autoFocus
          disabled={forgotPasswordMutation.isPending || forgotPasswordMutation.isSuccess}
        />

        <Button
          type="submit"
          fullWidth
          variant="contained"
          size="large"
          disabled={forgotPasswordMutation.isPending || forgotPasswordMutation.isSuccess}
          startIcon={forgotPasswordMutation.isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
          sx={{ mt: 3, mb: 2 }}
        >
          {forgotPasswordMutation.isPending ? t('common.loading', 'Loading...') : t('auth.sendResetLink', 'Send Reset Link')}
        </Button>

        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
          <Link component={RouterLink} to="/login" variant="body2">
            {t('auth.backToLogin', 'Back to login')}
          </Link>
        </Box>
      </form>
    </Box>
  );
};

export default ForgotPasswordPage;
