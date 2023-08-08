namespace Sandbox.Tools
{
	[Library( "tool_stargate_bearing", Title = "Gate Bearing", Description = "Used to create a bearing on the Universe gate.\n\nMOUSE1 - Spawn Bearing\nMOUSE2 - Remove Bearing", Group = "construction" )]
	public partial class GateBearingTool : BaseTool
	{
		PreviewEntity previewModel;
		private string Model => "models/sbox_stargate/universe_bearing/universe_bearing.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is StargateUniverse )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				if ( Owner.IsValid() )
				{
					previewModel.RelativeToNormal = false;
					previewModel.OffsetBounds = false;
					previewModel.PositionOffset = new Vector3( 0, 0, 90 );
					previewModel.RotationOffset = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
				}

			}
		}

		public override void OnFrame()
		{
			base.OnFrame();

			if ( Owner.IsValid() && Owner.Health > 0 )
			{
				RefreshPreviewAngles();
			}
		}

		public void RefreshPreviewAngles()
		{
			foreach ( var preview in Previews )
			{
				if ( !preview.IsValid() || !Owner.IsValid() )
					continue;

				preview.Rotation = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
			}
		}

		public override void Simulate()
		{
			if ( !Game.IsServer ) return;

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.PrimaryAttack ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;
					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance ).Ignore( Owner ).Run();

					if ( !tr.Hit || !tr.Entity.IsValid() ) return;

					if ( tr.Entity is StargateUniverse gate )
					{
						var bearing = Stargate.AddBearing( gate );
						Event.Run( "entity.spawned", bearing, Owner );
						CreateHitEffects( tr.EndPosition );
					}
				}

				if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;
					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance ).Ignore( Owner ).Run();

					if ( !tr.Hit || !tr.Entity.IsValid() ) return;

					if ( tr.Entity is StargateUniverse gate )
					{
						Stargate.RemoveBearing( gate );
						CreateHitEffects( tr.EndPosition );
					}
				}

			}
		}
	}
}
