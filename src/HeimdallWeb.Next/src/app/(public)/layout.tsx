import Link from "next/link";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { cookies } from "next/headers";
import { LayoutDashboard } from "lucide-react";
import { routes } from "@/lib/constants/routes";

export default async function PublicLayout({ children }: { children: React.ReactNode }) {
  const cookieStore = await cookies();
  const token = cookieStore.get("heimdall_uid")?.value;
  const isAuthenticated = !!token;

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-50 border-b border-border bg-background/80 backdrop-blur-sm">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            <Link href="/" className="flex items-center gap-2">
              <span className="text-xl font-bold tracking-tight">Heimdall</span>
            </Link>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              {isAuthenticated ? (
                <Link href={routes.scan}>
                  <Button size="sm" className="gap-2">
                    <LayoutDashboard className="h-4 w-4" />
                    Meu Painel
                  </Button>
                </Link>
              ) : (
                <>
                  <Link href="/login">
                    <Button variant="ghost" size="sm">Entrar</Button>
                  </Link>
                  <Link href="/register">
                    <Button size="sm">Começar grátis</Button>
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      </header>
      {children}
    </div>
  );
}
