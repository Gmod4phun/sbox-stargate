using System.Text.Json;
using Sandbox;

public class StargateJsonModel : JsonModel
{
	public string Name { get; set; }
	public string Address { get; set; }
	public string Group { get; set; }
	public bool Private { get; set; }
	public bool AutoClose { get; set; }
	public bool Local { get; set; }

	public bool OnRamp { get; set; }
}

public partial class Stargate : IGateSpawner
{
	public virtual object ToJson()
	{
		return new StargateJsonModel
		{
			EntityName = ClassName,
			Position = Position,
			Rotation = Rotation,
			Name = GateName,
			Address = GateAddress,
			Group = GateGroup,
			Private = GatePrivate,
			AutoClose = AutoClose,
			Local = GateLocal,
			OnRamp = Ramp is not null && (Ramp as Entity).IsValid()
		};
	}

	public virtual async void FromJson( JsonElement data )
	{
		Position = Vector3.Parse( data.GetProperty( "Position" ).ToString() );
		Rotation = Rotation.Parse( data.GetProperty( "Rotation" ).ToString() );
		GateName = data.GetProperty( nameof(StargateJsonModel.Name) ).ToString();
		GateAddress = data.GetProperty( nameof(StargateJsonModel.Address) ).ToString();
		GateGroup = data.GetProperty( nameof(StargateJsonModel.Group) ).ToString();
		GatePrivate = data.GetProperty( nameof(StargateJsonModel.Private) ).GetBoolean();
		AutoClose = data.GetProperty( nameof(StargateJsonModel.AutoClose) ).GetBoolean();
		GateLocal = data.GetProperty( nameof(StargateJsonModel.Local) ).GetBoolean();

		var onRamp = data.GetProperty( nameof(StargateJsonModel.OnRamp) ).GetBoolean();
		if ( onRamp )
		{
			await GameTask.Delay( 1000 );
			var ramp = IStargateRamp.GetClosest( Position, 100f );
			if ( ramp is not null && (ramp as Entity).IsValid() )
				PutGateOnRamp( this, ramp );
		}
	}
}
