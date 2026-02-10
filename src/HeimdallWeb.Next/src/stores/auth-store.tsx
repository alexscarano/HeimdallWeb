"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { UserProfile } from "@/types/user";
import type { LoginRequest, RegisterRequest } from "@/types/user";
import { UserType } from "@/types/common";
import * as authApi from "@/lib/api/auth.api";
import * as userApi from "@/lib/api/user.api";

interface AuthState {
  user: UserProfile | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    try {
      const stored = localStorage.getItem("heimdall_user");
      if (!stored) {
        setUser(null);
        return;
      }
      const parsed = JSON.parse(stored) as { userId: string };
      const profile = await userApi.getUserProfile(parsed.userId);
      setUser(profile);
      localStorage.setItem("heimdall_user", JSON.stringify(profile));
    } catch {
      setUser(null);
      localStorage.removeItem("heimdall_user");
    }
  }, []);

  useEffect(() => {
    const stored = localStorage.getItem("heimdall_user");
    if (stored) {
      try {
        setUser(JSON.parse(stored) as UserProfile);
      } catch {
        localStorage.removeItem("heimdall_user");
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const response = await authApi.login(data);
    const profile: UserProfile = {
      userId: response.userId,
      username: response.username,
      email: response.email,
      userType: response.userType,
      isActive: response.isActive,
      profileImage: null,
      createdAt: new Date().toISOString(),
    };
    setUser(profile);
    localStorage.setItem("heimdall_user", JSON.stringify(profile));
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    const response = await authApi.register(data);
    const profile: UserProfile = {
      userId: response.userId,
      username: response.username,
      email: response.email,
      userType: response.userType,
      isActive: response.isActive,
      profileImage: null,
      createdAt: new Date().toISOString(),
    };
    setUser(profile);
    localStorage.setItem("heimdall_user", JSON.stringify(profile));
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      setUser(null);
      localStorage.removeItem("heimdall_user");
    }
  }, []);

  const value = useMemo<AuthState>(
    () => ({
      user,
      isLoading,
      isAuthenticated: !!user,
      isAdmin: user?.userType === UserType.Admin,
      login,
      register,
      logout,
      refreshUser,
    }),
    [user, isLoading, login, register, logout, refreshUser]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
