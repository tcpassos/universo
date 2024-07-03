using SQLite4Unity3d;

public class NoteEntity {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Content { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string Location { get; set; }
    public int VirtualBoardId { get; set; }
}