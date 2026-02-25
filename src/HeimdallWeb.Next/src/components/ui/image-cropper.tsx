"use client";

import { useState, useCallback } from "react";
import Cropper, { type Area } from "react-easy-crop";
import { Slider } from "@/components/ui/slider";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Loader2 } from "lucide-react";

interface ImageCropperProps {
    imageSrc: string | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onComplete: (croppedBase64: string) => void;
    aspect?: number;
}

export function ImageCropper({
    imageSrc,
    open,
    onOpenChange,
    onComplete,
    aspect = 1,
}: ImageCropperProps) {
    const [crop, setCrop] = useState({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<Area | null>(null);
    const [isProcessing, setIsProcessing] = useState(false);

    const onCropComplete = useCallback((_: Area, croppedAreaPixels: Area) => {
        setCroppedAreaPixels(croppedAreaPixels);
    }, []);

    const handleSave = async () => {
        if (!imageSrc || !croppedAreaPixels) return;

        try {
            setIsProcessing(true);
            const croppedImage = await getCroppedImg(imageSrc, croppedAreaPixels);
            if (croppedImage) {
                // Remove the data:image/jpeg;base64, prefix if present
                const base64 = croppedImage.split(",")[1];
                onComplete(base64);
                onOpenChange(false);
            }
        } catch (e) {
            console.error(e);
            // You might want to show a toast error here
        } finally {
            setIsProcessing(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>Ajustar Imagem</DialogTitle>
                </DialogHeader>
                <div className="relative h-64 w-full overflow-hidden rounded-md bg-black/5">
                    {imageSrc && (
                        <Cropper
                            image={imageSrc}
                            crop={crop}
                            zoom={zoom}
                            aspect={aspect}
                            onCropChange={setCrop}
                            onCropComplete={onCropComplete}
                            onZoomChange={setZoom}
                            showGrid={false}
                            cropShape="round"
                        />
                    )}
                </div>
                <div className="py-4">
                    <div className="flex items-center gap-4">
                        <span className="text-sm font-medium">Zoom</span>
                        <Slider
                            value={[zoom]}
                            min={1}
                            max={3}
                            step={0.1}
                            onValueChange={(val: number[]) => setZoom(val[0])}
                            className="flex-1"
                        />
                    </div>
                </div>
                <DialogFooter>
                    <Button
                        variant="outline"
                        onClick={() => onOpenChange(false)}
                        disabled={isProcessing}
                    >
                        Cancelar
                    </Button>
                    <Button onClick={handleSave} disabled={isProcessing}>
                        {isProcessing && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        Salvar
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

// Helper to crop image
async function getCroppedImg(
    imageSrc: string,
    pixelCrop: Area
): Promise<string | null> {
    const image = await createImage(imageSrc);
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");

    // Set max dimensions
    const MAX_WIDTH = 1024;
    const MAX_HEIGHT = 1024;

    let width = pixelCrop.width;
    let height = pixelCrop.height;

    // Scale down if needed
    if (width > MAX_WIDTH || height > MAX_HEIGHT) {
        const ratio = width / height;
        if (width > height) {
            width = MAX_WIDTH;
            height = MAX_WIDTH / ratio;
        } else {
            height = MAX_HEIGHT;
            width = MAX_HEIGHT * ratio;
        }
    }

    canvas.width = width;
    canvas.height = height;

    if (!ctx) {
        return null;
    }

    // Use high quality smoothing
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = "high";

    ctx.drawImage(
        image,
        pixelCrop.x,
        pixelCrop.y,
        pixelCrop.width,
        pixelCrop.height,
        0,
        0,
        width,
        height
    );

    // Compress JPEG slightly to ensure small size (0.8 quality)
    return canvas.toDataURL("image/jpeg", 0.85);
}

function createImage(url: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
        const image = new Image();
        image.addEventListener("load", () => resolve(image));
        image.addEventListener("error", (error) => reject(error));
        image.setAttribute("crossOrigin", "anonymous");
        image.src = url;
    });
}
