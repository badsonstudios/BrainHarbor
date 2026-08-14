# Brain Harbor — logo kit

Vector rebuild of the supplied artwork, split into the two pieces requested:
a circular **lighthouse mark** and a **wordmark**, plus a combined lockup.

## Note on the source artwork
In the uploaded raster the lighthouse replaces the letter "i" in "Brain", so the
name reads as "Braın Harbor". That hurts legibility (and screen readers read
the word wrong). In this kit the wordmark spells **Brain Harbor** in full and the
lighthouse lives in its own mark, which is also what makes the two pieces
independently usable.

## Files

### svg/ (use these on the web)
| File | Use |
|---|---|
| `lockup-horizontal.svg` | Primary lockup, with tagline |
| `lockup-horizontal-dark-bg.svg` | Same, for dark backgrounds |
| `lockup-no-tagline.svg` | Site header |
| `mark-color.svg` | Circular lighthouse mark |
| `mark-color-dark-bg.svg` | Mark for dark backgrounds |
| `mark-mono-navy.svg` / `mark-mono-white.svg` | One-color (print, merch, stamps); beams removed |
| `wordmark-color.svg` | Name + wave + tagline |
| `wordmark-dark-bg.svg` | Dark-background version |
| `wordmark-no-tagline.svg` | Name + wave only |
| `wordmark-mono-navy.svg` / `wordmark-mono-white.svg` | One-color |
| `favicon.svg` | Rounded browser favicon; simplified (no beams, thicker forms) for small sizes |
| `app-icon-square.svg` | Square **opaque** icon art for home-screen / PWA icons (no rounding — iOS and Android apply their own mask) |

All SVGs have transparent backgrounds except `favicon.svg` (intentional navy
rounded square) and `*-dark-bg` files (transparent, drawn in light inks).

### png/ (email, social, anywhere SVG is not supported)
`lockup-1600.png`, `lockup-800.png`, `lockup-dark-bg-1600.png`,
`wordmark-color-1200.png`, `wordmark-dark-bg-1200.png`,
`mark-color-512/256/64.png`, `mark-white-512.png` — transparent unless named
`-dark-bg`.

### App icons
`apple-touch-icon-180.png`, `icon-192.png`, `icon-512.png` — **fully opaque,
square, full-bleed navy**, rasterized from `app-icon-square.svg`. This is
deliberate: iOS renders alpha in an `apple-touch-icon` as *black*, so a rounded
transparent icon shows black wedges in the corners, and Android maskable icons
expect opaque full-bleed art. Do not add rounding — both platforms mask for you.

`favicon-32.png` stays rounded with transparency (browsers handle alpha
correctly there), as does `favicon.svg`.

`site.webmanifest` is ready to drop at the web root (paths assume `/img/brand/`).

## The waterline
The teal waterline under the name rises past the end of "Harbor" and **curls over
into a breaking wave crest**. It is a single tapered filled path (not a stroke),
so it stays crisp at any size; a thinner back-wave stroke sits behind it for depth.
The crest is the rightmost ink in the wordmark — when you measure clear space or
set a container width, measure to the crest, not to the "r".

## Palette
| Hex | Role |
|---|---|
| `#14294d` | Brand navy — tower, ring, wordmark |
| `#0d6a86` | Harbor teal — water, lantern glass. Same value as the site's `--color-accent` |
| `#0e5f78` | Deep teal — back wave |
| `#f2d492` | Beacon gold — light beams only. Never used for text (too low contrast) |

## Rules
- **Clear space:** the cap-height of the "B" on all sides.
- **Minimum sizes:** mark 24px (use `favicon.svg` below 32px); lockup with
  tagline 320px wide; lockup without tagline 180px.
- **Don't** recolor outside these files, stretch, rotate, add shadows/outlines,
  place the color mark on a mid-tone photo, or substitute the lighthouse for a letter.
- **Alt text:** `alt="Brain Harbor"` for the header logo (never "logo");
  `alt=""` for decorative repeats. The tagline inside the artwork is not text —
  repeat it as real text nearby if it matters for SEO/AT.

## Markup

```html
<a class="site-name" href="/">
  <img src="/img/brand/lockup-no-tagline.svg" alt="Brain Harbor" height="44" />
</a>

<link rel="icon" href="/img/brand/favicon.svg" type="image/svg+xml" />
<link rel="icon" href="/img/brand/favicon-32.png" sizes="32x32" />
<link rel="apple-touch-icon" href="/img/brand/apple-touch-icon-180.png" />
<link rel="manifest" href="/site.webmanifest" />
<meta property="og:image" content="https://brainharbor.org/img/brand/lockup-1600.png" />

<!-- automatic dark mode, no JS -->
<picture>
  <source srcset="/img/brand/lockup-horizontal-dark-bg.svg" media="(prefers-color-scheme: dark)" />
  <img src="/img/brand/lockup-horizontal.svg" alt="Brain Harbor" height="56" />
</picture>
```

## Why the wordmark is live text (and why that's now safe)
The name is a real `<text>` element in a Georgia/serif stack, not outlined paths.
Two reasons that's the better choice here:

1. **Outlining would need the actual font file embedded**, which Georgia's licence
   does not permit for redistribution. The honest alternatives are hand-drawn
   letterforms (lower quality) or a licensed webfont download on every page
   (slower, and the site is deliberately system-fonts-only).
2. **It stays selectable, searchable, and scalable**, and recolors from CSS.

The real risk with live text is *reflow* — a different serif changing the logo's
width and breaking the header layout. That is closed off: both text elements
carry `textLength` + `lengthAdjust="spacingAndGlyphs"`, so the wordmark occupies
**exactly the same width in any serif**. On a machine with no Georgia (some Linux
and Android builds) the letterforms differ very slightly; the size, spacing, and
layout do not.

**Rule of thumb:** use the SVG everywhere you control the page. Use the PNG for
email, social cards, and print — contexts where the renderer isn't yours. If you
later license a display serif for the brand, send it over and the wordmark can be
reset in it and outlined properly.
