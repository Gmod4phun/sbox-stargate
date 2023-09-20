using Sandbox;
using Sandbox.UI;

public class SGCMonitorHUDPanel : Panel
{
	private SGCMonitor _monitor;
	private KeyboardDialing _keyboard;

	public SGCMonitorHUDPanel( SGCMonitor monitor, SGCProgram program )
	{
		_monitor = monitor;

		StyleSheet.Load( "sbox_stargate/entities/dialing_computer/SGCMonitorHUDPanel.scss" );
		var programscreen = Add.Panel( "programscreen" );
		programscreen.AddChild( program );

		if ( program is SGCProgram_Dialing dialprog )
		{
			_keyboard = new KeyboardDialing();
			_keyboard.Program = dialprog;
			_keyboard.AddClass( "keyboard hidden" );

			programscreen.AddChild( _keyboard );

			AddKeyboardEvent();
		}

		AddEventListener( "onrightclick", () =>
			{
				SGCMonitor.KickCurrentUser( _monitor.NetworkIdent );
			}
		);
	}

	public override void Tick()
	{
		base.Tick();

		if ( !_monitor.IsValid() )
		{
			Delete( true );
			return;
		}
	}

	public void ClosePanel()
	{
		_monitor.ViewPanelOnWorld( To.Single( Game.LocalClient ) );
	}

	private async void AddKeyboardEvent()
	{
		await GameTask.DelaySeconds( 0.1f );

		var drawer = _keyboard.Drawer;
		if ( !drawer.IsValid() )
			return;

		_keyboard.Drawer.AddEventListener( "onclick", () =>
			{
				_keyboard.SetClass( "hidden", !_keyboard.HasClass( "hidden" ) );
			}
		);
	}
}
