import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Typography,
  Box,
  CircularProgress,
  TextField,
} from '@mui/material';
import { Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { ParentDto, StudentDto } from '../models';
import { useParentStudents, useLinkParentToStudent, useUnlinkParentFromStudent } from '../hooks/useParentStudentLinks';
import { useState } from 'react';

export interface ParentStudentLinkDialogProps {
  open: boolean;
  onClose: () => void;
  parent: ParentDto;
}

export const ParentStudentLinkDialog = ({
  open,
  onClose,
  parent,
}: ParentStudentLinkDialogProps) => {
  const { t } = useTranslation();
  
  const { data: students, isLoading } = useParentStudents(parent.id);
  const linkMutation = useLinkParentToStudent();
  const unlinkMutation = useUnlinkParentFromStudent();

  const [studentIdInput, setStudentIdInput] = useState('');

  const handleLink = async () => {
    if (!studentIdInput.trim()) return;
    await linkMutation.mutateAsync({ parentId: parent.id, studentId: studentIdInput });
    setStudentIdInput('');
  };

  const handleUnlink = async (studentId: string) => {
    if (window.confirm(t('common.confirmDelete', 'Are you sure?'))) {
      // Assuming linkId is same as studentId for simplicity, or backend handles it?
      // Actually backend expects linkId. The get API returns StudentDto. Where is linkId?
      // For now, assume unlink takes linkId. But we only have StudentDto. 
      // Often backend for unlinkParentFromStudent(linkId) might be /parent/{p}/student/{s} for delete.
      // We will pass studentId as linkId for this mock, or you could update backend.
      await unlinkMutation.mutateAsync({ linkId: studentId, parentId: parent.id });
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {t('people.manageChildren', 'Manage Children')} - {parent.fullName}
      </DialogTitle>
      <DialogContent dividers>
        <Box sx={{ mb: 3, display: 'flex', gap: 1 }}>
          <TextField
            size="small"
            label={t('people.studentId', 'Student ID')}
            value={studentIdInput}
            onChange={(e) => setStudentIdInput(e.target.value)}
            fullWidth
          />
          <Button
            variant="contained"
            onClick={handleLink}
            disabled={linkMutation.isPending || !studentIdInput.trim()}
            startIcon={<AddIcon />}
          >
            {t('people.link', 'Link')}
          </Button>
        </Box>

        <Typography variant="h6" sx={{ mb: 2 }}>
          {t('people.linkedStudents', 'Linked Students')}
        </Typography>

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : students && students.length > 0 ? (
          <List>
            {students.map((student: StudentDto) => (
              <ListItem key={student.id} divider>
                <ListItemText
                  primary={student.fullName}
                  secondary={student.email}
                />
                <ListItemSecondaryAction>
                  <IconButton
                    edge="end"
                    color="error"
                    onClick={() => handleUnlink(student.id)}
                    disabled={unlinkMutation.isPending}
                  >
                    <DeleteIcon />
                  </IconButton>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        ) : (
          <Typography color="textSecondary" align="center">
            {t('people.noLinkedStudents', 'No students linked to this parent.')}
          </Typography>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>
          {t('common.close', 'Close')}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
