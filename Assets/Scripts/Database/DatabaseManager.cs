using SQLite4Unity3d;
using UnityEngine;

public class DatabaseManager
{
    private SQLiteConnection _connection;
    public BoardRepository VirtualBoards { get; private set; }
    public NoteRepository Notes { get; private set; }
    public LocationRepository Locations { get; private set; }

    public DatabaseManager(string databaseName) {
        string dbPath = string.Format("{0}/{1}.db", Application.persistentDataPath, databaseName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        Debug.Log("Database path: " + dbPath);

        // Cria as tabelas se elas não existirem
        _connection.CreateTable<BoardEntity>();
        _connection.CreateTable<NoteEntity>();
        _connection.CreateTable<LocationEntity>();

        VirtualBoards = new BoardRepository(_connection);
        Notes = new NoteRepository(_connection);
        Locations = new LocationRepository(_connection);
    }
}
