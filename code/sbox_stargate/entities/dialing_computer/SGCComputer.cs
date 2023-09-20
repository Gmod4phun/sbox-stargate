using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

[Title( "SGC Computer" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class SGCComputer : ModelEntity, IUse
{
	public static readonly Color ColorSgBlue = Color.FromBytes( 0, 170, 185 );
	public static readonly Color ColorSgYellow = Color.FromBytes( 225, 225, 170 );

	private Sound _alarmSound;

	[Net, Change]
	public Stargate Gate { get; set; } = null;

	[Net]
	public IList<SGCMonitor> Monitors { get; private set; } = new List<SGCMonitor>();

	public static float GetSinFromTime()
	{
		var s = (float)Math.Sin( Time.Now );
		return s * s;
	}

	public override void Spawn()
	{
		base.Spawn();

		Scale = 4;

		Transmit = TransmitType.Always;
		SetModel( "models/editor/ortho.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, new Vector3( -5, -5, -5 ), new Vector3( 5, 5, 5 ) );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		RenderColor = Color.Black;

		Tags.Add( "solid" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is Stargate gate )
		{
			Gate = gate;
		}
	}

	public void AddMonitor( SGCMonitor monitor )
	{
		if ( !Monitors.Contains( monitor ) )
			Monitors.Add( monitor );
	}

	public void RemoveMonitor( SGCMonitor monitor )
	{
		Monitors.Remove( monitor );
	}

	public bool OnUse( Entity user )
	{
		// turn on/off?

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		StopAlarmSound();
	}

	private void OnGateChanged( Stargate oldGate, Stargate newGate )
	{
		// update monitors?
	}

	// Alarm sound
	private void PlayAlarmSound()
	{
		StopAlarmSound();
		_alarmSound = Sound.FromEntity( "sg.alarm.sgc", this );
	}

	private void StopAlarmSound()
	{
		_alarmSound.Stop();
	}

	// RPC's

	[ClientRpc]
	private void DialProgramEncodeBoxAppear( char sym )
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.EncodeBoxAppear( sym );
			}
		}
	}

	[ClientRpc]
	private void DialProgramReturnToIdle()
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.ReturnToIdle();
			}
		}
	}

	[ClientRpc]
	private void DialProgramEncodeBoxMove( int num, bool last )
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.EncodeBoxMove( num, last );
			}
		}
	}

	[ClientRpc]
	private void DialProgramIndicatorBlink()
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.IndicatorBlink();
			}
		}
	}

	[ClientRpc]
	private void DialProgramAddGlyph( char sym )
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.AddGlyphToAddress( sym );
			}
		}
	}

	[ClientRpc]
	private void DialProgramBlinkAddressBoxes()
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.AddressBoxesBlink();
			}
		}
	}

	[ClientRpc]
	private void DialProgramBox_89_Appear( int num )
	{
		foreach ( var monitor in Monitors )
		{
			foreach ( var program in monitor.Programs.OfType<SGCProgram_Dialing>() )
			{
				program.Box_89_Appear( num );
			}
		}
	}

	// Events

	[StargateEvent.GateClosed]
	private void GateClosed( Stargate gate )
	{
		if ( gate != Gate ) return;

		DialProgramReturnToIdle( To.Everyone );
		StopAlarmSound();
	}

	[StargateEvent.ChevronEncoded]
	private void ChevronEncoded( Stargate gate, int num )
	{
		if ( gate != Gate ) return;

		if ( Gate.CurDialType == Stargate.DialType.SLOW )
			DialProgramEncodeBoxMove( To.Everyone, num, false );
		else
		{
			DialProgramAddGlyph( To.Everyone, Gate.CurDialingSymbol );
			if ( num == 7 )
				DialProgramBox_89_Appear( To.Everyone, 8 );
			else if ( num == 8 )
				DialProgramBox_89_Appear( To.Everyone, 9 );
		}
	}

	[StargateEvent.ChevronLocked]
	private void ChevronLocked( Stargate gate, int num, bool valid )
	{
		if ( gate != Gate ) return;

		if ( valid )
			if ( Gate.CurDialType == Stargate.DialType.SLOW )
				DialProgramEncodeBoxMove( To.Everyone, num, true );
			else
			{
				DialProgramAddGlyph( To.Everyone, Gate.CurDialingSymbol );
				DialProgramBlinkAddressBoxes( To.Everyone );
			}
	}

	[StargateEvent.DHDChevronEncoded]
	private void DHDChevronEncoded( Stargate gate, char sym )
	{
		if ( gate != Gate ) return;

		Log.Info( $"Stargate {gate} has DHD chevron encoded with sym {sym}" );
	}

	[StargateEvent.DHDChevronLocked]
	private void DHDChevronLocked( Stargate gate, char sym, bool valid )
	{
		if ( gate != Gate ) return;

		Log.Info( $"Stargate {gate} has DHD {(valid ? "valid" : "invalid")} chevron locked with sym {sym}" );
	}

	[StargateEvent.DHDChevronUnlocked]
	private void DHDChevronUnlocked( Stargate gate, char sym )
	{
		if ( gate != Gate ) return;

		Log.Info( $"Stargate {gate} has DHD chevron unlocked with sym {sym}" );
	}

	[StargateEvent.RingSpinDown]
	private void RingSpinDown( Stargate gate )
	{
		if ( gate != Gate ) return;

		if ( !Gate.ShouldStopDialing && Gate.CurDialType == Stargate.DialType.SLOW )
			DialProgramIndicatorBlink( To.Everyone );
	}

	[StargateEvent.ReachedDialingSymbol]
	private void ReachedDialingSymbol( Stargate gate, char sym )
	{
		if ( gate != Gate ) return;

		if ( Gate.CurDialType == Stargate.DialType.SLOW )
			DialProgramEncodeBoxAppear( To.Everyone, sym );
	}

	[StargateEvent.DialBegin]
	private void DialBegin( Stargate gate, string address )
	{
		if ( gate != Gate ) return;

		PlayAlarmSound();
		DialProgramReturnToIdle( To.Everyone );
	}

	[StargateEvent.InboundBegin]
	private void InboundBegin( Stargate gate )
	{
		if ( gate != Gate ) return;

		PlayAlarmSound();
		DialProgramReturnToIdle( To.Everyone );
	}

	[StargateEvent.DialAbortFinished]
	private void DialAbortFinished( Stargate gate )
	{
		if ( gate != Gate ) return;

		StopAlarmSound();
		DialProgramReturnToIdle( To.Everyone );
	}

	[StargateEvent.Reset]
	private void Reset( Stargate gate )
	{
		if ( gate != Gate ) return;

		StopAlarmSound();
		DialProgramReturnToIdle( To.Everyone );
	}
}
