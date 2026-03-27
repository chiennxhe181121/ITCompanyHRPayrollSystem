---
name: hrm-admin-manager-pattern
description: Generate and finish HRM admin/manager pages in this ASP.NET Core MVC project by following the existing folder structure and code patterns for Admin CRUD (controllers/views/DTO/service + TempData + Tailwind + SweetAlert2) and for Manager dashboards (layouts/tabs partials + `wwwroot/js/manager.js`). Use when the user asks to implement or complete “admin/manager” code, especially under routes like `HumanResourcesManager/admin` or `HumanResourcesManager/manager`, or when working on `Controllers/Admin`, `Views/Admin`, `Controllers/Manager`, `Views/Manager`, and related shared layouts/partials.
---

# HRM Admin/Manager Coding Pattern

## What this skill does
This skill teaches the agent how to implement new modules or finish existing screens in the current `HumanResourcesManager` app by reusing the project’s conventions:
- Admin CRUD style (ASP.NET Core MVC controllers returning Razor views with pagination/filter, create/edit, active/inactive toggles, and client-side confirm dialogs)
- Manager dashboard style (tabbed UI driven by `wwwroot/js/manager.js`, shared layout/sidebar, and tab partials)

## Strictness (medium, layered architecture enforced)
When the user asks to implement a new feature/module “similar to existing ones”, the agent MUST:
- Follow **layering**: **Controller → BLL Service → DAL Repository → Database**, using DTOs between layers.
- Create the **full deliverables set** (unless user explicitly excludes something):
  - Controller
  - Views (Index/Create/Edit)
  - DTOs (Create/Update/List/Detail as needed)
  - Service interface + service implementation (BLL)
  - Repository (DAL) + its interface
- Keep controllers thin:
  - no EF/DbContext queries in controllers
  - no business rules in controllers
- Produce output in **step-by-step format**:
  - Step includes **file path(s)** + what is added/changed + brief rationale
  - Preserve the project’s existing routes/view paths/layout usage

## Decision: Admin CRUD vs Manager Dashboard
1. If the request mentions `HumanResourcesManager/admin` routes, `Controllers/Admin`, or CRUD-like actions (list/create/edit/toggle status), follow the Admin workflow below.
2. If the request mentions `HumanResourcesManager/manager` routes, `Controllers/Manager`, or “dashboard/tabs/approve overtime/schedule”, follow the Manager workflow below.

## Repository structure to follow
### Admin (CRUD)
- Controllers: `HumanResourcesManager/Controllers/Admin/*Controller.cs`
  - Namespace: `HumanResourcesManager.Controllers.Admin`
  - Authorization: `[Authorize(Roles = "ADMIN")]`
  - Route prefix examples: `HumanResourcesManager/admin/departments`, `.../employees`, `.../UserAccounts`
- Views:
  - Layout: `HumanResourcesManager/Views/Shared/_AdminLayout.cshtml`
  - Sidebar: `HumanResourcesManager/Views/Shared/_AdminSidebar.cshtml` (uses `USER_SESSION`)
  - Screens: `HumanResourcesManager/Views/Admin/<Module>/(Index|Create|Edit).cshtml`
- JS:
  - Confirm dialogs and list/grid switching are usually embedded inside the Razor view (bottom `<script>`).
  - If tabs are needed, there is also `HumanResourcesManager/wwwroot/js/admin_tab.js`.

### Manager (Dashboard)
- Controller: `HumanResourcesManager/Controllers/Manager/ManagerController.cs`
  - Authorization: `[Authorize(Roles = "MANAGER")]`
  - Route: `HumanResourcesManager/manager`
- Views:
  - Layout: `HumanResourcesManager/Views/Manager/_ManagerLayout.cshtml`
  - Sidebar: `HumanResourcesManager/Views/Manager/_ManagerSidebar.cshtml`
  - Tabs partials:
    - `_ProfileTab.cshtml` (expects `profileTab`)
    - `_TeamTab.cshtml` (expects `teamTab`)
    - `_OvertimeTab.cshtml` (expects `overtimeTab`)
    - `_ScheduleTab.cshtml` (expects `scheduleTab`)
- JS:
  - `HumanResourcesManager/wwwroot/js/manager.js` controls:
    - tab switching via `.nav-item[data-tab="..."]`
    - clock/date in header
    - logout redirect to `/HumanResourcesManager/logout`

## Layer map (where code belongs)
Use this mapping when implementing features so logic doesn’t leak across layers.

### Controller (routing + HTTP + view model selection)
Put in `HumanResourcesManager/Controllers/<Area>/*Controller.cs`:
- `[Authorize]`, `[Route]`, `[HttpGet]`, `[HttpPost]`
- read query params / route params
- `ModelState` validation (only validation flow, not business rules)
- call service methods
- set `ViewBag` (pagination/filter) and `TempData` (success/error)
- return Razor views using project conventions:
  - Admin often uses explicit view paths: `View("~/Views/Admin/<Module>/Index.cshtml", model)`

