namespace Sandbox.Tools
{
	[Library( "tool_dhd_spawner", Title = "DHD", Description = "Used to control a Stargate.\n\nMOUSE1 - Spawn DHD\nMOUSE2 - Update DHD", Group = "construction" )]
	public partial class DhdSpawnerTool : BaseTool
	{
		PreviewEntity previewModel;

		private string Model => "models/sbox_stargate/dhd/dhd.vmdl";
		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is Stargate )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				if (Owner.IsValid())
				{
					previewModel.RelativeToNormal = false;
					previewModel.OffsetBounds = false;
					previewModel.PositionOffset = new Vector3( 0, 0, -5 );
					previewModel.RotationOffset = new Angles( 15, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
				}

			}
		}

		public override void OnFrame()
		{
			base.OnFrame();

			if ( Owner.IsValid() && Owner.Health > 0)
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

				preview.Rotation = new Angles( 15, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();

			}
		}

		public override void Simulate()
		{
			if ( !Game.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.PrimaryAttack ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPosition );

					var dhd = new DhdMilkyWay();
					dhd.Position = tr.EndPosition;
					dhd.Rotation = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
					Event.Run( "entity.spawned", dhd, Owner );
				}
				else if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					if (tr.Entity is Dhd dhd)
					{
						CreateHitEffects( tr.EndPosition );

						dhd.Gate = Stargate.FindNearestGate( dhd );
					}
				}

			}
		}
	}
}
