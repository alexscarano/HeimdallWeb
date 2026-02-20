import { apiClient } from "./client";
import { endpoints } from "./endpoints";
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from "@/types/user";

export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>(endpoints.auth.login, data);
  return response.data;
}

export async function register(data: RegisterRequest): Promise<RegisterResponse> {
  const response = await apiClient.post<RegisterResponse>(endpoints.auth.register, data);
  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post(endpoints.auth.logout);
}

export async function forgotPassword(email: string): Promise<void> {
  await apiClient.post("/auth/forgot-password", { email });
}

export async function resetPassword(token: string, newPassword: string): Promise<void> {
  await apiClient.post("/auth/reset-password", { token, newPassword });
}

export async function loginWithGoogle(idToken: string): Promise<{ token: string }> {
  const { data } = await apiClient.post<{ token: string }>("/auth/google", { idToken });
  return data;
}
