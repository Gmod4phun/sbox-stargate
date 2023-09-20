using Sandbox;

public partial class StargateIrisAtlantis : StargateIris
{
	protected Sound IrisLoop;
	private readonly float OpenCloseDleay = 1f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_stargate/iris_atlantis/iris_atlantis.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		Transmit = TransmitType.Always;
		Tags.Add( "solid" );
		Tags.Add( StargateTags.BeforeGate );
	}

	public override async void Close()
	{
		if ( Busy || Closed ) return;

		Busy = true;

		Closed = true;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Sound.FromEntity( "stargate.iris.atlantis.close", this );

		await GameTask.DelaySeconds( OpenCloseDleay );
		if ( !this.IsValid() ) return;

		Busy = false;

		await GameTask.DelaySeconds( 0.6f );
		if ( !this.IsValid() ) return;

		if ( !Closed ) return;

		IrisLoop.Stop();
		IrisLoop = Sound.FromEntity( "stargate.iris.atlantis.loop", this );
	}

	public override async void Open()
	{
		if ( Busy || !Closed ) return;

		IrisLoop.Stop();

		Busy = true;

		Closed = false;
		EnableAllCollisions = false;
		EnableDrawing = false;
		Sound.FromEntity( "stargate.iris.atlantis.open", this );

		await GameTask.DelaySeconds( OpenCloseDleay );
		if ( !this.IsValid() ) return;

		Busy = false;
	}

	public override void PlayHitSound()
	{
		Sound.FromEntity( "stargate.iris.atlantis.hit", this );
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

		PlayHitSound();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		IrisLoop.Stop();

		if ( Closed ) Sound.FromEntity( "stargate.iris.atlantis.open", this );
	}
}
