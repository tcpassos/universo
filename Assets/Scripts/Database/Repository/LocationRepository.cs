using SQLite4Unity3d;

public class LocationRepository : RepositoryBase<LocationEntity>
{
    public LocationRepository(SQLiteConnection connection) : base(connection) { }
}
