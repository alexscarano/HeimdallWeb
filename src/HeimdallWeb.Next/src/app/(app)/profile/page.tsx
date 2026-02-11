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
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

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

      {/* Edit Profile */}
      <EditProfileCard />

      {/* Change Password */}
      <ChangePasswordCard />

      {/* Danger Zone */}
      <DangerZoneCard />
    </div>
  );
}

function ProfileHeaderCard() {
  const { user } = useAuth();
  const updateImage = useUpdateProfileImage();
  const fileRef = useRef<HTMLInputElement>(null);

  if (!user) return null;

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      const base64 = (reader.result as string).split(",")[1];
      updateImage.mutate({ imageBase64: base64 });
    };
    reader.readAsDataURL(file);
  };

  const initials = user.username.slice(0, 2).toUpperCase();
  const imageUrl = user.profileImage
    ? `${process.env.NEXT_PUBLIC_API_URL}/${user.profileImage}`
    : undefined;

  return (
    <Card className="p-6">
      <div className="flex items-center gap-6">
        <div className="relative">
          <Avatar className="h-20 w-20">
            <AvatarImage src={imageUrl} alt={user.username} />
            <AvatarFallback className="text-xl">{initials}</AvatarFallback>
          </Avatar>
          <button
            onClick={() => fileRef.current?.click()}
            className="absolute -bottom-1 -right-1 flex h-8 w-8 items-center justify-center rounded-full border bg-background shadow-sm hover:bg-muted"
          >
            <Camera className="h-4 w-4" />
          </button>
          <input
            ref={fileRef}
            type="file"
            accept="image/png,image/jpeg,image/jpg"
            className="hidden"
            onChange={handleImageUpload}
          />
        </div>
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

function EditProfileCard() {
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
    <Card className="p-6">
      <div className="mb-4 flex items-center gap-2">
        <Mail className="h-5 w-5 text-muted-foreground" />
        <h3 className="font-semibold">Informações Pessoais</h3>
      </div>
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
    </Card>
  );
}

function ChangePasswordCard() {
  const updatePassword = useUpdatePassword();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

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
    <Card className="p-6">
      <div className="mb-4 flex items-center gap-2">
        <Lock className="h-5 w-5 text-muted-foreground" />
        <h3 className="font-semibold">Alterar Senha</h3>
      </div>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-2">
          <label className="text-sm font-medium">Senha atual</label>
          <Input
            type="password"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            placeholder="Sua senha atual"
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm font-medium">Nova senha</label>
            <Input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Mínimo 8 caracteres"
              minLength={8}
            />
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Confirmar nova senha</label>
            <Input
              type="password"
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
    </Card>
  );
}

function DangerZoneCard() {
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
      <Card className="border-destructive/50 p-6">
        <div className="mb-4 flex items-center gap-2">
          <Trash2 className="h-5 w-5 text-destructive" />
          <h3 className="font-semibold text-destructive">Zona de Perigo</h3>
        </div>
        <p className="mb-4 text-sm text-muted-foreground">
          Uma vez deletada, sua conta e todos os dados associados serão
          permanentemente removidos. Esta ação não pode ser desfeita.
        </p>
        <Button variant="destructive" onClick={() => setShowDialog(true)}>
          <Trash2 className="mr-2 h-4 w-4" />
          Deletar minha conta
        </Button>
      </Card>

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

function ProfileSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <Skeleton className="h-32 w-full rounded-xl" />
      <Skeleton className="h-48 w-full rounded-xl" />
      <Skeleton className="h-64 w-full rounded-xl" />
      <Skeleton className="h-32 w-full rounded-xl" />
    </div>
  );
}
