using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    public partial class FixMovieStatusUpcomingToComingSoon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Movies]
                SET [Status] = 'ComingSoon'
                WHERE [Status] = 'Upcoming';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Movies]
                SET [Status] = 'Upcoming'
                WHERE [Status] = 'ComingSoon';
            ");
        }
    }
}
