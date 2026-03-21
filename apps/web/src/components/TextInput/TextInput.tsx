"use client";

import { useState, useCallback, useRef, type ChangeEvent } from "react";
import { cn } from "@/lib/utils";
import { formatNumber } from "@/utils/formatNumber";

interface TextInputProps {
  value: string;
  onChange: (value: string) => void;
  maxLength?: number;
  placeholder?: string;
  detectedLanguage?: string | null;
  contentType?: string | null;
  className?: string;
}

export function TextInput({
  value,
  onChange,
  maxLength = 500000,
  placeholder = "Paste your text or upload a file...",
  detectedLanguage,
  contentType,
  className,
}: TextInputProps) {
  const [isDragOver, setIsDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleChange = useCallback(
    (e: ChangeEvent<HTMLTextAreaElement>) => {
      onChange(e.target.value);
    },
    [onChange]
  );

  const handleFileUpload = useCallback(
    async (file: File) => {
      const allowedTypes = [
        "text/plain",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      ];

      if (!allowedTypes.includes(file.type) && !file.name.endsWith(".txt")) {
        alert("Please upload a .txt, .docx, or .pdf file");
        return;
      }

      if (file.type === "text/plain" || file.name.endsWith(".txt")) {
        const text = await file.text();
        onChange(text);
      } else {
        // For PDF and DOCX, we'd need backend processing
        alert(
          "PDF and DOCX parsing requires backend processing. Text files work locally."
        );
      }
    },
    [onChange]
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragOver(false);

      const file = e.dataTransfer.files[0];
      if (file) {
        handleFileUpload(file);
      }
    },
    [handleFileUpload]
  );

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback(() => {
    setIsDragOver(false);
  }, []);

  const handleFileInputChange = useCallback(
    (e: ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (file) {
        handleFileUpload(file);
      }
    },
    [handleFileUpload]
  );

  const characterCount = value.length;
  const isOverLimit = characterCount > maxLength;

  return (
    <div className={cn("space-y-2", className)}>
      <div
        className={cn(
          "relative rounded-lg border-2 transition-colors",
          isDragOver ? "border-indigo bg-indigo-subtle" : "border-border-subtle",
          isOverLimit && "border-state-error-border"
        )}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
      >
        <textarea
          value={value}
          onChange={handleChange}
          placeholder={placeholder}
          className="w-full h-64 p-4 rounded-lg resize-none focus:outline-none bg-transparent"
        />

        {isDragOver && (
          <div className="absolute inset-0 flex items-center justify-center bg-indigo-subtle/90 rounded-lg">
            <p className="text-indigo font-medium">Drop file here</p>
          </div>
        )}
      </div>

      <div className="flex items-center justify-between text-sm">
        <div className="flex items-center gap-4">
          {detectedLanguage && (
            <span className="rounded-full bg-bg-sunken px-3 py-1 text-text-secondary">
              {detectedLanguage}
            </span>
          )}
          {contentType && (
            <span className="text-text-muted">{contentType}</span>
          )}
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            className="text-text-link hover:underline"
          >
            Upload file
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept=".txt,.pdf,.docx"
            onChange={handleFileInputChange}
            className="hidden"
          />
        </div>

         <span
           className={cn(
             "tabular-nums",
              isOverLimit ? "text-error" : "text-text-muted"
           )}
         >
           {formatNumber(characterCount)} / {formatNumber(maxLength)}
         </span>
      </div>
    </div>
  );
}
