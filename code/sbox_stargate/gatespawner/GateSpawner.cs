using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox;

public partial class GateSpawner
{
	[ConVar.Replicated( "gatespawner_enabled" )]
	public static bool Enabled { get; set; }

	public static List<Entity> GateSpawnerEntites = new();

	public static void CreateGateSpawner()
	{
		var fileName = Game.Server.MapIdent;

		if ( !FileSystem.Data.DirectoryExists( "data/gatespawner/" ) )
			FileSystem.Data.CreateDirectory( "data/gatespawner/" );

		var model = new GatespawnerModel();
		foreach ( IGateSpawner e in Entity.All.OfType<IGateSpawner>() )
		{
			model.Entities.Add( e.ToJson() );
		}
		FileSystem.Data.WriteAllText( $"data/gatespawner/{fileName}.json", JsonSerializer.Serialize( model, new JsonSerializerOptions() { WriteIndented = true } ) );
		Log.Info("Created GateSpawner file.");
	}

	//[Event.Hotload]
	public static void GetGatespawnerFileByName(string mapName)
	{
		var fList = new List<string>();
		foreach ( var dir in FileSystem.Mounted.FindDirectory( "", recursive: true ) )
		{
			foreach ( var file in FileSystem.Mounted.FindFile( dir, $"{mapName}.json", true ) )
			{
				var targetName = ( dir + "/" + file );
				if ( !fList.Contains( targetName ) )
					fList.Add( targetName );
			}
		}

		foreach ( var fName in fList )
			Log.Info( fName );
	}

	public static async void LoadGateSpawner()
	{
		if ( !Enabled )
		{
			NotEnabledMessage();
			return;
		}

		UnloadGateSpawner(); // unload it before loading, we dont want to have multiple instances of loaded entities

		await GameTask.Delay( 1000 );

		var mapName = Game.Server.MapIdent;

		Log.Info( $"Trying to load GateSpawner for map: {mapName}" );

		var fileToLoad = "";
		var foundGatespawnerFile = false;

		foreach ( var fileName in FileSystem.Mounted.FindFile( "", recursive: true ).Where( f => f.Contains( $"{mapName}.json" ) ) )
		{
			fileToLoad = fileName;
			foundGatespawnerFile = true;
		}

		if ( !foundGatespawnerFile )
		{
			Log.Warning( $"Can't find GateSpawner file for {Game.Server.MapIdent}" );
			return;
		}

		Log.Info( $"Found GateSpawner file: {fileToLoad}" );

		var rawData = FileSystem.Mounted.ReadAllText( fileToLoad );
		var data = JsonSerializer.Deserialize<GatespawnerModel>( rawData );

		foreach ( JsonElement o in data.Entities )
		{
			string entityName = o.GetProperty( "EntityName" ).ToString();
			Entity e = TypeLibrary.Create<Entity>( entityName, false );
			if ( e is null || !e.IsValid() )
				continue;

			(e as IGateSpawner).FromJson( o );

			GateSpawnerEntites.Add( e );
			e.Tags.Add( "physgun-block" );
		}

		SuccessMessage();
	}

	public static void UnloadGateSpawner()
	{
		if ( !Enabled )
		{
			NotEnabledMessage();
			return;
		}

		var neededUnloading = GateSpawnerEntites.Count > 0;
		foreach ( var ent in GateSpawnerEntites )
		{
			if ( ent.IsValid() ) ent.Delete();
		}
		GateSpawnerEntites.Clear();

		if (neededUnloading)
			SuccessMessage( true );
	}

	private static void NotEnabledMessage()
	{
		Log.Warning("Can't proceed, GateSpawner is not enabled");
	}

	private static void SuccessMessage(bool unloaded = false)
	{
		var action = unloaded ? "unloaded" : "loaded";
		Log.Warning( $"GateSpawner successfully {action}" );
	}

	[ConCmd.Server( "gatespawner" )]
	public static void GateSpawnerCmd( string action )
	{
		switch ( action )
		{
			case "create":
				CreateGateSpawner();
				break;
			case "load":
				LoadGateSpawner();
				break;
			case "unload":
				UnloadGateSpawner();
				break;
		}
	}

	[GameEvent.Entity.PostSpawn]
	private static void PostSpawn()
	{
		LoadGateSpawner();
	}
}

public class GatespawnerModel
{
	public string Version { get; set; } = "0.0.1";

	public List<object> Entities { get; set; } = new();
}
