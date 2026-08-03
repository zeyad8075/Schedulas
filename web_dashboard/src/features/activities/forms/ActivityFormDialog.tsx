/* oxlint-disable react-hooks/rules-of-hooks */
import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  CircularProgress,
  Alert,
  AlertTitle,
} from '@mui/material';
import Grid from '@mui/material/Grid';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useTranslation } from 'react-i18next';
import { ActivityDto, ActivityType, ActivitySubmissionResult } from '../models';
import { useCreateActivity, useUpdateActivity, useOverrideActivity } from '../hooks/useActivities';

const schema = z.object({
  classId: z.string().min(1, 'Class ID is required'),
  activityType: z.number(),
  title: z.string().min(1, 'Title is required'),
  description: z.string().optional(),
  scheduledDate: z.string().min(1, 'Date is required'),
  scheduledTime: z.string().optional(),
  endTime: z.string().optional(),
  duration: z.string().optional(),
  priority: z.number().min(0, 'Priority must be 0 or greater'),
  estimatedWeight: z.number().optional(),
});

type FormData = z.infer<typeof schema>;

interface ActivityFormDialogProps {
  open: boolean;
  onClose: () => void;
  activity?: ActivityDto;
}

export const ActivityFormDialog = ({ open, onClose, activity }: ActivityFormDialogProps) => {
  const { t } = mergeTranslations();
  const [submissionResult, setSubmissionResult] = useState<ActivitySubmissionResult | null>(null);
  
  const createMutation = useCreateActivity();
  const updateMutation = useUpdateActivity();
  const overrideMutation = useOverrideActivity();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      classId: '',
      activityType: ActivityType.StandardClass,
      title: '',
      description: '',
      scheduledDate: '',
      scheduledTime: '',
      endTime: '',
      duration: '',
      priority: 0,
    },
  });

  useEffect(() => {
    if (open) {
      if (activity) {
        reset({
          classId: activity.classId,
          activityType: activity.activityType,
          title: activity.title,
          description: activity.description || '',
          scheduledDate: activity.scheduledDate,
          scheduledTime: activity.scheduledTime || '',
          endTime: activity.endTime || '',
          duration: activity.duration || '',
          priority: activity.priority,
          estimatedWeight: activity.estimatedWeight,
        });
      } else {
        reset({
          classId: '',
          activityType: ActivityType.StandardClass,
          title: '',
          description: '',
          scheduledDate: '',
          scheduledTime: '',
          endTime: '',
          duration: '',
          priority: 0,
        });
      }
      setSubmissionResult(null);
    }
  }, [open, activity, reset]);

  const onSubmit = async (data: FormData) => {
    setSubmissionResult(null);
    let result: ActivitySubmissionResult;

    if (activity) {
      result = await updateMutation.mutateAsync({ id: activity.id, data });
    } else {
      result = await createMutation.mutateAsync(data);
    }

    if (result.success && !result.requiresOverride) {
      onClose();
    } else {
      setSubmissionResult(result);
    }
  };

  const handleOverride = async () => {
    if (submissionResult?.activity?.id) {
      const result = await overrideMutation.mutateAsync(submissionResult.activity.id);
      if (result.success) {
        onClose();
      } else {
        setSubmissionResult(result);
      }
    }
  };

  const isLoading = createMutation.isPending || updateMutation.isPending || overrideMutation.isPending;

  // Simple helper to avoid hook issues with i18n
  function mergeTranslations() {
      const { t } = useTranslation();
      return { t };
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogTitle>
          {activity ? t('activities.editActivity', 'Edit Activity') : t('activities.addActivity', 'Add Activity')}
        </DialogTitle>
        <DialogContent dividers>
          {submissionResult && !submissionResult.success && !submissionResult.requiresOverride && (
            <Alert severity="error" sx={{ mb: 2 }}>
              <AlertTitle>{t('activities.validationError', 'Validation Error')}</AlertTitle>
              {submissionResult.reasonCode}
            </Alert>
          )}

          {submissionResult && submissionResult.requiresOverride && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              <AlertTitle>{t('activities.ruleViolationWarning', 'Rule Violation Warning')}</AlertTitle>
              {t('activities.overrideWarningDesc', 'This scheduling violates a rule. Do you want to override it?')}
              <br />
              <strong>{submissionResult.reasonCode}</strong>
            </Alert>
          )}

          <Grid container spacing={2}>
            <Grid size={12}>
              <Controller
                name="title"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('activities.title', 'Title')}
                    fullWidth
                    error={!!errors.title}
                    helperText={errors.title?.message}
                  />
                )}
              />
            </Grid>
            <Grid size={12}>
              <Controller
                name="description"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('activities.description', 'Description')}
                    fullWidth
                    multiline
                    rows={3}
                  />
                )}
              />
            </Grid>
            <Grid size={6}>
              <Controller
                name="activityType"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    select
                    label={t('activities.type', 'Type')}
                    fullWidth
                  >
                    {Object.values(ActivityType)
                      .filter((v) => typeof v === 'number')
                      .map((type) => (
                        <MenuItem key={type} value={type}>
                          {t(`activities.types.${(ActivityType as any)[type]}`, (ActivityType as any)[type]) as string}
                        </MenuItem>
                      ))}
                  </TextField>
                )}
              />
            </Grid>
            <Grid size={6}>
              <Controller
                name="classId"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('activities.classId', 'Class ID')}
                    fullWidth
                    error={!!errors.classId}
                    helperText={errors.classId?.message}
                  />
                )}
              />
            </Grid>
            <Grid size={6}>
              <Controller
                name="scheduledDate"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('activities.scheduledDate', 'Scheduled Date')}
                    type="date"
                    fullWidth
                    slotProps={{ inputLabel: { shrink: true } }}
                    error={!!errors.scheduledDate}
                    helperText={errors.scheduledDate?.message}
                  />
                )}
              />
            </Grid>
            <Grid size={6}>
              <Controller
                name="scheduledTime"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('activities.scheduledTime', 'Scheduled Time')}
                    type="time"
                    fullWidth
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                )}
              />
            </Grid>
            <Grid size={6}>
              <Controller
                name="endTime"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={t('activities.endTime', 'End Time')}
                    type="time"
                    fullWidth
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                )}
              />
            </Grid>
            <Grid size={6}>
              <Controller
                name="priority"
                control={control}
                render={({ field: { onChange, value, ...rest } }) => (
                  <TextField
                    {...rest}
                    value={value}
                    onChange={(e) => onChange(Number(e.target.value))}
                    label={t('activities.priority', 'Priority')}
                    type="number"
                    fullWidth
                    error={!!errors.priority}
                    helperText={errors.priority?.message}
                  />
                )}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isLoading}>
            {t('common.cancel', 'Cancel')}
          </Button>

          {submissionResult?.requiresOverride ? (
            <Button
              onClick={handleOverride}
              color="warning"
              variant="contained"
              disabled={isLoading}
            >
              {isLoading ? <CircularProgress size={24} /> : t('activities.override', 'Override & Save')}
            </Button>
          ) : (
            <Button type="submit" variant="contained" color="primary" disabled={isLoading}>
              {isLoading ? <CircularProgress size={24} /> : t('common.save', 'Save Changes')}
            </Button>
          )}
        </DialogActions>
      </form>
    </Dialog>
  );
};