### Service (business rules + orchestration + mapping)
Put in BLL (typically `HumanResourcesManager.BLL/*`):
- validation rules beyond DataAnnotations (duplicates, cross-field constraints, authorization checks based on current user)
- orchestrate repository calls inside a transaction boundary if needed
- mapping entities ↔ DTOs (or call mapper helper if project has one)
- return:
  - DTOs for read flows
  - `bool` + message/error code for write flows (pattern exists in `ADEmployeeController` calling service with `out message`)

### Repository (data access to database)
Put in DAL (typically `HumanResourcesManager.DAL/*`):
- EF Core queries / DbContext access
- filtering/sorting/paging query construction
- projections to lightweight read models when appropriate
- “no UI concepts”: no `TempData`, no `ViewBag`, no Razor, no HTTP

### DTOs (data contracts)
Put in `HumanResourcesManager.BLL/DTOs/<Module>/*DTO.cs`:
- Create/Update DTOs include DataAnnotations for basic validation
- List/Detail DTOs shape exactly what UI needs (avoid passing EF entities to views when possible)

## Deliverables template (copy when building a new module)
When the user says “làm module X”, the agent should create/update these artifacts.

### Expected file tree (Admin CRUD module)
Use this template and replace `<Entity>` / `<plural>`:
- `HumanResourcesManager/Controllers/Admin/<Entity>Controller.cs`
- `HumanResourcesManager/Views/Admin/<Entity>/Index.cshtml`
- `HumanResourcesManager/Views/Admin/<Entity>/Create.cshtml`
- `HumanResourcesManager/Views/Admin/<Entity>/Edit.cshtml`
- `HumanResourcesManager.BLL/DTOs/<Entity>/<Entity>CreateUpdateDTO.cs`
- `HumanResourcesManager.BLL/DTOs/<Entity>/<Entity>ListDTO.cs`
- `HumanResourcesManager.BLL/Interfaces/I<Entity>Service.cs`
- `HumanResourcesManager.BLL/Services/<Entity>Service.cs` (or project’s service folder naming)
- `HumanResourcesManager.DAL/Interfaces/I<Entity>Repository.cs` (or project’s DAL interface pattern)
- `HumanResourcesManager.DAL/Repositories/<Entity>Repository.cs` (or project’s DAL repository folder pattern)

If the project already has different folder names for services/repositories, keep the same naming convention; do not invent a new structure.

### Controller template (Admin)
Follow this structure (adjust param names to match view query strings):
- Class attributes:
  - `[Authorize(Roles = "ADMIN")]`
  - `[Route("HumanResourcesManager/admin/<plural>")]`
- Actions:
  - `Index(string? keyword, int? status, int page = 1)` → sets ViewBag, returns `~/Views/Admin/<Entity>/Index.cshtml`
  - `Create()` GET → view + new DTO
  - `Create(dto)` POST → `ValidateAntiForgeryToken`, ModelState, service.Create, TempData, redirect
  - `Edit(id)` GET → service.GetById, NotFound, view
  - `Edit(id, dto, page, keyword, status)` POST → validate, service.Update, TempData, redirect preserve filters
  - `Active/Inactive` POST (or `ToggleStatus`) → service call + redirect preserve filters

### Service template (Admin)
Service interface methods usually needed:
- `PagedResult<<Entity>ListDTO> Search(string? keyword, int? status, int page, int pageSize)`
- `<Entity>CreateUpdateDTO? GetById(int id)`
- `bool Create(<Entity>CreateUpdateDTO dto, out string message)` (or `bool Create(dto)`)
- `bool Update(<Entity>CreateUpdateDTO dto, out string message)` (or `bool Update(dto)`)
- `void SetActive(int id)` / `bool DeleteOrInactive(int id, out string message)`

Service implementation:
- Calls repository for queries and writes
- Enforces business rules (duplicate name/code, cannot inactive if referenced, etc.)
- Returns clear error message/codes that controllers translate to ModelState/TempData

### Repository template (Admin)
Repository interface typically includes:
- Query by filters (keyword/status) with paging
- Get by id
- Create/update/status change
- Any uniqueness checks needed by service (or service can check by querying repository)

Repository implementation:
- Uses EF Core / DbContext
- Does not return `IQueryable` to controller; return materialized results or paged DTO projections

## Admin workflow (Controllers + Views)
### 1) Controller skeleton
When adding a new Admin module/controller:
1. Create `HumanResourcesManager/Controllers/Admin/<Entity>Controller.cs`
2. Use:
   - `namespace HumanResourcesManager.Controllers.Admin`
   - `[Authorize(Roles = "ADMIN")]`
   - `[Route("HumanResourcesManager/admin/<plural>")]`
