using Sandbox;
using Editor;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "DHD (Pegasus)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class DhdPegasus : Dhd
{
	public const string Model = "models/sbox_stargate/dhd/dhd.vmdl";

	public DhdPegasus()
	{
		Data = new DhdData(1, 1, "dhd.atlantis.press", "dhd.press_dial");
	}

	public Vector3 SpawnOffset { get; } = new(0, 0, -5);
	public Angles SpawnOffsetAng { get; } = new(15, 0, 0);

	public static void DrawGizmos( EditorContext context )
	{
		var buttons = Gizmo.Draw.Model( "models/sbox_stargate/dhd/buttons/dhd_buttons_all.vmdl", Transform.Zero );
		buttons.SetMaterialGroup( "peg" );
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( Model );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		SetMaterialGroup( 1 );

		CreateButtons();

		foreach ( var button in Buttons.Values )
		{
			button.SetMaterialGroup( "peg" );
		}
	}
}
