using Movie_Site_Management_System.ViewModels.Account;
using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class UsersIndexVM
    {
        public List<UserRowVM> Users { get; set; } = new();
    }
}
