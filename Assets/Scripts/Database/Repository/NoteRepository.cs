using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

public class NoteRepository : RepositoryBase<NoteEntity>
{
    public NoteRepository(SQLiteConnection connection) : base(connection) { }

    public List<NoteEntity> GetNotesByBoardId(int boardId)
    {
        return _connection.Table<NoteEntity>().Where(note => note.VirtualBoardId == boardId).ToList<NoteEntity>();
    }
}
