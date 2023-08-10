using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class StargateRingMilkyWay : StargatePlatformEntity
{
	// ring variables

	[Net]
	public Stargate Gate { get; set; } = null;
	[Net]
	public string RingSymbols { get; set; } = "#0JKNTR3MBZX*H69IGPL @QFS1E4AU85OCW72YVD";
	[Net]
	public float RingAngle { get; private set; } = 0.0f;
	[Net]
	public float DesiredRingAngleDifference { get; private set; } = 0.0f;
	[Net]
	public char CurDialingSymbol { get; private set; } = ' ';
	[Net]
	public char CurRingSymbol { get; private set; } = ' ';
	public float TargetRingAngle { get; private set; } = 0.0f;

	private float RingCurSpeed = 0f;
	protected float RingMaxSpeed = 50f;
	protected float RingAccelStep = 1f;
	protected float RingDeccelStep = 0.75f;
	protected float RingAngToRotate = 170f;
	protected float RingTargetAngleOffset = 0.5f;

	private int RingDirection = 1;
	private bool ShouldAcc = false;
	private bool ShouldDecc = false;

	private bool ShouldStopAtAngle = false;
	private float CurStopAtAngle = 0f;

	private float StartedAccelAngle = 0f;
	private float StoppedAccelAngle = 0f;

	public string StartSoundName = "stargate.milkyway.ring_start_long";
	public string StopSoundName = "stargate.milkyway.ring_stop";

	protected Sound? StartSoundInstance;
	protected Sound? StopSoundInstance;

	public bool StopSoundOnSpinDown = false; // play the stopsound on spindown, or on spin stop

	private bool hasReachedDialingSymbol = false;

	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_sg1/ring_sg1.vmdl" );
		
		LoopMovement = true;
		MoveDirType = PlatformMoveType.RotatingContinious;
		MoveDirIsLocal = true;
		MoveDir = Rotation.Up.EulerAngles;
		MoveDistance = 360;
		StartsMoving = false;

		base.Spawn();

		EnableAllCollisions = false;
	}

	protected override void OnDestroy()
	{
		if ( StartSoundInstance.HasValue ) StartSoundInstance.Value.Stop();
		if ( StopSoundInstance.HasValue ) StopSoundInstance.Value.Stop();

		base.OnDestroy();
	}

	// symbol pos/ang
	public virtual float GetSymbolPosition( char sym ) // gets the symbols position on the ring
	{
		sym = sym.ToString().ToUpper()[0];
		return RingSymbols.Contains( sym ) ? RingSymbols.IndexOf( sym ) : -1;
	}

	public virtual float GetSymbolAngle( char sym ) // gets the symbols angle on the ring
	{
		sym = sym.ToString().ToUpper()[0];
		return GetSymbolPosition( sym ) * (360f / RingSymbols.Length);
	}

	// sounds
	public void StopStartSound()
	{
		if ( StartSoundInstance.HasValue )
		{
			StartSoundInstance.Value.SetVolume( 0 );
		}
	}

	public void PlayStartSound()
	{
		StopStartSound();
		StartSoundInstance = PlaySound( StartSoundName );
	}

	public void StopStopSound()
	{
		if ( StopSoundInstance.HasValue ) StopSoundInstance.Value.SetVolume( 0 );
	}

	public void PlayStopSound()
	{
		StopStopSound();
		StopSoundInstance = PlaySound( StopSoundName );
	}

	// spinup/spindown - starts or stops rotating the ring
	public void SpinUp()
	{
		ShouldDecc = false;
		ShouldAcc = true;

		Event.Run( StargateEvent.RingSpinUp, Gate );
	}

	TimeSince lastSpinDown = 0;
	public void SpinDown()
	{
		if ( lastSpinDown < 1 )
			return;

		lastSpinDown = 0;
		ShouldAcc = false;
		ShouldDecc = true;

		Event.Run( StargateEvent.RingSpinDown, Gate );

		if ( StopSoundOnSpinDown )
		{
			PlayStopSound();
			StopStartSound();
		}
	}

	public void OnStart()
	{
		PlayStartSound();
	}

	public void OnStop()
	{
		if (Gate.IsValid())
		{
			hasReachedDialingSymbol = false;
			Event.Run( StargateEvent.RingStopped, Gate );
			if ( !StopSoundOnSpinDown )
			{
				PlayStopSound();
				StopStartSound();
			}
		}
	}

	// rotate to angle/symbol
	public virtual void RotateRingTo( float targetAng ) // starts rotating the ring and stops (hopefully) at the specified angle
	{
		TargetRingAngle = targetAng;
		ShouldStopAtAngle = true;
		SpinUp();
	}

	public virtual void RotateRingToSymbol( char sym, int angOffset = 0 )
	{
		if ( RingSymbols.Contains( sym ) ) RotateRingTo( GetDesiredRingAngleForSymbol( sym, angOffset ) );
	}

	// helper calcs
	public virtual float GetDesiredRingAngleForSymbol( char sym, int angOffset = 0 )
	{
		//if ( sym is '#' && (Gate.IsValid() && Gate.EarthPointOfOrigin) ) sym = '?';
		// eliminate custom points of origin for now, use just Giza, we will only have one (probably will have Abydos/Beta switchable on the gate model tho)

		// get the symbol's position on the ring
		var symPos = GetSymbolPosition( sym );

		// if we input an invalid symbol, return current ring angles
		if ( symPos == -1 ) return RingAngle;

		// if its a valid symbol, lets calc the required angle
		//var symAng = symPos * 9; // there are 40 symbols, each 9 degrees apart
		var symAng = GetSymbolAngle( sym );

		// clockwise and counterclockwise symbol angles relative to 0 (the top chevron)
		var D_CW = -symAng - RingAngle - angOffset; // offset, if we want it to be relative to another chevron (for movie stargate dialing)
		var D_CCW = 360 - D_CW;

		D_CW = D_CW.UnsignedMod( 360 );
		D_CCW = D_CCW.UnsignedMod( 360 );

		// angle differences are setup, choose based on the direction of ring rotation
		// if the required angle to too small, spin it around once
		var angToRotate = (RingDirection == -1) ? D_CCW : D_CW;
		if ( angToRotate < RingAngToRotate ) angToRotate += 360f;

		// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
		var finalAng = RingAngle + (angToRotate * RingDirection);

		//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

		return finalAng;
	}

	public async Task<bool> RotateRingToSymbolAsync( char sym, int angOffset = 0 )
	{
		RotateRingToSymbol( sym, angOffset );
		CurDialingSymbol = sym;
		Gate.CurDialingSymbol = CurDialingSymbol;

		await GameTask.DelaySeconds( Game.TickInterval ); // wait, otherwise it hasnt started moving yet and can cause issues

		while (IsMoving)
		{
			await GameTask.DelaySeconds( Game.TickInterval ); // wait here, too, otherwise game hangs :)
			if ( !this.IsValid() ) return false;

			if ( Gate.ShouldStopDialing )
			{
				Gate.StopDialing();
				return false;
			}
		}

		return true;
	}

	public void RingSymbolThink() // keeps track of the current symbol under the top chevron
	{
		var symRange = 360f / RingSymbols.Length;
		var symCoverage = (RingAngle + symRange/2f).UnsignedMod( symRange );
		var symIndex = ((int) Math.Round((-RingAngle + Gate.CurRingSymbolOffset) / ( symRange ))).UnsignedMod( RingSymbols.Length );
		
		CurRingSymbol = (symCoverage < 8 && symCoverage > 1) ? RingSymbols[symIndex] : ' ';
		Gate.CurRingSymbol = CurRingSymbol;
	}

	public void RingRotationThink()
	{
		if ( !Gate.IsValid() ) return;

		if ( IsMoving && Gate.ShouldStopDialing )
		{
			if ( !ShouldDecc )
			{
				Gate.StopDialing();
			}
		}

		if ( ShouldAcc )
		{
			if ( !IsMoving )
			{
				StartedAccelAngle = RingAngle;
				StartMoving();
				OnStart();
			}

			if ( RingCurSpeed < RingMaxSpeed )
			{
				RingCurSpeed += RingAccelStep;
			}
			else
			{
				RingCurSpeed = RingMaxSpeed;
				ShouldAcc = false;
				StoppedAccelAngle = MathF.Abs( RingAngle - StartedAccelAngle ) + RingTargetAngleOffset;
				CurStopAtAngle = TargetRingAngle - (StoppedAccelAngle * RingDirection * (RingAccelStep / RingDeccelStep));
			}
		}
		else if ( ShouldDecc )
		{
			if ( RingCurSpeed > 0 )
			{
				RingCurSpeed -= RingDeccelStep;

				if (!hasReachedDialingSymbol)
				{
					if ( CurRingSymbol == CurDialingSymbol && Gate.Dialing )
					{
						hasReachedDialingSymbol = true;
						Event.Run( StargateEvent.ReachedDialingSymbol, Gate, CurDialingSymbol );
					}
				}
			}
			else
			{
				RingCurSpeed = 0;
				ShouldDecc = false;
				StopMoving();
				OnStop();

				ReverseMoving();
				CurrentRotation %= 360f;
			}
		}

		SetSpeed( RingCurSpeed );

		if ( ShouldStopAtAngle && IsMoving )
		{
			if ( !ShouldAcc && !ShouldDecc )
			{
				var angDiff = MathF.Abs( CurrentRotation - CurStopAtAngle );
				//Log.Info( $"RingAng={RingAngle}, AngDiff={angDiff}" );
				DesiredRingAngleDifference = angDiff;
				if ( angDiff < 1f ) // if the angle difference is smal enough, start spindown
				{
					SpinDown();
					ShouldStopAtAngle = false;
				}
			}
		}

		RingAngle = CurrentRotation;
		RingDirection = IsMovingForwards ? 1 : -1;

		//Log.Info( $"Speed={RingCurSpeed}, Ang={RingAngle}" );

		//Log.Info( DesiredRingAngleDifference );
	}

	[GameEvent.Tick.Server]
	public void Think()
	{
		RingRotationThink();
		RingSymbolThink();
	}

	// DEBUG
	public void DrawSymbols()
	{
		var deg = 360f / RingSymbols.Length;
		var i = 0;
		var ang = Rotation.Angles();
		foreach ( char sym in RingSymbols )
		{
			var rotAng = ang.WithRoll( ang.roll - (i * deg) );
			var newRot = rotAng.ToRotation();
			var pos = Position + newRot.Forward * 4 + newRot.Up * 117.5f;
			//if ( sym == CurDialingSymbol )
			//{
				DebugOverlay.Text( sym.ToString(), pos, sym == CurDialingSymbol ? Color.Green : Color.Yellow );
			//}
			i++;
		}

		DebugOverlay.Text( CurRingSymbol.ToString(), Position, Color.White, 0, 512 );
	}

	//[GameEvent.Client.Frame]
	public void RingSymbolsDebug()
	{
		DrawSymbols();
	}
}
