using Sandbox;

public partial class EventHorizonTrigger : ModelEntity
{
	private readonly string _triggerModel = "models/sbox_stargate/event_horizon/event_horizon_trigger.vmdl";

	private EventHorizon _eventHorizon = null;

	public EventHorizonTrigger()
	{
	}

	public EventHorizonTrigger( EventHorizon eh )
	{
		_eventHorizon = eh;
	}

	public EventHorizonTrigger( EventHorizon eh, string model )
	{
		_eventHorizon = eh;
		_triggerModel = model;

		Spawn();
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		SetModel( _triggerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );

		Tags.Add( "trigger" );

		EnableAllCollisions = false;
		EnableTraceAndQueries = false;
		EnableTouch = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !Game.IsServer ) return;

		_eventHorizon.OnEntityTriggerStartTouch( this, other );
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( !Game.IsServer ) return;

		_eventHorizon.OnEntityTriggerEndTouch( this, other );
	}
}