3. Inject the correct BLL service interface (pattern from existing controllers, e.g. `IDepartmentService`, `IUserAccountService`).
4. Keep controllers “thin”:
   - controllers call service methods
   - controllers do not talk directly to DAL/DbContext/EF

### 2) Index action (search + pagination)
Implement Index as follows:
1. Accept query parameters like:
   - `string? keyword`
   - `int? status` (or string status, depending on DTO)
   - `int page = 1`
2. Normalize `page`:
   - if `page < 1`, set to `1`
3. Call the service search/query method and receive a paged result:
   - controller sets `ViewBag.Keyword`, `ViewBag.Status`, `ViewBag.CurrentPage`, `ViewBag.TotalPages`
4. Return:
   - `return View("~/Views/Admin/<Module>/Index.cshtml", result.Items);`

### 3) Create (GET + POST)
GET:
1. Return view `~/Views/Admin/<Module>/Create.cshtml`
2. Pass a new DTO instance (e.g. `new <Entity>CreateUpdateDTO()`).

POST:
1. Use `[ValidateAntiForgeryToken]`
2. If `!ModelState.IsValid`, return the same view with the DTO.
3. Call `_service.Create(dto)` (or `_service.CreateX(dto)`).
4. If the service fails:
   - either `ModelState.AddModelError(field, message)`
   - and return to the same view
5. On success:
   - set `TempData["Success"]`
   - redirect back to Index with the appropriate page (either `page = 1` or computed last page).

### 4) Edit (GET + POST)
GET:
1. Load DTO via `_service.GetById(id)`
2. If null -> `return NotFound()`
3. Return edit view: `~/Views/Admin/<Module>/Edit.cshtml`

POST:
1. Validate `id` vs DTO id when both exist (return `BadRequest()` on mismatch)
2. If `!ModelState.IsValid`, return view
3. Call `_service.Update(dto)` (often with error handling)
4. Set TempData success/error and redirect to Index preserving filters (keyword/status/page).

### 5) Active/Inactive or ToggleStatus
1. Add `HttpPost` endpoints named similarly to existing code:
   - examples: `inactive/{id}`, `active/{id}`, or `ToggleStatus`
2. Use `[ValidateAntiForgeryToken]` when appropriate.
3. Call service method and set `TempData["Success"]` or `TempData["Error"]`.
4. Redirect back to Index with the same filter parameters.

### 6) Special UI permission logic (UserAccounts example)
If editing UserAccounts, follow the project’s pattern:
- UI checks sometimes enforce “cannot edit yourself” or “cannot edit same-level admin” in Razor using `currentUserId` and role codes.
- Keep backend/service authorization as well (the skill should not remove security checks).

## Admin view workflow (Razor conventions)
### Layout and page header
For Admin pages:
1. `Layout = "~/Views/Shared/_AdminLayout.cshtml";`
2. Set `ViewData["Title"] = "<Vietnamese title>"` (as used across the project).

### Filters and list rendering
1. Use a filter `<form method="get">` and name inputs to match controller parameters (`keyword`, `status`, etc.).
2. Render:
   - either table list or grid list
   - many screens support both list/grid with a `switchView(mode)` function and `localStorage` key.
3. Pagination:
   - generate `?page=...&keyword=...&status=...` links matching controller query parameter names.

### TempData alerts and SweetAlert2 confirmations
1. Show TempData blocks for `TempData["Error"]` and `TempData["Success"]` (often in a fixed `#notification-area`).
2. Include SweetAlert2 confirm handlers to submit hidden forms (pattern used in Department/Employee screens):
   - confirmAction(...) calls `Swal.fire(...)`
   - if confirmed, submit the correct hidden `<form>` by id

### Scripts section and validation partials
1. For Create/Edit forms, include:
   - `@section Scripts { <partial name="_ValidationScriptsPartial" /> }` when the view uses MVC validation.
2. Keep embedded scripts inside the view at the bottom (or inside `@section Scripts` for Create/Edit).

## Database and session interaction conventions
### Where database logic lives
- Controllers call BLL interfaces.
- Service implementations are responsible for:
  - querying data
  - mapping to DTOs
  - handling duplicates and business rules

### Session usage (Admin profile)
If updating the current user profile (similar to `ADProfileController`):
1. Read `USER_SESSION` from session: `HttpContext.Session.GetObject<UserSessionDTO>("USER_SESSION")`
2. After successful update, update session fields (e.g., full name and avatar) and re-save with `HttpContext.Session.SetObject("USER_SESSION", sessionUser)`.

