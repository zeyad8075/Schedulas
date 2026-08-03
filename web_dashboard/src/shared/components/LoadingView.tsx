import { Box, CircularProgress, Typography } from '@mui/material';

interface LoadingViewProps {
  message?: string;
}

export const LoadingView = ({ message = 'Loading...' }: LoadingViewProps) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        p: 4,
        minHeight: 200,
      }}
    >
      <CircularProgress />
      <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
        {message}
      </Typography>
    </Box>
  );
};
