"use client";

import { useState, useCallback, useEffect } from "react";
import { createPortal } from "react-dom";
import {
    Shield,
    Lock,
    Globe,
    Search,
    Zap,
    CheckCircle2,
    X,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

// Scanner definition with backend keys
export interface ScannerItem {
    key: string;
    label: string;
    description: string;
}

export interface ScannerCategory {
    key: string;
    label: string;
    icon: React.ElementType;
    color: string;
    scanners: ScannerItem[];
}

export const SCANNER_CATEGORIES: ScannerCategory[] = [
    {
        key: "security_http",
        label: "Segurança HTTP",
        icon: Shield,
        color: "#8b5cf6",
        scanners: [
            { key: "Headers", label: "Headers de Segurança", description: "HSTS, CSP, X-Frame-Options" },
            { key: "CSP", label: "Análise CSP", description: "Content-Security-Policy detalhado" },
            { key: "SecurityTxt", label: "Security.txt", description: "RFC 9116 compliance" },
        ],
    },
    {
        key: "tls",
        label: "TLS & Certificado",
        icon: Lock,
        color: "#22c55e",
        scanners: [
            { key: "TLS", label: "Capacidade TLS", description: "TLS 1.2/1.3, ciphers" },
            { key: "SSL", label: "Certificado SSL", description: "Validade, cadeia, expiração" },
        ],
    },
    {
        key: "infrastructure",
        label: "Infraestrutura",
        icon: Globe,
        color: "#06b6d4",
        scanners: [
            { key: "IpChange", label: "Resolução IP", description: "IPv4/IPv6, CDN" },
            { key: "Subdomain", label: "Subdomínios", description: "Descoberta automática" },
            { key: "DomainAge", label: "Idade do Domínio", description: "WHOIS, data de criação" },
            { key: "Redirect", label: "Redirect HTTP", description: "Redirecionamento HTTPS" },
        ],
    },
    {
        key: "scanning",
        label: "Escaneamento",
        icon: Search,
        color: "#f97316",
        scanners: [
            { key: "Port", label: "Scanner de Portas", description: "Portas abertas e serviços" },
            { key: "Sensitive", label: "Caminhos Sensíveis", description: "Arquivos e diretórios expostos" },
            { key: "Robots", label: "Robots.txt", description: "robots.txt e sitemap" },
        ],
    },
    {
        key: "performance",
        label: "Performance",
        icon: Zap,
        color: "#ec4899",
        scanners: [
            { key: "ResponseBehavior", label: "Comportamento de Resposta", description: "TTFB, erros 404" },
        ],
    },
];

export const ALL_SCANNER_KEYS = SCANNER_CATEGORIES.flatMap((c) => c.scanners.map((s) => s.key));

interface CustomScanModalProps {
    isOpen: boolean;
    onClose: () => void;
    onApply: (selectedScanners: string[]) => void;
    initialSelection?: string[];
}

export function CustomScanModal({
    isOpen,
    onClose,
    onApply,
    initialSelection,
}: CustomScanModalProps) {
    const [selected, setSelected] = useState<Set<string>>(
        () => new Set(initialSelection ?? ALL_SCANNER_KEYS)
    );

    const toggle = useCallback((key: string) => {
        setSelected((prev) => {
            const next = new Set(prev);
            if (next.has(key)) {
                next.delete(key);
            } else {
                next.add(key);
            }
            return next;
        });
    }, []);

    const toggleCategory = useCallback((category: ScannerCategory) => {
        setSelected((prev) => {
            const next = new Set(prev);
            const allSelected = category.scanners.every((s) => next.has(s.key));
            for (const s of category.scanners) {
                if (allSelected) {
                    next.delete(s.key);
                } else {
                    next.add(s.key);
                }
            }
            return next;
        });
    }, []);

    const selectAll = useCallback(() => {
        setSelected(new Set(ALL_SCANNER_KEYS));
    }, []);

    const clearAll = useCallback(() => {
        setSelected(new Set());
    }, []);

    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    if (!isOpen || !mounted) return null;

    return createPortal(
        <div className="fixed inset-0 z-[100] flex items-center justify-center">
            {/* Backdrop */}
            <div
                className="absolute inset-0 bg-black/50 backdrop-blur-sm"
                onClick={onClose}
            />

            {/* Modal */}
            <div className="relative z-10 mx-4 w-full max-w-2xl rounded-2xl border bg-background shadow-2xl animate-in fade-in zoom-in-95 duration-200">
                {/* Header */}
                <div className="flex items-center justify-between border-b px-6 py-4">
                    <div>
                        <h2 className="text-lg font-bold">Configurar Scan Customizado</h2>
                        <p className="text-sm text-muted-foreground">
                            Selecione os scanners que deseja executar
                        </p>
                    </div>
                    <button
                        onClick={onClose}
                        className="flex h-8 w-8 items-center justify-center rounded-full hover:bg-muted transition-colors"
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>

                {/* Body */}
                <div className="max-h-[60vh] overflow-y-auto px-6 py-4 space-y-5">
                    {/* Select/Clear all */}
                    <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">
                            {selected.size} de {ALL_SCANNER_KEYS.length} scanners selecionados
                        </span>
                        <div className="flex gap-2">
                            <button
                                type="button"
                                onClick={selectAll}
                                className="text-xs font-medium text-accent-primary hover:underline"
                            >
                                Selecionar Todos
                            </button>
                            <span className="text-muted-foreground">|</span>
                            <button
                                type="button"
                                onClick={clearAll}
                                className="text-xs font-medium text-muted-foreground hover:text-foreground hover:underline"
                            >
                                Limpar
                            </button>
                        </div>
                    </div>

                    {/* Categories */}
                    {SCANNER_CATEGORIES.map((category) => {
                        const CategoryIcon = category.icon;
                        const allSelected = category.scanners.every((s) => selected.has(s.key));
                        const someSelected = category.scanners.some((s) => selected.has(s.key));

                        return (
                            <div key={category.key} className="space-y-2">
                                {/* Category header */}
                                <button
                                    type="button"
                                    onClick={() => toggleCategory(category)}
                                    className="flex w-full items-center gap-2.5 rounded-lg px-2 py-1.5 hover:bg-muted/50 transition-colors"
                                >
                                    <div
                                        className="flex h-7 w-7 items-center justify-center rounded-lg"
                                        style={{ backgroundColor: `${category.color}15` }}
                                    >
                                        <CategoryIcon
                                            className="h-4 w-4"
                                            style={{ color: category.color }}
                                        />
                                    </div>
                                    <span className="text-sm font-semibold flex-1 text-left">
                                        {category.label}
                                    </span>
                                    <div
                                        className={cn(
                                            "flex h-5 w-5 items-center justify-center rounded border-2 transition-colors",
                                            allSelected
                                                ? "border-accent-primary bg-accent-primary"
                                                : someSelected
                                                    ? "border-accent-primary/50 bg-accent-primary/20"
                                                    : "border-muted-foreground/30"
                                        )}
                                    >
                                        {(allSelected || someSelected) && (
                                            <CheckCircle2
                                                className={cn(
                                                    "h-3 w-3",
                                                    allSelected ? "text-white" : "text-accent-primary"
                                                )}
                                            />
                                        )}
                                    </div>
                                </button>

                                {/* Scanner items */}
                                <div className="ml-4 grid gap-1.5">
                                    {category.scanners.map((scanner) => {
                                        const isSelected = selected.has(scanner.key);
                                        return (
                                            <button
                                                key={scanner.key}
                                                type="button"
                                                onClick={() => toggle(scanner.key)}
                                                className={cn(
                                                    "flex items-center gap-3 rounded-xl border px-4 py-3 text-left transition-all",
                                                    isSelected
                                                        ? "border-accent-primary/30 bg-accent-primary-subtle/30"
                                                        : "border-transparent hover:border-border hover:bg-muted/30"
                                                )}
                                            >
                                                <div
                                                    className={cn(
                                                        "flex h-5 w-5 shrink-0 items-center justify-center rounded border-2 transition-colors",
                                                        isSelected
                                                            ? "border-accent-primary bg-accent-primary"
                                                            : "border-muted-foreground/30"
                                                    )}
                                                >
                                                    {isSelected && (
                                                        <CheckCircle2 className="h-3 w-3 text-white" />
                                                    )}
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <p className="text-sm font-medium">{scanner.label}</p>
                                                    <p className="text-xs text-muted-foreground">
                                                        {scanner.description}
                                                    </p>
                                                </div>
                                            </button>
                                        );
                                    })}
                                </div>
                            </div>
                        );
                    })}
                </div>

                {/* Footer */}
                <div className="flex items-center justify-end gap-3 border-t px-6 py-4">
                    <Button
                        variant="outline"
                        onClick={onClose}
                        className="rounded-xl"
                    >
                        Cancelar
                    </Button>
                    <Button
                        onClick={() => onApply(Array.from(selected))}
                        disabled={selected.size === 0}
                        className="rounded-xl bg-accent-primary text-accent-primary-foreground hover:bg-accent-primary-hover"
                    >
                        Aplicar ({selected.size} scanners)
                    </Button>
                </div>
            </div>
        </div>,
        document.body
    );
}
