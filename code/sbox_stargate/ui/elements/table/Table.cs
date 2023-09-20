using Sandbox.UI;
using Sandbox.UI.Tests;

public partial class Table : Panel
{
	public TableHead Head = new();
	public VirtualScrollPanel Rows;

	public Table()
	{
		AddClass( "table" );
		AddChild( out Rows, "table-rows" );
	}

	public void SetColumns( string[] columns )
	{
		foreach ( string col in columns )
		{
			Head.AddColumn( col );
		}
	}

	public override void SetProperty( string name, string value )
	{
		Log.Info( name );
		Log.Info( value );
	}

	public override void SetContent( string content )
	{
		Log.Info( content );
	}

	//public override bool OnTemplateElement( INode element )
	//{
	//	foreach (INode child in element.Children) {
	//		if (child.Name == "tablehead") {
	//			Head.OnTemplateElement(child);
	//			AddChild(Head);
	//		}
	//	}
	//	return true;
	//}
}
