using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbContextScope.UnitTest
{
    public class DbContextB : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("TestB");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
