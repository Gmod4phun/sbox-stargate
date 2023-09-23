using System.Text.Json;

public interface IGateSpawner
{
	object ToJson();

	void FromJson( JsonElement data );
}

public class JsonModel
{
	public string EntityName { get; set; }
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
}
