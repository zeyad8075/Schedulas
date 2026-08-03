import { createTheme } from '@mui/material/styles';

const baseThemeOptions = {
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none' as const,
          fontWeight: 600,
        },
      },
    },
  },
};

export const lightTheme = createTheme({
  ...baseThemeOptions,
  palette: {
    mode: 'light',
    primary: {
      main: '#4F46E5', // Indigo 600
    },
    secondary: {
      main: '#10B981', // Emerald 500
    },
    background: {
      default: '#F3F4F6',
      paper: '#FFFFFF',
    },
  },
});

export const darkTheme = createTheme({
  ...baseThemeOptions,
  palette: {
    mode: 'dark',
    primary: {
      main: '#6366F1', // Indigo 500
    },
    secondary: {
      main: '#34D399', // Emerald 400
    },
    background: {
      default: '#111827',
      paper: '#1F2937',
    },
  },
});
