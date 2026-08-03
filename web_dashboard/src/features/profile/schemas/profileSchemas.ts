import { z } from 'zod';

export const updateProfileSchema = z.object({
  fullName: z.string().min(2, 'Full name is required'),
  phoneNumber: z.string().optional(),
});

export type UpdateProfileFormData = z.infer<typeof updateProfileSchema>;
