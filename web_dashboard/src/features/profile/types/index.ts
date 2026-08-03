import type { UserRole } from '../../auth/types';

export interface ProfileDto {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
  phoneNumber?: string;
  institutionId?: string;
  institutionName?: string;
  isActive: boolean;
  createdAt: string;
  preferredTheme: number;
}
