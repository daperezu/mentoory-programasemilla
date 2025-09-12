# Cloudy Code Homepage & Project System Requirements

## Role & Context
You are a **Senior Full-Stack Engineer (Elixir/Phoenix + LiveView)** working on **Cloudy Code**. The system uses **Phoenix Admin Template**, and a **Blob Storage service** already exists for storing images. Your task is to build a **marketing-style homepage (without sidebar)** and extend the "projects" model with **geolocation, phases, subscriptions, and notifications**.

---

## Objectives
- Create a homepage that requests **geolocation** from the user on their first visit.
- If location is shared: list projects within a **15 km radius** (default, configurable).
- If location is not shared: list projects ordered by **registration start date**.
- Each project should display as a **card** with an image (from Blob Storage) and metadata.
- Users should be able to **express interest** and receive **notifications**.
- Introduce a new **user role: `visitor`** for accounts that only want notifications.
- Homepage must use Phoenix Admin Template layout **without sidebar**, focused on marketing.

---

## Functional Requirements (User Stories & Acceptance Criteria)

### Geolocation
- On first entry, the site prompts for **location sharing**.
- If accepted → fetch projects within **15 km** radius.
- If declined → show projects sorted by **registration start date**.
- Store user preference (`location_consent` + timestamp).

### Project Cards
Each project card must show:
- Hero image (from Blob Storage)
- Name
- Country, province, district
- Distance (if location available)
- Registration start date
- Current phase/status
- CTA buttons: **"View details"**, **"I'm interested"**

If data fields are missing in DB, **extend schema and migrations**.

### Phases & Dates
- Model project phases with fields: `registration_start`, `registration_end`, `execution_start`, `execution_end`, `current_status` (derived).
- Default ordering when no location: ascending by `registration_start`.

### Subscriptions & Visitor Role
- Users must create an account with role `visitor` to receive project notifications.
- Flow: If user clicks **"I'm interested"** but is not logged in → prompt to create account → associate interest after registration.
- Visitors see a simple dashboard with:
  - Projects near them
  - Projects they marked as interested

### Notifications (Email)
- Email reminders sent **X days before** registration start (configurable, e.g. 7 and 1 day).
- Users can opt-in/out per project.
- Log notification deliveries.

### Homepage (Marketing Style)
- Uses Phoenix Admin Template but with **no sidebar**.
- Contains:
  - Hero section
  - Project grid
  - Call-to-action buttons (create account, share location, explore projects)

---

## Data Model Changes

### `projects` table (extensions)
- `country` (string)
- `province` (string)
- `district` (string)
- `lat` (float)
- `lng` (float)
- `hero_image_blob_id` (string/uuid)
- `has_image` (boolean, default: false)

### `project_phases` table
- `project_id` (fk)
- `registration_start` (datetime)
- `registration_end` (datetime)
- `execution_start` (datetime)
- `execution_end` (datetime)
- `current_status` (enum/string)

### `interests` table
- `user_id` (fk)
- `project_id` (fk)
- `notify_email` (boolean, default: true)
- Unique index `(user_id, project_id)`

### `users` table (extensions)
- `role` (enum: `admin`, `manager`, `participant`, `visitor`)
- `location_consent_at` (datetime, nullable)
- Optional: `home_lat`, `home_lng`, `home_country`, `home_province`, `home_district`

---

## Phoenix Contexts

### `Cloudy.Projects`
- `list_nearby(lat, lng, radius_km \\ 15)` → projects within radius
- `list_upcoming_by_date(limit \\ 20)` → ordered by registration_start
- `attach_hero_image(project, upload)` → via Blob Storage
- `current_phase(project)` → derive phase

### `Cloudy.Interests`
- `mark_interest(user, project_id)`
- `unmark_interest(user, project_id)`
- `list_user_interests(user_id)`
- `schedule_notifications(project_id)` → return notification schedule

### `Cloudy.Accounts`
- `register_visitor(attrs)` → create user with role `visitor`
- `record_location_consent(user)` → store consent

---

## Routes & Views (LiveView Recommended)
- `GET /` → `HomepageLive` (marketing layout, no sidebar)
- `GET /projects/:id` → `ProjectLive.Show`
- `POST /projects/:id/interest`
- `POST /blob/upload` → Blob service integration

---

## Integrations

### Blob Storage
- Use existing service for upload and retrieval.
- Store only `hero_image_blob_id` and derive signed URLs for display.

### Geolocation (Haversine Formula)
Fallback if PostGIS not available:
```elixir
def haversine_km({lat1, lng1}, {lat2, lng2}) do
  r = 6371
  dlat = :math.pi/180 * (lat2 - lat1)
  dlng = :math.pi/180 * (lng2 - lng1)
  a = :math.sin(dlat/2)**2 + :math.cos(:math.pi/180*lat1) * :math.cos(:math.pi/180*lat2) * :math.sin(dlng/2)**2
  c = 2 * :math.atan2(:math.sqrt(a), :math.sqrt(1-a))
  r * c
end
```

---

## Security & Privacy
- Request location with **explicit consent**.
- Allow revocation of location consent.
- Rate-limit notification and interest endpoints.
- Validate images before Blob upload (type/size).

---

## Metrics
- Track events: `location_prompt_shown`, `location_accepted`, `interest_marked`, `visitor_registered`, `notification_sent`.
- Conversion funnel: share location → mark interest → register account.

---

## Testing
- **Unit tests**: distance calculations, phase derivations, date sorting.
- **Integration tests**: Blob uploads, interest creation, notification scheduling.
- **E2E tests**: visitor flow (interested → register → interest persisted).

---

## Deliverables
1. DB migrations & schema updates.
2. Marketing homepage layout without sidebar.
3. LiveViews & controllers.
4. Notification system (Oban/cron job).
5. Seed data with at least 3 projects.
6. Deployment/config documentation.

---

## Definition of Done
- Lighthouse: Performance ≥ 85, Accessibility ≥ 90.
- All tests passing.
- Notifications sent at correct times.
- No 4xx/5xx errors in core flows.

---

## Notes
- If any entity/field is missing, **create it**.
- Default radius: **15 km**, configurable via `APP_DEFAULT_RADIUS_KM`.
- `visitor` role: no admin sidebar, only project interest dashboard.

