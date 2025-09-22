# TODO

A lightweight backlog for the Playwright NTLM wizard automation. Keep the current baseline stable; only pick items that won’t jeopardize the working flow.

---

## ✅ Baseline (already working)
- [x] NTLM-enabled Chromium launch
- [x] Wizard navigation + schema-driven filler
- [x] Select2 support (container open + search + pick)
- [x] AJAX-aware decorator (wait for response/network idle)
- [x] DOM-order extraction with labels
- [x] Console logging
- [x] CSV-based exclusions via LINQ (local customization)

---

## Near-term (low risk)
- [ ] **Add `TODO.md` to repo** (this file) to track work
- [ ] **Document current CSV exclusions** format in `README.md` (sample file + where it’s loaded)
- [ ] **Guard hidden/absent fields at runtime**: `page.IsVisibleAsync(selector)` -> skip if false  
      _Benefit:_ fewer flaky interactions with conditionally hidden fields
- [ ] **Add Playwright tracing toggle** (per run): `context.Tracing.StartAsync()` / `context.Tracing.StopAsync()` and save `trace.zip` to `artifacts/`
- [ ] **Add `--headless` CLI arg** support (Program.cs) while leaving default = headed

---

## Logging (optional; keep console as default)
- [ ] Add file logger provider (e.g., `Serilog.Extensions.Logging.File`)
  - [ ] `logs/playwright-demo.log` rolling file
  - [ ] README snippet for enabling/disabling
- [ ] Add minimal **structured logs** around:
  - [ ] Wizard step transitions
  - [ ] Per-field handler results (success/skip/exception)
  - [ ] Response wait conditions (AJAX, network idle, selector)

---

## Exclusions / Form logic
- [ ] **(When ready)** Replace ad-hoc CSV parsing with `CsvHelper` for robustness
  - [ ] Support columns: `Id`, `Name`, `Step` (`All` for global)
  - [ ] Unit test: ensure rows are applied only on the right step
- [ ] **Extractor hinting**: mark `IsHidden` during extraction (type=`hidden`, `aria-hidden`, common hidden classes)  
  _Benefit:_ keep CSV small by auto-excluding obvious hidden fields
- [ ] Add **per-step** exclusion files (optional):
  - [ ] `exclusions.step1.csv`, `exclusions.step2.csv`, …

---

## WizardSchemaExtractor enhancements
- [ ] Capture **page URL** and **step index** in `PageSchema`
- [ ] Record **button** elements with `IsAjax` + `AjaxHintUrl` when detectable
- [ ] Optional: write **a compact summary** alongside JSON (for quick reviews)
- [ ] Add retries/backoff for transient page load issues

---

## Field handlers (SOLID)
- [ ] **Select2 edge cases**:
  - [ ] Support tagging mode (free text)
  - [ ] Handle multi-select (select several by default or CSV-driven)
- [ ] **Checkbox groups**: opt-in to selecting multiple by name/id prefix
- [ ] **Date/time inputs**: configurable default (today, +7 days, etc.)
- [ ] **Numeric inputs**: min/max detection (from DOM attributes) to avoid invalid values

---

## Stability & Speed
- [ ] Centralize **timeouts** in config (e.g., `ajaxWaitMs`, `networkIdleMs`)
- [ ] Introduce **smart waits** for known dynamic panels (configurable selectors)
- [ ] Optional: **parallelize** independent suites (multiple browsers/contexts)
- [ ] Add **global error screenshots** on failure to `artifacts/`

---

## Configuration & Secrets
- [ ] Document **User Secrets** usage (what keys override what)
- [ ] Add schema validation on config load (fail fast on missing keys)
- [ ] Optional: environment variables override (CI-friendly)

---

## CI/CD (optional)
- [ ] GitHub Actions / Azure DevOps pipeline
  - [ ] `dotnet restore/build/test`
  - [ ] Playwright install (CLI) + shared browser cache
  - [ ] Publish artifacts (logs, traces, screenshots)
- [ ] Matrix for headless/headed (headed runs optional)

---

## DX & Docs
- [ ] Expand `README.md` with:
  - [ ] Quick “first run” checklist
  - [ ] CSV exclusions example
  - [ ] Troubleshooting (NTLM prompts, blocked scripts)
- [ ] Add a minimal **SmokeTest** schema (`WizardForms/Step1.json.example`) to verify filler without extractor

---

## Nice-to-have (later)
- [ ] Map field **labels → values** via config so you can set explicit non-defaults per field
- [ ] Export a **run report** (HTML/Markdown) summarizing each step, fields touched, and any skips
- [ ] Cross-browser smoke (Chromium + WebKit) when NTLM constraints allow
- [ ] Support **Kerberos on Linux** (if environment expands beyond Windows)

---

## Done / Notes
- Keep the working baseline stable. Prefer small, isolated PRs.
- Tag tasks with risk: _low/med/high_; default to low-risk first.