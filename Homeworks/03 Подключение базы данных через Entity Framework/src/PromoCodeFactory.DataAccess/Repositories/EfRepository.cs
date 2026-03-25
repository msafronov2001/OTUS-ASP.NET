using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Exceptions;
using System.Linq.Expressions;

namespace PromoCodeFactory.DataAccess.Repositories;

internal class EfRepository<T>(PromoCodeFactoryDbContext context) : IRepository<T> where T : BaseEntity
{

    protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query) => query;

    public async Task Add(T entity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        await context.Set<T>().AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);

    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);
        if(entity != null)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync(ct);
        }
        else
        {
            throw new EntityNotFoundException<T>(id);
        }
    }

    public async Task<IReadOnlyCollection<T>> GetAll(bool withIncludes = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        IQueryable<T> query = context.Set<T>().AsNoTracking();
        if (withIncludes)
        {
            query = ApplyIncludes(query);
        }    

        return await query.ToListAsync(ct);
    }

    public async Task<T?> GetById(Guid id, bool withIncludes = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        IQueryable<T> query = context.Set<T>();
        if (withIncludes)
        {
            query = ApplyIncludes(query);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<T>> GetByRangeId(IEnumerable<Guid> ids, bool withIncludes = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var idList = ids as ICollection<Guid> ?? ids.ToList();

        IQueryable<T> query = context.Set<T>();
        if (withIncludes)
        {
            query = ApplyIncludes(query);
        }

        return await query.Where(e => idList.Contains(e.Id)).ToListAsync(ct);

    }

    public async Task<IReadOnlyCollection<T>> GetWhere(Expression<Func<T, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        IQueryable<T> query = context.Set<T>().AsNoTracking().Where(predicate);
        if (withIncludes)
        {
            query = ApplyIncludes(query);
        }

        return await query.ToListAsync(ct);

    }

    public async Task Update(T entity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var exists = await context.Set<T>().AsNoTracking().AnyAsync(e => e.Id == entity.Id, ct);
        if (!exists)
            throw new EntityNotFoundException<T>(entity.Id);

        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(ct);
    }

}
