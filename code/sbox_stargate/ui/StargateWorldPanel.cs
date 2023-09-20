using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class StargateWorldPanel : WorldPanel
{
	private Label _address;
	private Label _group;
	private Label _isLocal;

	public StargateWorldPanel( Stargate gate )
	{
		StyleSheet.Load( "/sbox_stargate/ui/StargateWorldPanel.scss" );

		Gate = gate;

		_address = Add.Label( "Address" );
		_group = Add.Label( "Group" );
		_isLocal = Add.Label( "Local" );

		float width = 2048;
		float height = 2048;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;
	}

	public Stargate Gate { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( !Gate.IsValid() )
		{
			Delete();
			return;
		}

		Position = Gate.Position + Gate.Rotation.Up * 172 + Gate.Rotation.Forward * 16;
		Rotation = Gate.Rotation;

		UpdateGateInfo();

		var player = Game.LocalPawn;
		if ( player == null ) return;

		//player.Position.DistanceSquared(Gate.Position))
	}

	private void UpdateGateInfo()
	{
		if ( !Gate.IsValid() ) return;

		_address.Text = $"Address: {Gate.GateAddress}";
		_group.Text = $"Group: {Gate.GateGroup}";
		var localText = Gate.GateLocal ? "Yes" : "No";
		_isLocal.Text = $"Local: {localText}";
	}
}
