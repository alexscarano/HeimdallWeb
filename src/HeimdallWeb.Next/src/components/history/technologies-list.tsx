"use client";

import { Technology } from "@/lib/hooks/use-history";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";

interface TechnologiesListProps {
  technologies: Technology[];
}

export function TechnologiesList({ technologies }: TechnologiesListProps) {
  if (technologies.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-muted-foreground">
        Nenhuma tecnologia detectada.
      </div>
    );
  }

  const groupedByCategory = technologies.reduce((acc, tech) => {
    if (!acc[tech.category]) {
      acc[tech.category] = [];
    }
    acc[tech.category].push(tech);
    return acc;
  }, {} as Record<string, Technology[]>);

  return (
    <div className="space-y-6">
      {Object.entries(groupedByCategory).map(([category, items]) => (
        <div key={category}>
          <h3 className="mb-3 text-sm font-semibold uppercase tracking-wider text-muted-foreground">
            {category}
          </h3>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((tech) => (
              <Card key={tech.technologyId} className="p-4">
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-medium leading-none">{tech.name}</p>
                      {tech.version && (
                        <Badge variant="outline" className="text-xs">
                          v{tech.version}
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
                {tech.description && (
                  <p className="mt-2 text-xs text-muted-foreground">{tech.description}</p>
                )}
              </Card>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
