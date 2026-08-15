# START HERE — BrainHarbor homepage redesign

This bundle contains a finished redesign of the **brainharbor.org homepage**
(plus the matching research item page), ready to implement in the existing
Razor Pages + htmx site.

Two steps:

1. Unzip this folder where your Claude Code session can read it — easiest is
   inside the repo, e.g. `docs/design-handoff/`.
2. Paste the prompt at the bottom of this file into that session.

---

## What's in the kit

```
design_handoff_brainharbor_homepage/
├─ START-HERE.md          ← you are here (prompt at the bottom)
├─ README.md              ← the full design spec. The important file.
├─ index.html             ← homepage, "Harbor Banner" layout (reference markup)
├─ research-item.html     ← research item page (reference markup)
├─ css/brainharbor.css    ← complete class-based, token-driven stylesheet
└─ brand/                 ← logo kit (SVG, PNG, app icons, webmanifest) + README
```

## What changes, and why

Three problems on the live homepage, and the fix for each:

- **Updates were buried.** Three stacked explanation blocks (hero paragraph →
  three doors → the full-width "AI can make mistakes" panel) pushed the first
  update about a screen and a half down. Now it's one hero band, two doors, then
  updates. The AI notice moves to the bottom of the page — still honest and
  findable, no longer standing between the reader and the content.
- **The feed was short and "See all" was quiet.** Now **eight updates**, with
  "See all" as a filled button at the top of the section and a large outlined
  button below the feed.
- **The badge read "6 of 10 ready."** Ten steps is too fine a distinction to
  feel, and a fraction invites "my treatment is 60% done." It's now **four marks
  with a plain-word label**, on a clean 4-rung ladder with no gaps.

Also: three doors became **two** (three choices is three decisions before anyone
has read anything, and "browse all research" is what the feed already does), and
BrainHarbor branding now appears in the hero band and beside the "Latest updates"
heading — in space that was empty, not in front of content.

## Important
These HTML files are **reference markup to recreate as Razor views**, not
drop-in static pages. The stylesheet, however, is meant to be used essentially
as-is.

---

## Prompt to paste into your Claude Code session

Copy everything between the lines.

---

I have a finished design handoff to implement — a redesign of our homepage. It's
at `docs/design-handoff/` (adjust the path if I put it elsewhere).

Read these first, in this order:
1. `docs/design-handoff/README.md` — the full design spec. Treat it as the source
   of truth for layout, tokens, components, and accessibility constraints.
2. `docs/design-handoff/brand/README.md` — the logo kit spec.
3. `docs/design-handoff/css/brainharbor.css` — the stylesheet to fold in.
4. `docs/design-handoff/index.html` and `research-item.html` — reference markup.

Then, before writing any code, look at how this repo is actually organized: the
layout file, existing partials, view models, the current `site.css` and its
custom properties, how the research feed and item pages get their data, and how
the existing stage/evidence badge is currently implemented and stored. Tell me
your implementation plan and what you found, and flag anything in the design that
conflicts with what already exists. Don't start editing until I confirm.

What I want implemented:

- Fold `brainharbor.css` into the existing site stylesheet. Much of it is the
  same custom-property system already in use — reconcile rather than duplicate
  tokens. Note `--container` sizes the header, main and footer together.
- Update the shared layout: skip link, persistent helpline band, header (logo,
  nav including Search and the "Aa Larger text" toggle), and the footer with the
  not-medical-advice and AI-transparency lines.
- Rebuild the homepage: the navy hero band (lockup as the `<h1>`, lighthouse
  watermark, two doors, wave edge), then the "Latest updates" section with the
  lighthouse mark beside the heading and the wave rule under it, then **eight**
  feed cards, then the "See all research updates" button, then the AI notice.
- Change the feed from 4 to **8 items** on the homepage.
- Make "See all" prominent: a filled button in the section header and a large
  outlined button below the feed.
- **Change the evidence badge from the current 10-step scale to the 4-mark
  scale** in the README's stage table. Build it as a reusable partial/tag helper
  driven by the stage enum, and build the `aria-label` server-side (e.g.
  "Tested in people. Evidence strength 4 of 4."). Don't let content authors pick
  an arbitrary number — it comes from the enum.
- Preserve the heading structure exactly as specified: the hero lockup is the
  `<h1>` with visually-hidden text carrying the heading and the image marked
  decorative. Don't replace the `<h1>` with an image alone.
- Restyle the research item page to match, including the "What this means — and
  doesn't mean" block.
- Copy `brand/svg/` and `brand/png/` into the web assets folder (e.g.
  `wwwroot/img/brand/`) and `brand/site.webmanifest` to the web root. Wire up the
  header logo, favicon, apple-touch-icon, PWA manifest and `og:image` per the
  brand README. The app icons are intentionally opaque — don't "fix" them.
- Keep all real data, routes and endpoints working. Render the feed from the real
  data source, not the sample content in the reference HTML. Keep the topic tag
  chips on cards — tumor type is how a patient finds what's relevant to them.

Hard constraints from the design — do not compromise these:
- Body text ≥ 18px; smallest text ≥ 16px.
- WCAG AA minimum, AAA (7:1) for body text. The specified hex values already
  satisfy this — don't adjust them.
- Links always underlined; color is never the only cue.
- Visible `:focus-visible` outline on every interactive element; never remove it.
- Tap targets ≥ 44px.
- No CSS or JS framework. Plain semantic HTML + CSS; htmx only for what it
  already does. System fonts only — no CDN or webfont downloads.
- No animation beyond what's there; honor `prefers-reduced-motion`.
- Keep it print-friendly — patients print these pages for appointments.

Work incrementally: tokens/layout first, then the homepage, then the badge
component, then the item page, then the brand assets. Show me a diff at each step
rather than one large change.

---
