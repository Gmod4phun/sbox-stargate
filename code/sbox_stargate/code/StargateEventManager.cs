namespace Sandbox.sbox_stargate.code
{
	public class StargateEventManager
	{
		public void RunGateClosedEvent( Stargate gate )
		{
			Event.Run( StargateEvent.GateClosed, gate );
		}

		public void RunChevronEncodedEvent( Stargate gate, int num )
		{
			Event.Run( StargateEvent.ChevronEncoded, gate, num );
		}

		public void RunChevronLockedEvent( Stargate gate, int num, bool valid )
		{
			Event.Run( StargateEvent.ChevronLocked, gate, num, valid );
		}

		public void RunDHDChevronEncodedEvent( Stargate gate, char sym )
		{
			Event.Run( StargateEvent.DHDChevronEncoded, gate, sym );
		}

		public void RunDHDChevronLockedEvent( Stargate gate, char sym, bool valid )
		{
			Event.Run( StargateEvent.DHDChevronLocked, gate, sym, valid );
		}

		public void RunDHDChevronUnlockedEvent( Stargate gate, char sym )
		{
			Event.Run( StargateEvent.DHDChevronUnlocked, gate, sym );
		}

		public void RunRingSpinDownEvent( Stargate gate )
		{
			Event.Run( StargateEvent.RingSpinDown, gate );
		}

		public void RunReachedDialingSymbolEvent( Stargate gate, char sym )
		{
			Event.Run( StargateEvent.ReachedDialingSymbol, gate, sym );
		}

		public void RunDialBeginEvent( Stargate gate, string address )
		{
			Event.Run( StargateEvent.DialBegin, gate, address );
		}

		public void RunInboundBeginEvent( Stargate gate )
		{
			Event.Run( StargateEvent.InboundBegin, gate );
		}

		public void RunDialAbortFinishedEvent( Stargate gate )
		{
			Event.Run( StargateEvent.DialAbortFinished, gate );
		}

		public void RunResetEvent( Stargate gate )
		{
			Event.Run( StargateEvent.Reset, gate );
		}

		public void RunInboundAbortEvent( Stargate gate )
		{
			Event.Run( StargateEvent.InboundAbort, gate );
		}

		public void RunDialAbortEvent( Stargate gate )
		{
			Event.Run( StargateEvent.DialAbort, gate );
		}

		public void RunGateOpeningEvent( Stargate gate )
		{
			Event.Run( StargateEvent.GateOpening, gate );
		}

		public void RunGateOpenEvent( Stargate gate )
		{
			Event.Run( StargateEvent.GateOpen, gate );
		}

		public void RunGateClosingEvent( Stargate gate )
		{
			Event.Run( StargateEvent.GateClosing, gate );
		}

		public void RunRingStoppedEvent( Stargate gate )
		{
			Event.Run( StargateEvent.RingStopped, gate );
		}
	}
}
