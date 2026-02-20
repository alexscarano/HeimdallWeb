import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const publicPaths = [
  "/login",
  "/register",
  "/forgot-password",
  "/reset-password",
  "/support",
];

// The root "/" is the public landing page
const landingPath = "/";

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const isPublicPath = publicPaths.some((path) => pathname.startsWith(path));
  const isLandingPath = pathname === landingPath;
  const token = request.cookies.get("authHeimdallCookie")?.value;

  // Landing page: unauthenticated users see it; authenticated redirect to /scan
  if (isLandingPath) {
    if (token) {
      return NextResponse.redirect(new URL("/scan", request.url));
    }
    return NextResponse.next();
  }

  if (!isPublicPath && !token) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("redirect", pathname);
    return NextResponse.redirect(loginUrl);
  }

  if (isPublicPath && token) {
    // Allow forced login (e.g., session recovery when frontend state is lost)
    const isForced = request.nextUrl.searchParams.get("force") === "1";
    if (!isForced) {
      return NextResponse.redirect(new URL("/scan", request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)",
  ],
};
