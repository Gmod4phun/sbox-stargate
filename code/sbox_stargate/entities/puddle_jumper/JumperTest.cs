using Sandbox;

[Title( "Test Jumper" ), Category( "Stargate" ), Icon( "chair" )]
public partial class JumperTest : Prop, IUse
{
	private InputState currentInput;

	private MovementState currentMovement;

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

		if ( currentInput.forward )
			desiredForward = 1;
		else if ( currentInput.back )
			desiredForward = -1;

		if ( currentInput.right )
			desiredRight = 1;
		else if ( currentInput.left )
			desiredRight = -1;

		if ( currentInput.up )
			desiredUp = 1;
		else if ( currentInput.down )
			desiredUp = -1;

		if ( currentInput.boost )
		{
			desiredForward = desiredForward * 4f;
		}

		currentMovement.accelForward = currentMovement.accelForward.LerpTo( desiredForward, dt );
		currentMovement.accelRight = currentMovement.accelRight.LerpTo( desiredRight, dt );
		currentMovement.accelUp = currentMovement.accelUp.LerpTo( desiredUp, dt );

		if ( currentMovement.accelForward > 0.01 || currentMovement.accelForward < -0.01 )
		{
			body.Position += rot.Forward * currentMovement.accelForward;
		}

		if ( currentMovement.accelRight > 0.01 || currentMovement.accelRight < -0.01 )
		{
			body.Position += rot.Right * currentMovement.accelRight;
		}

		if ( currentMovement.accelUp > 0.01 || currentMovement.accelUp < -0.01 )
		{
			body.Position += rot.Up * currentMovement.accelUp;
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
				currentInput.Reset();
				currentInput.forward = Input.Down( InputButton.Forward );
				currentInput.back = Input.Down( InputButton.Back );
				currentInput.left = Input.Down( InputButton.Left );
				currentInput.right = Input.Down( InputButton.Right );
				currentInput.up = Input.Down( InputButton.Jump );
				currentInput.down = Input.Down( InputButton.Duck );
				currentInput.boost = Input.Down( InputButton.Run );
			}
		}
	}

	private void RemoveDriver( SandboxPlayer player )
	{
		Driver = null;

		currentInput.Reset();

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
		public bool forward;
		public bool back;
		public bool left;
		public bool right;
		public bool up;
		public bool down;
		public bool boost;

		public void Reset()
		{
			forward = false;
			back = false;
			left = false;
			right = false;
			up = false;
			down = false;
			boost = false;
		}
	}

	private struct MovementState
	{
		public float accelForward;
		public float accelRight;
		public float accelUp;

		public MovementState()
		{
			accelForward = 0;
			accelRight = 0;
			accelUp = 0;
		}
	}
}
