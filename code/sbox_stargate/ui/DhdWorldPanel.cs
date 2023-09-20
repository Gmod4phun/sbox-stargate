using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class DhdWorldPanel : WorldPanel
{
	private Vector3 _symbolPosition;

	public DhdWorldPanel( Dhd dhd, string symbol, Vector3 symbolPosition )
	{
		StyleSheet.Load( "/sbox_stargate/ui/DhdWorldPanel.scss" );

		Symbol = symbol;
		var lab = Symbol == "DIAL" ? "#" : Symbol;
		Add.Label( lab );

		float width = lab.Length == 1 ? 64 : 128;
		float height = 64;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;

		Dhd = dhd;
		_symbolPosition = symbolPosition;
		MaxInteractionDistance = 64;
	}

	public Dhd Dhd { get; set; }

	public string Symbol { get; private set; } = "";

	public override void Tick()
	{
		base.Tick();

		if ( !Dhd.IsValid() )
		{
			Delete();
			return;
		}

		Position = Dhd.Position + Dhd.Rotation.Forward * _symbolPosition.x + Dhd.Rotation.Left * _symbolPosition.y + Dhd.Rotation.Up * _symbolPosition.z;
		Rotation = Dhd.Rotation.RotateAroundAxis( Vector3.Right, 90 );
	}
}
