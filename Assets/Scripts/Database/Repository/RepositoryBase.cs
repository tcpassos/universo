using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

public abstract class RepositoryBase<T> where T : new()
{
    protected SQLiteConnection _connection;

    public RepositoryBase(SQLiteConnection connection)
    {
        _connection = connection;
    }

    public void Save(T entity)
    {
        _connection.Insert(entity);
    }

    public List<T> GetAll()
    {
        return _connection.Table<T>().ToList<T>();
    }

    public void Update(T entity)
    {
        _connection.Update(entity);
    }

    public void Delete(int id)
    {
        _connection.Delete<T>(id);
    }
}
