using Sandbox;
using Sandbox.UI;

//[UseTemplate]
public class StargateMenuV2 : Panel
{
	private Stargate Gate;
	private Dhd DHD;

	private Titlebar menuBar;
	private StargateDialMenu dialMenu;

	public StargateMenuV2( Stargate gate, Dhd dhd = null )
	{
		StyleSheet.Load( "sbox_stargate/ui/stargatemenu/StargateMenuV2.scss" );

		SetGate( gate );
		DHD = dhd;

		menuBar = AddChild<Titlebar>();
		menuBar.SetTitle( true, "Stargate" );
		menuBar.SetCloseButton( true, "X", () => CloseMenu() );

		dialMenu = AddChild<StargateDialMenu>();
		dialMenu.SetGate( gate );
		dialMenu.SetDHD( dhd );
		dialMenu.MenuPanel = this;
	}

	public void CloseMenu()
	{
		Blur(); // finally, this makes it lose focus
		Delete();
	}

	public override void Tick()
	{
		base.Tick();

		// closes menu if player goes too far -- in the future we will want to freeze player's input
		if ( !Gate.IsValid() )
		{
			CloseMenu();
			return;
		}

		if ( !DHD.IsValid() )
		{
			var dist = Game.LocalPawn.Position.Distance( Gate.Position );
			if ( dist > 220 * Gate.Scale ) CloseMenu();
		}
		else
		{
			var dist = Game.LocalPawn.Position.Distance( DHD.Position );
			if ( dist > 80 * DHD.Scale ) CloseMenu();
		}
	}

	public void SetGate( Stargate gate )
	{
		Gate = gate;
	}
}
