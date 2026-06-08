import { HugeiconsIcon } from "@hugeicons/react";
import { Notification03Icon, Menu01Icon, Search01Icon } from "@hugeicons/core-free-icons";

interface AppHeaderProps {
  onToggleSidebar: () => void;
}

export function AppHeader({ onToggleSidebar }: AppHeaderProps) {
  return (
    <header className="sticky top-0 z-20 flex h-14 items-center justify-between border-b border-border bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="flex items-center gap-4">
        <button
          onClick={onToggleSidebar}
          className="inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground md:hidden"
        >
          <HugeiconsIcon icon={Menu01Icon} size={20} />
          <span className="sr-only">Toggle Sidebar</span>
        </button>
        <div className="hidden items-center gap-2 md:flex">
          <span className="text-sm font-semibold text-foreground">Dealership</span>
        </div>
      </div>

      <div className="flex items-center gap-4">

        <button className="relative inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground">
          <HugeiconsIcon icon={Notification03Icon} size={20} />
          <span className="absolute right-2 top-2 size-2 rounded-full bg-destructive"></span>
        </button>
        
        {/* User avatar placeholder */}
        <button className="flex size-8 items-center justify-center rounded-full bg-muted border border-border">
          <span className="text-xs font-medium text-muted-foreground">U</span>
        </button>
      </div>
    </header>
  );
}
