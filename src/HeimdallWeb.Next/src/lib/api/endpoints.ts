export const endpoints = {
  auth: {
    login: "/auth/login",
    register: "/auth/register",
    logout: "/auth/logout",
  },
  users: {
    profile: (id: string) => `/users/${id}/profile`,
    statistics: (id: string) => `/users/${id}/statistics`,
    update: (id: string) => `/users/${id}`,
    updatePassword: (id: string) => `/users/${id}/password`,
    updateProfileImage: (id: string) => `/users/${id}/profile-image`,
    delete: (id: string) => `/users/${id}`,
  },
  scans: {
    execute: "/scans",
    list: "/scans",
  },
  scanHistories: {
    getById: (id: string) => `/scan-histories/${id}`,
    findings: (id: string) => `/scan-histories/${id}/findings`,
    technologies: (id: string) => `/scan-histories/${id}/technologies`,
    aiSummary: (id: string) => `/scan-histories/${id}/ai-summary`,
    export: (id: string) => `/scan-histories/${id}/export`,
    exportAll: "/scan-histories/export",
    delete: (id: string) => `/scan-histories/${id}`,
  },
  dashboard: {
    admin: "/dashboard/admin",
    users: "/dashboard/users",
  },
  admin: {
    toggleUserStatus: (id: string) => `/admin/users/${id}/status`,
    deleteUser: (id: string) => `/admin/users/${id}`,
  },
} as const;
