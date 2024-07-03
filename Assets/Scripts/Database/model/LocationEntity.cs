using SQLite4Unity3d;

public class LocationEntity {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}