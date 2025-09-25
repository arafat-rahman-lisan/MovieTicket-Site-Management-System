
using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class UserRowVM
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public bool Locked { get; set; }
    }
}