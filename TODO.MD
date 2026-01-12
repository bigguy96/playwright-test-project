# TODO

A lightweight backlog for the Playwright NTLM wizard automation.  
Keep the current baseline stable; only pick items that won’t jeopardize the working flow.

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

## Near-term (low-risk, high value)
- [ ] **Runtime visibility guard** in `FormAutoFiller`  
  Skip conditionally hidden or non-visible fields with `page.IsVisibleAsync()`.  
  ✅ Done when “not visible” errors no longer appear.

- [ ] **Tracing toggle (opt-in)**  
  Add CLI arg/env var to enable Playwright tracing (`trace.zip` in `artifacts/`).  
  ✅ Done when tracing can be turned on/off without code changes.

- [ ] **README: CSV exclusions spec**  
  Document CSV format and how it’s applied.  
  ✅ Done when a teammate can add a row and see it take effect.

- [ ] **Example schema** (`WizardForms/Step1.json.example`)  
  Minimal smoke-test schema to verify filler without running extractor.  
  ✅ Done when `PlaywrightNtlmDemo` runs successfully against it.

---

## Config ergonomics (still low-risk)
- [ ] **Headless mode flag**  
  Support `--headless true|false` via args/env; default to current (headed).  
  ✅ Done when headless can be flipped without code edits.

- [ ] **Centralized timeouts**  
  Move AJAX/network idle/selector timeouts into config.  
  ✅ Done when adjusting config changes waits at runtime.

---

## Extraction hints (medium-risk)
- [ ] **Mark `IsHidden` during extraction**  
  Flag obvious hidden fields (`type=hidden`, `aria-hidden`, hidden classes).  
  ✅ Done when JSON has `IsHidden=true` and filler skips automatically.

---

## Logging (optional, default-off)
- [ ] **File logging provider** (e.g., Serilog file sink)  
  Write logs to `logs/playwright-demo.log`.  
  ✅ Done when enabling a flag creates a rolling log file.

- [ ] **Structured logs**  
  Log step transitions, per-field results, and AJAX waits.  
  ✅ Done when logs provide a clear replay of test actions.

---

## Field handler enhancements (medium)
- [ ] **Select2 multi-select**  
  Detect and select multiple values (configurable).  
  ✅ Done when multi-selects are handled without manual edits.

- [ ] **Date/time & number inputs**  
  Respect min/max/step and provide sane defaults (today, +1).  
  ✅ Done when validation errors disappear for these fields.

---

## Exclusion system evolution (later)
- [ ] **CSV → schema annotation**  
  Apply exclusions directly in schema (`Exclude=true`).  
  ✅ Done when filler only reads JSON for skips.

- [ ] **Per-step CSVs**  
  Support `exclusions.step1.csv`, `exclusions.step2.csv`, etc.  
  ✅ Done when step-specific ignores work without code changes.

---

## Stability & CI (later)
- [ ] **Artifacts on failure**  
  On exceptions, save screenshot + trace to `artifacts/`.  
  ✅ Done when every failure produces artifacts.

- [ ] **CI pipeline**  
  Add GitHub Actions/Azure DevOps workflow: restore, build, smoke-test, upload artifacts.  
  ✅ Done when smoke test runs automatically on push.

---

## Nice-to-have (longer-term)
- [ ] Map field **labels → values** via config (explicit defaults)
- [ ] Export **run report** (HTML/Markdown) with summary of each step
- [ ] Cross-browser smoke (Chromium + WebKit, if NTLM allows)
- [ ] Support Kerberos on Linux (if environment expands beyond Windows)
