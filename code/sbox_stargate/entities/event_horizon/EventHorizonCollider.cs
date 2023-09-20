using Sandbox;

public partial class EventHorizonCollider : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/event_horizon/event_horizon_collider_floor.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );

		Tags.Add( StargateTags.FakeWorld );

		EnableAllCollisions = false;
		EnableSolidCollisions = true;
		EnableTraceAndQueries = false;
		EnableTouch = false;
	}
}
