# Optimized Software Requirement: Coordinator/Project Admin "Fill on Behalf" Feature

## Context
- **Current capability**: Coordinators and Project Administrators can **review** forms submitted by Starters (participants) for approval.  
- **New need**: Sometimes the Starter cannot or should not fill the diagnostic form. In those cases, the Coordinator/Project Admin must be able to **fill and submit the same form on the Starter’s behalf**.  
- **Non-goal**: Do **not** change the existing approval workflow. After submission, the form still enters the same approval stage, where a coordinator reviews and approves/rejects.  
- **Architectural principle**: **No over-engineering. Reuse existing components and screens as much as possible**. Avoid side effects.

---

## Objective
Design and specify a **“Fill on behalf”** capability that lets a **Coordinator/Project Admin** complete a **pending diagnostic form** for any **active participant (Starter) who accepted the project invitation**, **without logging in as that user**. The resulting submission must behave **exactly as if the Starter submitted it**, except it records **who performed the action**.

---

## Constraints & Guardrails
- **Reusability first**: Reuse the existing “Active Participants” list screen used by coordinators/admins. Add only minimal UI/actions.  
- **No impersonation login**: Implement **scoped delegation** (acting-on-behalf) rather than session or auth identity swap.  
- **Workflow unchanged**: Post-submission approval steps remain identical.  
- **Minimal schema/API changes**: Prefer additive changes (nullable columns, new audit table) over breaking changes.  
- **Security & compliance**: Strong authorization checks, full audit trail, and event logs.  
- **Side-effect safety**: Ensure notifications, analytics, and automations treat these submissions correctly; no duplicate triggers.

---

## Deliverables

### 1. High-level design
Introduce a **delegation model** where Coordinators/Admins can act on behalf of a Starter. No identity switching—just a scoped action. The submission is stored as if the Starter submitted it, but with metadata identifying the acting Coordinator/Admin.

### 2. Permission model
| Role             | View participants | View pending forms | Open on-behalf form | Submit on-behalf | Approve/reject | View audit |
|------------------|------------------|--------------------|---------------------|------------------|----------------|------------|
| Starter          | ✅               | ✅                 | ❌                  | ✅ (self only)   | ❌              | Own only   |
| Coordinator      | ✅               | ✅                 | ✅                  | ✅               | ✅              | ✅          |
| Project Admin    | ✅               | ✅                 | ✅                  | ✅               | ✅              | ✅          |
| Approver (if distinct) | ✅        | ✅                 | ❌                  | ❌               | ✅              | ✅          |

### 3. UI/UX spec (reuse-first)
- Extend **Active Participants** list with action: *“Fill diagnostic form on behalf”*.  
- Action enabled if participant has a **pending diagnostic form**.  
- Edge cases: Disabled if no pending form; show tooltip: *“No pending diagnostic form”*.  
- Confirmation dialog: *“You are about to submit this form on behalf of {Starter}. Approval flow remains unchanged.”*.  
- Notifications: Success/error toasts.

### 4. Backend changes
- **Data model**: Extend `FormSubmission` with:  
  - `submitted_by_user_id` (Coordinator/Admin actor)  
  - `on_behalf_of_user_id` (Starter)  
  - `submission_mode ENUM('self','on_behalf')`  
- **Audit trail**: Record actor/subject, timestamps, payload hash, IP, UA.  
- **Domain logic**: Validation unchanged except actor checks.  
- **Events/Notifications**: Ensure downstream sees `submission_mode`.

### 5. Endpoints design (minimal)
- `GET /projects/{id}/participants?status=active` (reuse).  
- `GET /forms/{formId}/pending?userId={starterId}`.  
- `POST /forms/{formId}/submit-on-behalf`  
  ```json
  {
    "starterId": "UUID",
    "answers": {...},
    "actorId": "UUID"
  }
  ```

### 6. Sequence diagram (textual)
1. Coordinator clicks **Fill on behalf**.  
2. System loads pending form for Starter.  
3. Coordinator completes answers.  
4. Submit triggers `POST /submit-on-behalf`.  
5. Backend stores submission with `actorId`, `starterId`, mode `on_behalf`.  
6. Audit log created.  
7. Notification sent to Starter + Approver.  
8. Approval flow proceeds unchanged.

### 7. Data migration plan
- No need, systems is not in production yet.

### 8. Risk & mitigation
- **Starter unaware** → Notify Starter explicitly.  
- **Analytics skew** → Mark `submission_mode` in reports.  
- **Concurrency** → Lock form per user per project until submitted.  
- **Partial save** → Ensure draft persistence works with `on_behalf` mode.

### 9. Testing strategy
- **Acceptance criteria (Gherkin)**:
  ```gherkin
  Scenario: Coordinator submits on behalf
    Given a participant with a pending diagnostic form
    When a Coordinator submits the form on their behalf
    Then the system records the Coordinator as actor
    And the Starter as subject
    And the form enters approval workflow unchanged
  ```
- Unit/integration tests: permissions, audit integrity, notification delivery, analytics tagging.

### 10. Rollout plan
- Deploy behind **feature flag**.  
- Test with a pilot project.  
- Monitor metrics: error rate, % on-behalf submissions, approval cycle time.  
- Rollback: disable flag.

### 12. Pseudocode
```python
def submit_on_behalf(actor_id, starter_id, form_id, answers):
    assert user_has_role(actor_id, ["Coordinator", "Administrator"])
    assert form_is_pending(form_id, starter_id)

    submission = FormSubmission(
        form_id=form_id,
        submitted_by_user_id=actor_id,
        on_behalf_of_user_id=starter_id,
        submission_mode="on_behalf",
        answers=answers
    )
    db.save(submission)

    audit_log(
        actor_id=actor_id,
        subject_user_id=starter_id,
        form_id=form_id,
        action="SUBMIT_ON_BEHALF"
    )

    notify(starter_id, "Your Coordinator submitted a form on your behalf.")
    notify_approver(form_id)
    return submission
```