using System;
using Sandbox;

public partial class Kawoosh : ModelEntity
{
	private CapsuleLightEntity _light;
	public ModelEntity _kawooshInside;

	private bool _isExpanding = false;
	private float _currentProgress = 0;
	private static float _minProgress = 0;
	private static float _maxProgress = 1.5f;

	public EventHorizon EventHorizon;
	public EventHorizonTrigger Trigger;

	private Plane KawooshClipPlane
	{
		get => new( Position - Camera.Position + Rotation.Forward * 10f, Rotation.Forward.Normal );
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/kawoosh/kawoosh.vmdl" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		EnableDrawing = false;

		_kawooshInside = new ModelEntity( GetModelName() )
		{
			Position = Position,
			Rotation = Rotation,
			Parent = this,
			Scale = 0.95f,
			EnableDrawing = false
		};

		Morphs.Set( 0, 1 ); // set initial morph value for kawoosh to 1 (retracted)

		_kawooshInside.SetMaterialGroup( "inside" );
		_kawooshInside.Morphs.Set( 0, 1 );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Trigger?.Delete();
		_light?.Delete();
		_kawooshInside?.Delete();
	}

	public async void DoKawooshAnimation()
	{
		Trigger = new EventHorizonTrigger( EventHorizon, "models/sbox_stargate/event_horizon/event_horizon_trigger_kawoosh.vmdl" )
		{	Position = EventHorizon.Position + EventHorizon.Rotation.Forward * 2,
			Rotation = EventHorizon.Rotation,
			Parent = EventHorizon.Gate
		};

		KawooshClientAnim( To.Everyone );

		await GameTask.DelaySeconds( 1.5f );

		KawooshClientAnim( To.Everyone, true );
	}

	[ClientRpc]
	public void KawooshClientAnim( bool ending = false )
	{
		_isExpanding = !ending;
		if ( _isExpanding )
		{
			_currentProgress = 0;

			EnableDrawing = true;
			_kawooshInside.EnableDrawing = true;

			_light = new CapsuleLightEntity
			{
				Position = Position,
				Parent = this,
				Rotation = Rotation,
				Color = Color.FromBytes(25, 150, 250),
				LightSize = 80f,
				Brightness = 0,
				Enabled = true
			};
		}
	}

	[GameEvent.Client.Frame]
	private void ClientAnimLogic()
	{
		var delta = Time.Delta * (_isExpanding ? 1.8f : 3.2f);
		_currentProgress = _currentProgress.LerpTo(_isExpanding ? _maxProgress : _minProgress, delta);
		var morphValue = CalculateAnimationValue( _maxProgress - _currentProgress );
		Morphs.Set( 0, morphValue);
		_kawooshInside?.Morphs.Set( 0, morphValue );

		if ( _light.IsValid() )
		{
			var remappedProgress = _currentProgress.Remap( _minProgress, _maxProgress, 0, 1 );
			var lightLength = 128 * remappedProgress;

			_light.Enabled = true;
			_light.CapsuleLength = lightLength;
			_light.Position = Position + Rotation.Forward * lightLength;
			_light.Brightness = remappedProgress * 0.1f;
		}

		SceneObject.ClipPlaneEnabled = true;
		SceneObject.ClipPlane = KawooshClipPlane;

		if ( _kawooshInside.IsValid() )
		{
			_kawooshInside.SceneObject.ClipPlaneEnabled = true;
			_kawooshInside.SceneObject.ClipPlane = KawooshClipPlane;
			_kawooshInside.SceneObject.Attributes.Set( "emission", 10 );
		}
	}

	/// <summary>
	/// Calculates the animation value based on a given input.
	/// </summary>
	/// <param name="inputValue">Input float value which serves as a base for the calculation.</param>
	/// <returns>Animation value calculated by applying a sinusoidal function on the cubed input value, scaled by a constant factor.</returns>
	private float CalculateAnimationValue( float inputValue )
	{
		return (float)Math.Sin( 0.5f * Math.Pow( inputValue, 3 ) );
	}
}
