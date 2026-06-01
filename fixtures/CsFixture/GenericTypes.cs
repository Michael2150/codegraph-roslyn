using System.Collections.Generic;
using System.Linq;

namespace Fixtures
{
    // Covers: generic interface with type constraint
    public interface IRepository<T> where T : class
    {
        T? GetById(int id);
        IEnumerable<T> GetAll();
        void Save(T entity);
        void Delete(int id);
    }

    // Covers: generic abstract class implementing generic interface,
    //         Dictionary<,> field, abstract method
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        private readonly Dictionary<int, T> _store = new();

        public T? GetById(int id) =>
            _store.TryGetValue(id, out var item) ? item : null;

        public IEnumerable<T> GetAll() => _store.Values;

        public virtual void Save(T entity)
        {
            var id = GetId(entity);
            _store[id] = entity;
        }

        public virtual void Delete(int id) => _store.Remove(id);

        protected abstract int GetId(T entity);
    }

    // Simple entity class used as the generic type argument
    public class Customer
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Covers: concrete generic subclass, iterator method (yield return)
    public class CustomerRepository : RepositoryBase<Customer>
    {
        protected override int GetId(Customer entity) => entity.Id;

        public IEnumerable<Customer> FindByName(string name)
        {
            foreach (var c in GetAll())
                if (c.Name.Contains(name))
                    yield return c;
        }

        public override void Save(Customer entity)
        {
            ValidateName(entity);
            base.Save(entity);
        }

        private void ValidateName(Customer entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new System.ArgumentException("Customer name is required");
        }
    }

    // Covers: static generic class, extension methods (this parameter),
    //         generic method with constraint, Func<T,TResult> parameter
    public static class CollectionExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
        {
            foreach (var item in source)
                if (item is not null)
                    yield return item;
        }

        public static TResult Pipe<T, TResult>(this T value, Func<T, TResult> transform) =>
            transform(value);

        public static IEnumerable<T> Paginate<T>(
            this IEnumerable<T> source, int page, int pageSize) =>
            source.Skip(page * pageSize).Take(pageSize);
    }
}
