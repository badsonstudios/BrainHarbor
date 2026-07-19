# Handoff: BrainHarbor — "Clear & Kind" theme + Entry Hub home layout

## Overview
BrainHarbor (brainharbor.org) is a nonprofit-style, plain-language brain-tumor
research hub for patients and caregivers. This package delivers a chosen visual
direction (**Clear & Kind**) and a chosen home-page composition (**Entry Hub**),
covering the two core page types: the **home page** and a **research item page**.

The content and information architecture are fixed (they come from the existing
mock-up); this handoff changes only the **visual design** and the **home-page
layout**.

## About the design files
The files in this bundle are **design references**, authored as plain semantic
HTML + one CSS file so they map directly onto a token-driven site. They are not
a drop-in app. The task is to **recreate these pages inside the existing
BrainHarbor codebase** (Razor Pages + htmx) using its established patterns:
- Fold `css/brainharbor.css` into the existing `wwwroot/css/site.css` — most of
  it is the same custom-property system already in use; only `--color-*` and the
  new component tokens/classes are additions.
- Turn the two HTML files into the appropriate Razor views / partials
  (`_Layout`, a home `Index`, a `Research/Item` page), and render the feed and
  badges from real data with the markup shown here.
- No JS framework, no CSS framework. The badge, hub, and card are pure HTML+CSS;
  htmx can drive "See all" / pagination as usual.

## Fidelity
**High-fidelity.** Final colors, typography, spacing, radii, shadows, and states
are all specified below and encoded in `css/brainharbor.css`. Recreate to match.

## Constraints this design satisfies (keep them when you implement)
1. Body text ≥ 18px (html font-size 112.5% = 18px base); smallest text ≥ 16px.
   Reading measure ≤ 65ch (`--measure`); article body ≤ 46rem (`--measure-read`).
2. WCAG AA everywhere; AAA (≥7:1) for body text (ratios below).
3. Visible focus (`:focus-visible` 3px outline) — never remove it. Links are
   always underlined (color is never the only cue).
4. Tap targets ≥ 44px (`--tap-target: 2.75rem`) on nav, buttons, inputs, doors.
5. Calm palette — no red/alarm colors. No aggressive animation;
   `prefers-reduced-motion` respected.
6. No hype aesthetics, no stock photography.
7. Plain semantic HTML + CSS; system fonts only (no CDN fonts).
8. Print-friendly (`@media print` hides chrome, flattens cards, darkens links).

---

## Screens / views

### 1. Home page (`index.html`) — "Entry Hub" layout
**Purpose:** orient a visitor immediately — answer "where do I go?" before the
feed. Serves the scared newcomer and the returning regular equally.

**Layout (top → bottom):**
- Helpline band (full-width, dark `--color-band`).
- Header: site name left, nav right (flex, wraps on mobile). "Get Help Now" is a
  filled accent pill.
- `main.container` (max 72rem, centered).
  - **Hub** (`.hub`): a rounded `--color-surface` panel, 20px radius, `--space-6`
    padding. Contains the H1, a lede paragraph (≤62ch), and a **three-door grid**
    (`.hub__doors`, `grid-template-columns: repeat(auto-fit, minmax(230px,1fr))`
    — three across on desktop, stacks on mobile).
    - Door 1 `.door.door--primary` (filled accent): "Just diagnosed? Start here."
    - Door 2 `.door`: "Browse all research".
    - Door 3 `.door`: "Talk to someone now" (surfaces the helpline number).
    - Each door is a full `<a>` (whole card clickable), with a bold title, a
      muted sub-line, and a `.door__cta` link cue pinned to the bottom.
  - **Section head** (`.section-head`): "Latest updates" H2 + "See all →" link,
    then a one-line explainer of the badge marks.
  - **Feed grid** (`ul.feed-grid`, `minmax(300px,1fr)` auto-fit — two-up on
    desktop, one column on mobile) of `.card`s.
  - "See all research updates →" link.
  - **Digest signup** (`.digest-signup`): email + submit, inline flex form.
- Footer: link list + not-medical-advice note + the AI-transparency line.

### 2. Research item page (`research-item.html`)
**Purpose:** a calm, single-column read of one summary. Deliberately kept as a
plain reading flow (not a multi-column layout).

**Layout:** same band/header/footer shell; `main.container.container--read`
(max 46rem). Order: back-link → stage badge → H1 → original-title (muted) →
"The short version" bulleted list (with glossary `.term` buttons) → **means
block** → "How early is this?" (badge + one paragraph) → provenance/AI note.

---

## Signature components

### Stage badge — evidence dot-meter (the core trust device)
Trust is read from **filled marks + words**, never color alone. One color per
theme; meaning comes from how many marks are filled.

- `.badge.badge--result` → `.badge__meter` of five `.step` dots; the first N get
  `.step--on`. Evidence strength N/5:
  - Tested in people — 5 · Review of existing research — 4 ·
    Early research (animals) — 2 · Early research (lab cells) — 1
- `.badge.badge--progress` → a `.badge__glyph` "→" (square). Use for
  **New or updated trial** (not a finding yet).
- `.badge.badge--info` → a `.badge__glyph` "i" (circle). Use for **News**.
- `.badge.badge--unverified` → dashed `.step` meter. Use for
  **Preprint — not yet checked by other scientists**.
- Accessibility: the visual meter is `aria-hidden`; the whole badge is
  `role="img"` with an `aria-label` that states the label and, for results,
  "Evidence strength N of 5." Implement this label server-side per stage.

