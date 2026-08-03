export const UserRole = {
  Student: 0,
  Teacher: 1,
  Admin: 2,
  PlatformAdmin: 3,
} as const;

export type UserRole = typeof UserRole[keyof typeof UserRole];

export interface JwtPayload {
  sub: string;
  email?: string;
  role: string | number;
  institutionId?: string;
  exp?: number;
}

export interface User {
  id: string;
  email: string;
  role: UserRole;
  institutionId?: string;
}
