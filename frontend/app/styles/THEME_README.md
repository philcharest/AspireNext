# Theme Management System

This document explains how to use and customize the centralized theme system in your Next.js application.

## Overview

The theme system uses **CSS Variables** and a centralized **TypeScript configuration** to manage all colors, typography, and spacing across the application. This allows you to:

- ✅ Change colors, fonts, and spacing in one place and have them reflect everywhere
- ✅ Support light and dark modes automatically
- ✅ Access theme values in TypeScript for component logic
- ✅ Maintain consistency across the entire application
- ✅ Easily customize typography (font families, sizes, weights, line heights)

## File Structure

```
frontend/app/
├── styles/
│   ├── theme.config.ts      (TypeScript color definitions)
│   └── theme.css            (CSS variable declarations)
├── globals.css              (Imports theme.css)
└── components/
    └── *.module.css         (Use theme variables)
```

## Using Theme Colors in CSS

### In your CSS modules, use CSS variables:

```css
/* navigation.module.css */
.brand {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-primary);
}

.link {
  font-family: var(--font-sans);
  font-size: var(--font-size-base);
  color: var(--color-secondary);
  transition: color var(--transition-fast);
}

.link:hover {
  color: var(--color-secondary-dark);
}

.card {
  background-color: var(--color-background);
  border: 1px solid var(--color-border);
}

.title {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  line-height: var(--line-height-snug);
  letter-spacing: var(--letter-spacing-wide);
}

.button {
  font-family: var(--font-sans);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  background-color: var(--color-accent);
  transition: opacity var(--transition-fast);
}

.code {
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  background-color: var(--color-muted-background);
}
```

## Available CSS Variables

### Typography Variables

**Font Families:**
- `--font-sans` - Primary font for body and general text
- `--font-mono` - Monospace font for code blocks
- `--font-serif` - Serif font for headings (optional)

**Font Sizes:**
- `--font-size-xs` - 12px
- `--font-size-sm` - 14px
- `--font-size-base` - 16px (default)
- `--font-size-lg` - 18px
- `--font-size-xl` - 20px
- `--font-size-2xl` - 24px
- `--font-size-3xl` - 30px
- `--font-size-4xl` - 36px
- `--font-size-5xl` - 48px

**Font Weights:**
- `--font-weight-light` - 300
- `--font-weight-normal` - 400
- `--font-weight-medium` - 500
- `--font-weight-semibold` - 600
- `--font-weight-bold` - 700
- `--font-weight-extrabold` - 800

**Line Heights:**
- `--line-height-tight` - 1.1
- `--line-height-snug` - 1.25
- `--line-height-normal` - 1.5
- `--line-height-relaxed` - 1.625
- `--line-height-loose` - 2

**Letter Spacing:**
- `--letter-spacing-tight` - -0.02em
- `--letter-spacing-normal` - 0em
- `--letter-spacing-wide` - 0.02em
- `--letter-spacing-wider` - 0.05em
- `--letter-spacing-widest` - 0.1em

**Primary Colors:**
- `--color-primary` - Main text color
- `--color-primary-light` - Lighter variant
- `--color-primary-dark` - Darker variant

**Secondary Colors:**
- `--color-secondary` - Secondary action color
- `--color-secondary-light` - Lighter variant
- `--color-secondary-dark` - Darker variant

**Accent Colors:**
- `--color-accent` - Accent/highlight color
- `--color-accent-light` - Lighter variant
- `--color-accent-dark` - Darker variant

**Neutral Colors:**
- `--color-background` - Page background
- `--color-foreground` - Primary text
- `--color-border` - Border color
- `--color-muted` - Secondary text
- `--color-muted-background` - Muted background areas

**Status Colors:**
- `--color-success` - Success state
- `--color-warning` - Warning state
- `--color-error` - Error/danger state
- `--color-info` - Info state

### Utility Variables

**Spacing:**
- `--spacing-xs`, `--spacing-sm`, `--spacing-md`, `--spacing-lg`, `--spacing-xl`, `--spacing-2xl`

**Border Radius:**
- `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-xl`

**Transitions:**
- `--transition-fast` - 150ms
- `--transition-base` - 200ms
- `--transition-slow` - 300ms

## Customizing Colors

### Step 1: Update `theme.config.ts`

```typescript
// frontend/app/styles/theme.config.ts
export const themeConfig = {
  fonts: {
    sans: 'var(--font-geist-sans), -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    mono: '"Courier New", Courier, monospace',
    serif: 'Georgia, "Times New Roman", serif',
  },
  fontSizes: {
    xs: '0.75rem',
    sm: '0.875rem',
    base: '1rem',
    lg: '1.125rem',
    // ... more sizes
  },
  light: {
    primary: '#1a1a1a',        // Change primary color
    secondary: '#0066cc',      // Change secondary color
    accent: '#ff6b35',         // Change accent color
    // ... other colors
  },
  dark: {
    primary: '#ffffff',
    secondary: '#60a5fa',
    accent: '#ff8c42',
    // ... other colors
  },
};
```

### Step 2: Update `theme.css`

Copy the color and typography values from your `theme.config.ts` to `theme.css`:

