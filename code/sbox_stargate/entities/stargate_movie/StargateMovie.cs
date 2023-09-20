using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "Stargate (Movie)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class StargateMovie : StargateMilkyWay
{
	public StargateMovie()
	{
		SoundDict = new()
		{
			{ "gate_open", "stargate.movie.open" },
			{ "gate_close", "stargate.movie.close" },
			{ "chevron_open", "stargate.movie.chevron_open" },
			{ "chevron_close", "stargate.movie.chevron_close" },
			{ "dial_fail", "stargate.milkyway.dial_fail_noclose" },
			{ "dial_fail_noclose", "stargate.milkyway.dial_fail_noclose" },
			{ "dial_begin_9chev", "stargate.universe.dial_begin_9chev" },
			{ "dial_fail_9chev", "stargate.universe.dial_fail_9chev" }
		};

		GateGlyphType = GlyphType.MILKYWAY;

		MovieDialingType = true;
		ChevronLightup = false;
	}

	public static void DrawGizmos( EditorContext context )
	{
		Gizmo.Draw.Model( "models/sbox_stargate/sg_mw/sg_mw_ring.vmdl" );

		for ( var i = 0; i < 9; i++ )
		{
			Gizmo.Draw.Model( "models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl", new Transform( Vector3.Zero, Rotation.FromRoll( i * 40 ) ) );
		}
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();
		SetBodyGroup( 0, 1 );

		Ring.StartSoundName = "stargate.movie.ring_roll";
		Ring.StopSoundName = "";
		Ring.StopSoundOnSpinDown = true;

		Ring.SetBodyGroup( 0, 1 );
	}

	// CHEVRONS

	public override Chevron CreateChevron( int n )
	{
		var chev = base.CreateChevron( n );
		chev.UsesDynamicLight = false;

		chev.ChevronStateSkins = new() { { "Off", 2 }, { "On", ChevronLightup ? 1 : 2 }, };

		if ( n == 7 ) chev.SetBodyGroup( 0, 1 );

		return chev;
	}
}
