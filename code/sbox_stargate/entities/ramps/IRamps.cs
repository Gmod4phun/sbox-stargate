using System.Collections.Generic;
using System.Linq;
using Sandbox;

public interface IStargateRamp
{
	int AmountOfGates { get; }
	Vector3[] StargatePositionOffset { get; }
	Angles[] StargateRotationOffset { get; }

	List<Stargate> Gate { get; set; }

	public static IStargateRamp GetClosest( Vector3 position, float max = -1f )
	{
		var ramps = Entity.All.OfType<IStargateRamp>().Where( x => x.IsGateSlotFree() );

		if ( !ramps.Any() ) return null;

		float dist = -1f;
		IStargateRamp ramp = null;
		foreach ( IStargateRamp r in ramps )
		{
			var currDistance = position.Distance( (r as Entity).Position );
			if ( max != -1f && currDistance > max )
				continue;

			if ( dist == -1f || currDistance < dist )
			{
				dist = currDistance;
				ramp = r;
			}
		}

		return ramp;
	}

	public bool IsGateSlotFree() => Gate.Count < AmountOfGates;
}

public interface IRingsRamp
{
	Vector3 RingsPositionOffset { get; }
	Angles RingsRotationOffset { get; }

	Rings RingBase { get; set; }
}

public interface IDHDRamp
{
	Vector3 DHDPositionOffset { get; }
	Angles DHDRotationOffset { get; }

	Rings DHD { get; set; }
}
