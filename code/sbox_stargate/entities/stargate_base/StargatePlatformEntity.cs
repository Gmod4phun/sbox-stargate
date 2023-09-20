using Sandbox;
using System;
using System.Threading.Tasks;

public partial class StargatePlatformEntity : KeyframeEntity
{
	private Rotation _rotationA;
	private bool _isWaitingToMove = false;
	private int _movement = 0;
	private Sound? _moveSoundInstance = null;

	// The values correspond to DoorHelper.
	public enum PlatformMoveType
	{
		Rotating = 1,
		RotatingContinious = 4
	}

	/// <summary>
	/// Specifies the direction to move in when the platform is used, or axis of rotation for rotating platforms.
	/// </summary>
	[Property( "movedir", Title = "Move Direction" )]
	public Angles MoveDir { get; set; } = new Angles( 0, 0, 0 );

	/// <summary>
	/// If checked, the movement direction angle is in local space and should be rotated by the entity's angles after spawning.
	/// </summary>
	[Property( "movedir_islocal", Title = "Move Direction is Expressed in Local Space" )]
	public bool MoveDirIsLocal { get; set; } = true;

	/// <summary>
	/// Movement type of the platform.<br/>
	/// <b>Rotating</b>: Rotating and reversing direction at final rotation if Looping is enabled.<br/>
	/// <b>Rotating Continious</b>: Rotating continiously past Move Distance. OnReached outputs are fired every Move Distance degrees.<br/>
	/// </summary>
	[Property( "movedir_type", Title = "Movement Type" )]
	public PlatformMoveType MoveDirType { get; set; } = PlatformMoveType.RotatingContinious;

	/// <summary>
	/// How much to move in the move direction, or rotate around the axis for rotating move type.
	/// </summary>
	[Property]
	public float MoveDistance { get; set; } = 100.0f;

	/// <summary>
	/// The speed to move/rotate with.
	/// </summary>
	[Property]
	public float Speed { get; protected set; } = 64.0f;

	/// <summary>
	/// If set to above 0 and <b>Loop Movement</b> is enabled, the amount of time to wait before automatically toggling direction.
	/// </summary>
	[Property]
	public float TimeToHold { get; set; } = 0.0f;

	/// <summary>
	/// If set, the platform will automatically go back upon reaching either end of the movement range.
	/// </summary>
	[Property]
	public bool LoopMovement { get; set; } = false;

	// TODO: Acceleration/deceleration?

	/// <summary>
	/// If set, the platform will start moving on spawn.
	/// </summary>
	[Property, Category( "Spawn Settings" )]
	public bool StartsMoving { get; set; } = true;

	/// <summary>
	/// At what percentage between start and end positions should the platform spawn in
	/// </summary>
	[Property, Category( "Spawn Settings" ), MinMax( 0, 100 )]
	public float StartPosition { get; set; } = 0;

	/// <summary>
	/// Sound to play when starting to move
	/// </summary>
	[Property, FGDType( "sound" ), Category( "Sounds" )]
	public string StartMoveSound { get; set; }

	/// <summary>
	/// Sound to play when we stopped moving
	/// </summary>
	[Property, FGDType( "sound" ), Category( "Sounds" )]
	public string StopMoveSound { get; set; }

