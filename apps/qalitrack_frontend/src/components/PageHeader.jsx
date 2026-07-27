import React from 'react';

// Shared page-header bar — every page should render this instead of hand-rolling
// its own header markup, so icon size, padding, rounding, and text styling stay
// identical everywhere instead of drifting per-page (some rounded, some square,
// some using h1/h2/p tags with default browser margins that misalign the text).
//
// `flush` is for pages nested inside another page's own bordered card (e.g. a tab's
// content sitting inside a hub's white card) — no floating margin/rounding of its
// own; it sits flush at the top and lets the parent card's overflow-hidden clip it.
export default function PageHeader({ icon: Icon, title, subtitle, actions, flush = false, className = '' }) {
    const base = flush
        ? 'px-4 py-2.5 shrink-0 flex items-center justify-between gap-3 flex-wrap'
        : 'mb-3 mx-4 sm:mx-6 mt-4 sm:mt-6 rounded-lg px-4 py-2.5 shrink-0 flex items-center justify-between gap-3 flex-wrap';
    return (
        <div
            className={`${base} ${className}`}
            style={{ backgroundColor: 'var(--cs-appbar-bg)' }}
        >
            <div className="flex items-center gap-2.5 min-w-0">
                {Icon && (
                    <div className="w-9 h-9 rounded-lg cs-icon-box flex items-center justify-center shadow-sm shrink-0">
                        <Icon className="w-5 h-5" style={{ color: 'var(--cs-icon-accent)' }} />
                    </div>
                )}
                <div className="min-w-0">
                    <div className="text-sm font-bold leading-tight truncate" style={{ color: 'var(--cs-appbar-text)' }}>{title}</div>
                    {subtitle && (
                        <div className="text-[11px] font-medium leading-tight truncate" style={{ color: 'var(--cs-appbar-text)', opacity: 0.7 }}>
                            {subtitle}
                        </div>
                    )}
                </div>
            </div>
            {actions && <div className="flex items-center gap-1.5 shrink-0 flex-wrap">{actions}</div>}
        </div>
    );
}
