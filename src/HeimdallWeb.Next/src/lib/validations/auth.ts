import { z } from "zod/v4";

export const loginSchema = z.object({
  emailOrLogin: z
    .string()
    .min(3, "Informe seu email ou nome de usuário"),
  password: z
    .string()
    .min(1, "Informe sua senha"),
});

export type LoginFormData = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    username: z
      .string()
      .min(3, "Mínimo de 3 caracteres")
      .max(30, "Máximo de 30 caracteres")
      .regex(/^[a-zA-Z0-9_]+$/, "Apenas letras, números e _"),
    email: z
      .email("Email inválido"),
    password: z
      .string()
      .min(8, "Mínimo de 8 caracteres")
      .regex(/[A-Z]/, "Deve conter uma letra maiúscula")
      .regex(/[a-z]/, "Deve conter uma letra minúscula")
      .regex(/[0-9]/, "Deve conter um número")
      .regex(/[^a-zA-Z0-9]/, "Deve conter um caractere especial"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "As senhas não coincidem",
    path: ["confirmPassword"],
  });

export type RegisterFormData = z.infer<typeof registerSchema>;
