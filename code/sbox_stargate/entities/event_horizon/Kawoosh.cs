using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

public partial class Kawoosh : ModelEntity
{
	private readonly string _kawooshModel = "models/sbox_stargate/kawoosh/kawoosh.vmdl";

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
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !Game.IsServer ) return;
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( !Game.IsServer ) return;

	}

	internal async Task RunAnimation()
	{
		Morphs.Set( 0, 1f );
		EnableDrawing = true;

		var cts = new CancellationTokenSource();
		var token = cts.Token;

		const double morphDelta = 0.02;

		_ = RotateKawoosh( token );

		for ( double morphValue = 1; morphValue > 0; morphValue -= morphDelta )
		{
			await MorphKawoosh( (float)morphValue );
		}

		await GameTask.DelaySeconds( 0.5f );

		for ( double morphValue = 0; morphValue < 1; morphValue += morphDelta )
		{
			await MorphKawoosh( (float)morphValue );
		}

		cts.Cancel();
	}

	private async Task MorphKawoosh(float morphValue)
	{
		const int shapekeyIndex = 0;
		await GameTask.DelaySeconds( 0.001f );
		Morphs.Set( shapekeyIndex, morphValue );
	}

	private async Task RotateKawoosh(CancellationToken token)
	{
		var random = new Random();
		while ( token.IsCancellationRequested is false )
		{
			await GameTask.DelaySeconds( 0.001f );
			Rotation = Rotation.RotateAroundAxis( Vector3.Forward, Rotation.x - random.Next(1, 16) );
		}
	}
}
