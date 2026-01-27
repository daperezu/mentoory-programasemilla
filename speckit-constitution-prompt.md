🔍 ROLE & CONTEXT

You are Claude Code, acting as a spec-driven development analyst and constitution architect.

Your task is to analyze this entire repository deeply, similar in spirit to Claude Code /init, but with a stronger emphasis on extracting governing principles rather than setup instructions.

This analysis will be used to generate the best possible /speckit.constitution command invocation for GitHub’s Spec Kit workflow.

Reference tool documentation for conceptual alignment:
https://github.com/github/spec-kit

🧠 PHASE 1 — DEEP PROJECT ANALYSIS

Perform a holistic scan of the repository, including but not limited to:

- Directory structure and architecture patterns

- Programming languages, frameworks, and tooling

- Existing documentation (README, ADRs, comments, docs)

- Configuration files (linting, formatting, CI/CD, testing)

- Testing strategy and quality signals

- Naming conventions and style consistency

- Performance, security, or UX implications

- Implicit design philosophies (e.g., simplicity over abstraction, safety over speed)

- Constraints or tradeoffs already encoded in the codebase

Your goal is to infer the project’s values, not just describe files.

Ask yourself:

- What does this project optimize for?

- What does it avoid?

- What decisions would contributors need guidance on?

- What principles should future specs be forced to respect?

🧩 PHASE 2 — PRINCIPLE SYNTHESIS

From the analysis, synthesize a clear set of governing principles, including:

- Mission & scope assumptions 
- Architectural rules and non-goals 
- Code quality and maintainability expectations 
- Testing and correctness standards 
- UX, performance, and reliability priorities 
- Dependency and tech-stack governance 
- Decision-making rules for tradeoffs 
- Constraints that future specs must obey
 
These principles should be explicit, enforceable, and spec-driven, not generic best practices.

🏛️ PHASE 3 — CONSTITUTION COMMAND GENERATION

Now, generate the single best possible invocation of the Spec Kit constitution command.

Your output MUST:

- Produce a fully written /speckit.constitution command
- Include detailed, structured instructions after the command
- Explicitly instruct the agent to:
	- Generate a constitution.md
	- Store it in .specify/memory/constitution.md
	- Encode the synthesized principles as binding rules
	- Ensure all future /speckit.specify, /speckit.plan, /speckit.tasks, and /speckit.implement phases must comply

The command should be project-specific, not generic.

📤 OUTPUT FORMAT (STRICT)

Your final response must contain only the following sections:

A) Project Insight Summary

A concise bullet summary of the key principles and values inferred from the repository.

B) Final /speckit.constitution Command

A single, copy-paste-ready command, for example:

/speckit.constitution
<highly detailed, multi-paragraph instruction block>


Do NOT include explanations after this section.

⚠️ IMPORTANT CONSTRAINTS

Do NOT explain Spec Kit basics unless necessary for precision

Do NOT include implementation tasks or plans

Do NOT generate the constitution itself — only the command that will generate it

Optimize for clarity, enforceability, and long-term guidance

Assume this constitution will govern the project for its entire lifecycle