namespace Sandbox.Tools
{
	[Library( "tool_stargate_iris", Title = "Iris", Description = "Used to create the iris on the gate.\n\nMOUSE1 - Spawn Iris\nE + MOUSE1 - Spawn Atlantis Gate Shield\nMOUSE2 - Remove Iris\nR - Toggle Iris", Group = "construction" )]
	public partial class StargateIrisTool : BaseTool
	{
		private PreviewEntity _previewModel;
		private string Model => "models/sbox_stargate/iris/iris.vmdl";

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref _previewModel, Model ) )
			{
				if ( Owner.IsValid() )
				{
					_previewModel.RelativeToNormal = false;
					_previewModel.OffsetBounds = false;
					_previewModel.PositionOffset = new Vector3( 0, 0, 90 );
					_previewModel.RotationOffset = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
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

					if ( tr.Entity is Stargate gate )
					{
						var iris = Stargate.AddIris( gate, Owner, Input.Down( InputButton.Use ) );
						iris.Close();
						Event.Run( "entity.spawned", iris, Owner );
						CreateHitEffects( tr.EndPosition );
					}
				}

				if ( Input.Pressed( InputButton.Reload ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;
					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance ).Ignore( Owner ).Run();

					if ( !tr.Hit || !tr.Entity.IsValid() ) return;

					if ( tr.Entity is Stargate gate && gate.Iris.IsValid() )
					{
						gate.Iris.Toggle();
						CreateHitEffects( tr.EndPosition );
					}
				}

				if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;
					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance ).Ignore( Owner ).Run();

					if ( !tr.Hit || !tr.Entity.IsValid() ) return;

					if ( tr.Entity is Stargate gate )
					{
						Stargate.RemoveIris( gate );
						CreateHitEffects( tr.EndPosition );
					}
				}
			}
		}

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is Stargate )
				return false;

			return true;
		}
	}
}
