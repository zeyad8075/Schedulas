import { Box, Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Toolbar, Typography, Divider, Collapse } from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import SchoolIcon from '@mui/icons-material/School';
import PeopleIcon from '@mui/icons-material/People';
import EventIcon from '@mui/icons-material/Event';
import NotificationsIcon from '@mui/icons-material/Notifications';
import BarChartIcon from '@mui/icons-material/BarChart';

import SettingsIcon from '@mui/icons-material/Settings';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../features/auth/hooks/useAuth';
import { UserRole } from '../features/auth/types';

interface SidebarProps {
  drawerWidth: number;
  mobileOpen: boolean;
  onClose: () => void;
  isMobile: boolean;
}

const Sidebar = ({ drawerWidth, mobileOpen, onClose, isMobile }: SidebarProps) => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [academicOpen, setAcademicOpen] = useState(false);
  const [peopleOpen, setPeopleOpen] = useState(false);
  const [schedulingOpen, setSchedulingOpen] = useState(false);
  const [reportsOpen, setReportsOpen] = useState(false);

  const handleAcademicClick = () => {
    setAcademicOpen(!academicOpen);
  };

  const handlePeopleClick = () => {
    setPeopleOpen(!peopleOpen);
  };

  const handleSchedulingClick = () => {
    setSchedulingOpen(!schedulingOpen);
  };

  const handleReportsClick = () => {
    setReportsOpen(!reportsOpen);
  };

  const menuItems = [
    { text: t('nav.dashboard', 'Dashboard'), icon: <DashboardIcon />, path: '/' },
  ];

  if (user?.role === UserRole.Admin || user?.role === UserRole.PlatformAdmin) {
    menuItems.push({ text: t('nav.settings', 'Settings'), icon: <SettingsIcon />, path: '/admin/settings' });
  }

  const drawer = (
    <div>
      <Toolbar sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <img src="/logo.png" alt="Schedulas Logo" style={{ width: 32, height: 32, objectFit: 'contain' }} />
        <Typography variant="h6" noWrap component="div" sx={{ fontWeight: 'bold', color: 'primary.main' }}>
          Schedulas
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {menuItems.map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton
              component={NavLink}
              to={item.path}
              onClick={isMobile ? onClose : undefined}
              sx={{
                '&.active': {
                  bgcolor: 'action.selected',
                  borderRight: '4px solid',
                  borderColor: 'primary.main',
                },
              }}
            >
              <ListItemIcon sx={{ color: 'inherit' }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}

        {user?.role === UserRole.Admin || user?.role === UserRole.PlatformAdmin ? (
          <>
            <ListItem disablePadding>
              <ListItemButton onClick={handleAcademicClick}>
                <ListItemIcon sx={{ color: 'inherit' }}><SchoolIcon /></ListItemIcon>
                <ListItemText primary={t('nav.academic', 'Academic')} />
                {academicOpen ? <ExpandLess /> : <ExpandMore />}
              </ListItemButton>
            </ListItem>
            <Collapse in={academicOpen} timeout="auto" unmountOnExit>
              <List component="div" disablePadding>
                {[
                  { text: t('academic.institutions', 'Institutions'), path: '/admin/academic/institutions' },
                  { text: t('academic.departments', 'Departments'), path: '/admin/academic/departments' },
                  { text: t('academic.programs', 'Programs'), path: '/admin/academic/programs' },
                  { text: t('academic.courses', 'Courses'), path: '/admin/academic/courses' },
                  { text: t('academic.classes', 'Classes'), path: '/admin/academic/classes' },
                  { text: t('academic.terms', 'Terms'), path: '/admin/academic/terms' },
                  { text: t('academic.holidays', 'Holidays'), path: '/admin/academic/holidays' },
                ].map((subItem) => (
                  <ListItemButton
                    key={subItem.text}
                    component={NavLink}
                    to={subItem.path}
                    onClick={isMobile ? onClose : undefined}
                    sx={{
                      pl: 4,
                      '&.active': {
                        bgcolor: 'action.selected',
                        borderRight: '4px solid',
                        borderColor: 'primary.main',
                      },
                    }}
                  >
                    <ListItemText primary={subItem.text} />
                  </ListItemButton>
                ))}
              </List>
            </Collapse>

            <ListItem disablePadding>
              <ListItemButton onClick={handlePeopleClick}>
                <ListItemIcon sx={{ color: 'inherit' }}><PeopleIcon /></ListItemIcon>
                <ListItemText primary={t('nav.people', 'People')} />
                {peopleOpen ? <ExpandLess /> : <ExpandMore />}
              </ListItemButton>
            </ListItem>
            <Collapse in={peopleOpen} timeout="auto" unmountOnExit>
              <List component="div" disablePadding>
                {[
                  { text: t('people.profiles', 'Profiles'), path: '/admin/people/profiles' },
                  { text: t('people.students', 'Students'), path: '/admin/people/students' },
                  { text: t('people.teachers', 'Teachers'), path: '/admin/people/teachers' },
                  { text: t('people.parents', 'Parents'), path: '/admin/people/parents' },
                ].map((subItem) => (
                  <ListItemButton
                    key={subItem.text}
                    component={NavLink}
                    to={subItem.path}
                    onClick={isMobile ? onClose : undefined}
                    sx={{
                      pl: 4,
                      '&.active': {
                        bgcolor: 'action.selected',
                        borderRight: '4px solid',
                        borderColor: 'primary.main',
                      },
                    }}
                  >
                    <ListItemText primary={subItem.text} />
                  </ListItemButton>
                ))}
              </List>
            </Collapse>

            <ListItem disablePadding>
              <ListItemButton onClick={handleSchedulingClick}>
                <ListItemIcon sx={{ color: 'inherit' }}><EventIcon /></ListItemIcon>
                <ListItemText primary={t('nav.scheduling', 'Scheduling')} />
                {schedulingOpen ? <ExpandLess /> : <ExpandMore />}
              </ListItemButton>
            </ListItem>
            <Collapse in={schedulingOpen} timeout="auto" unmountOnExit>
              <List component="div" disablePadding>
                {[
                  { text: t('nav.activities', 'Activities'), path: '/admin/activities' },
                  { text: t('nav.calendar', 'Calendar'), path: '/admin/calendar' },
                ].map((subItem) => (
                  <ListItemButton
                    key={subItem.text}
                    component={NavLink}
                    to={subItem.path}
                    onClick={isMobile ? onClose : undefined}
                    sx={{
                      pl: 4,
                      '&.active': {
                        bgcolor: 'action.selected',
                        borderRight: '4px solid',
                        borderColor: 'primary.main',
                      },
                    }}
                  >
                    <ListItemText primary={subItem.text} />
                  </ListItemButton>
                ))}
              </List>
            </Collapse>

            <ListItem disablePadding>
              <ListItemButton onClick={handleReportsClick}>
                <ListItemIcon sx={{ color: 'inherit' }}><BarChartIcon /></ListItemIcon>
                <ListItemText primary={t('nav.reports', 'Reports')} />
                {reportsOpen ? <ExpandLess /> : <ExpandMore />}
              </ListItemButton>
            </ListItem>
            <Collapse in={reportsOpen} timeout="auto" unmountOnExit>
              <List component="div" disablePadding>
                {[
                  { text: t('nav.institutionWorkload', 'Institution'), path: '/admin/reports/institution' },
                  { text: t('nav.teacherWorkload', 'Teachers'), path: '/admin/reports/teachers' },
                  { text: t('nav.studentWorkload', 'Students'), path: '/admin/reports/students' },
                  { text: t('nav.activityDistribution', 'Distribution'), path: '/admin/reports/distribution' },
                ].map((subItem) => (
                  <ListItemButton
                    key={subItem.text}
                    component={NavLink}
                    to={subItem.path}
                    onClick={isMobile ? onClose : undefined}
                    sx={{
                      pl: 4,
                      '&.active': {
                        bgcolor: 'action.selected',
                        borderRight: '4px solid',
                        borderColor: 'primary.main',
                      },
                    }}
                  >
                    <ListItemText primary={subItem.text} />
                  </ListItemButton>
                ))}
              </List>
            </Collapse>

            <ListItem disablePadding>
              <ListItemButton
                component={NavLink}
                to="/admin/notifications"
                onClick={isMobile ? onClose : undefined}
                sx={{
                  '&.active': {
                    bgcolor: 'action.selected',
                    borderRight: '4px solid',
                    borderColor: 'primary.main',
                  },
                }}
              >
                <ListItemIcon sx={{ color: 'inherit' }}><NotificationsIcon /></ListItemIcon>
                <ListItemText primary={t('nav.notifications', 'Notifications')} />
              </ListItemButton>
            </ListItem>
          </>
        ) : null}
      </List>
    </div>
  );

  return (
    <Box component="nav" sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}>
      {isMobile ? (
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={onClose}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawer}
        </Drawer>
      ) : (
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth, borderRight: '1px solid', borderColor: 'divider' },
          }}
          open
        >
          {drawer}
        </Drawer>
      )}
    </Box>
  );
};

export default Sidebar;
