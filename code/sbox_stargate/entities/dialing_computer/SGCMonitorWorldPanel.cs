using Sandbox;
using Sandbox.UI;

public class SGCMonitorWorldPanel : WorldPanel
{
	private SGCMonitor _monitor;

	private Panel _programScreen = null;

	public SGCMonitorWorldPanel( SGCMonitor monitor, SGCProgram program )
	{
		StyleSheet.Load( "sbox_stargate/entities/dialing_computer/SGCMonitorWorldPanel.scss" );

		_monitor = monitor;

		PanelBounds = new Rect( -RenderSize / 2, -RenderSize / 2, RenderSize, RenderSize );

		Add.Panel( "background" );
		_programScreen = Add.Panel( "programscreen" );

		AddClass( "SGCMonitorWorldPanel" );
		AddProgram( program );
	}

	public float RenderSize { get; set; } = 1600;
	public float ActualSize { get; set; } = 496;

	public void AddProgram( SGCProgram program )
	{
		_programScreen.AddChild( program );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !_monitor.IsValid() )
		{
			Delete();
			return;
		}

		Position = _monitor.Position;
		Rotation = _monitor.Rotation;

		var scaleFactor = ActualSize / RenderSize;

		Transform = Transform.WithScale( scaleFactor );
	}
}
