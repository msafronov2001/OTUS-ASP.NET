using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Exceptions;
using System.Collections.Concurrent;

namespace PromoCodeFactory.DataAccess.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ConcurrentDictionary<Guid, T> _data;

    public InMemoryRepository(IEnumerable<T> data)
    {
        _data = new ConcurrentDictionary<Guid, T>(data.Select(e => new KeyValuePair<Guid, T>(e.Id, e)));
    }
    public Task<IReadOnlyCollection<T>> GetAll(CancellationToken ct)
    {
        return Task.FromResult((IReadOnlyCollection<T>)_data.Values);
    }

    public Task<T?> GetById(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _data.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task Add(T entity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_data.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"Сотрудник с ID - { entity.Id } уже существует");

        return Task.FromResult(entity);
    }

    public Task Update(T entity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_data.ContainsKey(entity.Id))
        {
            throw new EntityNotFoundException<T>(entity.Id);
        }

        _data[entity.Id] = entity;

        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_data.TryRemove(id, out _))
            throw new EntityNotFoundException<T>(id);

        return Task.CompletedTask;
    }
}
