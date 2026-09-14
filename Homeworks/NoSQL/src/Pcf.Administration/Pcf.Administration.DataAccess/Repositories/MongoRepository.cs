using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain;

namespace Pcf.Administration.DataAccess.Repositories
{
    public class MongoRepository<T> : IRepository<T>
        where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();

        public async Task<T> GetByIdAsync(Guid id) =>
            await _collection.Find(entity => entity.Id == id).FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<Guid> ids) =>
            await _collection.Find(Builders<T>.Filter.In(entity => entity.Id, ids)).ToListAsync();

        public async Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate) =>
            await _collection.Find(predicate).FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate) =>
            await _collection.Find(predicate).ToListAsync();

        public async Task AddAsync(T entity) => await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(T entity) =>
            await _collection.ReplaceOneAsync(item => item.Id == entity.Id, entity);

        public async Task DeleteAsync(T entity) =>
            await _collection.DeleteOneAsync(item => item.Id == entity.Id);
    }
}