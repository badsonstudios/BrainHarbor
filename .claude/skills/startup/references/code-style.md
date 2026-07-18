# Code Style — BrainHarbor

Standard modern C# / .NET conventions; nothing exotic.

- **C#:** file-scoped namespaces, nullable reference types **enabled** and
  respected, `var` where the type is obvious, expression-bodied members where
  they aid clarity, async all the way (`Async` suffix, no `.Result`/`.Wait()`).
- **Naming:** PascalCase types/methods/properties, camelCase locals/params,
  `_camelCase` private fields, interfaces `IThing`, one type per file.
- **Razor Pages:** page models thin; real logic in `Services/`. htmx endpoints
  are page handlers returning partials — name them for what they return
  (`OnGetResultsPartial`).
- **SQL (DbUp scripts + Dapper):** lowercase snake_case identifiers matching
  `docs/data-model.md`; parameterized queries always; no string-built SQL.
- **Dapper:** queries live near their use in focused repository/service
  classes; multi-row mapping helpers over dynamic.
- **Error handling:** no swallowed exceptions; Pipeline fetchers isolate
  failures per source (one bad source never kills the run); log with context.
- **Comments:** only for constraints the code can't express (licensing rules,
  medical-content rules, API quirks). No narration.
- **Front-end:** semantic HTML first; CSS in `site.css` with custom properties
  for tokens; no CSS frameworks; htmx attributes over custom JS; any custom JS
  is ES2019-compatible and progressive-enhancement only.
- **User-facing text** (labels, errors, empty states) follows the plain-language
  style guide in `docs/content-pipeline.md` §4 — ≤8th-grade, calm, concrete.
