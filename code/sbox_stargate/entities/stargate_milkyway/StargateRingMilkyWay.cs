using System;
using System.Threading.Tasks;
using Sandbox;

public partial class StargateRingMilkyWay : StargatePlatformEntity
{
	private float _ringCurSpeed = 0f;
	private int _ringDirection = 1;
	private bool _shouldAcc = false;
	private bool _shouldDecc = false;
	private bool _shouldStopAtAngle = false;
	private float _curStopAtAngle = 0f;
	private float _startedAccelAngle = 0f;
	private float _stoppedAccelAngle = 0f;
	private bool _hasReachedDialingSymbol = false;

	private TimeSince _lastSpinDown = 0;
	public string StartSoundName { get; set; } = "stargate.milkyway.ring_start_long";

	public string StopSoundName { get; set; } = "stargate.milkyway.ring_stop";

	public bool StopSoundOnSpinDown { get; set; } = false; // play the stopsound on spindown, or on spin stop
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

	protected float RingMaxSpeed { get; set; } = 50f;

	protected float RingAccelStep { get; set; } = 1f;

	protected float RingDeccelStep { get; set; } = 0.75f;

	protected float RingAngToRotate { get; set; } = 170f;

	protected float RingTargetAngleOffset { get; set; } = 0.5f;

	protected Sound? StartSoundInstance { get; set; }

	protected Sound? StopSoundInstance { get; set; }

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
		_shouldDecc = false;
		_shouldAcc = true;

		Event.Run( StargateEvent.RingSpinUp, Gate );
	}

	public void SpinDown()
	{
		if ( _lastSpinDown < 1 )
			return;

		_lastSpinDown = 0;
		_shouldAcc = false;
		_shouldDecc = true;

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
		if ( Gate.IsValid() )
		{
			_hasReachedDialingSymbol = false;
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
		_shouldStopAtAngle = true;
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
		var angToRotate = (_ringDirection == -1) ? D_CCW : D_CW;
		if ( angToRotate < RingAngToRotate ) angToRotate += 360f;

		// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
		var finalAng = RingAngle + (angToRotate * _ringDirection);

		//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

		return finalAng;
	}

	public async Task<bool> RotateRingToSymbolAsync( char sym, int angOffset = 0 )
	{
		RotateRingToSymbol( sym, angOffset );
		CurDialingSymbol = sym;
		Gate.CurDialingSymbol = CurDialingSymbol;

		await GameTask.DelaySeconds( Game.TickInterval ); // wait, otherwise it hasnt started moving yet and can cause issues

		while ( IsMoving )
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
		var symCoverage = (RingAngle + symRange / 2f).UnsignedMod( symRange );
		var symIndex = ((int)Math.Round( (-RingAngle + Gate.CurRingSymbolOffset) / (symRange) )).UnsignedMod( RingSymbols.Length );

		CurRingSymbol = (symCoverage < 8 && symCoverage > 1) ? RingSymbols[symIndex] : ' ';
		Gate.CurRingSymbol = CurRingSymbol;
	}

	public void RingRotationThink()
	{
		if ( !Gate.IsValid() ) return;

		if ( IsMoving && Gate.ShouldStopDialing )
		{
			if ( !_shouldDecc )
			{
				Gate.StopDialing();
			}
		}

		if ( _shouldAcc )
		{
			if ( !IsMoving )
			{
				_startedAccelAngle = RingAngle;
				StartMoving();
				OnStart();
			}

			if ( _ringCurSpeed < RingMaxSpeed )
			{
				_ringCurSpeed += RingAccelStep;
			}
			else
			{
				_ringCurSpeed = RingMaxSpeed;
				_shouldAcc = false;
				_stoppedAccelAngle = MathF.Abs( RingAngle - _startedAccelAngle ) + RingTargetAngleOffset;
				_curStopAtAngle = TargetRingAngle - (_stoppedAccelAngle * _ringDirection * (RingAccelStep / RingDeccelStep));
			}
		}
		else if ( _shouldDecc )
		{
			if ( _ringCurSpeed > 0 )
			{
				_ringCurSpeed -= RingDeccelStep;

				if ( !_hasReachedDialingSymbol )
				{
					if ( CurRingSymbol == CurDialingSymbol && Gate.Dialing )
					{
						_hasReachedDialingSymbol = true;
						Event.Run( StargateEvent.ReachedDialingSymbol, Gate, CurDialingSymbol );
					}
				}
			}
			else
			{
				_ringCurSpeed = 0;
				_shouldDecc = false;
				StopMoving();
				OnStop();

				ReverseMoving();
				CurrentRotation %= 360f;
			}
		}

		SetSpeed( _ringCurSpeed );

		if ( _shouldStopAtAngle && IsMoving )
		{
			if ( !_shouldAcc && !_shouldDecc )
			{
				var angDiff = MathF.Abs( CurrentRotation - _curStopAtAngle );
				//Log.Info( $"RingAng={RingAngle}, AngDiff={angDiff}" );
				DesiredRingAngleDifference = angDiff;
				if ( angDiff < 1f ) // if the angle difference is smal enough, start spindown
				{
					SpinDown();
					_shouldStopAtAngle = false;
				}
			}
		}

		RingAngle = CurrentRotation;
		_ringDirection = IsMovingForwards ? 1 : -1;

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

	protected override void OnDestroy()
	{
		if ( StartSoundInstance.HasValue ) StartSoundInstance.Value.Stop();
		if ( StopSoundInstance.HasValue ) StopSoundInstance.Value.Stop();

		base.OnDestroy();
	}
}
