using System;
using System.Collections.Generic;

namespace Sandbox.Tools
{
	[Library( "tool_stargate_spawner", Title = "Stargate", Description = "Use wormholes to transport matter\n\nMOUSE1 - Spawn gate\nE + MOUSE1 - Cycle gate types\nR - copy gate address\n\nMOUSE2 - Close gate/Stop dialling/Fast dial copied address\nSHIFT + MOUSE2 - Slow dial copied address\nCTRL + MOUSE2 - Instant dial copied address\n", Group = "construction" )]
	public partial class StargateSpawnerTool : BaseTool
	{
		PreviewEntity previewModel;

		static Stargate CopiedGate = null;

		[Net] private string Model { get; set; } = "models/sbox_stargate/gate_sg1/gate_sg1.vmdl";
		[Net] private int CurGateType { get; set; } = 0;

		private List<string> GateTypes = new() { "StargateMilkyWay", "StargateMovie", "StargatePegasus", "StargateUniverse" };
		private List<string> GateModels = new() { "models/sbox_stargate/gate_sg1/gate_sg1.vmdl", "models/sbox_stargate/gate_sg1/ring_sg1.vmdl", "models/sbox_stargate/gate_atlantis/gate_atlantis.vmdl", "models/sbox_stargate/gate_universe/gate_universe.vmdl" };

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
					previewModel.PositionOffset = new Vector3( 0, 0, 90 );
					previewModel.RotationOffset = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
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

				preview.Rotation = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
				preview.SetModel( Model );
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

					if (Input.Down(InputButton.Use))
					{
						CurGateType++;
						if ( CurGateType >= GateTypes.Count ) CurGateType = 0;
						Model = GateModels[CurGateType];
						Log.Info( $"{Model}  |  {CurGateType}" );
						return;
					}

					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPosition );

					if ( tr.Entity is Stargate )
					{
						// TODO: Set properties

						return;
					}

					var gateType = TypeLibrary.GetType<Entity>( GateTypes[CurGateType] )?.TargetType;
					if ( gateType == null ) return;

					var gate = TypeLibrary.Create<Entity>( gateType ) as Stargate;
					gate.Position = tr.EndPosition;
					gate.Rotation = new Angles( 0, Owner.EyeRotation.Angles().yaw + 180, 0 ).ToRotation();
					Event.Run( "entity.spawned", gate, Owner );

					if ( tr.Entity is IStargateRamp ramp ) Stargate.PutGateOnRamp( gate, ramp );
				}

				if ( Input.Pressed( InputButton.Reload ) )
				{					
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPosition );

					if ( tr.Entity is Stargate gate)
					{
						CopiedGate = gate;
						return;
					}

				}


				if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{
					if ( !CopiedGate.IsValid() )
						return;

					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPosition );

					if ( tr.Entity is Stargate gate )
					{
						if ( gate.Busy ) return;

						if (gate.Open)
						{
							gate.DoStargateClose(true);
						}
						else
						{
							if ( !gate.Dialing )
							{
								var finalAddress = Stargate.GetOtherGateAddressForMenu( gate, CopiedGate );
								Log.Info( $"Dialing {finalAddress}" );

								if (Input.Down(InputButton.Run))
								{
									gate.BeginDialSlow( finalAddress );
								}
								else if (Input.Down(InputButton.Duck))
								{
									gate.BeginDialInstant( finalAddress );
								}
								else
								{
									gate.BeginDialFast( finalAddress );
								}
							}
							else
							{
								gate.StopDialing();
							}
						}


					}

				}


			}
		}
	}
}
