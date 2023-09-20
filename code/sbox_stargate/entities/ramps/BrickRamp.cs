using System.Collections.Generic;
using System.Text.Json;
using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "Brick Ramp" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class BrickRamp : Prop, IStargateRamp, IGateSpawner
{
	public const string Model = "models/sbox_stargate/ramps/brick/brick.vmdl";

	[Net]
	public Vector3 SpawnOffset { get; private set; } = new(0, 0, 70);

	public int AmountOfGates => 1;

	public Vector3[] StargatePositionOffset => new[] { new Vector3( 0, 0, 95 ) };

	public Angles[] StargateRotationOffset => new[] { Angles.Zero };

	public List<Stargate> Gate { get; set; } = new();

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Default;

		SetModel( Model );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );

		Tags.Add( "solid" );
	}

	public void FromJson( JsonElement data )
	{
		Position = Vector3.Parse( data.GetProperty( "Position" ).ToString() );
		Rotation = Rotation.Parse( data.GetProperty( "Rotation" ).ToString() );

		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}

	public object ToJson()
	{
		return new JsonModel() { EntityName = ClassName, Position = Position, Rotation = Rotation };
	}
}
