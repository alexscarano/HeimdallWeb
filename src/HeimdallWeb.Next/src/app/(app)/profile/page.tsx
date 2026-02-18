"use client";

import { useState, useRef } from "react";
import { User, Mail, Lock, Camera, Trash2, Shield } from "lucide-react";
import { useAuth } from "@/stores/auth-store";
import {
  useUpdateProfile,
  useUpdatePassword,
  useUpdateProfileImage,
  useDeleteAccount,
} from "@/lib/hooks/use-profile";
import { UserType } from "@/types/common";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { PasswordInput } from "@/components/ui/password-input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { ImageCropper } from "@/components/ui/image-cropper";

function getPasswordStrength(password: string): {
  level: "weak" | "medium" | "strong";
  width: string;
  color: string;
} {
  if (!password) return { level: "weak", width: "0%", color: "bg-muted" };
  let score = 0;
  if (password.length >= 8) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;

  if (score <= 2) return { level: "weak", width: "33%", color: "bg-destructive" };
  if (score <= 3) return { level: "medium", width: "66%", color: "bg-warning" };
  return { level: "strong", width: "100%", color: "bg-success" };
}

export default function ProfilePage() {
  const { user, isLoading } = useAuth();

  if (isLoading) return <ProfileSkeleton />;
  if (!user) return null;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-primary-subtle">
          <User className="h-5 w-5 text-accent-primary" />
        </div>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Meu Perfil</h1>
          <p className="text-sm text-muted-foreground">
            Gerencie suas informações pessoais
          </p>
        </div>
      </div>

      {/* Profile Header Card */}
      <ProfileHeaderCard />

      {/* Tabbed Settings */}
      <Card className="p-0 overflow-hidden">
        <Tabs defaultValue="informacoes" className="w-full">
          <div className="border-b px-4 pt-4 sm:px-6">
            <TabsList className="w-full justify-start gap-0 bg-transparent p-0 h-auto flex-wrap">
              <TabsTrigger
                value="informacoes"
                className="relative rounded-none border-b-2 border-transparent px-4 py-2.5 data-[state=active]:border-accent-primary data-[state=active]:bg-transparent data-[state=active]:shadow-none"
              >
                <Mail className="mr-2 h-4 w-4" />
                Informações Pessoais
              </TabsTrigger>
              <TabsTrigger
                value="senha"
                className="relative rounded-none border-b-2 border-transparent px-4 py-2.5 data-[state=active]:border-accent-primary data-[state=active]:bg-transparent data-[state=active]:shadow-none"
              >
                <Lock className="mr-2 h-4 w-4" />
                Alterar Senha
              </TabsTrigger>
              <TabsTrigger
                value="foto"
                className="relative rounded-none border-b-2 border-transparent px-4 py-2.5 data-[state=active]:border-accent-primary data-[state=active]:bg-transparent data-[state=active]:shadow-none"
              >
                <Camera className="mr-2 h-4 w-4" />
                Foto de Perfil
              </TabsTrigger>
              <TabsTrigger
                value="perigo"
                className="relative rounded-none border-b-2 border-transparent px-4 py-2.5 data-[state=active]:border-destructive data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:text-destructive"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Zona de Perigo
              </TabsTrigger>
            </TabsList>
          </div>

          <div className="p-4 sm:p-6">
            <TabsContent value="informacoes" className="mt-0">
              <EditProfileSection />
            </TabsContent>

            <TabsContent value="senha" className="mt-0">
              <ChangePasswordSection />
            </TabsContent>

            <TabsContent value="foto" className="mt-0">
              <ProfilePhotoSection />
            </TabsContent>

            <TabsContent value="perigo" className="mt-0">
              <DangerZoneSection />
            </TabsContent>
          </div>
        </Tabs>
      </Card>
    </div>
  );
}

/* ─── Profile Header (standalone card) ─── */

