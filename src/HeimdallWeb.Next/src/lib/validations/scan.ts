import { z } from "zod/v4";

export const scanSchema = z.object({
  target: z
    .url("Insira uma URL válida (ex: https://exemplo.com)")
    .startsWith("http", "A URL deve começar com http:// ou https://"),
});

export type ScanFormData = z.infer<typeof scanSchema>;
