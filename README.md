# CINEX 🎬 – Movie Ticket Management System #

CINEX is a full-stack ASP.NET Core MVC application for online movie ticket booking.
It covers the entire flow:

**movies → theatres/halls → showtimes → seat map → booking → payment → invoice PDF.**

Admins use a single Manage dropdown in the navbar—linking to 

**Theatres, Halls, Hall Slots, Seat Types, Seats, Users, Schedule Builder, and Payments → Management (KPIs + filters by date/method/status)**

—all role-protected.

**Project Demonstration :**
https://drive.google.com/file/d/1JG7huw6qL3Fw1rP2YCNr-M8DnT_7aXu0/view?usp=drive_link

# ✨ Features

 Browse movies (**Now Showing, Coming Soon, Top Rated, Genres**). 
 Filter by **date, theatre, location, or language**

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/729e43dcb072a33dec654a111368c8cc93d93554/Screenshot%202025-09-27%20034340.png)

 Watch trailer or view detailed synopsis.

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/1edbcabd84dff6641f5af0fbdbda115e3c72b592/details.png)

 Get Ticket by **Theatres**

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/00ac973a66100f71f8c4ab7419ce9d0824f437e2/get%20ticket.png)

 Real-time **seat map** with seat hold logic (**2-minute auto release**). 
 **Book seats, confirm booking, proceed to payment**.

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/930e6b6367377246941b37048aefac15d56ee62c/seatmap.png)


![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/771226769ced1761aca2a446769a52abf230b1c3/confirm2.png)


 Download **QuestPDF invoice** after successful payment.

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/531655e472e2a7c5e29b1c1b5fd3073c3d440cea/Invoice.png)

# 🛠 Admin Side 
**Manage** - Theatres, Halls, Hall Slots, Seat Types, and Seats. 

**Schedule Builder** for creating shows (group by theatre, hall, slot). 

**User Management** (create, edit, reset password, delete). 

**Payment Management Dashboard**: filter by date, method, status; view KPIs & revenue. 

Ultra-glassmorphism UI with **retro film rails** design across CRUD pages.


**🚀Getting Started**

### Prerequisites 

**.NET 8 SDK**

**SQL Server (local or remote)** 

**Node.js (optional, for front-end builds)**


### Setup
bash
# 1. Clone repo
git clone https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System.git

or **Download**

# 2) Restore the Database from .bak

File path (adjust if different):
./Data/Backups/E-Ticket-Management.bak (or wherever the .bak lives in your repo)


**A) Easiest (Windows + SSMS GUI)**

1) Open SSMS → connect to your local SQL Server instance (e.g., localhost or .\SQLEXPRESS).

2) Right-click Databases → Restore Database…

3) Source: Device → Add… → select E-Ticket-Management.bak.

4) Destination: Database name → E_Ticket_Management (or your choice).

5) In Files tab, verify MDF/LDF paths are writable.

6) After wiring , Open the PM in VS and run the command below,

-- Add-InitialMigration
-- UPDATE-DATABASE
-

Click OK to restore.


**B) Command Line (Windows PowerShell)**

**Change these paths/instance as needed**


_$SqlInstance = ".\SQLEXPRESS"       # or "localhost"
$DbName     = "E_Ticket_Management"
$BakPath    = "C:\path\to\E-Ticket-Management.bak"

sqlcmd -S $SqlInstance -Q "RESTORE DATABASE [$DbName]
FROM DISK = N'$BakPath'
WITH REPLACE, RECOVERY;"_


|| If you get logical name/file path issues, you can move/relocate with MOVE options:

_sqlcmd -S $SqlInstance -Q "
RESTORE FILELISTONLY FROM DISK = N'$BakPath';
"
#Note the logical names, then:
sqlcmd -S $SqlInstance -Q "
RESTORE DATABASE [$DbName] FROM DISK = N'$BakPath'
WITH MOVE 'E_Ticket_Management' TO 'C:\SQLData\E_Ticket_Management.mdf',
     MOVE 'E_Ticket_Management_log' TO 'C:\SQLData\E_Ticket_Management_log.ldf',
     REPLACE, RECOVERY;
"_



# **3) Set the Connection String & Secrets**

|| Do not store real secrets in appsettings.json. Use User Secrets in development.

From the project folder (where the .csproj lives):

# Set your SQL Server connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=E_Ticket_Management;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# Seed Admin user (the app seeds roles/admin on startup)
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


**Notes**

If you used a different DB name in restore, update it in the connection string.

If your SQL Server uses SQL Auth:
Server=localhost;Database=E_Ticket_Management;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true

# **4) Run the App**

_#If you restored the DB, usually no migration is needed.

#But running migrations won't hurt if the schema matches:

dotnet ef database update

#Run
dotnet run

#Output: app listening on https://localhost:5001 (or shown URL)_


**Default Access**

**Admin:** login with the email/password you set in user-secrets.

**Public:** browse movies, details, seat map, proceed to booking/payment.

**After successful payment:** download the PDF invoice.


