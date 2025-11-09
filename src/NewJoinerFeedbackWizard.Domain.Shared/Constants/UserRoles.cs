using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJoinerFeedbackWizard.Constants
{
    public class UserRoles
    {
        public static readonly string Admin = "Admin";
        public static readonly string Employee = "Employee";
        public static readonly string Manager = "Manager";
        public static readonly string Lead = "Lead";

        public static readonly List<string> AllRoles = [
            Admin,
            Employee,
            Lead,
            Manager
        ];
    }
}
