using SQLite4Unity3d;

public class BoardRepository : RepositoryBase<BoardEntity>
{
    public BoardRepository(SQLiteConnection connection) : base(connection) { }
}
