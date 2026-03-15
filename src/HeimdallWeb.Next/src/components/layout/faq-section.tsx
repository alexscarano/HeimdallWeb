import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import { HelpCircle } from "lucide-react";

const faqData: { question: string; answer: string }[] = [
    {
        question: "O que é o HeimdallWeb?",
        answer: "O HeimdallWeb é uma plataforma avançada de escaneamento e análise de vulnerabilidades web. Ele realiza análises passivas e ativas em portas, domínios, cabeçalhos HTTP, políticas de segurança, configurações de SSL/TLS, e integra uma inteligência artificial (Gemini) para interpretar os resultados de forma unificada.",
    },
    {
        question: "Os scans realizados pelo HeimdallWeb afetam a performance do meu site?",
        answer: "Não. Todos os módulos de scan foram desenhados para serem não-intrusivos. Nós enviamos um número muito pequeno de requisições de reconhecimento, similares a um navegador normal, sem sobrecarregar sua infraestrutura, realizar ataques de negação de serviço (DoS) ou executar payloads pesados.",
    },
    {
        question: "Como a inteligência artificial ajuda na análise de risco?",
        answer: "A IA analisa os resultados absolutos e técnicos obtidos por nossos 13 scanners subjacentes e consolida todas as informações. Ele cruza os dados para identificar o nível real de severidade (por exemplo, percebendo que a falta de um header específico é apenas informativa, enquanto um TLS desatualizado e uma porta de banco exposta configuram risco alto), além de sugerir estratégias de mitigação em linguagem clara.",
    },
    {
        question: "Quais vulnerabilidades a plataforma consegue detectar?",
        answer: "Detectamos falhas de configuração de cabeçalhos de segurança (CSP, HSTS), certificados SSL/TLS inválidos ou fracos, portas expostas indevidamente, redirecionamentos incorretos, problemas de DNS e WHOIS (domínios suspeitos), caminhos sensíveis expostos (painéis de admin, backups) e arquivos básicos faltantes como security.txt e sitemaps.",
    },
    {
        question: "O que significa o 'Score de Segurança' (A, B, C...)?",
        answer: "Após cada scan, a inteligência artificial junto do nosso calculador interno geram uma nota de 0 a 100. Pontuações A (80+) representam infraestruturas maduras e seguras. Pontuações B ou C apontam alguns riscos moderados. Pontuações D ou E significam que há vulnerabilidades de alta severidade (exposição direta) que devem ser urgentemente corrigidas.",
    },
    {
        question: "Minhas informações de scan ficam públicas ou são sigilosas?",
        answer: "Todas as suas informações e históricos de scan ficam restritos à sua conta (via tenant/ID) e protegidos por autenticação JWT. Eles não figuram em buscadores públicos e não podem ser vistos por outros usuários da plataforma HeimdallWeb.",
    },
    {
        question: "Por que meu scan retornou instantaneamente?",
        answer: "O HeimdallWeb utiliza um cache compartilhado de 30 minutos por alvo. Se qualquer usuário já escaneou o mesmo domínio nos últimos 30 minutos, os resultados armazenados são retornados imediatamente — sem executar um novo scan completo. Isso garante respostas rápidas e reduz a carga nos servidores alvo. Após 30 minutos, um novo scan completo é realizado automaticamente.",
    },
];

export function FaqSection() {
    return (
        <div className="w-full space-y-6">
            <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-emerald-600/10 dark:bg-indigo-400/10">
                    <HelpCircle className="h-5 w-5 text-emerald-600 dark:text-indigo-400" />
                </div>
                <h2 className="text-2xl font-bold tracking-tight">
                    Perguntas Frequentes (FAQ)
                </h2>
            </div>

            <Accordion type="single" collapsible className="w-full">
                {faqData.map((faq, index) => (
                    <AccordionItem key={index} value={`item-${index}`}>
                        <AccordionTrigger className="text-left font-medium">
                            {faq.question}
                        </AccordionTrigger>
                        <AccordionContent className="text-muted-foreground leading-relaxed">
                            {faq.answer}
                        </AccordionContent>
                    </AccordionItem>
                ))}
            </Accordion>
        </div>
    );
}
