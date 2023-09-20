using Sandbox;
using Sandbox.UI;

public class SGCMonitorHUDPanel : Panel
{
	private SGCMonitor Monitor;
	private KeyboardDialing Keyboard;

	public SGCMonitorHUDPanel( SGCMonitor monitor, SGCProgram program )
	{
		Monitor = monitor;

		StyleSheet.Load( "sbox_stargate/entities/dialing_computer/SGCMonitorHUDPanel.scss" );
		var programscreen = Add.Panel( "programscreen" );
		programscreen.AddChild( program );

		if ( program is SGCProgram_Dialing dialprog )
		{
			Keyboard = new KeyboardDialing();
			Keyboard.Program = dialprog;
			Keyboard.AddClass( "keyboard hidden" );

			programscreen.AddChild( Keyboard );

			AddKeyboardEvent();
		}

		AddEventListener( "onrightclick", () =>
			{
				SGCMonitor.KickCurrentUser( Monitor.NetworkIdent );
			}
		);
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Monitor.IsValid() )
		{
			Delete( true );
			return;
		}
	}

	public void ClosePanel()
	{
		Monitor.ViewPanelOnWorld( To.Single( Game.LocalClient ) );
	}

	private async void AddKeyboardEvent()
	{
		await GameTask.DelaySeconds( 0.1f );

		var drawer = Keyboard.Drawer;
		if ( !drawer.IsValid() )
			return;

		Keyboard.Drawer.AddEventListener( "onclick", () =>
			{
				Keyboard.SetClass( "hidden", !Keyboard.HasClass( "hidden" ) );
			}
		);
	}
}
