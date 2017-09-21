using DbContextScope.UnitTest.DomainModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbContextScope.UnitTest
{
    public class DbContextA : DbContext
    {

        // Map our 'User' model by convention
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Test");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
