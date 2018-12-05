using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _222.EF
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }
      
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }

        
    }
}
