export const routes = {
  home: "/",
  scan: "/scan",
  login: "/login",
  register: "/register",
  history: "/history",
  historyDetail: (id: string) => `/history/${id}`,
  dashboardUser: "/dashboard/user",
  dashboardAdmin: "/dashboard/admin",
  profile: "/profile",
  adminUsers: "/admin/users",
  monitor: "/monitor",
  support: "/support",
} as const;

export const publicRoutes = [routes.login, routes.register] as const;