	/// <summary>
	/// Sound to play while platform is moving.
	/// </summary>
	[Property( "moving_sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string MovingSound { get; set; }

	public bool IsMoving { get; protected set; }

	public bool IsMovingForwards { get; protected set; }

	/// <summary>
	/// Contains the current rotation of the platform in degrees.
	/// </summary>
	public float CurrentRotation { get; protected set; } = 0;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		// PlatformMoveType.Rotating
		{
			_rotationA = LocalRotation;
		}

		IsMoving = false;
		IsMovingForwards = true;

		if ( StartPosition > 0 )
		{
			SetPosition( StartPosition / 100.0f );
		}

		if ( StartsMoving )
		{
			StartMoving();
		}
	}

	public void MoveFinished()
	{
		LocalVelocity = Vector3.Zero;
		AngularVelocity = Angles.Zero;
	}

	/// <summary>
	/// Start moving in platform's current move direction
	/// </summary>
	[Input]
	public void StartMoving()
	{
		_ = DoMove();
	}

	/// <summary>
	/// Set the move direction to forwards and start moving
	/// </summary>
	[Input]
	public void StartMovingForward()
	{
		IsMovingForwards = true;
		StartMoving();
	}

	/// <summary>
	/// Set the move direction to backwards and start moving
	/// </summary>
	[Input]
	public void StartMovingBackwards()
	{
		IsMovingForwards = false;
		StartMoving();
	}

	/// <summary>
	/// Reverse current move direction. Will NOT start moving if stopped
	/// </summary>
	[Input]
	public void ReverseMoving()
	{
		IsMovingForwards = !IsMovingForwards;

		if ( IsMoving )
		{
			// We changed direction, play the start moving sound
			// TODO: Have a "Change direction while moving" sound option?
			PlaySound( StartMoveSound );
			StartMoving();
		}
	}

	/// <summary>
	/// Stop moving, preserving move direction
	/// </summary>
	[Input]
	public void StopMoving()
	{
		if ( !IsMoving && !_isWaitingToMove ) return;

		_movement++;
		_ = LocalKeyframeTo( LocalPosition, 0, null ); // Bad
		_ = LocalRotateKeyframeTo( LocalRotation, 0, null );

		LocalVelocity = Vector3.Zero;
		AngularVelocity = Angles.Zero;

		if ( !IsMoving ) return;

		IsMoving = false;
		PlaySound( StopMoveSound );

		if ( _moveSoundInstance.HasValue )
		{
			_moveSoundInstance.Value.Stop();
			_moveSoundInstance = null;
		}
	}

	/// <summary>
	/// Toggle moving, preserving move direction
	/// </summary>
	[Input]
	public void ToggleMoving()
	{
		if ( IsMoving || _isWaitingToMove )
		{
			StopMoving();
			return;
		}

		StartMoving();
	}

	/// <summary>
	/// Sets the move speed
	/// </summary>
	[Input]
	public void SetSpeed( float speed )
	{
		Speed = speed;
	}

	protected override void OnDestroy()
	{
		if ( _moveSoundInstance.HasValue )
		{
			_moveSoundInstance.Value.Stop();
			_moveSoundInstance = null;
		}

		base.OnDestroy();
	}

	private Vector3 GetRotationAxis()
	{
		var axis = Rotation.From( MoveDir ).Up;
		if ( !MoveDirIsLocal ) axis = Transform.NormalToLocal( axis );

		return axis;
	}

	private async Task DoMove()
	{
		if ( !IsMoving ) Sound.FromEntity( StartMoveSound, this );

		IsMoving = true;
		var moveId = ++_movement;

		if ( !_moveSoundInstance.HasValue && !string.IsNullOrEmpty( MovingSound ) )
		{
			_moveSoundInstance = PlaySound( MovingSound );
		}

		if ( MoveDirType == PlatformMoveType.RotatingContinious || MoveDirType == PlatformMoveType.Rotating )
		{
			var moveDist = MoveDistance;
			if ( moveDist == 0 ) moveDist = 360.0f;
			if ( !IsMovingForwards ) moveDist = -moveDist;

			// If speed is negative, allow going backwards
			if ( Speed < 0 )
			{
				moveDist = -moveDist;
			}

			var initialRotation = CurrentRotation - (CurrentRotation % moveDist);

			// TODO: Move the platform via MoveWithVelocity( Angles, timeToTake )

			var axis_rot = GetRotationAxis();
			var finalRot = _rotationA.RotateAroundAxis( axis_rot, initialRotation + moveDist );
			var lastTime = Time.Now;
			for ( float f = CurrentRotation % moveDist / moveDist; f < 1; )
			{
				await GameTask.NextPhysicsFrame();
				var diff = Math.Max( Time.Now - lastTime, 0 );
				lastTime = Time.Now;

				if ( moveId != _movement || !this.IsValid() ) return;

				var timeToTake = Math.Abs( moveDist ) / Math.Abs( Speed ); // Get the fresh speed
				var delta = diff / timeToTake;
				CurrentRotation += delta * moveDist;
				LocalRotation = _rotationA.RotateAroundAxis( axis_rot, CurrentRotation );
				f += delta;
			}

			// Snap to the ideal final position
			CurrentRotation = initialRotation + moveDist;
			LocalRotation = finalRot;
		}
		else
		{
			Log.Warning( $"{this}: Unknown platform move type {MoveDirType}!" );
			await GameTask.Delay( 100 );
		}

		if ( moveId != _movement || !this.IsValid() ) return;

		if ( MoveDirType != PlatformMoveType.RotatingContinious || TimeToHold > 0 )
		{
			IsMoving = false;

			if ( _moveSoundInstance.HasValue )
			{
				_moveSoundInstance.Value.Stop();
				_moveSoundInstance = null;
			}

			// Do not play the stop sound for instant changing direction
			if ( !LoopMovement || TimeToHold > 0 )
			{
				Sound.FromEntity( StopMoveSound, this );
			}
		}

		if ( !LoopMovement ) return;

		// ToggleMovement input during this time causes unexpected behavior
		_isWaitingToMove = true;
		if ( TimeToHold > 0 ) await GameTask.DelaySeconds( TimeToHold );
		_isWaitingToMove = false;

		if ( moveId != _movement || !this.IsValid() ) return;

		if ( MoveDirType != PlatformMoveType.RotatingContinious )
		{
			IsMovingForwards = !IsMovingForwards;
		}

		_ = DoMove();
	}

	/// <summary>
	/// Sets the platforms's position to given percentage between start and end positions. The expected input range is 0..1
	/// </summary>
	[Input]
	private void SetPosition( float progress )
	{
		progress = Math.Clamp( progress, 0.0f, 1.0f );

		LocalRotation = Rotation.Lerp( _rotationA, _rotationA.RotateAroundAxis( GetRotationAxis(), MoveDistance != 0 ? MoveDistance : 360.0f ), progress );
		CurrentRotation = 0.0f.LerpTo( MoveDistance != 0 ? MoveDistance : 360.0f, progress );
	}
}
