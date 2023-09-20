using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( MODEL )]
[Title( "Ring Panel (Goauld)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class RingPanelGoauld : RingPanel
{
	public const string MODEL = "models/sbox_stargate/rings_panel/goauld/ring_panel_goauld.vmdl";
	protected override string[] ButtonsSounds { get; } = { "ringpanel.goauld.button1", "ringpanel.goauld.button2" };

	public static void DrawGizmos( EditorContext context )
	{
		for ( var i = 1; i <= 6; i++ )
		{
			Gizmo.Draw.Model( $"models/sbox_stargate/rings_panel/goauld/ring_panel_goauld_button_{i}.vmdl" );
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( MODEL );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtons();
	}

	public virtual void CreateButtons() // visible models of buttons that turn on/off and animate
	{
		for ( var i = 1; i <= 6; i++ )
		{
			var button = new RingPanelButton();
			button.SetModel( $"models/sbox_stargate/rings_panel/goauld/ring_panel_goauld_button_{i}.vmdl" );
			button.SetupPhysicsFromModel( PhysicsMotionType.Static, true ); // needs to have physics for traces
			button.PhysicsBody.BodyType = PhysicsBodyType.Static;
			button.EnableAllCollisions = false; // no collissions needed
			button.EnableTraceAndQueries = true; // needed for Use

			button.Position = Position;
			button.Rotation = Rotation;
			button.Scale = Scale;
			button.SetParent( this );

			var action = (i == 6) ? "DIAL" : i.ToString();
			button.Action = action;
			button.RingPanel = this;
			Buttons.Add( action, button );
		}
	}
}
