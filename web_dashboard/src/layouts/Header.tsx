import { AppBar, IconButton, Toolbar, Typography, Box, Badge, Menu, MenuItem, Avatar, useTheme } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import NotificationsIcon from '@mui/icons-material/Notifications';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import { useState } from 'react';
import { useAuth } from '../features/auth/hooks/useAuth';
import { useTranslation } from 'react-i18next';
import { useThemeContext } from '../theme/ThemeContextProvider';
import { useNavigate } from 'react-router-dom';
import { useUnreadNotificationsCount } from '../features/notifications/hooks/useNotifications';

interface HeaderProps {
  drawerWidth: number;
  onMenuClick: () => void;
}

const Header = ({ drawerWidth, onMenuClick }: HeaderProps) => {
  const { user, logout } = useAuth();
  const { t, i18n } = useTranslation();
  const { toggleTheme, mode } = useThemeContext();
  const theme = useTheme();
  const navigate = useNavigate();
  const { data: unreadCount } = useUnreadNotificationsCount();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    handleClose();
    logout();
  };

  const navigateToProfile = () => {
    handleClose();
    navigate('/profile');
  };

  const toggleLanguage = () => {
    const newLang = i18n.language === 'en' ? 'ar' : 'en';
    i18n.changeLanguage(newLang);
    document.dir = newLang === 'ar' ? 'rtl' : 'ltr';
  };

  return (
    <AppBar
      position="fixed"
      sx={{
        width: { md: `calc(100% - ${drawerWidth}px)` },
        ml: { md: `${drawerWidth}px` },
        bgcolor: 'background.paper',
        color: 'text.primary',
        boxShadow: 1,
      }}
    >
      <Toolbar>
        <IconButton
          color="inherit"
          aria-label="open drawer"
          edge="start"
          onClick={onMenuClick}
          sx={{ mr: 2, display: { md: 'none' } }}
        >
          <MenuIcon />
        </IconButton>
        
        <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
          {t('common.dashboard', 'Dashboard')}
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <IconButton color="inherit" onClick={toggleLanguage}>
            <Typography variant="button">{i18n.language === 'en' ? 'عربي' : 'EN'}</Typography>
          </IconButton>
          
          {user && (
            <IconButton color="inherit" onClick={() => navigate('/admin/notifications')}>
              <Badge badgeContent={unreadCount || 0} color="error">
                <NotificationsIcon />
              </Badge>
            </IconButton>
          )}
          
          <IconButton color="inherit" onClick={toggleTheme}>
            {mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
          </IconButton>

          <IconButton
            size="large"
            aria-label="account of current user"
            aria-controls="menu-appbar"
            aria-haspopup="true"
            onClick={handleMenu}
            color="inherit"
          >
            <Avatar sx={{ width: 32, height: 32, bgcolor: theme.palette.primary.main }}>
              {user?.email?.[0]?.toUpperCase() || 'U'}
            </Avatar>
          </IconButton>
          
          <Menu
            id="menu-appbar"
            anchorEl={anchorEl}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            keepMounted
            transformOrigin={{ vertical: 'top', horizontal: 'right' }}
            open={Boolean(anchorEl)}
            onClose={handleClose}
          >
            <MenuItem onClick={navigateToProfile}>{t('common.profile', 'Profile')}</MenuItem>
            <MenuItem onClick={handleLogout}>{t('common.logout', 'Logout')}</MenuItem>
          </Menu>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Header;
