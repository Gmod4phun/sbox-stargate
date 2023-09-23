using System;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Utility;

public partial class RingRing : KeyframeEntity {

	public Rings RingParent;

	public bool isUpsideDown = false;

	private bool reachedPos = false;
	public bool Ready {
		get {
			return reachedPos;
		}
	}
	public Vector3 desiredPos;

	public bool ShouldRetract = false;

	public override void Spawn() {
		base.Spawn();
		Tags.Add( "solid", "no_rings_teleport" );

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/rings_ancient/ring_ancient.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		EnableAllCollisions = false;
		RenderColor = RenderColor.WithAlpha( 0 );
	}

	public void MoveFinished() {
		reachedPos = true;

		if (ShouldRetract) {
			RingParent.OnRingReturn();
			Delete();
		}
	}

	public void MoveBlocked( Entity ent ) {
		var dmg = new DamageInfo();
		dmg.Attacker = RingParent;
		dmg.Damage = 200;
		ent.TakeDamage( dmg );
	}

	public void MoveUp() {
		RenderColor = RenderColor.WithAlpha(1);
		EnableAllCollisions = true;
		ShouldRetract = false;
		Move();
	}

	public async void Move() {
		var targetPos = ShouldRetract ? RingParent.Position : RingParent.Transform.PointToWorld( desiredPos );

		//Log.Info( $"BasePos = {RingParent.Position}, TargetPos = {targetPos}" );

		var newTransform = new Transform( targetPos, Rotation );

		var moveDone = await KeyframeTo( newTransform, 0.3f, Easing.QuadraticInOut);

		if ( moveDone )
		{
			MoveFinished();
		}

	}

	public void Retract() {
		ShouldRetract = true;
		Move();
	}

}
