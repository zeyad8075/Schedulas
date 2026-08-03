import { ReactNode } from 'react';
import { Box, Typography, Button, Breadcrumbs, Link } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { Link as RouterLink } from 'react-router-dom';

interface Breadcrumb {
  label: string;
  href?: string;
}

interface PageHeaderProps {
  title: string;
  breadcrumbs?: Breadcrumb[];
  actionLabel?: string;
  onActionClick?: () => void;
  actionHref?: string;
  actionIcon?: ReactNode;
}

export const PageHeader = ({
  title,
  breadcrumbs,
  actionLabel,
  onActionClick,
  actionHref,
  actionIcon = <AddIcon />,
}: PageHeaderProps) => {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mb: 4 }}>
      {breadcrumbs && breadcrumbs.length > 0 && (
        <Breadcrumbs aria-label="breadcrumb">
          {breadcrumbs.map((crumb, index) => {
            const isLast = index === breadcrumbs.length - 1;
            return isLast || !crumb.href ? (
              <Typography key={index} color="text.primary">
                {crumb.label}
              </Typography>
            ) : (
              <Link
                key={index}
                component={RouterLink}
                to={crumb.href}
                underline="hover"
                color="inherit"
              >
                {crumb.label}
              </Link>
            );
          })}
        </Breadcrumbs>
      )}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>
          {title}
        </Typography>

        {actionLabel && (
          <Button
            variant="contained"
            color="primary"
            startIcon={actionIcon}
            onClick={onActionClick}
            {...(actionHref ? { component: RouterLink, to: actionHref } : {})}
          >
            {actionLabel}
          </Button>
        )}
      </Box>
    </Box>
  );
};
