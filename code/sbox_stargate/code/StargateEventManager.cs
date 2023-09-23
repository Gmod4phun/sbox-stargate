namespace Sandbox.StargateAddon.code
{
	public class StargateEventManager
	{
		public void GateClosed( Stargate gate )
		{
			Event.Run( StargateEvent.GateClosed, gate );
		}

		public void ChevronEncoded( Stargate gate, int num )
		{
			Event.Run( StargateEvent.ChevronEncoded, gate, num );
		}

		public void ChevronLocked( Stargate gate, int num, bool valid )
		{
			Event.Run( StargateEvent.ChevronLocked, gate, num, valid );
		}

		public void DHDChevronEncoded( Stargate gate, char sym )
		{
			Event.Run( StargateEvent.DHDChevronEncoded, gate, sym );
		}

		public void DHDChevronLocked( Stargate gate, char sym, bool valid )
		{
			Event.Run( StargateEvent.DHDChevronLocked, gate, sym, valid );
		}

		public void DHDChevronUnlocked( Stargate gate, char sym )
		{
			Event.Run( StargateEvent.DHDChevronUnlocked, gate, sym );
		}

		public void RingSpinDown( Stargate gate )
		{
			Event.Run( StargateEvent.RingSpinDown, gate );
		}

		public void ReachedDialingSymbol( Stargate gate, char sym )
		{
			Event.Run( StargateEvent.ReachedDialingSymbol, gate, sym );
		}

		public void DialBegin( Stargate gate, string address )
		{
			Event.Run( StargateEvent.DialBegin, gate, address );
		}

		public void InboundBegin( Stargate gate )
		{
			Event.Run( StargateEvent.InboundBegin, gate );
		}

		public void DialAbortFinished( Stargate gate )
		{
			Event.Run( StargateEvent.DialAbortFinished, gate );
		}

		public void Reset( Stargate gate )
		{
			Event.Run( StargateEvent.Reset, gate );
		}

		public void InboundAbort( Stargate gate )
		{
			Event.Run( StargateEvent.InboundAbort, gate );
		}

		public void DialAbort( Stargate gate )
		{
			Event.Run( StargateEvent.DialAbort, gate );
		}

		public void GateOpening( Stargate gate )
		{
			Event.Run( StargateEvent.GateOpening, gate );
		}

		public void GateOpen( Stargate gate )
		{
			Event.Run( StargateEvent.GateOpen, gate );
		}

		public void GateClosing( Stargate gate )
		{
			Event.Run( StargateEvent.GateClosing, gate );
		}

		public void RingStopped( Stargate gate )
		{
			Event.Run( StargateEvent.RingStopped, gate );
		}
	}
}
