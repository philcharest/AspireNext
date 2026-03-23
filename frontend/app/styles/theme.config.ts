/**
 * Theme Configuration
 * Central place to manage all color, typography, and spacing themes for the application
 * 
 * Add or modify values here and they will be automatically available
 * across the entire application as CSS variables
 */

export const themeConfig = {
  // Font Families
  fonts: {
    // Primary font for body and general text
    sans: 'var(--font-geist-sans), -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    // Monospace font for code blocks
    mono: 'var(--font-geist-mono), "Courier New", Courier, monospace',
    // Serif font for headings (optional)
    serif: 'Georgia, "Times New Roman", serif',
  },

  // Font Sizes
  fontSizes: {
    xs: '0.75rem',      // 12px
    sm: '0.875rem',     // 14px
    base: '1rem',       // 16px
    lg: '1.125rem',     // 18px
    xl: '1.25rem',      // 20px
    '2xl': '1.5rem',    // 24px
    '3xl': '1.875rem',  // 30px
    '4xl': '2.25rem',   // 36px
    '5xl': '3rem',      // 48px
  },

  // Font Weights
  fontWeights: {
    light: 300,
    normal: 400,
    medium: 500,
    semibold: 600,
    bold: 700,
    extrabold: 800,
  },

  // Line Heights
  lineHeights: {
    tight: 1.1,
    snug: 1.25,
    normal: 1.5,
    relaxed: 1.625,
    loose: 2,
  },

  // Letter Spacing
  letterSpacing: {
    tight: '-0.02em',
    normal: '0em',
    wide: '0.02em',
    wider: '0.05em',
    widest: '0.1em',
  },

  light: {
    // Primary Colors
    primary: '#000000',
    primaryLight: '#333333',
    primaryDark: '#000000',

    // Secondary Colors
    secondary: '#0066cc',
    secondaryLight: '#3385ff',
    secondaryDark: '#004a99',

    // Accent Colors
    accent: '#ff6b35',
    accentLight: '#ff8c5a',
    accentDark: '#e55a2b',

    // Neutral Colors
    background: '#ffffff',
    foreground: '#171717',
    border: '#e5e5e5',
    muted: '#a1a1a1',
    mutedBackground: '#f5f5f5',

    // Status Colors
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6',
  },
  dark: {
    // Primary Colors
    primary: '#ffffff',
    primaryLight: '#cccccc',
    primaryDark: '#ffffff',

    // Secondary Colors
    secondary: '#60a5fa',
    secondaryLight: '#93c5fd',
    secondaryDark: '#1e40af',

    // Accent Colors
    accent: '#ff8c42',
    accentLight: '#ffb380',
    accentDark: '#e67e22',

    // Neutral Colors
    background: '#0a0a0a',
    foreground: '#ededed',
    border: '#2a2a2a',
    muted: '#666666',
    mutedBackground: '#1a1a1a',

    // Status Colors
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6',
  },
} as const;

/**
 * Get theme colors for light mode
 */
export function getLightTheme() {
  return themeConfig.light;
}

/**
 * Get theme colors for dark mode
 */
export function getDarkTheme() {
  return themeConfig.dark;
}

/**
 * Get all available color names
 */
export function getColorNames() {
  return Object.keys(themeConfig.light) as Array<keyof typeof themeConfig.light>;
}

/**
 * Get font configuration
 */
export function getFonts() {
  return themeConfig.fonts;
}

/**
 * Get font sizes
 */
export function getFontSizes() {
  return themeConfig.fontSizes;
}

/**
 * Get font weights
 */
export function getFontWeights() {
  return themeConfig.fontWeights;
}

/**
 * Get line heights
 */
export function getLineHeights() {
  return themeConfig.lineHeights;
}

/**
 * Get letter spacing
 */
export function getLetterSpacing() {
  return themeConfig.letterSpacing;
}
