"use client";

import { forwardRef, useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type PasswordInputProps = Omit<React.ComponentProps<typeof Input>, "type">;

export const PasswordInput = forwardRef<HTMLInputElement, PasswordInputProps>(
  ({ className, ...props }, ref) => {
    const [visible, setVisible] = useState(false);

    return (
      <div className="relative">
        <Input
          ref={ref}
          type={visible ? "text" : "password"}
          className={cn("pr-10", className)}
          {...props}
        />
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="absolute right-0 top-0 h-full w-10 px-2 hover:bg-transparent"
          onClick={() => setVisible((v) => !v)}
          tabIndex={-1}
          aria-label={visible ? "Ocultar senha" : "Mostrar senha"}
        >
          {visible ? (
            <EyeOff className="h-4 w-4 text-muted-foreground" />
          ) : (
            <Eye className="h-4 w-4 text-muted-foreground" />
          )}
        </Button>
      </div>
    );
  }
);

PasswordInput.displayName = "PasswordInput";
