using Sandbox;

public partial class StargateIris : AnimatedEntity
{
	private float _openCloseDelay = 3f;
	public bool Busy { get; set; } = false;

	[Net]
	public Stargate Gate { get; set; } = null;

	[Net]
	public bool Closed { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_stargate/iris/iris.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		SetMaterialGroup( 1 );

		Transmit = TransmitType.Always;
		Tags.Add( "solid" );
		Tags.Add( StargateTags.BeforeGate );
	}

	public virtual async void Close()
	{
		if ( Busy || Closed ) return;

		Busy = true;

		Closed = true;
		EnableAllCollisions = true;
		CurrentSequence.Name = "iris_close";
		Sound.FromEntity( "stargate.iris.close", this );

		await GameTask.DelaySeconds( _openCloseDelay );
		if ( !this.IsValid() ) return;

		Busy = false;
	}

	public virtual async void Open()
	{
		if ( Busy || !Closed ) return;

		Busy = true;

		Closed = false;
		EnableAllCollisions = false;
		CurrentSequence.Name = "iris_open";
		Sound.FromEntity( "stargate.iris.open", this );

		await GameTask.DelaySeconds( _openCloseDelay );
		if ( !this.IsValid() ) return;

		Busy = false;
	}

	public void Toggle()
	{
		if ( Busy ) return;

		if ( Closed ) Open();
		else Close();
	}

	public virtual void PlayHitSound()
	{
		Sound.FromEntity( "stargate.iris.hit", this );
	}

	[GameEvent.Tick.Server]
	public void IrisTick()
	{
		if ( Gate.IsValid() && Scale != Gate.Scale ) Scale = Gate.Scale; // always keep the same scale as gate
		if ( Gate.IsValid() && Gate is StargateUniverse gate ) LocalRotation = new Angles( 0, 0, -gate.Ring.RingAngle ).ToRotation(); // rotate iris with Universe gate
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( Gate.IsValid() ) Gate.Iris = null;
	}
}
