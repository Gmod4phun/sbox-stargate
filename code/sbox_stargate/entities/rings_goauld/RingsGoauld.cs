using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "Transportation Rings (Goauld)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class RingsGoauld : Rings
{
	public const string Model = "models/sbox_stargate/rings_ancient/ring_ancient.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( Model );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}

	protected override void HideBase() { }
	protected override void ShowBase() { }
}
