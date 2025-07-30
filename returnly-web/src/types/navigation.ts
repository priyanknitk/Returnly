// Generic navigation types
export interface NavigationItem {
  path: string;
  label: string;
  icon: React.ComponentType<any>;
  requiresAuth?: boolean;
  requiresData?: string[]; // Array of required data keys
}

export interface RouteConfig {
  path: string;
  element: React.ReactElement;
  redirectTo?: string;
  requiresData?: string[];
}

export interface AppState {
  form16Data: any | null;
  taxResults: any | null;
  currentForm16Data: any | null;
  [key: string]: any;
}
