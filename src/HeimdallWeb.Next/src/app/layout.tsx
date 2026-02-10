import type { Metadata } from "next";
import { Inter, JetBrains_Mono } from "next/font/google";
import { ThemeProvider } from "@/providers/theme-provider";
import { QueryProvider } from "@/providers/query-provider";
import { AuthProvider } from "@/stores/auth-store";
import { TooltipProvider } from "@/components/ui/tooltip";
import { Toaster } from "sonner";
import "./globals.css";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

const jetbrainsMono = JetBrains_Mono({
  variable: "--font-jetbrains-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "HeimdallWeb",
    template: "%s | HeimdallWeb",
  },
  description:
    "Web security scanning platform — analyze vulnerabilities, SSL, ports, headers and more",
  keywords: [
    "security",
    "scanner",
    "vulnerability",
    "ssl",
    "headers",
    "ports",
    "web security",
  ],
  authors: [{ name: "HeimdallWeb" }],
  robots: {
    index: false,
    follow: false,
  },
  openGraph: {
    title: "HeimdallWeb",
    description:
      "Web security scanning platform — analyze vulnerabilities, SSL, ports, headers and more",
    type: "website",
  },
  twitter: {
    card: "summary",
    title: "HeimdallWeb",
    description:
      "Web security scanning platform — analyze vulnerabilities, SSL, ports, headers and more",
  },
  icons: {
    icon: "/favicon.ico",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body className={`${inter.variable} ${jetbrainsMono.variable} font-sans antialiased`}>
        <ThemeProvider>
          <QueryProvider>
            <AuthProvider>
              <TooltipProvider>
                {children}
                <Toaster position="bottom-right" richColors />
              </TooltipProvider>
            </AuthProvider>
          </QueryProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
