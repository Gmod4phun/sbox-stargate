using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

public partial class Kawoosh : ModelEntity
{
	private const float DurationSec = 1.1f;
	private readonly string _kawooshModel = "models/sbox_stargate/kawoosh/kawoosh.vmdl";

	private readonly CancellationTokenSource _cancellationTokenSource = new();
	private EventHandler _animationEventHandler;
	private CapsuleLightEntity _capsuleLightEntity;

	private float _elapsedTime;
	private float _startValue = 1.45f;
	private float _endValue;
	private bool _isAnimationComplete;
	private float _lightMovementDistance;
	private Vector3 _lightMovementDirection = Vector3.Forward;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		SetModel( _kawooshModel );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );

		Tags.Add( "trigger" );

		EnableAllCollisions = false;
		EnableTraceAndQueries = false;
		EnableTouch = true;
		EnableShadowReceive = false;
		_lightMovementDistance = 6f * Scale;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !Game.IsServer )
		{
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( !Game.IsServer )
		{
		}
	}

	[GameEvent.Client.Frame]
	public void AnimationFrame()
	{
		_animationEventHandler?.Invoke( this, null );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_capsuleLightEntity.Delete();
	}

	internal async Task RunAnimation()
	{
		_capsuleLightEntity = new CapsuleLightEntity
		{
			Position = Position,
			Parent = Parent,
			Rotation = Rotation,
			Color = new Color( 25, 178, 255 ),
			LightSize = 10f
		};

		const float morphInitialValue = 1f;
		MorphKawoosh( morphInitialValue );
		EnableDrawing = true;

		_animationEventHandler += OnAnimationEventHandler;

		await WaitForAnimation();
	}

	private async Task WaitForAnimation()
	{
		await Task.RunInThreadAsync( () =>
		{
			while ( !_cancellationTokenSource.IsCancellationRequested )
			{
			}

			return System.Threading.Tasks.Task.CompletedTask;
		} );
	}

	private void OnAnimationEventHandler( object sender, EventArgs e )
	{
		_elapsedTime += RealTime.Delta;

		var fraction = _elapsedTime / DurationSec;

		var currentValue = _startValue.LerpTo( _endValue, fraction );

		MorphKawoosh( CalculateAnimationValue( currentValue ) );
		_capsuleLightEntity.LocalPosition += _lightMovementDirection * CalculateAnimationValue( Math.Abs( currentValue ) ) * _lightMovementDistance;

		if ( _elapsedTime >= DurationSec )
		{
			if ( _isAnimationComplete )
			{
				_animationEventHandler -= OnAnimationEventHandler;
				_cancellationTokenSource.Cancel();
			}
			_elapsedTime = 0;
			_startValue = 0f;
			_endValue = 1.45f;
			_isAnimationComplete = true;
			_lightMovementDirection = Vector3.Backward;
		}
	}

	/// <summary>
	/// Calculates the animation value based on a given input.
	/// </summary>
	/// <param name="inputValue">Input float value which serves as a base for the calculation.</param>
	/// <returns>Animation value calculated by applying a sinusoidal function on the cubed input value, scaled by a constant factor.</returns>
	private float CalculateAnimationValue( float inputValue )
	{
		return (float)(1.01f * Math.Sin( 0.5f * Math.Pow( inputValue, 3 ) ));
	}

	private void MorphKawoosh( float morphValue )
	{
		const int shapekeyIndex = 0;
		Morphs.Set( shapekeyIndex, morphValue );
	}
}
