import { useTheme } from "next-themes";
import { Toaster as Sonner, toast } from "sonner";

const Toaster = ({ ...props }) => {
  const { theme = "system" } = useTheme();

  return (
    <Sonner
      theme={theme}
      className="toaster group"
      closeButton
      toastOptions={{
        unstyled: true,
        style: {
          backgroundColor: "rgb(var(--card))",
          opacity: 1,
          backdropFilter: "none",
        },
        classNames: {
          toast:
            "relative flex min-h-16 w-[min(28rem,calc(100vw-1rem))] max-w-[calc(100vw-1rem)] flex-row flex-nowrap items-center gap-3 rounded-lg border-2 !bg-card px-4 py-3 pr-12 text-foreground opacity-100 shadow-lg",
          icon: "flex size-5 shrink-0 items-center justify-center",
          content: "min-w-0 flex-1 overflow-visible",
          title: "whitespace-normal break-words text-sm font-medium leading-5 text-foreground",
          description:
            "mt-1 whitespace-normal break-words text-sm font-normal leading-5 text-muted-foreground",
          closeButton:
            "absolute right-2 top-2 z-10 rounded-md border border-border bg-card text-muted-foreground hover:bg-muted hover:text-foreground",
          success: "border-success text-foreground",
          error: "border-destructive text-foreground",
          warning: "border-warning text-foreground",
          info: "border-primary text-foreground",
          actionButton:
            "rounded-md bg-primary px-3 py-1.5 text-sm text-primary-foreground hover:bg-primary/90",
          cancelButton: "rounded-md bg-muted px-3 py-1.5 text-sm text-foreground hover:bg-muted/80",
        },
      }}
      {...props}
    />
  );
};

export { Toaster, toast };
