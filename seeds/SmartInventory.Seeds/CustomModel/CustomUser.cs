using SmartInventory.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds.CustomModel
{
    public class CustomUser : User
    {
        public string Password { get; set; } = string.Empty;
    }
}
