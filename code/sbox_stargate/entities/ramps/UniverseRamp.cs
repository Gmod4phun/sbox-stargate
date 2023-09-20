using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "Universe Ramp" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class UniverseRamp : Prop, IStargateRamp, IGateSpawner
{
	public const string Model = "models/sbox_stargate/ramps/sgu_ramp/sgu_ramp.vmdl";

	public List<PointLightEntity> Lights = new();
	public PointLightEntity CenterLight;

	[Net]
	public Vector3 SpawnOffset { get; private set; } = new(0, 0, 60);

	public int AmountOfGates => 1;

	public Vector3[] StargatePositionOffset => new[] { new Vector3(0, 0, 95) };

	public Angles[] StargateRotationOffset => new[] { Angles.Zero };

	public List<Stargate> Gate { get; set; } = new();

	public Color LightColor => Color.FromBytes( 230, 215, 240 ) * 0.3f;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Default;

		SetModel( Model );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );

		Tags.Add( "solid" );

		PostSpawn();
	}

	public async void PostSpawn()
	{
		await GameTask.NextPhysicsFrame();

		Transform t;
		RectangleLightEntity light;

		for ( var i = 1; i <= 4; i++ )
		{
			t = (Transform)GetAttachment( $"light{i}" );
			light = new RectangleLightEntity();
			light.Color = LightColor;
			light.PlaneHeight = 38;
			light.LightSize = 0.5f;
			light.Position = t.Position;
			light.Rotation = t.Rotation;
			light.Brightness = 0;
			light.SetParent( this );
			Lights.Add( light );
		}

		for ( var i = 5; i <= 8; i++ )
		{
			t = (Transform)GetAttachment( $"light{i}" );
			light = new RectangleLightEntity
			{
				Color = LightColor,
				PlaneHeight = 18,
				LightSize = 0.5f,
				Position = t.Position,
				Rotation = t.Rotation,
				Brightness = 0
			};
			light.SetParent( this );
			Lights.Add( light );
		}

		// center ramp light
		var tC = (Transform)GetAttachment( $"light9" );
		var lightC = new PointLightEntity
		{
			Color = LightColor,
			LightSize = 0.2f,
			Position = tC.Position,
			Rotation = tC.Rotation,
			Brightness = 0
		};
		lightC.SetParent( this );
		CenterLight = lightC;
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

	protected override void OnDestroy()
	{
		base.OnDestroy();

		foreach ( var light in Lights )
		{
			light?.Delete();
		}
	}

	[GameEvent.Tick.Server]
	private void LightThink()
	{
		var gate = Gate.FirstOrDefault();
		var shouldGlow = gate.IsValid() ? !gate.Idle : false;
		var shouldCenterGlow = gate.IsValid() ? (gate.Open || gate.Opening || gate.Closing) : false;

		foreach ( var light in Lights )
		{
			light.Brightness = light.Brightness.LerpTo( shouldGlow ? 0.1f : 0f, Time.Delta * (shouldGlow ? 4 : 16) );
			light.Color = LightColor;
		}

		if ( CenterLight.IsValid() )
		{
			CenterLight.Brightness = CenterLight.Brightness.LerpTo( shouldCenterGlow ? 0.1f : 0f, Time.Delta * (shouldCenterGlow ? 4 : 16) );
			CenterLight.Color = LightColor;
		}

		SetMaterialGroup( shouldCenterGlow ? 2 : 0 );
	}
}
