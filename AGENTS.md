# Claude Code Game Studios — Agent Architecture & Workspace Rules

<!-- caveman-begin -->
Respond terse like smart caveman. All technical substance stay. Only fluff die.

Rules:
- Answer first: Answer, then reason, then next step.
- Kill ceremony: No greeting, hedging, pleasantries, recap, or closer.
- Short word: "fix" not "implement a solution for".
- Articles optional, meaning never: Drop a/an/the when the sentence still reads in one pass.
- One idea per sentence: ASD-STE100 is the floor: 20 words max, active voice, imperative for instructions, one term per thing, pronoun only with an obvious referent.
- Payload verbatim: Code blocks unchanged.
- Tool runs: bounded status: No text between routine calls.
- User's language: Compress the style, not the language.
- Never perform caveman: No "caveman mode on", no "me think", no "Caveman:" prefix, no normal answer plus caveman copy.

Switch: /caveman (default), /ultracave (fragments, each fact once), /megacave (Classical Chinese 文言文)
Stop: "stop caveman" or "normal mode"

Auto-Clarity: plain prose for security warnings, irreversible actions, step order a fragment could scramble, user confused, questions you ask the user. Resume after.

Boundaries: code, comments, commits, PRs, docs written normal; existing comments kept unless asked.
Floor: code, commands, paths, numbers and error strings verbatim; never drop not/never/no/only.
<!-- caveman-end -->

Indie game development managed through 49 coordinated subagents and 74 workflow skills.
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack & References

- **Primary Config**: `project.yaml` at the repo root (engine, specialists, naming, platform, performance, modes). Skills resolve it via `resolve_config` (see `.claude/docs/config-resolution.md`).
- **Project Structure**: See `.claude/docs/directory-structure.md`
- **Engine Version Reference**: `docs/engine-reference/<engine>/VERSION.md` (defaults to `docs/engine-reference/godot/VERSION.md` until `/setup-engine` runs)
- **Coordination Rules**: See `.claude/docs/coordination-rules.md`
- **Coding Standards**: See `.claude/docs/coding-standards.md`
- **Context Management**: Read `.claude/docs/context-management.md` on demand. `production/session-state/active.md` is the session checkpoint.

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools (exempt: `active.md` appends, agent memory, a subagent's new file at the path its orchestrating skill named; see `.claude/rules/agent-memory.md`)
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction
- A skipped or dismissed question is not an answer: act on nothing, re-ask in plain text and wait

See `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md` for full protocol and examples.

> **First session?** If the project has no engine configured and no game concept, invoke the `start` skill to begin the guided onboarding flow.

---

## Path-Scoped Domain Standards

When working in any of the following paths, enforce the corresponding standard (full details and code examples in `.claude/rules/`):

- **Gameplay Code** (`src/gameplay/**`, `Assets/**/Gameplay/**/*.cs`, `Source/**/Gameplay/**/*.{h,cpp}` — see `.claude/rules/gameplay-code.md`):
  - ALL gameplay values MUST come from external config/data files, NEVER hardcoded.
  - Use delta time for ALL time-dependent calculations.
  - NO direct references to UI code — use events/signals/delegates for cross-system communication.
  - Every gameplay system must implement a clear interface; state machines need explicit transition tables; write unit tests separating logic from presentation; no static singletons for game state.
- **Engine / Core Code** (`src/core/**`, `Assets/**/Core/**/*.cs`, `Source/**/Core/**/*.{h,cpp}` — see `.claude/rules/engine-code.md`):
  - ZERO allocations in hot paths (update loops, rendering, physics) — pre-allocate, pool, reuse.
  - Engine code must NEVER depend on gameplay code (`engine <- gameplay`).
  - Profile before AND after every optimization; document thread-safety and public API usage examples; verify APIs against `docs/engine-reference/`.
- **AI Code** (`src/ai/**`, `Assets/**/AI/**/*.cs`, `Source/**/AI/**/*.{h,cpp}` — see `.claude/rules/ai-code.md`):
  - 2ms per frame maximum AI update budget; all parameters tunable from data files; implement visualization hooks and transition logging; telegraph intentions; validate network AI input.
- **UI Code** (`src/ui/**`, `Assets/**/UI/**/*.cs`, `Source/**/UI/**/*.{h,cpp}` — see `.claude/rules/ui-code.md`):
  - UI must NEVER own or directly modify game state (display only, use commands/events).
  - All UI text through localization; support keyboard/mouse AND gamepad; skippable animations respecting motion preferences; mandatory scalable text and colorblind modes.
- **Network Code** (`src/networking/**`, `Assets/**/Networking/**/*.cs`, `Source/**/Networking/**/*.{h,cpp}` — see `.claude/rules/network-code.md`):
  - Server is AUTHORITATIVE for gameplay-critical state; version all messages; client prediction + rollback; validate packet sizes and field ranges.
- **Data Files** (`assets/data/**/*.json`, `Assets/**/Data/**/*.json`, `Content/**/Data/**/*.json` — see `.claude/rules/data-files.md`):
  - Valid JSON only; lowercase `[system]_[name].json` filenames; `snake_case` keys across all engines (use DTO mapping in C#/C++); documented schema and defaults.
- **Scene & Asset Files** (`**/*.tscn`, `**/*.tres`, `**/*.unity`, `**/*.prefab`, `Assets/**/*.asset` — see `.claude/rules/scene-files.md`):
  - Prefer editor changes. In Godot, never change `format=`, reuse `id`, copy `unique_id=`, invent `uid://`, edit TileMapLayer/GridMap data, or delete `.uid` files. In Unity, never hand-edit YAML while the Editor is reachable or touch `.meta` files.
- **Shader Code** (`assets/shaders/**`, `**/*.gdshader`, `Assets/**/*.{shader,hlsl,cginc,compute,shadergraph}`, `Shaders/**`, `**/*.{usf,ush}` — see `.claude/rules/shader-code.md`):
  - Follow `[type]_[category]_[name].[ext]` naming; group uniforms; no magic numbers; avoid dynamic branching or texture reads in loops; document target platform and render pipeline.
- **Design Documents** (`design/gdd/**` — see `.claude/rules/design-docs.md`):
  - Follow `modes.workflow` section requirements (`full`: 8 sections, `standard`: 5 + Formulas for numeric rules, `minimal`: `design/game-brief.md`); bidirectional dependencies; testable acceptance criteria; write incrementally section by section with user approval.
- **Narrative Docs** (`design/narrative/**` — see `.claude/rules/narrative.md`):
  - Cross-reference lore; specify canon level (`Established` / `Provisional` / `Under Review`); localization-ready text; max 120 chars per dialogue line.
- **Prototypes** (`prototypes/**` — see `.claude/rules/prototype-code.md`):
  - Isolated in `prototypes/[name]/` with a `README.md` stating hypothesis, run instructions, status, and findings. Never import prototype code into production.
- **Tests** (`tests/**`, `Assets/Tests/**/*.cs`, `Source/**/Tests/**/*.{h,cpp}` — see `.claude/rules/test-standards.md`):
  - Engine-idiomatic naming (`[system]_[feature]_test.gd`, `[System]Tests.cs`, `F[System][Scenario]Test`); Arrange/Act/Assert structure; watch every regression test fail before fixing.
- **Skill & Gate Authoring & Agent Memory** (`.claude/skills/**`, `.claude/agents/**`, `.claude/agent-memory/**` — see `.claude/rules/skill-authoring.md` and `.claude/rules/agent-memory.md`):
  - Include `NOT ASSESSED` in gating verdicts; announce skipped steps; never record temporary project emptiness as a durable fact in agent memory.
