using SQLite4Unity3d;

public class BoardEntity {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
}