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
		CollisionRules rules = new()
		{
			Defaults = new Dictionary<string, Result>
			{
				["solid"] = Result.Collide,
				["trigger"] = Result.Trigger,
				["ladder"] = Result.Ignore,
				["water"] = Result.Trigger,
				[StargateTags.FakeWorld] = Result.Ignore
			}
		};

		rules.Pairs = new HashSet<Pair>
		{
			new( "solid", "solid", Result.Collide ),
			new( "solid", "trigger", Result.Trigger ),
			new( "trigger", "glass", Result.Ignore ),
			new( "debris", "player", Result.Ignore ),
			new( "player", "glass", Result.Trigger ),
			new( "glass", "glass", Result.Ignore ),

			new( StargateTags.BehindGate, StargateTags.InBufferFront, Result.Ignore ),
			new( StargateTags.BehindGate, StargateTags.BeforeGate, Result.Ignore ),

			new( StargateTags.BeforeGate, StargateTags.InBufferBack, Result.Ignore ),

			new( StargateTags.InBufferFront, StargateTags.FakeWorld, Result.Collide ),
			new( StargateTags.InBufferFront, "world", Result.Ignore ),

			new( StargateTags.InBufferFront, StargateTags.InBufferBack, Result.Ignore ),

			new( StargateTags.InBufferBack, StargateTags.FakeWorld, Result.Collide ),
			new( StargateTags.InBufferBack, "world", Result.Ignore )
		};

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
			var spawnOffset = entityDesc.GetProperty( "SpawnOffset" );
			if ( spawnOffset != null ) ent.Position += (Vector3)spawnOffset.GetValue( ent );

			var spawnOffsetAng = entityDesc.GetProperty( "SpawnOffsetAng" );
			if ( spawnOffsetAng != null )
			{
				var ang = (Angles)spawnOffsetAng.GetValue( ent );
				var newRot = (ent.Rotation.Angles() + ang).ToRotation();
				ent.Rotation = newRot;
			}
		}
	}
}
