using System.Collections.Generic;
using Sandbox;

[Category( "Transportation Rings" )]
public partial class RingPanel : Prop
{
	public Dictionary<string, RingPanelButton> Buttons { get; protected set; } = new();
	protected TimeSince TimeSinceButtonPressed { get; set; } = 0;
	protected float ButtonPressDelay { get; set; } = 0.35f;
	protected float ButtonGlowDelay { get; set; } = 0.2f;
	protected string ComposedAddress { get; private set; } = "";
	protected virtual string[] ButtonsSounds { get; } = { "goauld_button1", "goauld_button2" };
	protected virtual string ValidButtonActions => "12345678";

	public RingPanelButton GetButtonByAction( string action )
	{
		return Buttons.GetValueOrDefault( action );
	}

	public void SetButtonState( RingPanelButton b, bool glowing )
	{
		if ( b.IsValid() ) b.On = glowing;
	}

	public void SetButtonState( string action, bool glowing )
	{
		var b = GetButtonByAction( action );
		SetButtonState( b, glowing );
	}

	public void ResetAddress()
	{
		ComposedAddress = "";
	}

	public void TriggerAction( string action ) // this gets called from the Panel Button after pressing it
	{
		if ( TimeSinceButtonPressed < ButtonPressDelay ) return;

		if ( ValidButtonActions.Contains( action ) || action is "DIAL" )
		{
			if ( action is "DIAL" ) // we pressed dial button
			{
				Rings ringPlatform = Rings.GetClosestRing( Position, null, 500f );
				if ( ringPlatform.IsValid() )
				{
					if ( ComposedAddress.Length is 0 )
					{
						ringPlatform.DialClosest();
					}
					else
					{
						ringPlatform.DialAddress( ComposedAddress );
						ResetAddress();
					}
				}
			}
			else // we pressed number action button
			{
				ComposedAddress += action;
			}

			ToggleButton( action );
			TimeSinceButtonPressed = 0;
		}
		else
		{
			return;
		}
	}

	public void ButtonResetThink()
	{
		if ( TimeSinceButtonPressed > 5 && ComposedAddress != "" ) ResetAddress();
	}

	[GameEvent.Tick.Server]
	public void Think()
	{
		ButtonResetThink();
	}

	protected async void ToggleButton( string action )
	{
		SetButtonState( action, true );
		PlaySound( action is not "DIAL" ? ButtonsSounds[1] : ButtonsSounds[0] );

		await GameTask.DelaySeconds( ButtonGlowDelay );

		SetButtonState( action, false );
	}
}
