using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "Transportation Rings (Ancient)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class RingsAncient : Rings
{
	public const string Model = "models/sbox_stargate/rings_ancient/ring_ancient_cover.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/rings_ancient/ring_ancient_cover.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}
}
