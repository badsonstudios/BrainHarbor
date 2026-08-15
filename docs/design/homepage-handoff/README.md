# Handoff: BrainHarbor homepage — "Harbor Banner" (6a)

## What this is
A redesign of the **brainharbor.org homepage**, plus the matching **research item
page**, in the site's existing "Clear & Kind" visual language. It was chosen from
a series of explored options; this folder contains only the approved one.

The design solves three specific problems observed on the live site:

1. **Updates were buried.** Three stacked explanation blocks (hero paragraph →
   three doors → full-width "AI can make mistakes" panel) pushed the first update
   roughly a screen and a half down. Now: one hero band, two doors, then updates.
2. **The feed was too short and "See all" too quiet.** Now **eight updates**, and
   "See all" is a filled button at the top of the section plus a large outlined
   button below the feed.
3. **The evidence badge read "6 of 10 ready."** Ten steps is too fine a
   distinction to feel, and a fraction invites the reading "my treatment is 60%
   done." It is now **four marks with a plain-word label**.

## Fidelity
**High-fidelity.** Colors, type, spacing, radii, shadows, and states are all
specified and encoded in `css/brainharbor.css`. Recreate to match.

## These HTML files are reference, not a drop-in app
They're plain semantic HTML so the markup and CSS map onto Razor views directly.
Recreate them as views/partials rendering **real data** — don't copy them into
`wwwroot` as static pages.

---

## Files

```
design_handoff_brainharbor_homepage/
├─ START-HERE.md          ← the prompt to paste into Claude Code
├─ README.md              ← this file: the design spec
├─ index.html             ← homepage, "Harbor Banner" layout
├─ research-item.html     ← research item page
├─ css/brainharbor.css    ← complete class-based, token-driven stylesheet
└─ brand/                 ← logo kit (svg, png, app icons, webmanifest) + its own README
```

---

## Homepage anatomy (top → bottom)

1. **Skip link** → `#main-content`.
2. **Helpline band** — full-width dark `--color-band`, phone number at
   `font-weight:800` as a `tel:` link. Persistent on every page.
3. **Header** — logo left; nav right (Research, Trials, Digest, Search,
   "Get Help Now" as a filled pill, and the "Aa Larger text" toggle).
4. **Hero band** (`.hero-band`) — the one branded moment on the page:
   - deep navy, full-bleed;
   - the **lockup with tagline** reversed out, which *is* the `<h1>`
     (see "Heading structure" below);
   - the lighthouse mark repeated large at 7% opacity as a watermark;
   - the **two doors** inside the band;
   - a **wave edge** SVG where the band meets the white page.
5. **`<main id="main-content">`**
   - **Section head** — the lighthouse mark (46px) beside the "Latest updates"
     `<h2>`, with "See all updates →" as a filled button on the right;
   - the logo's **wave, reused as a rule** under the heading;
   - one line: "More marks mean the finding has been tested more.";
   - **feed grid** — 8 `.card`s, `auto-fit minmax(320px,1fr)` (two-up desktop,
     one column mobile);
   - large outlined **"See all research updates →"** button, centered;
   - **AI notice** — amber-tinted panel, last thing before the footer.
6. **Footer** — links, not-medical-advice text, AI-transparency note.

