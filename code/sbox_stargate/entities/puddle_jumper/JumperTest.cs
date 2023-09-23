using Sandbox;

[Title( "Test Jumper" ), Category( "Stargate" ), Icon( "chair" )]
public partial class JumperTest : Prop, IUse
{
	private InputState _currentInput;

	private MovementState _currentMovement;

	[Net]
	public Vector3 SpawnOffset { get; private set; } = new(0, 0, 65);

	[Net]
	public Player Driver { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Predictable = false;

		SetModel( "models/sbox_stargate/puddle_jumper/puddle_jumper.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.UseController = false;
	}

	[GameEvent.Physics.PreStep]
	public void PhysicsSimulate()
	{
		if ( !Game.IsServer )
			return;

		var phys = PhysicsBody;
		if ( !phys.IsValid() )
			return;

		var body = phys.SelfOrParent;
		if ( !body.IsValid() )
			return;

		var dt = Time.Delta * 2f;
		var rot = body.Rotation;

		float desiredForward = 0;
		float desiredRight = 0;
		float desiredUp = 0;

		if ( _currentInput.Forward )
			desiredForward = 1;
		else if ( _currentInput.Back )
			desiredForward = -1;

		if ( _currentInput.Right )
			desiredRight = 1;
		else if ( _currentInput.Left )
			desiredRight = -1;

		if ( _currentInput.Up )
			desiredUp = 1;
		else if ( _currentInput.Down )
			desiredUp = -1;

		if ( _currentInput.Boost )
		{
			desiredForward = desiredForward * 4f;
		}

		_currentMovement.AccelForward = _currentMovement.AccelForward.LerpTo( desiredForward, dt );
		_currentMovement.AccelRight = _currentMovement.AccelRight.LerpTo( desiredRight, dt );
		_currentMovement.AccelUp = _currentMovement.AccelUp.LerpTo( desiredUp, dt );

		if ( _currentMovement.AccelForward > 0.01 || _currentMovement.AccelForward < -0.01 )
		{
			body.Position += rot.Forward * _currentMovement.AccelForward;
		}

		if ( _currentMovement.AccelRight > 0.01 || _currentMovement.AccelRight < -0.01 )
		{
			body.Position += rot.Right * _currentMovement.AccelRight;
		}

		if ( _currentMovement.AccelUp > 0.01 || _currentMovement.AccelUp < -0.01 )
		{
			body.Position += rot.Up * _currentMovement.AccelUp;
		}
	}

	public override void Simulate( IClient client )
	{
		SimulateDriver( client );
	}

	public override void FrameSimulate( IClient client )
	{
		base.FrameSimulate( client );

		Driver?.FrameSimulate( client );
	}

	public bool OnUse( Entity user )
	{
		if ( user is SandboxPlayer player )
		{
			player.Parent = this;
			player.LocalPosition = Vector3.Up * -50 + Vector3.Forward * 10;
			player.LocalRotation = Rotation.Identity;
			player.LocalScale = 1;
			player.PhysicsBody.Enabled = false;

			Driver = player;
			Driver.Client.Pawn = this;

			PhysicsBody.GravityEnabled = false;
		}

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return !Driver.IsValid();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Driver is SandboxPlayer player )
		{
			RemoveDriver( player );
		}
	}

	[GameEvent.Tick.Server]
	protected void PlayerAliveCheck()
	{
		if ( Driver is SandboxPlayer player && player.LifeState != LifeState.Alive )
		{
			RemoveDriver( player );
		}
	}

	void SimulateDriver( IClient client )
	{
		if ( !Driver.IsValid() ) return;

		if ( Game.IsServer )
		{
			if ( Input.Pressed( InputButton.Use ) )
			{
				RemoveDriver( Driver as SandboxPlayer );
				return;
			}
			else
			{
				_currentInput.Reset();
				_currentInput.Forward = Input.Down( InputButton.Forward );
				_currentInput.Back = Input.Down( InputButton.Back );
				_currentInput.Left = Input.Down( InputButton.Left );
				_currentInput.Right = Input.Down( InputButton.Right );
				_currentInput.Up = Input.Down( InputButton.Jump );
				_currentInput.Down = Input.Down( InputButton.Duck );
				_currentInput.Boost = Input.Down( InputButton.Run );
			}
		}
	}

	private void RemoveDriver( SandboxPlayer player )
	{
		Driver = null;

		_currentInput.Reset();

		if ( !player.IsValid() )
			return;

		player.Parent = null;
		player.Position += Vector3.Up * 100;

		if ( player.PhysicsBody.IsValid() )
		{
			player.PhysicsBody.Enabled = true;
			player.PhysicsBody.Position = player.Position;
		}

		player.Client.Pawn = player;

		PhysicsBody.GravityEnabled = true;
	}

	private struct InputState
	{
		public bool Forward { get; set; }
		public bool Back { get; set; }
		public bool Left { get; set; }
		public bool Right { get; set; }
		public bool Up { get; set; }
		public bool Down { get; set; }
		public bool Boost { get; set; }

		public void Reset()
		{
			Forward = false;
			Back = false;
			Left = false;
			Right = false;
			Up = false;
			Down = false;
			Boost = false;
		}
	}

	private struct MovementState
	{
		public float AccelForward { get; set; }
		public float AccelRight { get; set; }
		public float AccelUp { get; set; }

		public MovementState()
		{
			AccelForward = 0;
			AccelRight = 0;
			AccelUp = 0;
		}
	}
}
