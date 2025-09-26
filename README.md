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

cd Movie-Site-Management-System

