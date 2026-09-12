using System.Linq;
using MongoDB.Driver;
using Pcf.Administration.Core.Domain.Administration;

namespace Pcf.Administration.DataAccess.Data
{
    public class MongoDbInitializer : IDbInitializer
    {
        private readonly MongoDbContext _context;

        public MongoDbInitializer(MongoDbContext context)
        {
            _context = context;
        }

        public void InitializeDb()
        {
            var roles = _context.GetCollection<Role>();
            if (!roles.Find(Builders<Role>.Filter.Empty).Any())
            {
                roles.InsertMany(FakeDataFactory.Roles);
            }

            var employees = _context.GetCollection<Employee>();
            if (!employees.Find(Builders<Employee>.Filter.Empty).Any())
            {
                employees.InsertMany(FakeDataFactory.Employees);
            }
        }
    }
}