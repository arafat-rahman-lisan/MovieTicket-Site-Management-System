# CINEX 🎬 – Movie Ticket Management System #

CINEX is a full-stack ASP.NET Core MVC application for online movie ticket booking.
It covers the entire flow:

**movies → theatres/halls → showtimes → seat map → booking → payment → invoice PDF.**

Admins use a single Manage dropdown in the navbar—linking to 

**Theatres, Halls, Hall Slots, Seat Types, Seats, Users, Schedule Builder, and Payments → Management (KPIs + filters by date/method/status)**

—all role-protected.

# ✨ Features

Browse movies (**Now Showing, Coming Soon, Top Rated, Genres**). 
Filter by **date, theatre, location, or language**

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/729e43dcb072a33dec654a111368c8cc93d93554/Screenshot%202025-09-27%20034340.png)

# Watch trailer or view detailed synopsis.

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/1edbcabd84dff6641f5af0fbdbda115e3c72b592/details.png)

# Get Ticket by Theatres

![Image Alt](https://github.com/arafat-rahman-lisan/MovieTicket-Site-Management-System/blob/00ac973a66100f71f8c4ab7419ce9d0824f437e2/get%20ticket.png)



Clone the repository, then from the project folder:
# Restore & build
dotnet restore
dotnet build

# Create the database with EF Core migrations
dotnet ef database update

# Run the web app (Kestrel)
dotnet run
The app will be running at:
- http://localhost:5000 (HTTP)
- https://localhost:5001 (HTTPS)
This solution uses
- ASP.NET Core MVC for the web UI
- EF Core for ORM and migrations
- ASP.NET Core Identity for authentication/authorization
- SQL Server for storage (default). PostgreSQL/MySQL possible with provider swap
Run with Docker (optional)
If you prefer containers, run:
docker compose up --build
Minimal docker-compose.yml:
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_strong_password123
    ports:
      - "1433:1433"
  web:
    build: .
    environment:
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__Default=Server=db;Database=CinexDb;User=sa;Password=Your_strong_password123;TrustServerCertificate=True;
    ports:
      - "5000:5000"
    depends_on:
      - db
Run the app in Kubernetes (optional)
If you have manifests in `k8s/`, apply them:
kubectl apply -f k8s/
To remove:
kubectl delete -f k8s/
Architecture
High-level components:
- Front-end ASP.NET Core MVC app (browse movies, book tickets)
- EF Core models: Theatre→Hall→HallSlot; Movie→Show; SeatType→Seat; ShowSeat; Booking→BookingSeat; Identity (Users, Roles)
- SQL Server database backed by EF Core migrations
User flow:
1. Visitor lands on homepage → sees movie listings.
2. Click **Details/Get Ticket** → prompts Login/Register.
3. Logged-in User → books tickets (theatre, showtime, seats, payment).
4. Admin → login → dashboard → manage CRUD for all cinema entities.
Notes
- One booking per seat per show guaranteed via UNIQUE(ShowId, SeatId).
- Snapshot pricing ensures historical bookings stay consistent even if prices change later.
- Two roles: Admin (manage) and User (book).
- EF Core migrations keep DB schema in sync.
Quick commands
# Apply latest migrations
dotnet ef database update

# Add a new migration
dotnet ef migrations add Add_NewFeature

# Run the app (watch)
dotnet watch run
Repository layout (example)
src/
  CINEX.Web/                # ASP.NET Core MVC app
    Controllers/
    Models/
    Views/
    Data/AppDbContext.cs
    appsettings.json
  CINEX.Tests/              # optional tests
docs/
  diagrams/
  README-assets/
License
MIT (or your preferred license)
