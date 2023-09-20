using Sandbox;
using Sandbox.UI;

public class SGCMonitorWorldPanel : WorldPanel
{
	public float RenderSize = 1600;
	public float ActualSize = 496;
	private SGCMonitor Monitor;

	private Panel ProgramScreen = null;

	public SGCMonitorWorldPanel( SGCMonitor monitor, SGCProgram program )
	{
		StyleSheet.Load( "sbox_stargate/entities/dialing_computer/SGCMonitorWorldPanel.scss" );

		Monitor = monitor;

		PanelBounds = new Rect( -RenderSize / 2, -RenderSize / 2, RenderSize, RenderSize );

		Add.Panel( "background" );
		ProgramScreen = Add.Panel( "programscreen" );

		AddClass( "SGCMonitorWorldPanel" );
		AddProgram( program );
	}

	public void AddProgram( SGCProgram program )
	{
		ProgramScreen.AddChild( program );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Monitor.IsValid() )
		{
			Delete();
			return;
		}

		Position = Monitor.Position;
		Rotation = Monitor.Rotation;

		var scaleFactor = ActualSize / RenderSize;

		Transform = Transform.WithScale( scaleFactor );
	}
}
