/* oxlint-disable react/only-export-components */
import { useState, useEffect, createContext, type ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authStorage } from '../../../core/auth/authStorage';

import { UserRole, type JwtPayload, type User } from '../types';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (token: string, refreshToken: string) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const initAuth = () => {
    const token = authStorage.getToken();
    if (token) {
      try {
        const decoded = jwtDecode<JwtPayload>(token);
        const currentTime = Date.now() / 1000;
        
        if (decoded.exp && decoded.exp > currentTime) {
          const userMeta = (decoded as any).user_metadata || {};
          const rawRole = userMeta.role;
          const parsedRole = typeof rawRole === 'string' ? parseInt(rawRole, 10) : (rawRole ?? UserRole.Student);

          setUser({
            id: decoded.sub,
            email: decoded.email || '',
            role: parsedRole as UserRole,
            institutionId: userMeta.institutionId,
          });
        } else {
          authStorage.clear();
          setUser(null);
        }
      } catch {
        authStorage.clear();
        setUser(null);
      }
    } else {
      setUser(null);
    }
    setIsLoading(false);
  };

  useEffect(() => {
    initAuth();

    const handleLogout = () => {
      setUser(null);
    };

    window.addEventListener('auth:logout', handleLogout);
    return () => {
      window.removeEventListener('auth:logout', handleLogout);
    };
  }, []);

  const login = (token: string, refreshToken: string) => {
    authStorage.setToken(token);
    authStorage.setRefreshToken(refreshToken);
    initAuth();
  };

  const logout = () => {
    authStorage.clear();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};


