# Figures on curated pages

Images shown inside the body of a curated Markdown page live here (WI-561).
One file per figure, committed to the repo — a curated page may not point an
`<img>` at somebody else's server.

**Which pictures are wanted, and where each one goes, is
[`docs/images-needed.md`](../../../../../docs/images-needed.md) (WI-562).** Every
figure there has an id, and the file lands here named for it — `dia-grade-ladder`
becomes `dia-grade-ladder.svg`. That file also carries the draft alt text and
caption for each one, already graded at the 6.0 limit below, so the page edit is a
paste rather than a fresh piece of writing.

## The location family is ONE drawing (WI-572)

`dia-brain-regions.svg` is the master brain map. The thirteen
`dia-region-*.svg` variants and `dia-region-names.svg` are **derived from it**:
the same bytes with a banner, a rewritten `aria-label`, and one `<style>` block
that shades a region and hides every label but its own.

**Do not edit a derived file** — the edit is lost and the build goes red.
Change `dia-brain-regions.svg` and regenerate:

```bash
BH_WRITE_REGION_MAPS=1 dotnet test tests/BrainHarbor.Tests \
  --filter FullyQualifiedName~BrainRegionFamilyTests.WritesTheFamilyOnlyWhenAskedTo
```

Then run the suite **with the variable unset**: the two drift tests refuse to
run while it is set, because a run that may have rewritten the files it is
checking certifies nothing. `BrainRegionFamilyTests` rebuilds all fourteen and
fails if a committed file differs (line endings aside), and asserts separately
that the drawing itself is identical in all fifteen. Two more rules that bite on this master in particular:

- **No two hyphens in a row inside any comment in an SVG.** It is illegal in
  XML, the browser renders a broken-image icon, and in this family one mistake
  in the master breaks all fifteen files at once (content-pipeline §12.35).
- **A label drawn on a figure is a claim.** Every word the picture prints has
  to be findable in the text of the page that shows it, or on a page that
  section routes to; `FigureWordsTests` holds that over the whole corpus.

Every file in this directory must be declared by the page that shows it, in
that page's front matter:

```yaml
images:
  - src: /img/figures/pathology-report-parts.svg
    credit: "Made for BrainHarbor"
    license: "CC0 1.0 (public domain)"
    license_url: https://creativecommons.org/publicdomain/zero/1.0/
    source_url: https://example.org/where-it-came-from   # omit for our own work
    width: 1200
    height: 675
```

and shown with one line of Markdown, alone in its own paragraph:

```markdown
![What is in the picture.](/img/figures/pathology-report-parts.svg "What it means for you.")
```

`FigureTests` holds this directory to that in **both directions**: a declared
image that is not here fails, and a file here that no page declares fails. An
uncredited picture is a licensing problem that renders perfectly, so it is
never allowed to render.

Rules that are checked for you and will fail the build:

- **Alt text is required** (WCAG AA). An empty `alt=""` is allowed only with
  `decorative: true` in the entry above, so it is a decision rather than an
  omission.
- **`credit`, `license`, `license_url`, `width` and `height` are required.**
- **Never an NCI embedded image**, and nothing shipped with the AHFS or
  MedlinePlus drug monographs — they are licensed stock (PLAN.md §5). Their
  text is still citable; their pictures are not.
- **No AI-generated imagery**, consistent with the standing rule on feed cards.
  That one is a human rule: nothing can check it for you.

The feed-card backdrops are a different thing entirely and live in
`../cards/`, credited in `../cards/IMAGE-CREDITS.md`.
