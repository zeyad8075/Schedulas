import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, type LoginFormData } from '../schemas/authSchemas';
import { useLoginMutation } from '../hooks/useAuthMutations';
import { Box, Button, TextField, Typography, Alert, CircularProgress, IconButton, InputAdornment, Link } from '@mui/material';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import { Link as RouterLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

const LoginPage = () => {
  const { t } = useTranslation();
  const [showPassword, setShowPassword] = useState(false);
  const loginMutation = useLoginMutation();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = (data: LoginFormData) => {
    loginMutation.mutate(data);
  };

  const handleClickShowPassword = () => setShowPassword((show) => !show);
  const handleMouseDownPassword = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.preventDefault();
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 400, mx: 'auto', p: 3, boxShadow: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
      <Typography variant="h5" component="h1" gutterBottom align="center" sx={{ fontWeight: 'bold' }}>
        {t('auth.loginTitle', 'Welcome Back')}
      </Typography>
      
      {loginMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {/* @ts-ignore - Assuming standard Axios error */}
          {loginMutation.error?.response?.data?.message || t('auth.loginError', 'Failed to login. Please check your credentials.')}
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
        />

        <TextField
          fullWidth
          label={t('auth.password', 'Password')}
          type={showPassword ? 'text' : 'password'}
          margin="normal"
          {...register('password')}
          error={!!errors.password}
          helperText={errors.password?.message ? t(errors.password.message, errors.password.message) : undefined}
          autoComplete="current-password"
          slotProps={{
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle password visibility"
                    onClick={handleClickShowPassword}
                    onMouseDown={handleMouseDownPassword}
                    edge="end"
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }
          }}
        />

        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 1, mb: 2 }}>
          <Link component={RouterLink} to="/forgot-password" variant="body2">
            {t('auth.forgotPasswordLink', 'Forgot password?')}
          </Link>
        </Box>

        <Button
          type="submit"
          fullWidth
          variant="contained"
          size="large"
          disabled={loginMutation.isPending}
          startIcon={loginMutation.isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
          sx={{ mt: 1 }}
        >
          {loginMutation.isPending ? t('common.loading', 'Loading...') : t('auth.loginButton', 'Sign In')}
        </Button>
      </form>
    </Box>
  );
};

export default LoginPage;
