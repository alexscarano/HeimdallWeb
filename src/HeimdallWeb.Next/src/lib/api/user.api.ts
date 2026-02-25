import { apiClient } from "./client";
import { endpoints } from "./endpoints";
import type {
  UserProfile,
  UserStatistics,
  UpdateUserRequest,
  UpdateUserResponse,
  UpdatePasswordRequest,
  UpdateProfileImageRequest,
  UpdateProfileImageResponse,
  DeleteUserResponse,
} from "@/types/user";

export async function getUserProfile(userId: string): Promise<UserProfile> {
  // Add timestamp to prevent caching of user profile (for image updates)
  const url = `${endpoints.users.profile(userId)}?t=${new Date().getTime()}`;
  const response = await apiClient.get<UserProfile>(url);
  return response.data;
}

export async function getUserStatistics(userId: string): Promise<UserStatistics> {
  const response = await apiClient.get<UserStatistics>(endpoints.users.statistics(userId));
  return response.data;
}

export async function updateUser(userId: string, data: UpdateUserRequest): Promise<UpdateUserResponse> {
  const response = await apiClient.put<UpdateUserResponse>(endpoints.users.update(userId), data);
  return response.data;
}

export async function updatePassword(userId: string, data: UpdatePasswordRequest): Promise<void> {
  await apiClient.patch(endpoints.users.updatePassword(userId), data);
}

export async function updateProfileImage(
  userId: string,
  data: UpdateProfileImageRequest
): Promise<UpdateProfileImageResponse> {
  const response = await apiClient.post<UpdateProfileImageResponse>(
    endpoints.users.updateProfileImage(userId),
    data
  );
  return response.data;
}

export async function deleteUser(
  userId: string,
  password: string,
  confirmDelete: boolean
): Promise<DeleteUserResponse> {
  const response = await apiClient.delete<DeleteUserResponse>(endpoints.users.delete(userId), {
    params: { password, confirmDelete },
  });
  return response.data;
}
