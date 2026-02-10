export const routes = {
  home: "/",
  login: "/login",
  register: "/register",
  history: "/history",
  historyDetail: (id: string) => `/history/${id}`,
  dashboardUser: "/dashboard/user",
  dashboardAdmin: "/dashboard/admin",
  profile: "/profile",
  adminUsers: "/admin/users",
} as const;

export const publicRoutes = [routes.login, routes.register] as const;
