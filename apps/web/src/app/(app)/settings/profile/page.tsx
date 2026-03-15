"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import toast from "react-hot-toast";
import { useAuthStore } from "@/stores";
import {
  useUpdateProfile,
  useChangePassword,
  useSetPassword,
  useDeleteAccount,
} from "@/hooks/useProfile";

const profileSchema = z.object({
  name: z.string().min(1, "Name is required").max(100, "Name must be at most 100 characters"),
});

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

const setPasswordSchema = z
  .object({
    newPassword: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

const deleteAccountSchema = z.object({
  password: z.string().optional(),
});

type ProfileForm = z.infer<typeof profileSchema>;
type ChangePasswordForm = z.infer<typeof changePasswordSchema>;
type SetPasswordForm = z.infer<typeof setPasswordSchema>;
type DeleteAccountForm = z.infer<typeof deleteAccountSchema>;

function getErrorMessage(err: unknown, fallback: string): string {
  return ((err as Record<string, unknown>)?.message as string) || fallback;
}

export default function ProfileSettingsPage() {
  const user = useAuthStore((state) => state.user);
  const [isEditing, setIsEditing] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);

  const { mutate: updateProfile, isPending: isUpdatingProfile } = useUpdateProfile();
  const { mutate: changePassword, isPending: isChangingPassword } = useChangePassword();
  const { mutate: setPassword, isPending: isSettingPassword } = useSetPassword();
  const { mutate: deleteAccount, isPending: isDeletingAccount } = useDeleteAccount();

  const profileForm = useForm<ProfileForm>({
    resolver: zodResolver(profileSchema),
    defaultValues: { name: user?.name || "" },
  });

  const changePasswordForm = useForm<ChangePasswordForm>({
    resolver: zodResolver(changePasswordSchema),
  });

  const setPasswordForm = useForm<SetPasswordForm>({
    resolver: zodResolver(setPasswordSchema),
  });

  const deleteAccountForm = useForm<DeleteAccountForm>({
    resolver: zodResolver(deleteAccountSchema),
  });

  const onProfileSubmit = (data: ProfileForm) => {
    updateProfile(
      { name: data.name },
      {
        onSuccess: () => {
          toast.success("Profile updated successfully");
          setIsEditing(false);
        },
        onError: (err) => {
          const msg = getErrorMessage(err, "Failed to update profile");
          toast.error(String(msg));
        },
      }
    );
  };

  const onChangePasswordSubmit = (data: ChangePasswordForm) => {
    changePassword(
      { currentPassword: data.currentPassword, newPassword: data.newPassword },
      {
        onSuccess: () => {
          toast.success("Password changed. You may need to log in again.");
          changePasswordForm.reset();
        },
        onError: (err) => {
          const msg = getErrorMessage(err, "Failed to change password");
          toast.error(String(msg));
        },
      }
    );
  };

  const onSetPasswordSubmit = (data: SetPasswordForm) => {
    setPassword(
      { newPassword: data.newPassword },
      {
        onSuccess: () => {
          toast.success("Password set successfully. You may need to log in again.");
          setPasswordForm.reset();
        },
        onError: (err) => {
          const msg = getErrorMessage(err, "Failed to set password");
          toast.error(String(msg));
        },
      }
    );
  };

  const onDeleteAccountSubmit = (data: DeleteAccountForm) => {
    deleteAccount(
      { password: data.password },
      {
        onError: (err) => {
          const msg = getErrorMessage(err, "Failed to delete account");
          toast.error(String(msg));
        },
      }
    );
  };

  return (
    <div className="space-y-8">
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Profile Information</h2>
          {!isEditing && (
            <button
              onClick={() => setIsEditing(true)}
              className="text-sm text-blue-600 dark:text-blue-400 hover:underline"
            >
              Edit
            </button>
          )}
        </div>

        {isEditing ? (
          <form onSubmit={profileForm.handleSubmit(onProfileSubmit)} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Name
              </label>
              <input
                {...profileForm.register("name")}
                type="text"
                className="w-full max-w-md rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {profileForm.formState.errors.name && (
                <p className="mt-1 text-sm text-red-600">{profileForm.formState.errors.name.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Email
              </label>
              <p className="text-gray-900 dark:text-white">{user?.email || "--"}</p>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">Email cannot be changed</p>
            </div>

            <div className="flex gap-3 pt-2">
              <button
                type="submit"
                disabled={isUpdatingProfile}
                className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {isUpdatingProfile ? "Saving..." : "Save Changes"}
              </button>
              <button
                type="button"
                onClick={() => {
                  setIsEditing(false);
                  profileForm.reset({ name: user?.name || "" });
                }}
                className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
              >
                Cancel
              </button>
            </div>
          </form>
        ) : (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Name
              </label>
              <p className="text-gray-900 dark:text-white">{user?.name || "--"}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Email
              </label>
              <p className="text-gray-900 dark:text-white">{user?.email || "--"}</p>
            </div>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Password</h2>

        {user?.hasPassword ? (
          <form onSubmit={changePasswordForm.handleSubmit(onChangePasswordSubmit)} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Current Password
              </label>
              <input
                {...changePasswordForm.register("currentPassword")}
                type="password"
                autoComplete="current-password"
                placeholder="••••••••"
                className="w-full max-w-md rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {changePasswordForm.formState.errors.currentPassword && (
                <p className="mt-1 text-sm text-red-600">{changePasswordForm.formState.errors.currentPassword.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                New Password
              </label>
              <input
                {...changePasswordForm.register("newPassword")}
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                className="w-full max-w-md rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {changePasswordForm.formState.errors.newPassword && (
                <p className="mt-1 text-sm text-red-600">{changePasswordForm.formState.errors.newPassword.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Confirm New Password
              </label>
              <input
                {...changePasswordForm.register("confirmPassword")}
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                className="w-full max-w-md rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {changePasswordForm.formState.errors.confirmPassword && (
                <p className="mt-1 text-sm text-red-600">{changePasswordForm.formState.errors.confirmPassword.message}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={isChangingPassword}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {isChangingPassword ? "Updating..." : "Update Password"}
            </button>
          </form>
        ) : (
          <form onSubmit={setPasswordForm.handleSubmit(onSetPasswordSubmit)} className="space-y-4">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              You signed up with a social account. Set a password to also log in with email.
            </p>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                New Password
              </label>
              <input
                {...setPasswordForm.register("newPassword")}
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                className="w-full max-w-md rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {setPasswordForm.formState.errors.newPassword && (
                <p className="mt-1 text-sm text-red-600">{setPasswordForm.formState.errors.newPassword.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Confirm Password
              </label>
              <input
                {...setPasswordForm.register("confirmPassword")}
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                className="w-full max-w-md rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {setPasswordForm.formState.errors.confirmPassword && (
                <p className="mt-1 text-sm text-red-600">{setPasswordForm.formState.errors.confirmPassword.message}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={isSettingPassword}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {isSettingPassword ? "Setting..." : "Set Password"}
            </button>
          </form>
        )}
      </section>

      <section className="rounded-lg border border-red-200 dark:border-red-900 bg-red-50 dark:bg-red-950/30 p-6">
        <h2 className="text-xl font-semibold text-red-800 dark:text-red-400 mb-4">Danger Zone</h2>
        <p className="text-sm text-red-700 dark:text-red-400 mb-4">
          Once you delete your account, there is no going back. Please be certain.
        </p>
        <button
          onClick={() => setShowDeleteDialog(true)}
          className="rounded-lg border border-red-300 dark:border-red-800 px-4 py-2 text-sm text-red-700 dark:text-red-400 hover:bg-red-100 dark:hover:bg-red-950"
        >
          Delete Account
        </button>
      </section>

      {showDeleteDialog && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
          tabIndex={-1}
          onKeyDown={(e) => { if (e.key === 'Escape') setShowDeleteDialog(false); }}
        >
          <div
            className="w-full max-w-md rounded-lg bg-white dark:bg-gray-900 p-6 shadow-xl"
            role="dialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
          >
            <h3 id="delete-dialog-title" className="text-lg font-semibold text-gray-900 dark:text-white mb-2">Delete Account</h3>
            <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
              Your account will be deactivated. This action cannot be undone.
            </p>

            <form onSubmit={deleteAccountForm.handleSubmit(onDeleteAccountSubmit)} className="space-y-4">
              {user?.hasPassword && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Confirm your password
                  </label>
                  <input
                    {...deleteAccountForm.register("password")}
                    type="password"
                    autoComplete="current-password"
                    autoFocus
                    placeholder="••••••••"
                    className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-red-500 focus:outline-none focus:ring-1 focus:ring-red-500"
                  />
                </div>
              )}

              <div className="flex gap-3 pt-2">
                <button
                  type="submit"
                  disabled={isDeletingAccount}
                  autoFocus={!user?.hasPassword}
                  className="rounded-lg bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 disabled:opacity-50"
                >
                  {isDeletingAccount ? "Deleting..." : "Confirm Delete"}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowDeleteDialog(false);
                    deleteAccountForm.reset();
                  }}
                  className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
