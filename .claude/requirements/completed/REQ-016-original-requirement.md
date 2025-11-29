# Requirement: Automatic Creation of Project Form Submission Entries for Assigned Users

## Context

Currently, when a user (either newly created or existing) is assigned to a project that is in its **form reception stage**, the user does **not immediately see a pending form**.  
This occurs because, although the user gains access to the project within the **Auth domain**, this access alone does not enable them to start the form.  
The form availability depends on the **Project Form Submissions** table within the **Business Incubator domain**.

At present, demo users (e.g., `demo.starter`) are automatically seeded with entries in the `ProjectFormSubmissions` table when seats are created.  
These entries do not contain any meaningful data — they simply act as a **placeholder** that allows the user interface and backend logic to recognize that the user has an initial form to complete.

## Problem

When a user is assigned to a project **after** project creation, this placeholder record is not generated automatically.  
As a result, the system fails to recognize that the user has an initial form pending for that project, even though the project itself is in the stage where form collection is active.

## Requirement

When a user (new or existing) is assigned to a project, the system must ensure that the user has a corresponding entry in the `ProjectFormSubmissions` table.  
This should happen regardless of whether the user is created first and then assigned, or assigned to an existing project later.

The logic should:
1. **Detect all commands and orchestration commands** responsible for user creation and project assignment.
2. For each of these points, ensure that:
   - If the user does not yet have an entry in `ProjectFormSubmissions` for the assigned project,
   - And the project has an **initial stage** (`Type = 2` in `ProjectStages`),
   - Then a new record should be inserted in `ProjectFormSubmissions`, linking the user to that stage and project.

## Data Model References

- **ProjectStages**
  - Column: `Type`
  - Reserved types:
    - `2` → Indicates *initial form stage*
    - `4` → Indicates *final form stage*

- **ProjectFormSubmissions**
  - References `ProjectStageId`
  - Initial seeded entries always point to the `ProjectStageId` where `Type = 2`.

## Expected Behavior

- When a user (with roles such as **Starter**, **Participant**, or any equivalent role**) is created or assigned to a project:
  - The system checks if a `ProjectFormSubmissions` record exists for that combination of user and project.
  - If not, it automatically inserts one, using default or null values consistent with current seed data.
  - No over-engineering is required — simply replicate the behavior observed in the seeded demo user setup.

## Acceptance Criteria

1. **User Assignment**
   - Given a new or existing user,
   - When the user is assigned to a project,
   - Then the system ensures there is a corresponding entry in `ProjectFormSubmissions` for that project’s initial stage.

2. **Idempotency**
   - Reassigning a user to the same project does **not** create duplicate entries.

3. **Validation**
   - The system correctly identifies the project’s initial stage using `ProjectStages.Type = 2`.

4. **Scope**
   - The logic applies to all orchestration commands and command handlers that perform:
     - User creation with project linkage
     - Project assignment to an existing user

5. **Non-functional**
   - No additional form or entity logic should be altered.
   - The behavior must remain consistent with the current seed data defaults.

---

**Result:**  
Users will immediately see their pending forms after being assigned to a project in the reception phase, ensuring a consistent onboarding experience and eliminating manual form entry creation.