### Why branding sits where it does
The brand appears in three places and no more: the header logo, the hero band,
and the section heading. All three occupy space that was otherwise empty. The
mark is deliberately **not** repeated on feed cards — it would be identical on
every card (so it can't distinguish them) and would compete with the evidence
badge, which is the one element on a card that genuinely differs and that people
need to read. On the section heading it earns its place as a "new section starts
here" wayfinding cue.

---

## Signature components

### Stage badge — the evidence meter (the core trust device)
Trust is read from **filled marks + words**, never color alone.

| Stage | Class | Marks |
|---|---|---|
| Tested in people | `.badge--result` | 4 of 4 |
| Review of existing research | `.badge--result` | 3 of 4 |
| Early research (animals) | `.badge--result` | 2 of 4 |
| Early research (lab cells) | `.badge--result` | 1 of 4 |
| New or updated trial | `.badge--progress` | glyph `→` (not a finding yet) |
| News | `.badge--info` | glyph `i` |
| Preprint — not yet checked by other scientists | `.badge--unverified` | dashed meter |

> **Note on the ladder:** this is a clean 4-rung scale with no gaps. Build the
> badge from a `stage` enum server-side — do not let content authors pick an
> arbitrary number.

Accessibility: the meter is `aria-hidden`; the badge is `role="img"` with an
`aria-label` built server-side, e.g.
`"Tested in people. Evidence strength 4 of 4."` — glyph badges just get
`"New or updated trial."`

### Two doors (`.doors`, `.door`)
Only two, deliberately. Three equal choices is three decisions before anyone has
read anything; "browse all research" was cut because the feed underneath already
does that job. Each door is one full `<a>` with a large circular icon, a bold
title, and a supporting line.

### Feed card (`.card`)
Fixed anatomy: badge → title (`h3 > a`) → one-sentence `.card__hook` →
`.card__meta` with topic `.tag` chips + date · source. `.card__meta` uses
`margin-block-start:auto` so meta lines up across a row. **Keep the tag chips** —
tumor type ("Glioblastoma", "Brain metastases") is how a patient finds the item
relevant to them.

### Hero band, wave edge, wave rule
The wave edge and wave rule are inline SVGs lifted from the logo's waterline.
Both are `aria-hidden="true" focusable="false"`. The wave edge uses
`preserveAspectRatio="none"` so it stretches to any width.

### AI notice (`.ai-notice`)
Amber-tinted, at the **bottom** of the page rather than between the doors and the
feed. It stays honest and findable without standing between the reader and the
content. Note the site publishes AI summaries automatically after safety checks —
the copy says exactly that; don't soften it to "reviewed by a person."

---

## Heading structure (please preserve exactly)
The hero lockup **is** the `<h1>`:

```html
<h1>
  <span class="visually-hidden">Real brain tumor research, in plain language.</span>
  <img src="…/lockup-horizontal-dark-bg.svg" alt="" aria-hidden="true" />
</h1>
```

Alt text on an image is *not* a heading — it doesn't appear in a screen reader's
heading list, and heading navigation is a primary strategy for this audience. The
image is marked decorative because the header logo already announces
"Brain Harbor"; without that you get the brand name twice in a row.

Heading order on the homepage: `h1` (hero) → `h2` "Latest updates" → `h3` per
card → `h2` "AI writes these summaries". Don't skip levels.

---

## Design tokens (`css/brainharbor.css` `:root`)

### Color — contrast on `--color-paper` (#ffffff)
| Token | Hex | Use | Contrast |
|---|---|---|---|
| `--color-paper` | `#ffffff` | page bg | — |
| `--color-surface` | `#eef3f7` | header, tags, panels | — |
| `--color-ink` | `#16202b` | body text | **15.6:1 (AAA)** |
| `--color-ink-muted` | `#455263` | secondary text | **7.6:1 (AAA)** |
| `--color-accent` | `#0d6a86` | links, buttons, filled marks | 4.9:1 (AA, underlined) |
| `--color-accent-strong` | `#094f65` | hover | 6.9:1 |
| `--color-border` | `#5b6a78` | input borders | 4.5:1 |
| `--color-border-subtle` | `#dbe3ea` | hairlines | decorative |
| `--color-band` | `#0d3b4a` | helpline + hero band | white text 11:1 |
| `--color-notice-bg` | `#fff6e0` | AI notice bg | — |
| `--color-notice-edge` | `#8a6a12` | AI notice edge | 5.9:1 on notice bg |
| `--badge-empty` | `#bcd3db` | unfilled marks | decorative |

The logo's teal is the same `#0d6a86`, so the mark cannot clash with the theme.

### Type
`--font-body` / `--font-head`: `system-ui, -apple-system, "Segoe UI", Roboto,
Helvetica, Arial, sans-serif`. Scale (1.2 modular): `--text-sm .889rem`,
`--text-base 1rem`, `--text-lg 1.2rem`, `--text-xl 1.44rem`,
`--text-2xl 1.728rem`, `--text-3xl 2.074rem`. `--leading 1.6`,
`--leading-tight 1.25`. `html { font-size: 112.5% }` = 18px base.

### Spacing / layout / shape
`--space-1…8` (.25→4rem) · `--measure 65ch` · `--measure-read 46rem` ·
`--container 72rem` · `--tap-target 2.75rem` · `--focus-ring 3px` ·
`--radius 14px` · `--radius-lg 18px` · `--card-radius 16px` ·
`--card-border 1px solid #e6ecf1` · `--card-shadow 0 3px 14px rgba(20,45,65,.07)`.

> `--container` sizes the header, main, **and footer**. If you introduce a
> separate token for the top bar, make sure the footer uses the same value or it
> will visibly fail to line up.

> **Watch the global `p` rule.** `p, ul, ol` are capped at `--measure` (65ch) for
> readability. Any *full-width* block you wrap in a `<p>` — the centered "See all
> research updates" button is the one here — needs `max-width: none`, or it will
> lay out inside a 630px box and look accidentally left-shifted. Prefer a `<div>`
> for full-width blocks when you rebuild these as Razor views.

---

## Hard constraints — do not compromise
1. Body text ≥ 18px; smallest text ≥ 16px.
2. WCAG AA minimum; AAA (7:1) for body text. The specified hex values already
   satisfy this — don't "adjust" them.
3. Links always underlined; color is never the only cue.
4. Visible `:focus-visible` outline on every interactive element; never remove it.
5. Tap targets ≥ 44px (`--tap-target`).
6. No CSS or JS framework. Plain semantic HTML + CSS; system fonts only.
7. No animation beyond what's here; honor `prefers-reduced-motion`.
8. Print-friendly — patients print these pages for appointments. `@media print`
   hides chrome and the decorative waves and flattens cards.

## Responsive behavior
- Doors: `auto-fit minmax(300px,1fr)` → two-up desktop, stacked mobile.
- Feed: `auto-fit minmax(320px,1fr)` → two-up desktop, one column mobile.
- Header nav wraps under the logo on narrow screens; tap targets stay ≥ 44px.
- Hero lockup is `width:640px; max-width:100%` so it scales down cleanly.

## State / data
Render the feed from the real data source. Map the `stage` enum → badge kind +
rank using the table above, and build each `aria-label` server-side.

## Assets
Logo kit is in `brand/` — see `brand/README.md` for the full spec. Copy
`brand/svg/` and `brand/png/` to `wwwroot/img/brand/` and
`brand/site.webmanifest` to the web root. App icons
(`apple-touch-icon-180.png`, `icon-192/512.png`) are **intentionally opaque** —
iOS renders alpha in a touch icon as black.

No other images or icon fonts: the badge marks, the circled "i", the "→", and
the waves are all plain elements or inline SVG.
