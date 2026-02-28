# Project Instruction File
# Project: Habit Tracker & Life Assistant
# Architecture: Blazor WebAssembly PWA + IndexedDB
# Deployment: Static Hosting (GitHub Pages / Cloudflare Pages)

---

## 🎯 Core Objective

Build a mobile-first Progressive Web App (PWA) using Blazor WebAssembly.

This is a single-user personal productivity application.
No backend.
No server database.
All data stored locally using IndexedDB.

---

# 🏗️ Architecture Rules

## Application Type
- Blazor WebAssembly (Client-only)
- PWA enabled
- Offline support required

## Storage
- Use IndexedDB
- Use document-style storage
- No relational DB modeling
- No SQLite
- No server APIs

Use:
TG.Blazor.IndexedDB package

---

# 📱 Mobile First Design Rules

- All pages must be responsive
- Default layout optimized for mobile width (360px–480px)
- Use TailwindCSS preferred
- Avoid heavy desktop-first layouts
- Use collapsible sidebar
- Touch-friendly UI elements (min height 44px)

---

# 🧠 Code Quality Requirements

Follow:

- SOLID principles
- Separation of concerns
- Services for business logic
- Pages for UI only
- Async methods only
- Proper error handling
- Strong typing
- No magic strings (use constants/enums)

Do NOT:
- Put data logic inside Razor page
- Use synchronous methods
- Duplicate logic across services

---

# 📂 Project Structure

HabitTrackerLifeEnhancer/

- Pages/
- Components/
- Layout/
- Models/
- Services/
    - IndexedDbService.cs
    - RoutineService.cs
    - TaskService.cs
    - ActionService.cs
    - GoalService.cs
    - NotificationService.cs
    - MoodService.cs
- wwwroot/
    - manifest.json
    - service-worker.js

---

# 🗃️ IndexedDB Schema Design

Database Name: HabitTrackerDB
Version: 1

Stores:

- routines
- routineLogs
- tasks
- taskLogs
- actions
- goals
- goalCategories
- lifePrinciples
- visualizations
- moodLogs
- settings

Each entity must:
- Have Guid Id
- Have CreatedAt
- Have UpdatedAt

---

# 🔐 Passphrase System

- First launch: Set default passphrase = "Jay Shree Krushna"
- Store SHA256 hash only
- Never store plain text
- Validate on app start
- Lock UI until validated

---

# 📄 Pages Specification

## Dashboard
Must include:
- Pie chart: Routine completion %
- Bar chart: Task hours
- Mood summary
- Notification badge
- Calendar view
- AI-generated visualization
- Compact tab layout

---

## Routine Page

Fields:
- Name
- ScheduleType (Daily / SpecificDays)
- MeasurementType
- IsRecurring

UI:
- 7-day tracker grid
- Status:
    ? = Default (Gray)
    ✓ = Followed (Green)
    X = Not Followed (Red)
    ! = Ignored (Yellow)

---

## Tasks Page

Fields:
- Name
- TargetHours
- ScheduleType

UI:
- 7-day hour entry
- Color logic:
    0 = Red
    < Target = Red
    >= Target = Green
    Ignored = Yellow

---

## Action Page

- One-time items
- DueDate
- IsDone

Color:
- Overdue = Red
- Completed = Green

---

## Goals Page

- Category-based filtering
- CRUD Categories
- CRUD Goals

---

## Life Principles

- Simple CRUD list
- Text-based reminders

---

## Visualization Page

- Tangible / Intangible toggle
- CRUD support
- Auto-generate AI visualization
- Store AI-generated content locally

---

## Mood Logging

- One mood entry per day
- Scale 1–5
- Optional notes

---

# 🔔 Notification Rules

Must show:

- Actions due tomorrow
- Actions overdue
- Pending routines today

Implement:
- In-app notification service
- No push server required

---

# 📊 Metrics Rules

Calculate:

- Weekly completion %
- Monthly completion %
- Yearly completion %
- Task performance ratio
- Mood trend average

Use LINQ grouping.

---

# 💾 Export / Import

Export:
- Serialize entire IndexedDB to JSON
- Download file

Import:
- Upload JSON
- Validate structure
- Clear DB
- Restore records

---

# 🎨 Theme System

Support:
- Light mode
- Dark mode
- Custom theme

Implementation:
- CSS variables
- Persist in localStorage

---

# 🌐 PWA Requirements

Must include:
- manifest.json
- service-worker.js
- Offline caching
- Installable on mobile
- Standalone display mode

---

# ⚡ Performance Rules

- Avoid large in-memory collections
- Use pagination where needed
- Use async calls
- Avoid unnecessary re-renders
- Use @key in loops

---

# 🧠 AI Integration Rules

AI visualization generation:
- Generate motivational visualization text
- Based on Goals + Mood
- Store locally
- No server required
- Can use external AI API if user provides key

---

# 🔒 Security Rules

Even though single user:

- Hash passphrase
- Validate input lengths
- Avoid JS injection
- Sanitize text rendering

---

# 🚀 Deployment Rules

App must:
- Build with dotnet publish
- Deploy as static files
- Work on GitHub Pages
- Work on Cloudflare Pages
- No server dependency

---

# 🧩 Additional Enhancements

If possible implement:

- Streak tracking
- Productivity score
- Achievement badges
- Mood-based suggestion
- Soft delete instead of hard delete

---

# 🤖 Copilot Behavior Instructions

When generating code:

- Follow architecture strictly
- Do not mix UI & logic
- Always use async
- Create reusable services
- Prefer clean, modular components
- Avoid overengineering
- Keep UI minimal and mobile-first

---

# End of Instruction File