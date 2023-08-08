using Sandbox;
using Sandbox.Physics;
using System.Collections.Generic;
using static Sandbox.Physics.CollisionRules;

partial class StargateAddon
{
	[Event( "game.init" )]
	public static void Initialize()
	{
		Log.Info( "Init S&Box Stargate" );
		ApplyStargateCollisionRules();
	}

	public static void ApplyStargateCollisionRules()
	{
		if ( !Game.IsServer ) return;

		// todo addon: ideally this would _extend_ the gamemode's collision rules rather than replace them, but I can't find a PhysicsWorld.GetCollisionRules
		CollisionRules rules = new CollisionRules();

		rules.Defaults = new Dictionary<string, Result>();
		rules.Defaults["solid"] = Result.Collide;
		rules.Defaults["trigger"] = Result.Trigger;
		rules.Defaults["ladder"] = Result.Ignore;
		rules.Defaults["water"] = Result.Trigger;
		rules.Defaults[StargateTags.FakeWorld] = Result.Ignore;

		rules.Pairs = new HashSet<Pair>();
		rules.Pairs.Add( new Pair( "solid", "solid", Result.Collide ) );
		rules.Pairs.Add( new Pair( "solid", "trigger", Result.Trigger ) );
		rules.Pairs.Add( new Pair( "trigger", "glass", Result.Ignore ) );
		rules.Pairs.Add( new Pair( "debris", "player", Result.Ignore ) );
		rules.Pairs.Add( new Pair( "player", "glass", Result.Trigger ) );
		rules.Pairs.Add( new Pair( "glass", "glass", Result.Ignore ) );

		rules.Pairs.Add( new Pair( StargateTags.BehindGate, StargateTags.InBufferFront, Result.Ignore ) );
		rules.Pairs.Add( new Pair( StargateTags.BehindGate, StargateTags.BeforeGate, Result.Ignore ) );

		rules.Pairs.Add( new Pair( StargateTags.BeforeGate, StargateTags.InBufferBack, Result.Ignore ) );

		rules.Pairs.Add( new Pair( StargateTags.InBufferFront, StargateTags.FakeWorld, Result.Collide ) );
		rules.Pairs.Add( new Pair( StargateTags.InBufferFront, "world", Result.Ignore ) );

		rules.Pairs.Add( new Pair( StargateTags.InBufferFront, StargateTags.InBufferBack, Result.Ignore ) );

		rules.Pairs.Add( new Pair( StargateTags.InBufferBack, StargateTags.FakeWorld, Result.Collide ) );
		rules.Pairs.Add( new Pair( StargateTags.InBufferBack, "world", Result.Ignore ) );

		Game.PhysicsWorld.SetCollisionRules( rules );

		Log.Info( "Applied SG collision rules" );
	}

	[Event( "entity.spawned" )]
	public static void OnEntitySpawned( IEntity ent, IEntity owner )
	{
		var entityDesc = TypeLibrary.GetType( ent.GetType() );

		var hasSpawnOffsetProperty = entityDesc.GetProperty( "SpawnOffset" ) != null;
		if ( hasSpawnOffsetProperty ) // spawn offsets for Stargate stuff
		{
			var property_spawnoffset = entityDesc.GetProperty( "SpawnOffset" );
			if ( property_spawnoffset != null ) ent.Position += (Vector3)property_spawnoffset.GetValue( ent );

			var property_spawnoffset_ang = entityDesc.GetProperty( "SpawnOffsetAng" );
			if ( property_spawnoffset_ang != null )
			{
				var ang = (Angles)property_spawnoffset_ang.GetValue( ent );
				var newRot = (ent.Rotation.Angles() + ang).ToRotation();
				ent.Rotation = newRot;
			}
		}
	}
}
