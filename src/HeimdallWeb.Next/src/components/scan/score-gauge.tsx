"use client";

import { useEffect, useState } from "react";

/**
 * Grade color mapping — consistent across the application.
 */
const GRADE_COLORS: Record<string, { stroke: string; bg: string; text: string }> = {
    A: { stroke: "#22c55e", bg: "rgba(34,197,94,0.10)", text: "#22c55e" },
    B: { stroke: "#84cc16", bg: "rgba(132,204,22,0.10)", text: "#84cc16" },
    C: { stroke: "#eab308", bg: "rgba(234,179,8,0.10)", text: "#eab308" },
    D: { stroke: "#f97316", bg: "rgba(249,115,22,0.10)", text: "#f97316" },
    F: { stroke: "#ef4444", bg: "rgba(239,68,68,0.10)", text: "#ef4444" },
};

function getGradeColor(grade: string | null | undefined) {
    return GRADE_COLORS[grade ?? "F"] ?? GRADE_COLORS["F"];
}

interface ScoreGaugeProps {
    /** Score 0–100 */
    score: number | null | undefined;
    /** Grade letter (A–F) */
    grade: string | null | undefined;
    /** Size in px */
    size?: number;
    /** Stroke width in px */
    strokeWidth?: number;
    /** Whether to animate on mount */
    animate?: boolean;
    /** Show label below */
    showLabel?: boolean;
}

export function ScoreGauge({
    score,
    grade,
    size = 120,
    strokeWidth = 8,
    animate = true,
    showLabel = true,
}: ScoreGaugeProps) {
    const displayScore = score ?? 0;
    const displayGrade = grade ?? "F";
    const colors = getGradeColor(displayGrade);

    // Animation: animate from 0 to displayScore
    const [animatedScore, setAnimatedScore] = useState(animate ? 0 : displayScore);

    useEffect(() => {
        if (!animate) {
            setAnimatedScore(displayScore);
            return;
        }

        let start: number | null = null;
        const duration = 1200; // ms

        function step(timestamp: number) {
            if (!start) start = timestamp;
            const progress = Math.min((timestamp - start) / duration, 1);
            // easeOutExpo
            const eased = progress === 1 ? 1 : 1 - Math.pow(2, -10 * progress);
            setAnimatedScore(Math.round(eased * displayScore));

            if (progress < 1) {
                requestAnimationFrame(step);
            }
        }

        requestAnimationFrame(step);
    }, [displayScore, animate]);

    const radius = (size - strokeWidth) / 2;
    const circumference = 2 * Math.PI * radius;
    const progress = animatedScore / 100;
    const dashOffset = circumference * (1 - progress);
    const center = size / 2;

    return (
        <div className="flex flex-col items-center gap-2">
            <div className="relative" style={{ width: size, height: size }}>
                <svg
                    width={size}
                    height={size}
                    viewBox={`0 0 ${size} ${size}`}
                    className="drop-shadow-sm"
                    style={{ transform: "rotate(-90deg)" }}
                >
                    {/* Background circle */}
                    <circle
                        cx={center}
                        cy={center}
                        r={radius}
                        fill="none"
                        stroke="currentColor"
                        strokeWidth={strokeWidth}
                        className="text-muted/30"
                    />
                    {/* Progress arc */}
                    <circle
                        cx={center}
                        cy={center}
                        r={radius}
                        fill="none"
                        stroke={colors.stroke}
                        strokeWidth={strokeWidth}
                        strokeDasharray={circumference}
                        strokeDashoffset={dashOffset}
                        strokeLinecap="round"
                        style={{
                            transition: animate ? undefined : "stroke-dashoffset 0.5s ease-out",
                            filter: `drop-shadow(0 0 6px ${colors.stroke}40)`,
                        }}
                    />
                </svg>

                {/* Center label */}
                <div
                    className="absolute inset-0 flex flex-col items-center justify-center"
                    style={{ color: colors.text }}
                >
                    <span
                        className="font-bold leading-none"
                        style={{ fontSize: size * 0.22 }}
                    >
                        {animatedScore}
                    </span>
                    <span
                        className="font-semibold leading-none mt-0.5"
                        style={{ fontSize: size * 0.16, opacity: 0.85 }}
                    >
                        {displayGrade}
                    </span>
                </div>
            </div>

            {showLabel && (
                <span className="text-xs text-muted-foreground font-medium">
                    Security Score
                </span>
            )}
        </div>
    );
}

/**
 * Inline grade badge — for use in tables and compact layouts.
 */
interface GradeBadgeProps {
    grade: string | null | undefined;
    score?: number | null;
    className?: string;
}

export function GradeBadge({ grade, score, className = "" }: GradeBadgeProps) {
    const displayGrade = grade ?? "–";
    const colors = getGradeColor(grade);

    return (
        <span
            className={`inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-xs font-bold ${className}`}
            style={{
                backgroundColor: colors.bg,
                color: colors.text,
                border: `1px solid ${colors.stroke}30`,
            }}
        >
            {displayGrade}
            {score != null && (
                <span className="font-medium opacity-80">{score}</span>
            )}
        </span>
    );
}
