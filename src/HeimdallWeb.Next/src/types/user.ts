import { UserType } from "./common";

export interface LoginRequest {
  emailOrLogin: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  username: string;
  email: string;
  userType: UserType;
  token: string;
  isActive: boolean;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
  username: string;
  email: string;
  userType: UserType;
  token: string;
  isActive: boolean;
}

export interface UserProfile {
  userId: string;
  username: string;
  email: string;
  userType: UserType;
  isActive: boolean;
  profileImage: string | null;
  createdAt: string;
}

export interface UserStatistics {
  totalScans: number;
  completedScans: number;
  incompleteScans: number;
  averageDuration: string | null;
  lastScanDate: string | null;
  totalFindings: number;
  criticalFindings: number;
  highFindings: number;
  mediumFindings: number;
  lowFindings: number;
  informationalFindings: number;
  riskTrend: RiskTrendItem[];
  categoryBreakdown: CategoryBreakdownItem[];
}

export interface RiskTrendItem {
  date: string;
  findingsCount: number;
}

export interface CategoryBreakdownItem {
  category: string;
  count: number;
}

export interface UpdateUserRequest {
  newUsername?: string;
  newEmail?: string;
}

export interface UpdateUserResponse {
  userId: string;
  username: string;
  email: string;
  userType: UserType;
  isActive: boolean;
}

export interface UpdatePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface UpdateProfileImageRequest {
  imageBase64: string;
}

export interface UpdateProfileImageResponse {
  userId: string;
  profileImagePath: string;
}

export interface DeleteUserResponse {
  message: string;
  userId: string;
}

export interface UserListItem {
  userId: string;
  username: string;
  email: string;
  userType: UserType;
  isActive: boolean;
  profileImage: string | null;
  createdAt: string;
  scanCount: number;
  findingsCount: number;
}

export interface ToggleUserStatusResponse {
  userId: string;
  username: string;
  isActive: boolean;
}

export interface DeleteUserByAdminResponse {
  success: boolean;
  deletedUserId: string;
  deletedUsername: string;
}

export interface PaginatedUsersResponse {
  users: UserListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