```css
:root {
  /* Font Families */
  --font-sans: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  --font-mono: "Courier New", Courier, monospace;
  --font-serif: Georgia, "Times New Roman", serif;

  /* Font Sizes */
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  /* ... more sizes */

  /* Font Weights */
  --font-weight-light: 300;
  --font-weight-normal: 400;
  --font-weight-bold: 700;
  /* ... more weights */

  /* Colors */
  --color-primary: #1a1a1a;
  --color-secondary: #0066cc;
  --color-accent: #ff6b35;
  /* ... other colors */
}

@media (prefers-color-scheme: dark) {
  :root {
    --color-primary: #ffffff;
    --color-secondary: #60a5fa;
    --color-accent: #ff8c42;
    /* ... other colors */
  }
}
```

## Using Theme in TypeScript

```typescript
// Import theme configuration
import { 
  themeConfig, 
  getLightTheme, 
  getDarkTheme,
  getFonts,
  getFontSizes,
  getFontWeights,
  getLineHeights,
  getLetterSpacing
} from '@/app/styles/theme.config';

// Get all light theme colors
const lightColors = getLightTheme();
console.log(lightColors.primary); // '#000000'

// Get all dark theme colors
const darkColors = getDarkTheme();

// Get typography values
const fonts = getFonts();
console.log(fonts.sans); // Font family string
console.log(fonts.mono); // Monospace font

const fontSizes = getFontSizes();
console.log(fontSizes.lg); // '1.125rem'

const fontWeights = getFontWeights();
console.log(fontWeights.bold); // 700

const lineHeights = getLineHeights();
console.log(lineHeights.normal); // 1.5

const letterSpacing = getLetterSpacing();
console.log(letterSpacing.wide); // '0.02em'
```

## Dark Mode Behavior

The theme system automatically switches colors based on the user's system preference:

- **Light mode (default):** Uses colors defined in the `:root` selector
- **Dark mode:** Uses colors defined in `@media (prefers-color-scheme: dark)` selector

Users can also override this in their browser settings, and the theme will adapt accordingly.

## Tips

1. **Consistency:** Always use CSS variables instead of hardcoding colors
2. **Transitions:** Use the provided transition variables for smooth animations
3. **Spacing:** Use the spacing variables for consistent margins/padding
4. **Status Colors:** Use status colors (success, error, warning, info) appropriately
5. **Test Dark Mode:** Always test your components in both light and dark modes

## Examples: Creating Components with Theme

### Example 1: Button Component

```css
/* button.module.css */
.button {
  font-family: var(--font-sans);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  background-color: var(--color-secondary);
  color: var(--color-background);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-md);
  border: none;
  cursor: pointer;
  transition: background-color var(--transition-fast);
  letter-spacing: var(--letter-spacing-normal);
}

.button:hover {
  background-color: var(--color-secondary-dark);
}

.button.disabled {
  background-color: var(--color-muted);
  cursor: not-allowed;
  font-weight: var(--font-weight-normal);
}
```

### Example 2: Heading Component

```css
/* heading.module.css */
.h1 {
  font-family: var(--font-sans);
  font-size: var(--font-size-4xl);
  font-weight: var(--font-weight-bold);
  line-height: var(--line-height-snug);
  color: var(--color-primary);
  margin-bottom: var(--spacing-lg);
  letter-spacing: var(--letter-spacing-tight);
}

.h2 {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  line-height: var(--line-height-snug);
  color: var(--color-primary);
  margin-bottom: var(--spacing-md);
}

.subtitle {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-medium);
  color: var(--color-muted);
  line-height: var(--line-height-relaxed);
}
```

### Example 3: Code Block Component

```css
/* codeBlock.module.css */
.pre {
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  background-color: var(--color-muted-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  overflow-x: auto;
  line-height: var(--line-height-relaxed);
  letter-spacing: var(--letter-spacing-normal);
}

.code {
  color: var(--color-secondary);
  font-weight: var(--font-weight-medium);
}

.keyword {
  color: var(--color-accent);
  font-weight: var(--font-weight-bold);
}
```

### Example 4: Card Component

```css
/* card.module.css */
.card {
  background-color: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  transition: box-shadow var(--transition-base), transform var(--transition-base);
}

.card:hover {
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.cardTitle {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-primary);
  margin-bottom: var(--spacing-sm);
  letter-spacing: var(--letter-spacing-tight);
}

.cardDescription {
  font-size: var(--font-size-sm);
  color: var(--color-muted);
  line-height: var(--line-height-relaxed);
}
```

## Tips

1. **Consistency:** Always use CSS variables instead of hardcoding values
2. **Transitions:** Use the provided transition variables for smooth animations
3. **Spacing:** Use the spacing variables for consistent margins/padding
4. **Typography:** Use the typography variables for consistent font sizing and styling
5. **Status Colors:** Use status colors (success, error, warning, info) appropriately
6. **Test Dark Mode:** Always test your components in both light and dark modes
7. **Font Combinations:** Pair font families thoughtfully (e.g., sans for body, mono for code)

That's it! Your theme system is now fully configured with typography and ready to use across your application.
