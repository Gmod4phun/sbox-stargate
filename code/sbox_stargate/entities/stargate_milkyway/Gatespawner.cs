public class StargateMilkyWayJsonModel : StargateJsonModel
{
	public StargateMilkyWayJsonModel( StargateJsonModel parent )
	{
		EntityName = parent.EntityName;
		Position = parent.Position;
		Rotation = parent.Rotation;
		Name = parent.Name;
		Address = parent.Address;
		Group = parent.Group;
		Private = parent.Private;
		AutoClose = parent.AutoClose;
		Local = parent.Local;
		OnRamp = parent.OnRamp;
	}

	public bool MovieDialingType { get; set; }

	public bool ChevronLightup { get; set; }
}

public partial class StargateMilkyWay : IGateSpawner
{
	public override object ToJson()
	{
		var parent = (StargateJsonModel)base.ToJson();
		return new StargateMilkyWayJsonModel( parent ) { MovieDialingType = MovieDialingType, ChevronLightup = ChevronLightup };
	}

	public override void FromJson( System.Text.Json.JsonElement data )
	{
		base.FromJson( data );

		MovieDialingType = data.GetProperty( nameof(StargateMilkyWayJsonModel.MovieDialingType) ).GetBoolean();
		ChevronLightup = data.GetProperty( nameof(StargateMilkyWayJsonModel.ChevronLightup) ).GetBoolean();
	}
}
