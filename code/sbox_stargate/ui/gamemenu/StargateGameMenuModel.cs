using Sandbox;
using Sandbox.UI;

public class StargateGameMenuModel : Panel
{
	private readonly ScenePanel _scenePanel;
	private SceneModel _gateModel;
	private SceneModel _ringModel;

	public StargateGameMenuModel()
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
		_scenePanel.Camera.Ortho = true;
		_scenePanel.Camera.OrthoWidth = 1600;
		_scenePanel.Camera.OrthoHeight = 900;

		_scenePanel.Style.Width = Length.Percent( 100 );
		_scenePanel.Style.Height = Length.Percent( 100 );
		_scenePanel.Style.PointerEvents = PointerEvents.None;
		_scenePanel.Style.Cursor = "none";

		AddChild( _scenePanel );

		//new SceneSkyBox( world, Material.Load( "models/sbox_stargate/wormhole/skybox.vmat" ) );

		var tg = Transform.Zero;
		tg.Rotation = tg.Rotation.RotateAroundAxis( Vector3.Forward, 5 );
		_gateModel = new SceneModel( world, "models/sbox_stargate/sg_mw/sg_mw_gate.vmdl", tg );
		_ringModel = new SceneModel( world, "models/sbox_stargate/sg_mw/sg_mw_ring.vmdl", Transform.Zero );

		_gateModel.Batchable = false;
		_ringModel.Batchable = false;

		for ( var i = 0; i < 9; i++ )
		{
			var t = Transform.Zero;
			t.Rotation = t.Rotation.RotateAroundAxis( Vector3.Forward, 40 * i + _gateModel.Rotation.Roll() );
			var cmodel = new SceneModel( world, "models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl", t );
			new SceneLight( world, cmodel.Position + cmodel.Rotation.Up * 64 + cmodel.Rotation.Forward * 32, 200, Color.White * 1.0f );

			//cmodel.Attributes.Set( "selfillumscale", 1 );
			cmodel.Batchable = false;
		}

		//scenePanel.Camera.Position = GateModel.Position - GateModel.Rotation.Forward * 512;
		//scenePanel.Camera.Rotation = GateModel.Rotation; //.RotateAroundAxis( Vector3.Right, -90f );

		new SceneLight( world, Vector3.Forward * 128, 512, Color.White * 2.0f );
		//new SceneLight( world, Vector3.Forward * 64 - Vector3.Up * 256, 1024, Color.Red * 5.0f );
		//new SceneLight( world, Vector3.Forward * 64 - Vector3.Up * 128 - Vector3.Right * 128, 1024, Color.Green * 5.0f );
		//new SceneLight( world, Vector3.Forward * 64 - Vector3.Up * 128 + Vector3.Right * 128, 1024, Color.Blue * 5.0f );
	}

	public override void Tick()
	{
		base.Tick();

		_ringModel.Rotation = _gateModel.Rotation.RotateAroundAxis( Vector3.Forward, Time.Now * -16 );

		var distFromCenter = 90;
		_scenePanel.Camera.Position = Vector3.Forward * 256 + Vector3.Up * distFromCenter + Vector3.Right * distFromCenter;
		_scenePanel.Camera.Rotation = Rotation.From( new Angles( 180, 0, 180 ) );

		_scenePanel.Camera.OrthoHeight = 60;
		_scenePanel.Camera.OrthoWidth = _scenePanel.Camera.OrthoHeight;

		//scenePanel.Camera.OrthoWidth = 160;
		//scenePanel.Camera.OrthoHeight = 90;

		//scenePanel.Camera.Ortho = true;

		_gateModel.Update( Time.Delta );
		_ringModel.Update( Time.Delta );
	}
}
