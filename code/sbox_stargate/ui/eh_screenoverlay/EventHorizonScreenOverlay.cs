using Sandbox;
using Sandbox.UI;

class EventHorizonScreenOverlay : Panel
{
	private float _startTime = 0;

	public EventHorizonScreenOverlay()
	{
		StyleSheet.Load( "sbox_stargate/ui/eh_screenoverlay/EventHorizonScreenOverlay.scss" );
		_startTime = Time.Now;
	}

	public override void Tick()
	{
		if ( Time.Now > _startTime + 0.05f ) SetClass( "hidden", true );
		if ( Time.Now > _startTime + 0.5f ) Delete( true );
		base.Tick();
	}
}
