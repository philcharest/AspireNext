import type { Currency } from "@/lib/currency-context";

const formatters: Record<Currency, Intl.NumberFormat> = {
    CAD: new Intl.NumberFormat("en-CA", { style: "currency", currency: "CAD" }),
    USD: new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }),
};

export function formatPrice(amount: number, currency: Currency): string {
    // Falls back to CAD for any unrecognized value (e.g. pre-multi-currency data) rather than
    // throwing - formatters is keyed by the exact Currency union, so anything else is missing.
    return (formatters[currency] ?? formatters.CAD).format(amount);
}