## Manager workflow (Dashboard + Tabs)
### 1) Layout/sidebar conventions
When completing Manager screens:
1. Use `Layout = "~/Views/Manager/_ManagerLayout.cshtml";`
2. Use the existing sidebar partial `Views/Manager/_ManagerSidebar.cshtml`.
3. Ensure Manager layout includes SweetAlert2 when views use `Swal.fire(...)`.

### 2) Tabs partials contract
1. Ensure the main view includes:
   - `@await Html.PartialAsync("_ProfileTab")`
   - `@await Html.PartialAsync("_TeamTab")`
   - `@await Html.PartialAsync("_OvertimeTab")`
   - `@await Html.PartialAsync("_ScheduleTab")`
2. Ensure tab containers have ids matching `manager.js` expectations:
   - `profileTab`, `teamTab`, `overtimeTab`, `scheduleTab`
3. Ensure sidebar nav items have `data-tab` values matching those ids (without the `Tab` suffix):
   - profile -> `profileTab`
   - team -> `teamTab`
   - overtime -> `overtimeTab`
   - schedule -> `scheduleTab`

### 3) Client-side behavior
1. Keep tab switching in the Razor ids/classes (`.tab-content` with `hidden` class).
2. Put/adjust logic in `wwwroot/js/manager.js` if it is about:
   - tab switching
   - clock/date formatting
   - logout redirect and loading overlay
3. If a new action button is added to a tab:
   - add a DOM hook id (e.g., `addStaffBtn`, `addScheduleBtn`)
   - implement behavior in `manager.js` (initially can be placeholder hooks calling backend endpoints later).

### 4) Manager route-per-page mode (server-side navigation)
When user asks to replace JS tab switching with C# navigation:
1. Keep route prefix: `HumanResourcesManager/manager`.
2. Implement actions in `Controllers/Manager/ManagerController.cs`:
   - `GET /profile` -> `~/Views/Manager/Profile.cshtml`
   - `GET /team` -> `~/Views/Manager/Team.cshtml`
   - `GET /overtime` -> `~/Views/Manager/Overtime.cshtml`
   - `GET /schedule` -> `~/Views/Manager/Schedule.cshtml`
3. Sidebar links should use real URLs (`href` or `asp-action`) instead of `href="#"`.
4. Keep active state in sidebar based on current action.
5. In this mode, `manager.js` should not block anchor navigation.

### 5) Manager Profile complete feature pattern
When implementing full profile for manager:
1. Use model `EmployeeOwnerProfileDTO` for `Views/Manager/Profile.cshtml`.
2. Controller should call `IEmployeeService.GetOwnProfile(userId)` for GET profile.
3. Add POST update profile endpoint:
   - `[HttpPost("update-profile")]`
   - `[ValidateAntiForgeryToken]`
   - call `_employeeService.UpdateOwnProfile(...)`
4. On success:
   - set `TempData["Success"]`
   - update `USER_SESSION` (`FullName`, `ImgAvatar`) via session extensions.
5. Profile UI should support:
   - edit/save/cancel state
   - avatar upload and remove flag
   - validation feedback.
6. If using success toast, use SweetAlert2 toast style (same as Employee profile).

### 6) Vietnamese text consistency (important)
For all Manager UI/messages:
1. Use Vietnamese with full diacritics for labels, buttons, and success/error messages.
2. Avoid non-diacritic strings like:
   - `Quan ly` -> `Quản lý`
   - `Cap nhat` -> `Cập nhật`
   - `Luu thay doi` -> `Lưu thay đổi`
3. Keep wording consistent between controller `TempData` messages and UI notifications.

## Quality checklist before final output
- Controller route prefix matches the intended UI URL pattern (`HumanResourcesManager/admin/...` or `HumanResourcesManager/manager`).
- Admin controllers include `[Authorize(Roles = "ADMIN")]` and manager controllers include `[Authorize(Roles = "MANAGER")]`.
- POST actions:
  - include `ValidateAntiForgeryToken` where the project uses it
  - validate `ModelState` before service calls
- Layering is respected:
  - controller does not query DbContext/EF
  - service contains business rules
  - repository contains DB queries
- Admin views:
  - use the correct `Layout`
  - set `ViewData["Title"]`
  - wire filters/pagination query strings with names matching controller parameters
  - include TempData alerts and confirm dialogs for destructive/activation actions
- Manager views:
  - tabs ids and sidebar `data-tab` values align with `manager.js`
- No hardcoding of incorrect view paths: always return views from controller using the `~/Views/Admin/...` or `~/Views/Manager/...` convention.

## Example prompts to use this skill
- "Hoàn thiện code manager: tạo thêm tab và button `+ Thêm nhân sự` hoạt động theo pattern hiện tại."
- "Tạo module admin quản lý `Departments`: Index + Create + Edit + Active/Inactive theo style dự án."
- "Hoàn thiện `Admin/Allowance` để có confirm Swal và giữ đúng filter/pagination keyword/status."
