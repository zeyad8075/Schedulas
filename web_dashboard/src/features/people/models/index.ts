import { UserRole } from '../../auth/types';

export enum Theme {
  System = 0,
  Light = 1,
  Dark = 2,
}

export interface ProfileDto {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: UserRole;
  institutionId?: string;
  departmentId?: string;
  isActive: boolean;
  preferredTheme: Theme;
}

export interface StudentDto {
  id: string;
  profileId: string;
  institutionId: string;
  studentNumber?: string;
  fullName: string;
  email: string;
  isActive: boolean;
}

export interface TeacherDto {
  id: string;
  profileId: string;
  institutionId: string;
  departmentId?: string;
  fullName: string;
  email: string;
  isActive: boolean;
}

export interface ParentDto {
  id: string;
  profileId: string;
  fullName: string;
  email: string;
  isActive: boolean;
}

export interface ParentStudentLinkDto {
  id: string;
  parentId: string;
  studentId: string;
  linkedAt: string;
}
