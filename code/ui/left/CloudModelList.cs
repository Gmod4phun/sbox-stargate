using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;
using System.Threading.Tasks;

[Library, UseTemplate]
public partial class CloudModelList : Panel
{
	public VirtualScrollPanel Canvas { get; set; }

	public CloudModelList()
	{
	}

	/*
	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemWidth = 100;
		Canvas.Layout.ItemHeight = 100;

		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var file = (Package)data;
			var panel = cell.Add.Panel( "icon" );
			panel.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn", file.FullIdent ) );
			panel.Style.BackgroundImage = Texture.Load( file.Thumb );
		};

		_ = UpdateItems();
	}
	*/

	public async Task UpdateItems( int offset = 0 )
	{
		var found = await Package.FindAsync("type: model", 200, offset);
		if (found != null )
		{
			Canvas.SetItems( found.Packages );
		}

		// TODO - auto add more items here
	}

	public void RefreshItems()
	{
		Canvas.Clear();
		_ = UpdateItems();
	}

}
