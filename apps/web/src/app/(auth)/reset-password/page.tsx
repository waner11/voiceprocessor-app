"use client";

import { Suspense, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useResetPassword, type ApiError } from "@/hooks";

const resetPasswordSchema = z
  .object({
    newPassword: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;

function ResetPasswordContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const { mutate: resetPassword, isPending, isSuccess } = useResetPassword();
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordForm>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const onSubmit = async (data: ResetPasswordForm) => {
    setError(null);
    resetPassword(
      { token: token!, newPassword: data.newPassword },
      {
        onError: (err: Error) => {
          const apiErr = err as unknown as ApiError;
          setError(apiErr?.message || "Something went wrong. Please try again.");
        },
      }
    );
  };

    if (!token) {
      return (
        <div className="space-y-6 text-center">
          <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-state-error-bg">
            <svg
              className="h-6 w-6 text-state-error-text"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </div>
         <h1 className="text-2xl font-bold text-text-primary">Invalid reset link</h1>
         <p className="text-text-secondary">
           Invalid or missing reset link. Please request a new password reset.
         </p>
         <Link
           href="/forgot-password"
           className="inline-block text-text-link hover:underline"
         >
           Request new reset link
         </Link>
       </div>
     );
   }

    if (isSuccess) {
      return (
        <div className="space-y-6 text-center">
          <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-success-subtle">
            <svg
              className="h-6 w-6 text-state-success-text"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
         <h1 className="text-2xl font-bold text-text-primary">
           Password reset successfully
         </h1>
         <p className="text-text-secondary">
           Your password has been updated. You can now sign in with your new
           password.
         </p>
         <Link
           href="/login"
           className="inline-block text-text-link hover:underline"
         >
           Sign in
         </Link>
       </div>
     );
   }

   return (
     <div className="space-y-6">
       <div className="text-center">
         <h1 className="text-2xl font-bold text-text-primary">Reset your password</h1>
         <p className="mt-2 text-text-secondary">
           Enter your new password below
         </p>
       </div>

       <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
         {error && (
           <div className="rounded-lg bg-state-error-bg border border-state-error-border p-3 text-sm text-state-error-text">
             {error}
           </div>
         )}

         <div>
           <label
             htmlFor="newPassword"
             className="block text-sm font-medium text-text-secondary mb-1"
           >
             New Password
           </label>
           <input
             {...register("newPassword")}
             type="password"
             id="newPassword"
             className="w-full rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary placeholder-text-muted focus:border-border-focus focus:outline-none focus:ring-1 focus:ring-border-focus"
             placeholder="••••••••"
           />
           {errors.newPassword && (
             <p className="mt-1 text-sm text-state-error-text">
               {errors.newPassword.message}
             </p>
           )}
         </div>

         <div>
           <label
             htmlFor="confirmPassword"
             className="block text-sm font-medium text-text-secondary mb-1"
           >
             Confirm Password
           </label>
           <input
             {...register("confirmPassword")}
             type="password"
             id="confirmPassword"
             className="w-full rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary placeholder-text-muted focus:border-border-focus focus:outline-none focus:ring-1 focus:ring-border-focus"
             placeholder="••••••••"
           />
           {errors.confirmPassword && (
             <p className="mt-1 text-sm text-state-error-text">
               {errors.confirmPassword.message}
             </p>
           )}
         </div>

         <button
           type="submit"
           disabled={isPending}
           className="w-full rounded-lg bg-indigo py-2.5 text-white hover:bg-indigo-dark disabled:opacity-50"
         >
           {isPending ? "Resetting..." : "Reset password"}
         </button>
      </form>

       <p className="text-center text-sm text-text-secondary">
         Remember your password?{" "}
         <Link href="/login" className="text-text-link hover:underline">
           Sign in
         </Link>
       </p>
    </div>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense>
      <ResetPasswordContent />
    </Suspense>
  );
}
