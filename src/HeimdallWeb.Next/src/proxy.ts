import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const guestOnlyPaths = [
  "/login",
  "/register",
  "/forgot-password",
  "/reset-password",
];

const mixedPaths = [
  "/support",
];

// The root "/" is the public landing page
const landingPath = "/";

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const isGuestOnlyPath = guestOnlyPaths.some((path) => pathname.startsWith(path));
  const isMixedPath = mixedPaths.some((path) => pathname.startsWith(path));
  const isLandingPath = pathname === landingPath;
  const token = request.cookies.get("authHeimdallCookie")?.value;

  // Landing page: available to everyone
  if (isLandingPath) {
    return NextResponse.next();
  }

  if (isGuestOnlyPath && token) {
    // Allow forced login (e.g., session recovery when frontend state is lost)
    const isForced = request.nextUrl.searchParams.get("force") === "1";
    if (!isForced) {
      return NextResponse.redirect(new URL("/scan", request.url));
    }
  }

  if (!isGuestOnlyPath && !isMixedPath && !token) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("redirect", pathname);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)",
  ],
};
