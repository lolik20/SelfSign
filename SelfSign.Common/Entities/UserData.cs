using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.Entities
{
    public class UserData
    {
        public Guid Id { get; set; }
        public Role Role { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    }
}