function ProfileHeaderCard() {
  const { user } = useAuth();
  if (!user) return null;

  const initials = user.username.slice(0, 2).toUpperCase();
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5110";
  const imageUrl = user.profileImage
    ? `${apiUrl}/${user.profileImage}?t=${new Date().getTime()}`
    : undefined;

  return (
    <Card className="p-6">
      <div className="flex items-center gap-6">
        <Avatar className="h-20 w-20">
          <AvatarImage src={imageUrl} alt={user.username} />
          <AvatarFallback className="text-xl">{initials}</AvatarFallback>
        </Avatar>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h2 className="text-xl font-semibold">{user.username}</h2>
            <Badge
              className={
                user.userType === UserType.Admin
                  ? "bg-accent-primary/10 text-accent-primary"
                  : "bg-muted text-muted-foreground"
              }
            >
              {user.userType === UserType.Admin ? "Admin" : "Usuário"}
            </Badge>
          </div>
          <p className="mt-1 text-sm text-muted-foreground">{user.email}</p>
        </div>
      </div>
    </Card>
  );
}

/* ─── Tab 1: Informações Pessoais ─── */

function EditProfileSection() {
  const { user } = useAuth();
  const updateProfile = useUpdateProfile();
  const [username, setUsername] = useState(user?.username ?? "");
  const [email, setEmail] = useState(user?.email ?? "");

  if (!user) return null;

  const hasChanges = username !== user.username || email !== user.email;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const data: { newUsername?: string; newEmail?: string } = {};
    if (username !== user.username) data.newUsername = username;
    if (email !== user.email) data.newEmail = email;
    updateProfile.mutate(data);
  };

  return (
    <div className="space-y-1">
      <h3 className="text-lg font-semibold">Informações Pessoais</h3>
      <p className="text-sm text-muted-foreground mb-6">
        Atualize seu nome de usuário e endereço de email.
      </p>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm font-medium">Nome de usuário</label>
            <Input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Seu nome de usuário"
              minLength={6}
              maxLength={30}
            />
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Email</label>
            <Input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="seu@email.com"
              maxLength={100}
            />
          </div>
        </div>
        <Button type="submit" disabled={!hasChanges || updateProfile.isPending}>
          {updateProfile.isPending ? "Salvando..." : "Salvar alterações"}
        </Button>
      </form>
    </div>
  );
}

/* ─── Tab 2: Alterar Senha ─── */

function ChangePasswordSection() {
  const updatePassword = useUpdatePassword();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const strength = getPasswordStrength(newPassword);

  const canSubmit =
    currentPassword.length > 0 &&
    newPassword.length >= 8 &&
    newPassword === confirmPassword;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    updatePassword.mutate(
      {
        currentPassword,
        newPassword,
        confirmPassword: confirmPassword,
      },
      {
        onSuccess: () => {
          setCurrentPassword("");
          setNewPassword("");
          setConfirmPassword("");
        },
      }
    );
  };

  return (
    <div className="space-y-1">
      <h3 className="text-lg font-semibold">Alterar Senha</h3>
      <p className="text-sm text-muted-foreground mb-6">
        Atualize sua senha para manter sua conta segura.
      </p>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-2">
          <label className="text-sm font-medium">Senha atual</label>
          <PasswordInput
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            placeholder="Sua senha atual"
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm font-medium">Nova senha</label>
            <PasswordInput
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Mínimo 8 caracteres"
              minLength={8}
            />
            {newPassword && (
              <div className="space-y-1">
                <div className="h-0.5 w-full overflow-hidden rounded-full bg-muted">
                  <div
                    className={`h-full rounded-full transition-all duration-300 ${strength.color}`}
                    style={{ width: strength.width }}
                  />
                </div>
                <p className="text-xs text-muted-foreground capitalize">
                  Força:{" "}
                  {strength.level === "weak"
                    ? "fraca"
                    : strength.level === "medium"
                      ? "média"
                      : "forte"}
                </p>
              </div>
            )}
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Confirmar nova senha</label>
            <PasswordInput
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Confirme a nova senha"
            />
          </div>
        </div>
        {newPassword && confirmPassword && newPassword !== confirmPassword && (
          <p className="text-sm text-destructive">As senhas não coincidem.</p>
        )}
        <Button type="submit" disabled={!canSubmit || updatePassword.isPending}>
          {updatePassword.isPending ? "Alterando..." : "Alterar senha"}
        </Button>
      </form>
    </div>
  );
}

/* ─── Tab 3: Foto de Perfil ─── */

