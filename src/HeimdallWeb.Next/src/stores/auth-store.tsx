"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { UserProfile } from "@/types/user";
import type { LoginRequest, RegisterRequest } from "@/types/user";
import { UserType } from "@/types/common";
import * as authApi from "@/lib/api/auth.api";
import * as userApi from "@/lib/api/user.api";

const UID_COOKIE = "heimdall_uid";

function getUidCookie(): string | null {
  if (typeof document === "undefined") return null;
  const match = document.cookie.match(/(?:^|;\s*)heimdall_uid=([^;]+)/);
  return match ? match[1] : null;
}

function setUidCookie(userId: string) {
  document.cookie = `${UID_COOKIE}=${userId}; path=/; SameSite=Strict; max-age=86400`;
}

function clearUidCookie() {
  document.cookie = `${UID_COOKIE}=; path=/; SameSite=Strict; max-age=0`;
}

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
      const userId = stored
        ? (JSON.parse(stored) as { userId: string }).userId
        : getUidCookie();

      if (!userId) {
        setUser(null);
        return;
      }

      const profile = await userApi.getUserProfile(userId);
      setUser(profile);
      localStorage.setItem("heimdall_user", JSON.stringify(profile));
    } catch {
      setUser(null);
      localStorage.removeItem("heimdall_user");
      clearUidCookie();
    }
  }, []);

  useEffect(() => {
    async function validateSession() {
      // 1. Try to get userId: localStorage first, then uid cookie
      const stored = localStorage.getItem("heimdall_user");
      let userId: string | null = null;

      if (stored) {
        try {
          const parsed = JSON.parse(stored) as UserProfile;
          userId = parsed.userId;
          // Optimistic render while validating
          setUser(parsed);
        } catch {
          localStorage.removeItem("heimdall_user");
        }
      }

      if (!userId) {
        userId = getUidCookie();
      }

      // 2. No userId anywhere — no session to validate
      if (!userId) {
        setUser(null);
        setIsLoading(false);
        return;
      }

      // 3. Validate session via API (401 = cookie expired, interceptor handles redirect)
      try {
        const profile = await userApi.getUserProfile(userId);
        setUser(profile);
        localStorage.setItem("heimdall_user", JSON.stringify(profile));
        setUidCookie(profile.userId);
      } catch {
        setUser(null);
        localStorage.removeItem("heimdall_user");
        clearUidCookie();
      } finally {
        setIsLoading(false);
      }
    }

    validateSession();
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
    setUidCookie(profile.userId);
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
    setUidCookie(profile.userId);
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      setUser(null);
      localStorage.removeItem("heimdall_user");
      clearUidCookie();
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
