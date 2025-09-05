# Requirements Organization

This folder contains structured requirement documents for LinaSys development.

## Structure

```
.claude/requirements/
├── README.md                    # This file
├── TEMPLATE.md                  # Requirement template
├── pending/                     # New requirements to implement
├── active/                      # Currently being worked on
├── completed/                   # Finished requirements
└── backlog/                     # Future considerations
└── technical-specifications/    # Detailed specifications related to the requirements
```

## Workflow

1. **Create** new requirements in `pending/` using `TEMPLATE.md`
2. **Move** to `active/` when starting work
3. **Move** to `completed/` when finished
4. **Reference** in CLAUDE.md "Current Work" section

## Naming Convention

Use format: `{priority}-{module}-{feature}.md`

Examples:
- `P1-permissions-user-management.md`
- `P2-diagnostics-form-builder.md`
- `P3-reporting-analytics-dashboard.md`

## Priority Levels

- **P1**: Critical/Blocking
- **P2**: High Priority
- **P3**: Medium Priority
- **P4**: Low Priority/Enhancement