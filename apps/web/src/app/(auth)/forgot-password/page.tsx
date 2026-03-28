"use client";

import { useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useForgotPassword, type ApiError } from "@/hooks";

const forgotPasswordSchema = z.object({
  email: z.string().email("Please enter a valid email"),
});

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export default function ForgotPasswordPage() {
  const { mutate: forgotPassword, isPending } = useForgotPassword();
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordForm>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = async (data: ForgotPasswordForm) => {
    setError(null);
    forgotPassword(data, {
      onSuccess: () => {
        setIsSubmitted(true);
      },
      onError: (err: Error) => {
        const apiErr = err as unknown as ApiError;
        setError(apiErr?.message || "Something went wrong. Please try again.");
      },
    });
  };

    if (isSubmitted) {
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
         <h1 className="text-2xl font-bold text-text-primary">Check your email</h1>
         <p className="text-text-secondary">
           We&apos;ve sent a password reset link to your email address. Please
           check your inbox and follow the instructions.
         </p>
         <Link
           href="/login"
           className="inline-block text-text-link hover:underline"
         >
           Back to sign in
         </Link>
       </div>
     );
   }

   return (
     <div className="space-y-6">
       <div className="text-center">
         <h1 className="text-2xl font-bold text-text-primary">Forgot your password?</h1>
         <p className="mt-2 text-text-secondary">
           Enter your email and we&apos;ll send you a reset link
         </p>
       </div>

       <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
         {error && (
           <div className="rounded-lg bg-state-error-bg border border-state-error-border p-3 text-sm text-state-error-text">
             {error}
           </div>
         )}

         <div>
           <label htmlFor="email" className="block text-sm font-medium text-text-secondary mb-1">
             Email
           </label>
           <input
             {...register("email")}
             type="email"
             id="email"
             className="w-full rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary placeholder-text-muted focus:border-border-focus focus:outline-none focus:ring-1 focus:ring-border-focus"
             placeholder="you@example.com"
           />
           {errors.email && (
             <p className="mt-1 text-sm text-state-error-text">{errors.email.message}</p>
           )}
         </div>

         <button
           type="submit"
           disabled={isPending}
           className="w-full rounded-lg bg-indigo py-2.5 text-text-inverse hover:bg-indigo-dark disabled:opacity-50"
         >
           {isPending ? "Sending..." : "Send reset link"}
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
