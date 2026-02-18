# Como implementar com Claude Code

Você pode copiar e colar o seguinte prompt diretamente no terminal onde o Claude Code está rodando:

```text
/plan leia o arquivo implementation_plan.md (que está na raiz) e inicie a execução da "FASE 1: Core Engine Refactoring".

Por favor:
1. Crie a entidade RiskWeight
2. Refatore o ScanService para usar Task.WhenAll
3. Implemente o ScoreCalculatorService

Siga estritamente as instruções técnicas do plano.

Nota sobre Gemini (Fase 3):
Lembre-se de atualizar o prompt do GeminiService conforme descrito na seção 3.1 do plano quando chegar na Fase 3.

Nota sobre Frontend (Fase 6):
Quando chegar na Fase 6, use o agente `nexus-next-js` e skills de design/integração conforme detalhado no plano.
```
