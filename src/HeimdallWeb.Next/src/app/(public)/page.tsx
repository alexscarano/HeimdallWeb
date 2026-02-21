"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { ScoreGauge } from "@/components/scan/score-gauge";
import {
  Shield,
  Lock,
  Search,
  Globe,
  FileText,
  Zap,
} from "lucide-react";
import { FaqSection } from "@/components/layout/faq-section";

const features = [
  {
    icon: Shield,
    title: "SSL & TLS",
    description: "Valida a autenticidade e validade dos certificados, além de garantir a utilização de protocolos modernos como TLS 1.2/1.3, protegendo contra interceptações indesejadas na rede.",
  },
  {
    icon: Lock,
    title: "Security Headers",
    description: "Detecta preventivamente a ausência de cabeçalhos cruciais como CSP, HSTS e X-Frame-Options, evitando ataques comuns como Cross-Site Scripting (XSS) e Clickjacking em seus domínios.",
  },
  {
    icon: Search,
    title: "Port Scanner",
    description: "Varrimento minucioso para identificar portas abertas e serviços não documentados ou expostos acidentalmente, reduzindo a superfície de ataque e o risco de intrusão externa.",
  },
  {
    icon: Globe,
    title: "Domain Age",
    description: "Busca informações de registro via WHOIS para determinar a idade do domínio, ajudando a traçar reputação em verificações e auxiliando a descobrir pontos suspeitos na infraestrutura.",
  },
  {
    icon: FileText,
    title: "Robots & Paths",
    description: "Analisa arquivos base de governança (robots.txt, sitemap.xml) em busca de diretórios sensíveis ou caminhos administrativos que não deveriam estar expostos publicamente.",
  },
  {
    icon: Zap,
    title: "IA Risk Analysis",
    description: "Nosso motor com IA consolida centenas de pontos de dados de todos os scanners, interpretando o contexto e gerando um nível de risco real com instruções de mitigação fáceis de aplicar.",
  },
];

const gradeDescriptions = [
  {
    grade: "A",
    label: "Excelente",
    description: "Site bem protegido com boas práticas implementadas.",
  },
  {
    grade: "B / C",
    label: "Bom",
    description: "Algumas melhorias recomendadas para maior proteção.",
  },
  {
    grade: "D / E",
    label: "Crítico",
    description: "Vulnerabilidades severas que precisam de atenção imediata.",
  },
];

export default function LandingPage() {
  return (
    <main>
      {/* Hero */}
      <section className="relative overflow-hidden">
        {/* Subtle radial gradient background */}
        <div
          className="pointer-events-none absolute inset-0 -z-10"
          style={{
            background:
              "radial-gradient(ellipse 80% 50% at 50% -10%, rgba(5,150,105,0.12) 0%, transparent 60%)",
          }}
        />
        <div className="mx-auto max-w-7xl px-4 py-24 sm:px-6 sm:py-32 lg:px-8">
          <div className="flex flex-col items-center gap-8 text-center">
            <div className="flex flex-col items-center gap-4">
              <h1 className="max-w-3xl text-5xl font-bold tracking-tight sm:text-6xl">
                Escaneie. Analise. Proteja.
              </h1>
              <p className="mx-auto max-w-xl text-lg text-muted-foreground">
                Detecte vulnerabilidades de segurança, analise TLS, headers, portas e muito
                mais — com análise de IA integrada.
              </p>
            </div>
            <div className="flex flex-col items-center gap-3 sm:flex-row">
              <Link href="/register">
                <Button size="lg" className="min-w-48">
                  Começar gratuitamente
                </Button>
              </Link>
              <a href="#features">
                <Button variant="outline" size="lg" className="min-w-48">
                  Ver como funciona
                </Button>
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* Features */}
      <section id="features" className="border-t border-border bg-muted/30">
        <div className="mx-auto max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
          <div className="mb-12 text-center">
            <h2 className="text-3xl font-bold tracking-tight">
              Tudo que você precisa para proteger seu site
            </h2>
            <p className="mt-3 text-muted-foreground">
              13 scanners especializados em uma única plataforma.
            </p>
          </div>
          <div className="grid gap-6 md:grid-cols-3">
            {features.map((feature) => (
              <Card
                key={feature.title}
                className="bg-card border border-border rounded-xl"
              >
                <CardContent className="flex flex-col items-center text-center gap-4 p-6">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-emerald-600/10 dark:bg-indigo-400/10">
                    <feature.icon className="h-5 w-5 text-emerald-600 dark:text-indigo-400" />
                  </div>
                  <div>
                    <h3 className="font-semibold">{feature.title}</h3>
                    <p className="mt-1 text-sm text-muted-foreground">
                      {feature.description}
                    </p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Score Preview */}
      <section id="preview" className="border-t border-border">
        <div className="mx-auto max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
          <div className="flex flex-col items-center gap-12 lg:flex-row lg:items-start lg:gap-20">
            <div className="flex flex-col items-center gap-6 lg:w-64">
              <ScoreGauge score={87} grade="A" size={160} animate={false} />
              <p className="text-center text-sm text-muted-foreground">
                Score calculado em tempo real após cada scan.
              </p>
            </div>
            <div className="flex-1">
              <h2 className="text-3xl font-bold tracking-tight">
                Score de segurança em tempo real
              </h2>
              <p className="mt-3 text-muted-foreground">
                Cada scan gera um score de 0 a 100 e uma grade de A a E,
                baseado na quantidade e severidade das vulnerabilidades encontradas.
              </p>
              <ul className="mt-8 space-y-4">
                {gradeDescriptions.map((item) => (
                  <li key={item.grade} className="flex gap-4">
                    <span className="flex h-8 w-12 shrink-0 items-center justify-center rounded-md bg-emerald-600/10 text-sm font-bold text-emerald-600 dark:bg-indigo-400/10 dark:text-indigo-400">
                      {item.grade}
                    </span>
                    <div>
                      <p className="font-medium">{item.label}</p>
                      <p className="text-sm text-muted-foreground">{item.description}</p>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="border-t border-border bg-muted/50">
        <div className="mx-auto max-w-4xl px-4 py-20 sm:px-6 lg:px-8">
          <FaqSection />
        </div>
      </section>

      {/* CTA Final */}
      <section className="border-t border-border bg-card">
        <div className="mx-auto max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
          <div className="flex flex-col items-center gap-6 text-center">
            <h2 className="max-w-lg text-3xl font-bold tracking-tight">
              Comece a proteger seu site hoje mesmo
            </h2>
            <p className="max-w-md text-muted-foreground">
              Crie sua conta gratuitamente e execute seu primeiro scan em menos de um minuto.
            </p>
            <Link href="/register">
              <Button size="lg" className="min-w-48">
                Criar conta gratuita
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
