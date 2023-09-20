using Sandbox;
using Sandbox.UI;

//[UseTemplate]
public class StargateMenuV2 : Panel
{
	private Stargate _gate;
	private Dhd _dhd;

	private Titlebar _menuBar;
	private StargateDialMenu _dialMenu;

	public StargateMenuV2( Stargate gate, Dhd dhd = null )
	{
		StyleSheet.Load( "sbox_stargate/ui/stargatemenu/StargateMenuV2.scss" );

		SetGate( gate );
		_dhd = dhd;

		_menuBar = AddChild<Titlebar>();
		_menuBar.SetTitle( true, "Stargate" );
		_menuBar.SetCloseButton( true, "X", () => CloseMenu() );

		_dialMenu = AddChild<StargateDialMenu>();
		_dialMenu.SetGate( gate );
		_dialMenu.SetDHD( dhd );
		_dialMenu.MenuPanel = this;
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
		if ( !_gate.IsValid() )
		{
			CloseMenu();
			return;
		}

		if ( !_dhd.IsValid() )
		{
			var dist = Game.LocalPawn.Position.Distance( _gate.Position );
			if ( dist > 220 * _gate.Scale ) CloseMenu();
		}
		else
		{
			var dist = Game.LocalPawn.Position.Distance( _dhd.Position );
			if ( dist > 80 * _dhd.Scale ) CloseMenu();
		}
	}

	public void SetGate( Stargate gate )
	{
		_gate = gate;
	}
}
