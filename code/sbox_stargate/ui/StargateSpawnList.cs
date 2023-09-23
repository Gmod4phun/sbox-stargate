using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Linq;

[Library]
public partial class StargateSpawnList : Panel
{
	private VirtualScrollPanel _canvas;

	public StargateSpawnList()
	{
		AddClass( "spawnpage" );
		AddChild( out _canvas, "canvas" );

		_canvas.Layout.AutoColumns = true;
		_canvas.Layout.ItemWidth = 128;
		_canvas.Layout.ItemHeight = 128;

		_canvas.OnCreateCell = ( cell, data ) =>
		{
			if ( data is TypeDescription type )
			{
				var btn = cell.Add.Button( type.Title );
				btn.AddClass( "icon" );
				btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", type.ClassName ) );
				btn.Style.BackgroundImage = Texture.Load( FileSystem.Mounted, $"/entity/sbox_stargate/{type.ClassName}.png", false );

				var mar = Length.Pixels( 16 );
				cell.Style.PaddingBottom = mar;
			}
		};

		var ents = TypeLibrary.GetTypes<Entity>().Where( x => x.HasTag( "spawnable" ) && x.Group != null && x.Group.StartsWith( "Stargate" ) ).OrderBy( x => x.Title ).ToArray();

		foreach ( var entry in ents )
		{
			_canvas.AddItem( entry );
		}
	}

	[Event( "sandbox.hud.loaded" )]
	public static void Initialize()
	{
		var spawnList = SpawnMenu.Instance.SpawnMenuLeftBody.AddChild<StargateSpawnList>();
		SpawnMenu.Instance.SpawnMenuLeftTabs
			.AddButtonActive( "Stargate", ( b ) => spawnList.SetClass( "active", b ) );
	}
}
