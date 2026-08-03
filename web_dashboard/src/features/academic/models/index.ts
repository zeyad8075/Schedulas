export enum InstitutionType {
  School = 'School',
  University = 'University',
  TrainingCenter = 'TrainingCenter',
  Other = 'Other',
}

export interface InstitutionDto {
  id: string;
  name: string;
  type: InstitutionType;
  timezone: string;
  logoUrl?: string;
  isSuspended: boolean;
}

export interface DepartmentDto {
  id: string;
  institutionId: string;
  name: string;
  isActive: boolean;
}

export interface ProgramDto {
  id: string;
  departmentId: string;
  name: string;
  isActive: boolean;
}

export interface CourseDto {
  id: string;
  programId: string;
  name: string;
  code?: string;
  isActive: boolean;
}

export interface ClassDto {
  id: string;
  courseId: string;
  academicTermId: string;
  name: string;
  isActive: boolean;
}

export interface AcademicTermDto {
  id: string;
  institutionId: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface HolidayDto {
  id: string;
  institutionId: string;
  academicTermId?: string;
  name: string;
  holidayDate: string;
}
