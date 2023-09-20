using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class StargateWorldPanel : WorldPanel
{
	public Stargate Gate;

	private Label Address;
	private Label Group;
	private Label IsLocal;

	public StargateWorldPanel( Stargate gate )
	{
		StyleSheet.Load( "/sbox_stargate/ui/StargateWorldPanel.scss" );

		Gate = gate;

		Address = Add.Label( "Address" );
		Group = Add.Label( "Group" );
		IsLocal = Add.Label( "Local" );

		float width = 2048;
		float height = 2048;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;
	}

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

		Address.Text = $"Address: {Gate.GateAddress}";
		Group.Text = $"Group: {Gate.GateGroup}";
		var localText = Gate.GateLocal ? "Yes" : "No";
		IsLocal.Text = $"Local: {localText}";
	}
}
