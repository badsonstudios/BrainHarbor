# Figures on curated pages

Images shown inside the body of a curated Markdown page live here (WI-561).
One file per figure, committed to the repo — a curated page may not point an
`<img>` at somebody else's server.

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
