"use client";

import { useState, useCallback } from "react";
import { Copy, Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";

interface JsonViewerProps {
  json?: string;
}

function colorizeJson(jsonStr: string): string {
  return jsonStr
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(
      /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
      (match) => {
        let cls = "json-number";
        if (/^"/.test(match)) {
          cls = /:$/.test(match) ? "json-key" : "json-string";
        } else if (/true|false/.test(match)) {
          cls = "json-boolean";
        } else if (/null/.test(match)) {
          cls = "json-null";
        }
        return `<span class="${cls}">${match}</span>`;
      }
    );
}

export function JsonViewer({ json }: JsonViewerProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = useCallback(() => {
    if (!json) return;
    navigator.clipboard.writeText(json);
    setCopied(true);
    toast.success("JSON copiado para a área de transferência");
    setTimeout(() => setCopied(false), 2000);
  }, [json]);

  if (!json) {
    return (
      <div className="py-8 text-center text-sm text-muted-foreground">
        Nenhum dado JSON disponível.
      </div>
    );
  }

  let formatted = json;
  try {
    formatted = JSON.stringify(JSON.parse(json), null, 2);
  } catch {
    // use raw json if parse fails
  }

  const highlighted = colorizeJson(formatted);

  return (
    <>
      <style>{`
        .json-key    { color: #7dd3fc; }
        .json-string { color: #86efac; }
        .json-number { color: #fda4af; }
        .json-boolean{ color: #f9a8d4; }
        .json-null   { color: #94a3b8; }
        :root:not(.dark) .json-key    { color: #0369a1; }
        :root:not(.dark) .json-string { color: #15803d; }
        :root:not(.dark) .json-number { color: #be123c; }
        :root:not(.dark) .json-boolean{ color: #9d174d; }
        :root:not(.dark) .json-null   { color: #64748b; }
      `}</style>
      <div className="relative">
        <Button
          variant="outline"
          size="sm"
          onClick={handleCopy}
          className="absolute right-2 top-2 z-10"
        >
          {copied ? <Check className="mr-2 h-4 w-4" /> : <Copy className="mr-2 h-4 w-4" />}
          {copied ? "Copiado" : "Copiar"}
        </Button>
        <pre className="overflow-x-auto rounded-lg border bg-muted/30 p-4 pt-10 text-sm font-mono leading-relaxed">
          <code dangerouslySetInnerHTML={{ __html: highlighted }} />
        </pre>
      </div>
    </>
  );
}