### Helpline band (`.helpline-band`)
Persistent, on every page, top of the document. Dark `--color-band` with white
text (11:1). The phone number is `font-weight:800`, an `<a href="tel:…">`, and a
44px tap target. Findable in one second without dominating the page.

### Feed card (`.card`)
Fixed anatomy: badge → title (`h3 > a`) → one-sentence `.card__hook` →
`.tag`s + date · source in `.card__meta` (pinned to bottom via `margin-top:auto`
so cards in a row align). White, 16px radius, soft shadow.

### "What this means — and doesn't mean" block (`.means-block`)
The anti-hype block. Tinted `#eef6f8` panel, 6px accent left border, a circled
"i" mark, and a two-part body ("What it means" / "What it doesn't mean"). Visually
distinct from the summary body — a calm, honest aside.

### "Just diagnosed? Start here" (`.door--primary`)
The prominent first door in the hub — filled accent, clearly the primary path,
but calm (no alarm color, no oversized shouting).

### AI-transparency line (`.ai-note`)
"Summaries are drafted with AI assistance and reviewed by a person…" — given a
3px accent left border in the footer and on item pages so it reads as a
deliberate trust cue, not fine print.

---

## Design tokens (all in `css/brainharbor.css` `:root`)

### Color — contrast on `--color-paper` (#ffffff)
| Token | Hex | Use | Contrast |
|---|---|---|---|
| `--color-paper` | `#ffffff` | page bg | — |
| `--color-surface` | `#eef3f7` | hub, tags, header band | — |
| `--color-ink` | `#16202b` | body text | **15.6:1 (AAA)** |
| `--color-ink-muted` | `#455263` | secondary text | **7.6:1 (AAA)** |
| `--color-accent` | `#0d6a86` | links, buttons | 4.9:1 (AA, underlined) |
| `--color-accent-strong` | `#094f65` | hover | 6.9:1 |
| `--color-border` | `#5b6a78` | input borders | 4.5:1 |
| `--color-border-subtle` | `#dbe3ea` | hairlines | decorative |
| `--color-band` | `#0d3b4a` | helpline + dark panels | white text 11:1 |
| `--badge-fill` | `#0d6a86` | filled meter dots | — |
| `--badge-empty` | `#bcd3db` | empty meter dots | — |

### Type
`--font-body` / `--font-head`: `system-ui, -apple-system, "Segoe UI", Roboto,
Helvetica, Arial, sans-serif`. Scale (1.2 modular): `--text-sm .889rem`,
`--text-base 1rem`, `--text-lg 1.2rem`, `--text-xl 1.44rem`, `--text-2xl
1.728rem`, `--text-3xl 2.074rem`. `--leading 1.6`, `--leading-tight 1.25`.
Sans-serif and screen-optimised = fastest to scan for impaired readers; 18px
base with generous line-height.

### Spacing / layout / shape
`--space-1…8` (.25→4rem). `--measure 65ch`, `--measure-read 46rem`,
`--container 72rem`, `--tap-target 2.75rem`, `--focus-ring 3px`.
`--radius 14px`, `--card-radius 16px`, `--card-border 1px solid #e6ecf1`,
`--card-shadow 0 3px 14px rgba(20,45,65,.07)`.

> To adopt this theme in the existing site, change the `--color-*` values above
> and add `--radius/--card-*/--badge-*`; the `--text-*` and `--space-*` scales
> are unchanged from the current baseline.

---

## Interactions & behavior
- Links/buttons: `:hover` → `--color-accent-strong`; `:focus-visible` → 3px
  accent outline (keep on every interactive element).
- Doors and cards: whole element is a link; no JS needed.
- Digest form: standard POST (or htmx `hx-post`) to a subscribe endpoint.
- Glossary `.term` buttons: currently affordance-only (dotted underline, help
  cursor). Wire to the existing glossary tooltip/popover (WI-105) — keep them as
  `<button>` for keyboard access.
- "See all" links / pagination: fine to drive with htmx.
- No animations beyond default; honor `prefers-reduced-motion`.

## Responsive behavior
- Hub doors: `auto-fit minmax(230px,1fr)` → 3-up desktop, 1-up mobile.
- Feed grid: `auto-fit minmax(300px,1fr)` → 2-up desktop, 1-up mobile.
- Header nav wraps under the logo on narrow screens; tap targets stay ≥44px.

## State / data
Render each feed/item stage from a `stage` enum → badge kind + rank:
`human→result/5`, `review→result/4`, `animals→result/2`, `cells→result/1`,
`trial→progress`, `news→info`, `preprint→unverified`. Build the `aria-label`
server-side from the stage.

## Assets
None. No images, icons fonts, or stock photography — all UI is HTML + CSS
(the badge marks, the circled "i", the "→" are plain elements/characters).

## Files in this bundle
- `index.html` — home page, Entry Hub layout (reference markup).
- `research-item.html` — research item page (reference markup).
- `css/brainharbor.css` — full class-based, token-driven stylesheet.

### Live prototypes in the wider project (visual source of truth)
- `BrainHarbor - Entry Hub.dc.html` — the built-out chosen design.
- `BrainHarbor Directions.dc.html` — all 5 explored color directions.
- `BrainHarbor Layouts.dc.html` — all 5 explored home layouts.
- `brainharbor-clear-and-kind.css` — the token-only diff block.