function ProfilePhotoSection() {
  const { user } = useAuth();
  const updateImage = useUpdateProfileImage();
  const fileRef = useRef<HTMLInputElement>(null);
  const [cropperOpen, setCropperOpen] = useState(false);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  if (!user) return null;

  const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    e.target.value = "";

    const reader = new FileReader();
    reader.onload = () => {
      setSelectedImage(reader.result as string);
      setCropperOpen(true);
    };
    reader.readAsDataURL(file);
  };

  const handleCropComplete = (base64: string) => {
    updateImage.mutate({ imageBase64: base64 });
  };

  const initials = user.username.slice(0, 2).toUpperCase();
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5110";
  const imageUrl = user.profileImage
    ? `${apiUrl}/${user.profileImage}?t=${new Date().getTime()}`
    : undefined;

  return (
    <>
      <div className="space-y-1">
        <h3 className="text-lg font-semibold">Foto de Perfil</h3>
        <p className="text-sm text-muted-foreground mb-6">
          Personalize sua foto de perfil. A imagem será recortada em formato quadrado.
        </p>
      </div>

      <div className="flex flex-col items-center gap-6 py-4 sm:flex-row sm:items-start">
        <div className="relative shrink-0">
          <Avatar className="h-32 w-32 border-2 border-border">
            <AvatarImage src={imageUrl} alt={user.username} />
            <AvatarFallback className="text-3xl">{initials}</AvatarFallback>
          </Avatar>
        </div>

        <div className="flex flex-col gap-3 text-center sm:text-left">
          <p className="text-sm text-muted-foreground">
            Formatos aceitos: <strong>JPG, PNG</strong>. Tamanho máximo recomendado: <strong>2MB</strong>.
          </p>
          <div>
            <Button
              variant="outline"
              onClick={() => fileRef.current?.click()}
              disabled={updateImage.isPending}
            >
              <Camera className="mr-2 h-4 w-4" />
              {updateImage.isPending ? "Enviando..." : "Escolher imagem"}
            </Button>
            <input
              ref={fileRef}
              type="file"
              accept="image/png,image/jpeg,image/jpg"
              className="hidden"
              onChange={handleImageSelect}
            />
          </div>
        </div>
      </div>

      <ImageCropper
        imageSrc={selectedImage}
        open={cropperOpen}
        onOpenChange={setCropperOpen}
        onComplete={handleCropComplete}
        aspect={1}
      />
    </>
  );
}

/* ─── Tab 4: Zona de Perigo ─── */

function DangerZoneSection() {
  const deleteAccount = useDeleteAccount();
  const [showDialog, setShowDialog] = useState(false);
  const [password, setPassword] = useState("");

  const handleDelete = () => {
    deleteAccount.mutate(password, {
      onSuccess: () => setShowDialog(false),
    });
  };

  return (
    <>
      <div className="space-y-1">
        <h3 className="text-lg font-semibold text-destructive">Zona de Perigo</h3>
        <p className="text-sm text-muted-foreground mb-6">
          Ações irreversíveis para sua conta.
        </p>
      </div>

      <div className="rounded-lg border border-destructive/30 bg-destructive/5 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="font-medium">Deletar minha conta</p>
            <p className="text-sm text-muted-foreground">
              Uma vez deletada, sua conta e todos os dados associados serão
              permanentemente removidos. Esta ação não pode ser desfeita.
            </p>
          </div>
          <Button
            variant="destructive"
            className="shrink-0"
            onClick={() => setShowDialog(true)}
          >
            <Trash2 className="mr-2 h-4 w-4" />
            Deletar conta
          </Button>
        </div>
      </div>

      <Dialog open={showDialog} onOpenChange={setShowDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Deletar conta permanentemente</DialogTitle>
            <DialogDescription>
              Esta ação não pode ser desfeita. Todos os seus scans, resultados e
              dados serão permanentemente removidos.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-2">
            <label className="text-sm font-medium">
              Digite sua senha para confirmar
            </label>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Sua senha"
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDialog(false)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={!password || deleteAccount.isPending}
            >
              {deleteAccount.isPending ? "Deletando..." : "Deletar conta"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

/* ─── Skeleton ─── */

function ProfileSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <Skeleton className="h-32 w-full rounded-xl" />
      <Skeleton className="h-96 w-full rounded-xl" />
    </div>
  );
}
