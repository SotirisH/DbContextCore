using Mehdime.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace DbContextScope.UnitTest
{
    public class DbContextFactory : IDbContextFactory
    {
        public TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext
        {
            var typeTDbContext = typeof(TDbContext);
            if (typeTDbContext == typeof(DbContextA))
            {
                return new DbContextA() as TDbContext;
            }
            if (typeTDbContext == typeof(DbContextB))
            {
                return new DbContextB() as TDbContext;
            }

            return default;
        }
    }
       
}
