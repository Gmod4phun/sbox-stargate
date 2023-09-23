using Sandbox;
using Sandbox.UI;

public class WormholeCinematic : Panel
{
	private readonly ScenePanel _scenePanel;

	private SceneParticles _particleObj;

	private SceneModel _wormholeModel;

	private TimeSince _sinceStarted = 0;

	private Sound _wormholeSound;

	public WormholeCinematic()
	{
		Style.FlexWrap = Wrap.Wrap;
		Style.JustifyContent = Justify.Center;
		Style.AlignItems = Align.Center;
		Style.AlignContent = Align.Center;
		Style.Padding = 0;

		var world = new SceneWorld() { ClearColor = Color.Black };
		_scenePanel = new ScenePanel();
		_scenePanel.World = world;
		_scenePanel.Camera.FieldOfView = 90;
		_scenePanel.Camera.ZFar = 15000f;
		_scenePanel.Camera.AntiAliasing = true;

		_scenePanel.Style.Width = Length.Percent( 100 );
		_scenePanel.Style.Height = Length.Percent( 100 );
		_scenePanel.Style.PointerEvents = PointerEvents.All;
		_scenePanel.Style.Cursor = "none";

		AddChild( _scenePanel );

		new SceneSkyBox( world, Material.Load( "models/sbox_stargate/wormhole/skybox.vmat" ) );

		_wormholeModel = new SceneModel( world, "models/sbox_stargate/wormhole/wormhole.vmdl", Transform.Zero );

		var bone = _wormholeModel.GetBoneWorldTransform( 1 );

		_scenePanel.Camera.Position = bone.Position;
		_scenePanel.Camera.Rotation = bone.Rotation.RotateAroundAxis( Vector3.Right, -90f );

		_sinceStarted = 0;

		_particleObj = new SceneParticles( world, "particles/sbox_stargate/wormhole/wormhole_end.vpcf" );
		new SceneLight( world, Vector3.Zero, 100.0f, Color.White * 20.0f );

		_wormholeSound = Sound.FromScreen( "wormhole.sound_travel" );
	}

	public override void Tick()
	{
		base.Tick();

		_wormholeModel.Update( Time.Delta );

		var bone = _wormholeModel.GetBoneWorldTransform( 1 );

		_scenePanel.Camera.Position = bone.Position;
		_scenePanel.Camera.Rotation = bone.Rotation.RotateAroundAxis( Vector3.Right, -90f );

		if ( _sinceStarted.Relative >= 6.0f )
		{
			_particleObj?.Simulate( RealTime.Delta );
		}

		if ( Game.LocalPawn.Health <= 0 )
		{
			_wormholeSound.Stop();

			Delete();
		}
	}
}
