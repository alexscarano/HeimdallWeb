"use client";

import {
    Shield,
    Globe,
    Lock,
    Clock,
    Server,
    FileText,
    AlertTriangle,
    CheckCircle,
    XCircle,
    Info,
    Network,
    Search,
    Zap,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";

// Scanner key → human-readable config
const SCANNER_CONFIG: Record<
    string,
    { title: string; icon: React.ElementType; color: string }
> = {
    tls_capability: { title: "TLS / Protocolo", icon: Lock, color: "#22c55e" },
    resultsSslScanner: { title: "Certificado SSL", icon: Shield, color: "#3b82f6" },
    securityHeaders: { title: "Headers de Segurança", icon: Shield, color: "#8b5cf6" },
    csp_analysis: { title: "Content Security Policy", icon: FileText, color: "#6366f1" },
    ip_resolution: { title: "Resolução IP & CDN", icon: Globe, color: "#06b6d4" },
    domain_age: { title: "Idade do Domínio", icon: Clock, color: "#f59e0b" },
    response_behavior: { title: "Comportamento de Resposta", icon: Zap, color: "#ec4899" },
    subdomains: { title: "Subdomínios Descobertos", icon: Network, color: "#14b8a6" },
    security_txt: { title: "security.txt (RFC 9116)", icon: FileText, color: "#64748b" },
    robots: { title: "Robots.txt & Sitemap", icon: Search, color: "#84cc16" },
    resultsPortScanner: { title: "Portas Abertas", icon: Server, color: "#ef4444" },
    sensitivePathScanner: { title: "Caminhos Sensíveis", icon: AlertTriangle, color: "#f97316" },
    resultsHttpRedirectScanner: { title: "Redirects HTTP", icon: Globe, color: "#0ea5e9" },
};

// Category groups configuration
const SCANNER_CATEGORIES = [
    {
        key: "security_http",
        label: "Segurança HTTP",
        icon: Shield,
        color: "#8b5cf6",
        scanners: ["securityHeaders", "csp_analysis", "security_txt"],
    },
    {
        key: "tls",
        label: "TLS & Certificado",
        icon: Lock,
        color: "#22c55e",
        scanners: ["tls_capability", "resultsSslScanner"],
    },
    {
        key: "infrastructure",
        label: "Infraestrutura",
        icon: Globe,
        color: "#06b6d4",
        scanners: ["ip_resolution", "subdomains", "domain_age", "resultsHttpRedirectScanner"],
    },
    {
        key: "scanning",
        label: "Escaneamento",
        icon: Search,
        color: "#f97316",
        scanners: ["resultsPortScanner", "sensitivePathScanner", "robots"],
    },
    {
        key: "performance",
        label: "Performance",
        icon: Zap,
        color: "#ec4899",
        scanners: ["response_behavior"],
    },
];

interface ScannerResultCardsProps {
    rawJson: string;
}

export function ScannerResultCards({ rawJson }: ScannerResultCardsProps) {
    let data: Record<string, unknown>;
    try {
        data = JSON.parse(rawJson);
    } catch {
        return (
            <div className="py-8 text-center text-sm text-muted-foreground">
                Não foi possível interpretar os resultados do scan.
            </div>
        );
    }

    // Filter out non-scanner keys
    const metaKeys = new Set(["target", "scanTime", "ips", "cookies", "headers", "statusCodeHttpRequest"]);
    const scannerKeys = Object.keys(data).filter((k) => !metaKeys.has(k));

    if (scannerKeys.length === 0) {
        return null;
    }

    return (
        <div className="space-y-6">
            {SCANNER_CATEGORIES.map((category, idx) => {
                const categoryKeys = category.scanners.filter((k) => scannerKeys.includes(k));
                if (categoryKeys.length === 0) return null;

                const CategoryIcon = category.icon;

                return (
                    <div key={category.key}>
                        {idx > 0 && <Separator className="mb-6" />}
                        <div className="space-y-3">
                            {/* Category header */}
                            <div className="flex items-center gap-2.5 mb-4">
                                <div
                                    className="icon-box flex h-8 w-8 items-center justify-center rounded-lg"
                                    style={{ backgroundColor: `${category.color}15` }}
                                >
                                    <CategoryIcon className="h-[18px] w-[18px]" style={{ color: category.color }} />
                                </div>
                                <h3 className="text-base font-semibold">{category.label}</h3>
                            </div>
                            {/* Cards grid */}
                            <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 items-start">
                                {categoryKeys.map((key) => {
                                    const config = SCANNER_CONFIG[key] ?? {
                                        title: key.replace(/_/g, " ").replace(/^./, (c) => c.toUpperCase()),
                                        icon: Info,
                                        color: "#94a3b8",
                                    };
                                    const value = data[key];
                                    const Icon = config.icon;

                                    return (
                                        <Card
                                            key={key}
                                            className="h-full overflow-hidden rounded-2xl border border-border/60 shadow-sm transition-all hover:shadow-md hover:border-border"
                                        >
                                            <CardHeader className="flex flex-row items-center gap-4 pb-3 pt-5 px-5">
                                                <div
                                                    className="icon-box flex h-11 w-11 shrink-0 items-center justify-center rounded-xl"
                                                    style={{ backgroundColor: `${config.color}15` }}
                                                >
                                                    <Icon className="h-5 w-5" style={{ color: config.color }} />
                                                </div>
                                                <CardTitle className="text-base font-semibold leading-tight">
                                                    {config.title}
                                                </CardTitle>
                                            </CardHeader>
                                            <CardContent className="px-5 pb-5 pt-0">
                                                <ScannerCardBody scannerKey={key} value={value} />
                                            </CardContent>
                                        </Card>
                                    );
                                })}
                            </div>
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

// ---------------------------------------------------------------------------
// Per-scanner card body renderers
// ---------------------------------------------------------------------------

function ScannerCardBody({ scannerKey, value }: { scannerKey: string; value: unknown }) {
    if (value == null) {
        return <p className="text-xs text-muted-foreground">Sem dados disponíveis</p>;
    }

    // Handle error case
    if (typeof value === "object" && value !== null && "error" in value) {
        return (
            <p className="text-xs text-destructive">
                Erro: {String((value as Record<string, unknown>).error)}
            </p>
        );
    }

    switch (scannerKey) {
        case "tls_capability":
            return <TlsCard data={value as Record<string, unknown>} />;
        case "csp_analysis":
            return <CspCard data={value as Record<string, unknown>} />;
        case "domain_age":
            return <DomainAgeCard data={value as Record<string, unknown>} />;
        case "ip_resolution":
            return <IpResolutionCard data={value as Record<string, unknown>} />;
        case "response_behavior":
            return <ResponseBehaviorCard data={value as Record<string, unknown>} />;
        case "subdomains":
            return <SubdomainCard data={value as Record<string, unknown>} />;
        case "security_txt":
            return <SecurityTxtCard data={value as Record<string, unknown>} />;
        case "securityHeaders":
            return <SecurityHeadersCard data={value as Record<string, unknown>} />;
        case "robots":
            return <RobotsCard data={value as Record<string, unknown>} />;
        case "resultsPortScanner":
            return <PortScanCard data={value as unknown[]} />;
        case "resultsSslScanner":
            return <SslCard data={value as unknown[]} />;
        case "sensitivePathScanner":
            return <SensitivePathCard data={value as Record<string, unknown>} />;
        case "resultsHttpRedirectScanner":
            return <RedirectCard data={value as unknown[]} />;
        default:
            return <GenericCard value={value} />;
    }
}

// --- Individual card renderers ---

function TlsCard({ data }: { data: Record<string, unknown> }) {
    return (
        <div className="space-y-3">
            <Row label="TLS 1.2" value={<StatusIndicator ok={Boolean(data.tls12_supported)} />} />
            <Row label="TLS 1.3" value={<StatusIndicator ok={Boolean(data.tls13_supported)} />} />
            <Row label="Cifra" value={<span className="font-mono text-xs">{String(data.negotiated_cipher ?? "N/A")}</span>} />
            {Boolean(data.weak_cipher_detected) && (
                <RecommendationBlock text="Cifra fraca detectada" />
            )}
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function CspCard({ data }: { data: Record<string, unknown> }) {
    const issues = (data.issues as string[]) ?? [];
    return (
        <div className="space-y-3">
            <Row label="CSP Presente" value={<StatusIndicator ok={Boolean(data.csp_present)} />} />
            {issues.length > 0 && (
                <div className="mt-1 space-y-1">
                    {issues.slice(0, 3).map((issue, i) => (
                        <RecommendationBlock key={i} text={issue} />
                    ))}
                    {issues.length > 3 && (
                        <p className="text-xs text-muted-foreground">+{issues.length - 3} mais</p>
                    )}
                </div>
            )}
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function DomainAgeCard({ data }: { data: Record<string, unknown> }) {
    // Handle potential double-nesting: { domain_age: { ... } } or { domain, creation_date, ... }
    const inner = (data.domain_age ?? data) as Record<string, unknown>;
    const ageDays = inner.age_days as number | undefined;

    return (
        <div className="space-y-3">
            <Row label="Domínio" value={<span className="font-mono text-xs">{String(inner.domain ?? "N/A")}</span>} />
            <Row label="Criado em" value={<span className="text-xs">{String(inner.creation_date ?? "N/A")}</span>} />
            {ageDays != null && (
                <Row
                    label="Idade"
                    value={<span className="text-xs">{ageDays > 365 ? `${Math.floor(ageDays / 365)} anos` : `${ageDays} dias`}</span>}
                />
            )}
            <AlertsList alerts={inner.alerts as string[] | undefined} />
        </div>
    );
}

function IpResolutionCard({ data }: { data: Record<string, unknown> }) {
    const ips = (data.ipv4_addresses ?? data.ipv6_addresses ?? []) as string[];
    return (
        <div className="space-y-3">
            <Row label="Host" value={<span className="font-mono text-xs">{String(data.hostname ?? "N/A")}</span>} />
            {ips.length > 0 && <Row label="IPs" value={<span className="font-mono text-xs">{ips.join(", ")}</span>} />}
            <Row
                label="CDN"
                value={
                    data.behind_cdn
                        ? <span className="flex items-center gap-1"><CheckCircle className="h-3.5 w-3.5 text-emerald-500" />{String(data.cdn_provider)}</span>
                        : <span className="text-muted-foreground">Não identificado</span>
                }
            />
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function ResponseBehaviorCard({ data }: { data: Record<string, unknown> }) {
    const ttfb = data.ttfb_ms as number | undefined;
    return (
        <div className="space-y-3">
            {ttfb != null && (
                <Row
                    label="TTFB"
                    value={
                        <span className="flex items-center gap-1.5">
                            <span className="font-mono">{ttfb}ms</span>
                            {ttfb > 800 ? (
                                <Badge className="bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 border-0 text-[10px] px-1.5 py-0 h-4">Lento</Badge>
                            ) : ttfb > 400 ? (
                                <Badge className="bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 border-0 text-[10px] px-1.5 py-0 h-4">OK</Badge>
                            ) : (
                                <Badge className="bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400 border-0 text-[10px] px-1.5 py-0 h-4">Rápido</Badge>
                            )}
                        </span>
                    }
                />
            )}
            <Row
                label="Resposta 404 correta"
                value={
                    data.returns_proper_404
                        ? <StatusIndicator ok={true} label="Sim" />
                        : <StatusIndicator ok={false} label="Não (soft 404)" />
                }
            />
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function SubdomainCard({ data }: { data: Record<string, unknown> }) {
    const discovered = (data.discovered ?? []) as Array<{ subdomain: string }>;
    return (
        <div className="space-y-3">
            <Row label="Base" value={<span className="font-mono text-xs">{String(data.base_domain ?? "N/A")}</span>} />
            <Row label="Encontrados" value={<span>{discovered.length} subdomínios</span>} />
            {discovered.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mt-1">
                    {discovered.slice(0, 5).map((d, i) => (
                        <Badge key={i} variant="secondary" className="text-[11px] px-2 py-0.5 font-mono">
                            {d.subdomain}
                        </Badge>
                    ))}
                    {discovered.length > 5 && (
                        <Badge variant="outline" className="text-[11px] px-2 py-0.5">+{discovered.length - 5}</Badge>
                    )}
                </div>
            )}
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function SecurityTxtCard({ data }: { data: Record<string, unknown> }) {
    return (
        <div className="space-y-3">
            <Row label="Presente" value={<StatusIndicator ok={Boolean(data.present)} />} />
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function SecurityHeadersCard({ data }: { data: Record<string, unknown> }) {
    const missing = (data.missing ?? []) as string[];
    const present = (data.present ?? []) as string[];
    const weak = (data.weak ?? []) as string[];

    const presentCount = Array.isArray(present) ? present.length : (typeof data.present === "number" ? data.present : 0);
    const missingCount = Array.isArray(missing) ? missing.length : (typeof data.missing === "number" ? data.missing : 0);

    return (
        <div className="space-y-3">
            <Row label="Presentes" value={<span>{presentCount} headers</span>} />
            {missingCount > 0 && <Row label="Ausentes" value={<span className="text-red-600 dark:text-red-400 font-medium">{missingCount} headers</span>} />}
            {weak.length > 0 && <Row label="Fracos" value={<span className="text-amber-500 font-medium">{weak.length} headers</span>} />}
            {Array.isArray(missing) && missing.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mt-1">
                    {missing.slice(0, 3).map((h, i) => (
                        <RecommendationBlock key={i} text={`Sem ${h}`} />
                    ))}
                    {missing.length > 3 && (
                        <span className="text-xs text-muted-foreground">+{missing.length - 3} mais</span>
                    )}
                </div>
            )}
        </div>
    );
}

function RobotsCard({ data }: { data: Record<string, unknown> }) {
    return (
        <div className="space-y-3">
            <Row label="robots.txt" value={<StatusIndicator ok={Boolean(data.robots_found)} />} />
            <Row label="Sitemap XML" value={<StatusIndicator ok={Boolean(data.sitemap_found)} />} />
            <AlertsList alerts={data.alerts as string[] | undefined} />
        </div>
    );
}

function PortScanCard({ data }: { data: unknown[] }) {
    if (!Array.isArray(data)) return <p className="text-xs text-muted-foreground">Sem dados</p>;
    const openPorts = data.filter((p: unknown) => {
        const port = p as Record<string, unknown>;
        return port.open === true || port.status === "Open";
    });
    return (
        <div className="space-y-3">
            <Row label="Portas verificadas" value={<span>{data.length}</span>} />
            <Row label="Abertas" value={<span className={openPorts.length > 0 ? "text-red-600 dark:text-red-400 font-medium" : ""}>{openPorts.length}</span>} />
            {openPorts.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mt-1">
                    {openPorts.slice(0, 6).map((p: unknown, i) => {
                        const port = p as Record<string, unknown>;
                        return (
                            <Badge key={i} className="bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 border-0 text-[11px] px-2 py-0.5 font-mono">
                                {String(port.port ?? port.portNumber)}
                            </Badge>
                        );
                    })}
                </div>
            )}
        </div>
    );
}

/**
 * SSL Card — fixed field names to match backend output:
 *   subject, issuer, chainValid (not isValid), expired, daysToExpire (not daysUntilExpiry)
 */
function SslCard({ data }: { data: unknown[] }) {
    if (!Array.isArray(data) || data.length === 0) {
        return <p className="text-xs text-muted-foreground">Sem dados</p>;
    }
    const cert = data[0] as Record<string, unknown>;

    const isExpired = Boolean(cert.expired);
    const chainValid = Boolean(cert.chainValid);
    const isValid = !isExpired && chainValid;
    const daysToExpire = cert.daysToExpire as number | undefined;

    return (
        <div className="space-y-3">
            <Row label="Sujeito" value={<span className="font-mono text-xs break-all">{String(cert.subject ?? cert.commonName ?? "N/A")}</span>} />
            <Row label="Emissor" value={<span className="text-xs break-all">{String(cert.issuer ?? "N/A")}</span>} />
            <Row label="Válido" value={<StatusIndicator ok={isValid} label={isValid ? "Sim" : isExpired ? "Expirado" : "Cadeia inválida"} />} />
            {daysToExpire != null && (
                <Row
                    label="Expira em"
                    value={
                        <span className="flex items-center gap-1.5">
                            {daysToExpire} dias
                            {daysToExpire < 30 && (
                                <Badge className="bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 border-0 text-[10px] px-1.5 py-0 h-4">Atenção</Badge>
                            )}
                        </span>
                    }
                />
            )}
        </div>
    );
}

/**
 * Sensitive Paths Card — fixed structure:
 *   Backend returns { totalChecked, findings (count), results (array) }
 *   NOT { findings: [...] }
 */
function SensitivePathCard({ data }: { data: Record<string, unknown> }) {
    const inner = (data.sensitivePathScanner ?? data) as Record<string, unknown>;

    const totalChecked = inner.totalChecked as number | undefined;
    const findingsCount = inner.findings as number | undefined;
    const results = (inner.results ?? []) as Array<Record<string, unknown>>;

    const status = inner.status as string | undefined;
    const isFallback = status === "suspected-fallback";

    return (
        <div className="space-y-3">
            <Row label="Verificados" value={<span>{totalChecked ?? "N/A"}</span>} />
            <Row
                label="Encontrados"
                value={
                    <span className={findingsCount && findingsCount > 0 ? "text-red-600 dark:text-red-400 font-medium" : ""}>
                        {findingsCount ?? 0}
                    </span>
                }
            />
            {isFallback && (
                <div className="mt-1 rounded-md border border-amber-200 bg-amber-50 dark:border-amber-800/30 dark:bg-amber-950/20 px-2 py-1">
                    <p className="text-xs text-amber-700 dark:text-amber-400">
                        Fallback global detectado (SPA/catch-all). Caminhos individuais não confirmados.
                    </p>
                </div>
            )}
            {!isFallback && results.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mt-1">
                    {results.slice(0, 4).map((f, i) => (
                        <Badge key={i} className="bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400 border-0 text-[11px] px-2 py-0.5 font-mono">
                            {String(f.path ?? "?")}
                        </Badge>
                    ))}
                    {results.length > 4 && (
                        <Badge variant="outline" className="text-[11px] px-2 py-0.5">+{results.length - 4}</Badge>
                    )}
                </div>
            )}
        </div>
    );
}

function RedirectCard({ data }: { data: unknown[] }) {
    if (!Array.isArray(data) || data.length === 0) {
        return <p className="text-xs text-muted-foreground">Sem dados</p>;
    }
    const item = data[0] as Record<string, unknown>;
    return (
        <div className="space-y-3">
            <Row label="Redirecionamento HTTPS" value={<StatusIndicator ok={Boolean(item.redirectsToHttps)} />} />
            <Row label="Status HTTP" value={<span className="font-mono">{String(item.statusCode ?? "N/A")}</span>} />
        </div>
    );
}

function GenericCard({ value }: { value: unknown }) {
    if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
        return <p className="text-xs">{String(value)}</p>;
    }
    const json = JSON.stringify(value, null, 2);
    return (
        <pre className="max-h-32 overflow-auto rounded-md bg-muted/50 p-2.5 text-xs leading-relaxed text-muted-foreground font-mono">
            {json.length > 500 ? json.slice(0, 500) + "…" : json}
        </pre>
    );
}

// --- Shared UI components ---

function StatusIndicator({ ok, label }: { ok: boolean; label?: string }) {
    return (
        <span className="inline-flex items-center gap-2">
            {ok ? (
                <CheckCircle className="h-4 w-4 text-emerald-500" />
            ) : (
                <XCircle className="h-4 w-4 text-red-500" />
            )}
            <span className={`text-sm font-medium ${ok ? "text-emerald-600 dark:text-emerald-400" : "text-red-600 dark:text-red-400"}`}>
                {label ?? (ok ? "Sim" : "Não")}
            </span>
        </span>
    );
}

function Row({ label, value }: { label: string; value: React.ReactNode }) {
    return (
        <div className="flex items-start justify-between gap-4">
            <span className="shrink-0 text-sm text-muted-foreground">{label}</span>
            <span className="text-right text-sm font-medium">{value}</span>
        </div>
    );
}

type RecommendationVariant = "critical" | "warning" | "info";

function getRecommendationVariant(text: string): RecommendationVariant {
    const lower = text.toLowerCase();
    if (
        lower.includes("cipher") ||
        lower.includes("cifra") ||
        lower.includes("tls 1.0") ||
        lower.includes("tls 1.1") ||
        lower.includes("fraco") ||
        lower.includes("fraca") ||
        lower.includes("invalid") ||
        lower.includes("inválido") ||
        lower.includes("expirado") ||
        lower.includes("expired") ||
        lower.includes("crítico")
    ) {
        return "critical";
    }
    if (
        lower.includes("ausente") ||
        lower.includes("missing") ||
        lower.includes("não encontrado") ||
        lower.includes("warn") ||
        lower.includes("aviso") ||
        lower.includes("atenção") ||
        lower.includes("risco") ||
        lower.includes("soft 404")
    ) {
        return "warning";
    }
    return "info";
}

function RecommendationBlock({ text }: { text: string }) {
    const variant = getRecommendationVariant(text);

    const styles: Record<RecommendationVariant, { container: string; icon: string; text: string }> = {
        critical: {
            container: "rounded-lg border border-red-200 bg-red-50 dark:border-red-800/30 dark:bg-red-950/20 px-3 py-2",
            icon: "text-red-500",
            text: "text-red-700 dark:text-red-400",
        },
        warning: {
            container: "rounded-lg border border-amber-200 bg-amber-50 dark:border-amber-800/30 dark:bg-amber-950/20 px-3 py-2",
            icon: "text-amber-500",
            text: "text-amber-700 dark:text-amber-400",
        },
        info: {
            container: "rounded-lg border border-blue-200 bg-blue-50 dark:border-blue-800/30 dark:bg-blue-950/20 px-3 py-2",
            icon: "text-blue-500",
            text: "text-blue-700 dark:text-blue-400",
        },
    };

    const s = styles[variant];

    return (
        <div className={`flex items-start gap-2 ${s.container}`}>
            {variant === "info" ? (
                <Info className={`mt-0.5 h-3.5 w-3.5 shrink-0 ${s.icon}`} />
            ) : (
                <AlertTriangle className={`mt-0.5 h-3.5 w-3.5 shrink-0 ${s.icon}`} />
            )}
            <span className={`text-xs leading-relaxed ${s.text}`}>{text}</span>
        </div>
    );
}

function AlertsList({ alerts }: { alerts: string[] | undefined }) {
    if (!alerts || alerts.length === 0) return null;
    return (
        <div className="mt-1.5 space-y-1">
            {alerts.slice(0, 3).map((alert, i) => (
                <RecommendationBlock key={i} text={alert} />
            ))}
            {alerts.length > 3 && (
                <p className="text-xs text-muted-foreground">+{alerts.length - 3} alertas</p>
            )}
        </div>
    );
}
