using Sandbox;
using Sandbox.UI;

public class WormholeCinematic : Panel
{
	readonly ScenePanel scenePanel;

	SceneParticles particleObj;

	private SceneModel WormholeModel;

	private TimeSince sinceStarted = 0;

	private Sound WormholeSound;

	public WormholeCinematic()
	{
		Style.FlexWrap = Wrap.Wrap;
		Style.JustifyContent = Justify.Center;
		Style.AlignItems = Align.Center;
		Style.AlignContent = Align.Center;
		Style.Padding = 0;

		var world = new SceneWorld() { ClearColor = Color.Black };
		scenePanel = new ScenePanel();
		scenePanel.World = world;
		scenePanel.Camera.FieldOfView = 90;
		scenePanel.Camera.ZFar = 15000f;
		scenePanel.Camera.AntiAliasing = true;

		scenePanel.Style.Width = Length.Percent( 100 );
		scenePanel.Style.Height = Length.Percent( 100 );
		scenePanel.Style.PointerEvents = PointerEvents.All;
		scenePanel.Style.Cursor = "none";

		AddChild( scenePanel );

		new SceneSkyBox( world, Material.Load( "models/sbox_stargate/wormhole/skybox.vmat" ) );

		WormholeModel = new SceneModel( world, "models/sbox_stargate/wormhole/wormhole.vmdl", Transform.Zero );

		var bone = WormholeModel.GetBoneWorldTransform( 1 );

		scenePanel.Camera.Position = bone.Position;
		scenePanel.Camera.Rotation = bone.Rotation.RotateAroundAxis( Vector3.Right, -90f );

		sinceStarted = 0;

		particleObj = new SceneParticles( world, "particles/sbox_stargate/wormhole/wormhole_end.vpcf" );
		new SceneLight( world, Vector3.Zero, 100.0f, Color.White * 20.0f );

		WormholeSound = Sound.FromScreen( "wormhole.sound_travel" );
	}

	public override void Tick()
	{
		base.Tick();

		WormholeModel.Update( Time.Delta );

		var bone = WormholeModel.GetBoneWorldTransform( 1 );

		scenePanel.Camera.Position = bone.Position;
		scenePanel.Camera.Rotation = bone.Rotation.RotateAroundAxis( Vector3.Right, -90f );

		if ( sinceStarted.Relative >= 6.0f )
		{
			particleObj?.Simulate( RealTime.Delta );
		}

		if ( Game.LocalPawn.Health <= 0 )
		{
			WormholeSound.Stop();

			Delete();
		}
	}
}
