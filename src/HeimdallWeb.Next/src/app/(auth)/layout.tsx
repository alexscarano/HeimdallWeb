import { Shield } from "lucide-react";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm space-y-6">
        <div className="flex flex-col items-center gap-2">
          <Shield className="h-10 w-10 text-foreground" />
          <h1 className="text-2xl font-semibold tracking-tight">Heimdall</h1>
          <p className="text-sm text-muted-foreground">
            Security Scanner Platform
          </p>
        </div>
        {children}
      </div>
    </div>
  );
}
