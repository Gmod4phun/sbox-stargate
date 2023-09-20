using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Collections.Generic;
using System.Linq;

[Library( Title = "Stargate Addon" )]
public partial class StargateList : Panel
{
	//VirtualScrollPanel Canvas;

	private string[] categories = { "Stargate", "Rings", "Weapons", "Other" };

	public StargateList()
	{
		AddClass( "spawnpage" );

		StyleSheet.Load( "sbox_stargate/ui/elements/stargatelist/stargatelist.scss" );

		Dictionary<string, VirtualScrollPanel> CategoriesCanvas = new();

		foreach ( string cat in categories )
		{
			Add.Label( cat, "category" );
			var can = AddChild<VirtualScrollPanel>( "canvas" );

			can.Layout.AutoColumns = true;
			//can.Layout.ItemSize = new Vector2( 120, 120 );
			can.Layout.ItemWidth = 120;
			can.Layout.ItemHeight = 120;
			can.OnCreateCell = ( cell, data ) =>
			{
				var entry = (LibraryAttribute)data;

				var btn = cell.Add.Button( entry.Title );
				btn.AddClass( "icon" );
				btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", entry.Name ) );
				//btn.Style.Background = new PanelBackground
				//{
				//	Texture = Texture.Load( $"/entity/sbox_stargate/{entry.Name}.png", false )
				//};

				btn.Style.BackgroundImage = Texture.Load( $"/entity/sbox_stargate/{entry.Name}.png", false );
			};

			CategoriesCanvas.Add( cat, can );
		}

		var ents = TypeLibrary.GetTypes<Entity>().Where( x => x.HasTag( "spawnable" ) && x.Group != null && x.Group.StartsWith( "Stargate" ) ).OrderBy( x => x.Title ).ToArray();

		foreach ( var entry in ents )
		{
			var parse = entry.Group.Split( "Stargate." );
			if ( parse.Length > 1 && CategoriesCanvas[parse[1]] != null )
			{
				CategoriesCanvas[parse[1]].AddItem( entry );
			}
			else
			{
				CategoriesCanvas["Other"].AddItem( entry );
			}

			// Canvas.AddItem( entry );
		}
	}
}
