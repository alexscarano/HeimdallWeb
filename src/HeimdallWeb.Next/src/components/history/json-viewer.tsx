"use client";

import { useEffect, useState } from "react";
import { Copy, Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";

interface JsonViewerProps {
  json?: string;
}

export function JsonViewer({ json }: JsonViewerProps) {
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (typeof window !== "undefined" && json) {
      import("prismjs").then((Prism) => {
        // @ts-ignore
        import("prismjs/components/prism-json").then(() => {
          Prism.default.highlightAll();
        });
      });
    }
  }, [json]);

  if (!json) {
    return (
      <div className="py-8 text-center text-sm text-muted-foreground">
        Nenhum dado JSON disponível.
      </div>
    );
  }

  const handleCopy = () => {
    navigator.clipboard.writeText(json);
    setCopied(true);
    toast.success("JSON copiado para a área de transferência");
    setTimeout(() => setCopied(false), 2000);
  };

  return (
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
      <pre className="language-json overflow-x-auto rounded-lg border bg-muted/30 p-4 text-sm">
        <code className="language-json">{JSON.stringify(JSON.parse(json), null, 2)}</code>
      </pre>
    </div>
  );
}
