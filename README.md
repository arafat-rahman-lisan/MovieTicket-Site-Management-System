````markdown
<!-- README.md — CINEX (Movie Ticket Management System) -->

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-5C2D91?logo=.net&logoColor=white" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/SQL%20Server-DB-CC2927?logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/License-All%20Rights%20Reserved-111111" alt="License" />
</p>

<h1 align="center">🎬 CINEX — Movie Ticket Management System</h1>

<p align="center">
  A full-stack ASP.NET Core MVC app for online movie ticketing.<br/>
  <b>Movies → Theatres/Halls → Showtimes → Seat Map → Booking → Payment → Invoice (PDF)</b>
</p>

<p align="center">
  <a href="https://drive.google.com/file/d/1JG7huw6qL3Fw1rP2YCNr-M8DnT_7aXu0/view?usp=drive_link">▶️ Watch Demo Video</a> ·
  <a href="https://drive.google.com/file/d/1JvAFfOShtutkv6NINYVor4NSGrYmPmEV/view?usp=sharing">📑 Project Report</a>
</p>

---

## 🔗 Quick Links
- 📽️ **Demo:** <a href="https://drive.google.com/file/d/1JG7huw6qL3Fw1rP2YCNr-M8DnT_7aXu0/view?usp=drive_link">Watch</a>
- 📄 **Report:** <a href="https://drive.google.com/file/d/1JvAFfOShtutkv6NINYVor4NSGrYmPmEV/view?usp=sharing">Open</a>
- 📬 **Request Sensitive Files (bak/Startup/appsettings):** <a href="https://forms.gle/VFchq3unKmCrMKHm7">Google Form</a> · <a href="mailto:Arafatrahmanlis02@gmail.com">Email</a>

---

## ✨ Highlights
- 🎞 Browse movies: **Now Showing, Coming Soon, Top Rated, Genres**
- 🔎 Filter by **date, theatre, location, language**
- ▶️ Watch trailer & read detailed synopsis
- 🎟️ Get Ticket by **Theatre**
- 🪑 Real-time **Seat Map** with **2-minute hold** (auto release)
- 💳 Book → Confirm → Pay → ✅ **QuestPDF invoice** download
- 🔐 **Identity + Roles**; Admin-only Manage menu

---

## 🛠 Admin Console
- **Manage:** Theatres, Halls, Hall Slots, Seat Types, Seats, Users
- **Schedule Builder:** create shows (group by theatre → hall → slot)
- **Payments Management:** KPIs + filters (date/method/status)
- **UI:** Ultra-glassmorphism with retro film-rail accents

---

## 📸 Screenshots

| Movies | Details | Tickets |
|:------:|:-------:|:------:|
| ![Movies](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/729e43dcb072a33dec654a111368c8cc93d93554/Screenshot%202025-09-27%20034340.png) | ![Details](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/1edbcabd84dff6641f5af0fbdbda115e3c72b592/details.png) | ![Tickets](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/00ac973a66100f71f8c4ab7419ce9d0824f437e2/get%20ticket.png) |

| Seat Map | Confirmation | Invoice |
|:--------:|:------------:|:------:|
| ![SeatMap](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/930e6b6367377246941b37048aefac15d56ee62c/seatmap.png) | ![Confirm](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/771226769ced1761aca2a446769a52abf230b1c3/confirm2.png) | ![Invoice](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/531655e472e2a7c5e29b1c1b5fd3073c3d440cea/Invoice.png) |

---

## 📦 Prerequisites
- 🟦 **.NET 8 SDK**
- 🗄 **SQL Server** (LocalDB/Express/Remote)
- 🟩 **Node.js** *(optional, for front-end builds)*

---

## ⚙️ Setup

```bash
# 1) Clone
git clone https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System.git
cd MovieTicket-Site-Management-System
````

### 2) Restore Database from `.bak`

**File:** `./Data/Backups/E-Ticket-Management.bak`

> ⛔ The `.bak`, `Startup.cs`, and `appsettings.json` are **available on request only**
> 👉 Request: [https://forms.gle/VFchq3unKmCrMKHm7](https://forms.gle/VFchq3unKmCrMKHm7)

**A) SSMS (GUI)**

1. Open SSMS → connect (e.g., `localhost` or `.\SQLEXPRESS`)
2. *Databases* → **Restore Database…**
3. **Source:** *Device* → **Add…** → select `E-Ticket-Management.bak`
4. **Destination DB:** `E_Ticket_Management`
5. Verify MDF/LDF paths are writable
6. In Visual Studio **Package Manager Console**, run:

   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```

**B) PowerShell**

```powershell
$SqlInstance = ".\SQLEXPRESS"       # or "localhost"
$DbName     = "E_Ticket_Management"
$BakPath    = "C:\path\to\E-Ticket-Management.bak"

sqlcmd -S $SqlInstance -Q "RESTORE DATABASE [$DbName]
FROM DISK = N'$BakPath'
WITH REPLACE, RECOVERY;"
```

*If logical name/path errors:*

```powershell
sqlcmd -S $SqlInstance -Q "RESTORE FILELISTONLY FROM DISK = N'$BakPath';"

# Then use the logical names from above:
sqlcmd -S $SqlInstance -Q "
RESTORE DATABASE [$DbName] FROM DISK = N'$BakPath'
WITH MOVE 'E_Ticket_Management' TO 'C:\SQLData\E_Ticket_Management.mdf',
     MOVE 'E_Ticket_Management_log' TO 'C:\SQLData\E_Ticket_Management_log.ldf',
     REPLACE, RECOVERY;
"
```

### 3) Set Connection String & Secrets (Development)

> ✅ Use **User Secrets** — do **not** commit real secrets.

```powershell
# Connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=E_Ticket_Management;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# Seed Admin (roles/admin are created on startup)
dotnet user-secrets set "AdminUser:Email" "admin@cinex.com"
dotnet user-secrets set "AdminUser:Password" "YourStrongPassword#2025"

# (Optional) Google OAuth
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"

# (Optional) SMTP for invoice emails
dotnet user-secrets set "Smtp:Host" "smtp.yourhost.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:User" "no-reply@cinex.com"
dotnet user-secrets set "Smtp:Pass" "StrongSmtpPassword"
```

> If you restored to a different DB name, update it in the connection string.
> For SQL Auth: `Server=localhost;Database=E_Ticket_Management;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true`

### 4) Run

```powershell
dotnet ef database update
dotnet run
```

App will start at **[https://localhost:5001](https://localhost:5001)** (or the shown URL).

---

## 🔑 Access

* **Admin:** use the email/password set in user-secrets
* **Public:** browse movies, showtimes, seat map, booking, payment
* **After payment:** download PDF invoice (QuestPDF)

---

## 🎨 Brand Palette (for badges/assets)

* Neon Cyan: `#00E0FF`
* Cinema Red: `#FF004C`
* Soft White: `#F5F5F5`
* Charcoal: `#1C1C1C`

> Tip: Use these with Shields.io badges or images to keep a consistent “glass cinema” vibe.

---

## 📬 Request Sensitive Files

The following are **not in the repo**:

* `.bak` database backup
* `Startup.cs`
* `appsettings.json`

👉 **Request:** [https://forms.gle/VFchq3unKmCrMKHm7](https://forms.gle/VFchq3unKmCrMKHm7)
📧 **Email:** [Arafatrahmanlis02@gmail.com](mailto:Arafatrahmanlis02@gmail.com)

---

## 📜 License

**All Rights Reserved** — No copying, redistribution, or reuse without written permission.

```
```
