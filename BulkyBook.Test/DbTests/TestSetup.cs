using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAccess.Repository;

namespace BulkyBook.Test.DbTests
{
    [SetUpFixture]
    public class TestSetup
    {
        private static DbContextOptions<ApplicationDbContext> _options;
        private static ApplicationDbContext _dbContext;
        private static IUnitOfWork _unitOfWork;
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            _options = TestDatabaseInitializer.CreateOptions();
            _dbContext = new ApplicationDbContext(_options); // Pass the options to your DbContext constructor
            _dbContext.Database.OpenConnection();
            _dbContext.Database.EnsureCreated(); // Create the in-memory database
            TestDatabaseInitializer.SeedTestData(_dbContext); // Seed test data
            _unitOfWork = new UnitOfWork(_dbContext);
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            _dbContext.Database.CloseConnection();
            _dbContext.Database.EnsureDeleted(); // Delete the in-memory database after tests
            _dbContext.Dispose();
        }

        public static ApplicationDbContext GetDbContext()
        {
            return _dbContext;
        }
        public static IUnitOfWork GetUnitOfWork()
        {
            return _unitOfWork;
        }
    }
}
