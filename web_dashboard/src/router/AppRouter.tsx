import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { RequireAuth, RequireGuest, RequireRole } from './Guards';
import { UserRole } from '../features/auth/types';

import AppShell from '../layouts/AppShell';
import AuthLayout from '../layouts/AuthLayout';

import LoginPage from '../features/auth/pages/LoginPage';
import ForgotPasswordPage from '../features/auth/pages/ForgotPasswordPage';
import ProfilePage from '../features/profile/pages/ProfilePage';
import { InstitutionsPage } from '../features/academic/pages/InstitutionsPage';
import { DepartmentsPage } from '../features/academic/pages/DepartmentsPage';
import { ProgramsPage } from '../features/academic/pages/ProgramsPage';
import { CoursesPage } from '../features/academic/pages/CoursesPage';
import { ClassesPage } from '../features/academic/pages/ClassesPage';
import { TermsPage } from '../features/academic/pages/TermsPage';
import { HolidaysPage } from '../features/academic/pages/HolidaysPage';

import { ProfilesPage } from '../features/people/pages/ProfilesPage';
import { StudentsPage } from '../features/people/pages/StudentsPage';
import { TeachersPage } from '../features/people/pages/TeachersPage';
import { ParentsPage } from '../features/people/pages/ParentsPage';
import { ActivitiesPage } from '../features/activities/pages/ActivitiesPage';
import { CalendarPage } from '../features/calendar/pages/CalendarPage';
import { NotificationsPage } from '../features/notifications/pages/NotificationsPage';
import { InstitutionWorkloadPage } from '../features/reports/pages/InstitutionWorkloadPage';
import { TeacherWorkloadPage } from '../features/reports/pages/TeacherWorkloadPage';
import { StudentWorkloadPage } from '../features/reports/pages/StudentWorkloadPage';
import { ActivityDistributionPage } from '../features/reports/pages/ActivityDistributionPage';

// Placeholder Pages for Phase F1/F2
const Dashboard = () => <div>Dashboard Home</div>;
const Unauthorized = () => <div>403 - Unauthorized</div>;
const NotFound = () => <div>404 - Not Found</div>;

const router = createBrowserRouter([
  {
    element: <RequireGuest />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          { path: '/login', element: <LoginPage /> },
          { path: '/forgot-password', element: <ForgotPasswordPage /> },
        ],
      },
    ],
  },
  {
    element: <RequireAuth />,
    children: [
      {
        path: '/',
        element: <AppShell />,
        children: [
          { index: true, element: <Dashboard /> },
          { path: 'profile', element: <ProfilePage /> },
          {
            path: 'admin',
            element: <RequireRole roles={[UserRole.Admin, UserRole.PlatformAdmin]} />,
            children: [
              { path: 'settings', element: <div>Admin Settings</div> },
              {
                path: 'academic',
                children: [
                  { path: 'institutions', element: <InstitutionsPage /> },
                  { path: 'departments', element: <DepartmentsPage /> },
                  { path: 'programs', element: <ProgramsPage /> },
                  { path: 'courses', element: <CoursesPage /> },
                  { path: 'classes', element: <ClassesPage /> },
                  { path: 'terms', element: <TermsPage /> },
                  { path: 'holidays', element: <HolidaysPage /> },
                ]
              },
              {
                path: 'people',
                children: [
                  { path: 'profiles', element: <ProfilesPage /> },
                  { path: 'students', element: <StudentsPage /> },
                  { path: 'teachers', element: <TeachersPage /> },
                  { path: 'parents', element: <ParentsPage /> },
                ]
              },
              { path: 'activities', element: <ActivitiesPage /> },
              { path: 'calendar', element: <CalendarPage /> },
              { path: 'notifications', element: <NotificationsPage /> },
              {
                path: 'reports',
                children: [
                  { path: 'institution', element: <InstitutionWorkloadPage /> },
                  { path: 'teachers', element: <TeacherWorkloadPage /> },
                  { path: 'students', element: <StudentWorkloadPage /> },
                  { path: 'distribution', element: <ActivityDistributionPage /> },
                ]
              },
            ]
          }
        ],
      },
    ],
  },
  {
    path: '/unauthorized',
    element: <Unauthorized />,
  },
  {
    path: '*',
    element: <NotFound />,
  },
]);

export const AppRouter = () => {
  return <RouterProvider router={router} />;
};
