using DbContextScope.UnitTest.DomainModel;
using Mehdime.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbContextScope.UnitTest
{
    [TestClass]
    public class DbcontextScopeTest
    {
        private DbContextScopeFactory _dbContextScopeFactory = new DbContextScopeFactory(new DbContextFactory());
        [TestMethod]
        public void RefreshEntitiesInParentScopeTest()
        {
           
            // create a user
            var userId = Guid.NewGuid();
            using (var scopeCreate = _dbContextScopeFactory.Create())
            {
                var parentDbContext = scopeCreate.DbContexts.Get<DbContextA>();

                parentDbContext.Users.Add(new User() { Id = userId, Name = "testUser" });
                scopeCreate.SaveChanges();
            }
            using (var parentScope = _dbContextScopeFactory.Create())
            {
                var parentDbContext = parentScope.DbContexts.Get<DbContextA>();

                // Load John in the parent DbContext
                var john = parentDbContext.Users.Find(userId);
                // modify the user in another scope
                using (var dbContextScope = _dbContextScopeFactory.Create(DbContextScopeOption.ForceCreateNew))
                {
                    var dbContext = dbContextScope.DbContexts.Get<DbContextA>();
                    var user = dbContext.Users.Find(userId);

                    if (user == null)
                        throw new ArgumentException(String.Format("Invalid userId provided: {0}. Couldn't find a User with this ID.", userId));

                    user.WelcomeEmailSent = true;
                   
                    dbContextScope.SaveChanges();

                    // When you force the creation of a new DbContextScope, you must force the parent
                    // scope (if any) to reload the entities you've modified here. Otherwise, the method calling
                    // you might not be able to see the changes you made here.
                    dbContextScope.RefreshEntitiesInParentScope(new List<User> {user});
                    //
                    var refreshedUser = parentDbContext.Users.Find(userId);
                    Assert.AreEqual(user.WelcomeEmailSent, refreshedUser.WelcomeEmailSent);
                }
            }

        }
        [TestMethod]
        public void JoinTransactionTest()
        {
            /*
			 * Example of DbContextScope nesting in action. 
			 * 
			 * We already have a service method - CreateUser() - that knows how to create a new user
			 * and implements all the business rules around the creation of a new user 
			 * (e.g. validation, initialization, sending notifications to other domain model objects...).
			 * 
			 * So we'll just call it in a loop to create the list of new users we've 
			 * been asked to create.
			 * 
			 * Of course, since this is a business logic service method, we are making 
			 * an implicit guarantee to whoever is calling us that the changes we make to 
			 * the system will be either committed or rolled-back in an atomic manner. 
			 * I.e. either all the users we've been asked to create will get persisted
			 * or none of them will. It would be disastrous to have a partial failure here
			 * and end up with some users but not all having been created.
			 * 
			 * DbContextScope makes this trivial to implement. 
			 * 
			 * The inner DbContextScope instance that the CreateUser() method creates
			 * will join our top-level scope. This ensures that the same DbContext instance is
			 * going to be used throughout this business transaction.
			 * 
			 */

            var usersToCreate = new List<User>();
            usersToCreate.Add(new User()
            {
                Id = Guid.NewGuid(),
                Name = Faker.Name.FullName(),
                Email = Faker.Internet.Email(),
                CreditScore = Faker.RandomNumber.Next()
            });
            usersToCreate.Add(new User()
            {
                Id = Guid.NewGuid(),
                Name = Faker.Name.FullName(),
                Email = Faker.Internet.Email(),
                CreditScore = Faker.RandomNumber.Next()
            });

            using (var dbContextScope = _dbContextScopeFactory.Create())
            {
                foreach (var toCreate in usersToCreate)
                {
                    CreateUser(toCreate);
                    // try to find the user
                    var dbA = dbContextScope.DbContexts.Get<DbContextA>();
                    var userF =dbA.Users.Find(toCreate.Id);
                }

                // All the changes will get persisted here
                dbContextScope.SaveChanges();
            }

        }

        private void CreateUser(User userToCreate)
        {
            if (userToCreate == null)
                throw new ArgumentNullException("userToCreate");
            /*
			 * Typical usage of DbContextScope for a read-write business transaction. 
			 * It's as simple as it looks.
			 */
            using (var dbContextScope = _dbContextScopeFactory.Create())
            {
                var db = dbContextScope.DbContexts.Get<DbContextA>();
                db.Users.Add(userToCreate);
                dbContextScope.SaveChanges();
            }
        }
    }
}

