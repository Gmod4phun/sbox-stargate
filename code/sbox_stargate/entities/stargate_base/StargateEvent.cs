using Sandbox;

public static class StargateEvent
{
	public const string Reset = "stargate.reset";
	public class ResetAttribute : EventAttribute
	{
		public ResetAttribute() : base( Reset ) { }
	}

	public const string DialBegin = "stargate.dialbegin";
	public class DialBeginAttribute : EventAttribute
	{
		public DialBeginAttribute() : base( DialBegin ) { }
	}

	public const string DialAbort = "stargate.dialabort";
	public class DialAbortAttribute : EventAttribute
	{
		public DialAbortAttribute() : base( DialAbort ) { }
	}

	public const string DialAbortFinished = "stargate.dialabortfinished";
	public class DialAbortFinishedAttribute : EventAttribute
	{
		public DialAbortFinishedAttribute() : base( DialAbortFinished ) { }
	}

	public const string InboundBegin = "stargate.inboundbegin";
	public class InboundBeginAttribute : EventAttribute
	{
		public InboundBeginAttribute() : base( InboundBegin ) { }
	}

	public const string InboundAbort = "stargate.inboundabort";
	public class InboundAbortAttribute : EventAttribute
	{
		public InboundAbortAttribute() : base( InboundAbort ) { }
	}

	public const string GateOpening = "stargate.gateopening";
	public class GateOpeningAttribute : EventAttribute
	{
		public GateOpeningAttribute() : base( GateOpening ) { }
	}

	public const string GateOpen = "stargate.gateopen";
	public class GateOpenAttribute : EventAttribute
	{
		public GateOpenAttribute() : base( GateOpen ) { }
	}

	public const string GateClosing = "stargate.gateclosing";
	public class GateClosingAttribute : EventAttribute
	{
		public GateClosingAttribute() : base( GateClosing ) { }
	}

	public const string GateClosed = "stargate.gateclosed";
	public class GateClosedAttribute : EventAttribute
	{
		public GateClosedAttribute() : base( GateClosed ) { }
	}

	public const string ChevronEncoded = "stargate.chevronencoded";
	public class ChevronEncodedAttribute : EventAttribute
	{
		public ChevronEncodedAttribute() : base( ChevronEncoded ) { }
	}

	public const string ChevronLocked = "stargate.chevronlocked";
	public class ChevronLockedAttribute : EventAttribute
	{
		public ChevronLockedAttribute() : base( ChevronLocked ) { }
	}

	public const string DHDChevronEncoded = "stargate.dhdchevronencoded";
	public class DHDChevronEncodedAttribute : EventAttribute
	{
		public DHDChevronEncodedAttribute() : base( DHDChevronEncoded ) { }
	}

	public const string DHDChevronLocked = "stargate.dhdchevronlocked";
	public class DHDChevronLockedAttribute : EventAttribute
	{
		public DHDChevronLockedAttribute() : base( DHDChevronLocked ) { }
	}

	public const string DHDChevronUnlocked = "stargate.dhdchevronunlocked";
	public class DHDChevronUnlockedAttribute : EventAttribute
	{
		public DHDChevronUnlockedAttribute() : base( DHDChevronUnlocked ) { }
	}

	public const string RingSpinUp = "stargate.ringspinup";
	public class RingSpinUpAttribute : EventAttribute
	{
		public RingSpinUpAttribute() : base( RingSpinUp ) { }
	}

	public const string RingSpinDown = "stargate.ringspindown";
	public class RingSpinDownAttribute : EventAttribute
	{
		public RingSpinDownAttribute() : base( RingSpinDown ) { }
	}

	public const string RingStopped = "stargate.ringstopped";
	public class RingStoppedAttribute : EventAttribute
	{
		public RingStoppedAttribute() : base( RingStopped ) { }
	}

	public const string ReachedDialingSymbol = "stargate.reacheddialingsymbol";
	public class ReachedDialingSymbolAttribute : EventAttribute
	{
		public ReachedDialingSymbolAttribute() : base( ReachedDialingSymbol ) { }
	}

}
