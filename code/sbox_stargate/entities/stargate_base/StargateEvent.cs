using Sandbox;

public static class StargateEvent
{
	public const string Reset = "stargate.reset";

	public const string DialBegin = "stargate.dialbegin";

	public const string DialAbort = "stargate.dialabort";

	public const string DialAbortFinished = "stargate.dialabortfinished";

	public const string InboundBegin = "stargate.inboundbegin";

	public const string InboundAbort = "stargate.inboundabort";

	public const string GateOpening = "stargate.gateopening";

	public const string GateOpen = "stargate.gateopen";

	public const string GateClosing = "stargate.gateclosing";

	public const string GateClosed = "stargate.gateclosed";

	public const string ChevronEncoded = "stargate.chevronencoded";

	public const string ChevronLocked = "stargate.chevronlocked";

	public const string DHDChevronEncoded = "stargate.dhdchevronencoded";

	public const string DHDChevronLocked = "stargate.dhdchevronlocked";

	public const string DHDChevronUnlocked = "stargate.dhdchevronunlocked";

	public const string RingSpinUp = "stargate.ringspinup";

	public const string RingSpinDown = "stargate.ringspindown";

	public const string RingStopped = "stargate.ringstopped";

	public const string ReachedDialingSymbol = "stargate.reacheddialingsymbol";

	public class DHDChevronEncodedAttribute : EventAttribute
	{
		public DHDChevronEncodedAttribute() : base( DHDChevronEncoded ) { }
	}

	public class DHDChevronLockedAttribute : EventAttribute
	{
		public DHDChevronLockedAttribute() : base( DHDChevronLocked ) { }
	}

	public class DHDChevronUnlockedAttribute : EventAttribute
	{
		public DHDChevronUnlockedAttribute() : base( DHDChevronUnlocked ) { }
	}

	public class DialAbortAttribute : EventAttribute
	{
		public DialAbortAttribute() : base( DialAbort ) { }
	}

	public class DialAbortFinishedAttribute : EventAttribute
	{
		public DialAbortFinishedAttribute() : base( DialAbortFinished ) { }
	}

	public class DialBeginAttribute : EventAttribute
	{
		public DialBeginAttribute() : base( DialBegin ) { }
	}

	public class GateClosedAttribute : EventAttribute
	{
		public GateClosedAttribute() : base( GateClosed ) { }
	}

	public class GateClosingAttribute : EventAttribute
	{
		public GateClosingAttribute() : base( GateClosing ) { }
	}

	public class GateOpenAttribute : EventAttribute
	{
		public GateOpenAttribute() : base( GateOpen ) { }
	}

	public class GateOpeningAttribute : EventAttribute
	{
		public GateOpeningAttribute() : base( GateOpening ) { }
	}

	public class ChevronEncodedAttribute : EventAttribute
	{
		public ChevronEncodedAttribute() : base( ChevronEncoded ) { }
	}

	public class ChevronLockedAttribute : EventAttribute
	{
		public ChevronLockedAttribute() : base( ChevronLocked ) { }
	}

	public class InboundAbortAttribute : EventAttribute
	{
		public InboundAbortAttribute() : base( InboundAbort ) { }
	}

	public class InboundBeginAttribute : EventAttribute
	{
		public InboundBeginAttribute() : base( InboundBegin ) { }
	}

	public class ReachedDialingSymbolAttribute : EventAttribute
	{
		public ReachedDialingSymbolAttribute() : base( ReachedDialingSymbol ) { }
	}

	public class ResetAttribute : EventAttribute
	{
		public ResetAttribute() : base( Reset ) { }
	}

	public class RingSpinDownAttribute : EventAttribute
	{
		public RingSpinDownAttribute() : base( RingSpinDown ) { }
	}

	public class RingSpinUpAttribute : EventAttribute
	{
		public RingSpinUpAttribute() : base( RingSpinUp ) { }
	}

	public class RingStoppedAttribute : EventAttribute
	{
		public RingStoppedAttribute() : base( RingStopped ) { }
	}
}
