using Sandbox;
using Editor;

[HammerEntity, SupportsSolid, EditorModel( MODEL )]
[Title( "DHD (Milky Way)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class DhdMilkyWay : Dhd
{
	public const string MODEL = "models/sbox_stargate/dhd/dhd.vmdl";
	public Vector3 SpawnOffset { get; private set; } = new(0, 0, -5);
	public Angles SpawnOffsetAng { get; private set; } = new(15, 0, 0);

	public static void DrawGizmos( EditorContext context )
	{
		Gizmo.Draw.Model( "models/sbox_stargate/dhd/buttons/dhd_buttons_all.vmdl" );
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( MODEL );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtons();

		foreach ( var button in Buttons.Values )
		{
			button.SetMaterialGroup( "mw" );
		}
	}
}
