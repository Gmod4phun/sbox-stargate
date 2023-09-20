using Sandbox;
using System.Collections.Generic;
using Editor;
using System.Text.Json;

[HammerEntity, SupportsSolid, EditorModel( MODEL )]
[Title( "SGC Ramp" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class SGCRamp : Prop, IStargateRamp, IGateSpawner
{
	public const string MODEL = "models/sbox_stargate/ramps/sgc_ramp/sgc_ramp.vmdl";

	[Net]
	public Vector3 SpawnOffset { get; private set; } = new(0, 0, 148);

	public int AmountOfGates => 1;

	public Vector3[] StargatePositionOffset => new Vector3[] { Vector3.Zero };

	public Angles[] StargateRotationOffset => new Angles[] { Angles.Zero };

	public List<Stargate> Gate { get; set; } = new();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Default;

		SetModel( MODEL );
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
