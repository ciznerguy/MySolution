using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace Model
{
    public class Person : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleCode { get; set; }
        public DateTime CreatedAt { get; set; }

        public Person() : base() { }

        public Person(int id, string fullName, string email, int roleCode, DateTime createdAt)
            : base(id)
        {
            FullName = fullName;
            Email = email;
            RoleCode = roleCode;
            CreatedAt = createdAt;
        }
    }
}