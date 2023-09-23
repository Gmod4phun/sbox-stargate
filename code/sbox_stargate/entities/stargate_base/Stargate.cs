using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using Sandbox.sbox_stargate.code;

[Category( "Stargates" )]
public abstract partial class Stargate : Prop, IUse, IWireOutputEntity, IWireInputEntity
{
	/* WIRE SUPPORT */

	// self:CreateWireInputs("Dial Address","Dial String [STRING]","Dial Mode","Start String Dial","Close","Disable Autoclose","Disable Menu","Transmit [STRING]");

	WirePortData IWireEntity.WirePorts { get; } = new WirePortData();

	protected Stargate()
	{
		StargateEventManager = new StargateEventManager();
	}

	public virtual void WireInitialize()
	{
		var inputs = ((IWireEntity)this).WirePorts.inputs;

		this.RegisterInputHandler( "Dial Mode", ( bool value ) =>
		{
		} );

		this.RegisterInputHandler( "Input Address", ( string value ) =>
		{
		} );

		this.RegisterInputHandler( "Dial Address", ( bool value ) =>
		{
			var addr = inputs["Input Address"].value.ToString();
			var mode = (bool) inputs["Dial Mode"].value;

			if ( value )
			{
				if ( mode )
				{
					BeginDialFast( addr );
				}
				else
				{
					BeginDialSlow( addr );
				}
			}
		} );

		this.RegisterInputHandler( "Input Symbol", ( string value ) =>
		{
		} );

		this.RegisterInputHandler( "Encode Symbol", ( bool value ) =>
		{
			var sym = inputs["Input Symbol"].value.ToString().ElementAtOrDefault( 0 );
			var mode = (bool) inputs["Dial Mode"].value;

			if ( value )
			{
				DoManualChevronEncode( sym );
			}
		} );

		this.RegisterInputHandler( "Lock Symbol", ( bool value ) =>
		{
			var sym = inputs["Input Symbol"].value.ToString().ElementAtOrDefault( 0 );
			var mode = (bool) inputs["Dial Mode"].value;

			if ( value )
			{
				DoManualChevronLock( sym );
			}
		} );

		this.RegisterInputHandler( "Open", ( bool value ) =>
		{
			if ( value )
				BeginManualOpen( DialingAddress );
		} );

		this.RegisterInputHandler( "Close", ( bool value ) =>
		{
			if ( value )
				RequestClose( NetworkIdent );
		} );

		this.RegisterInputHandler( "Disable Autoclose", ( bool value ) =>
		{
			AutoClose = !value;
		} );

		this.RegisterInputHandler( "Disable Menu", ( bool value ) =>
		{
			CanOpenMenu = !value;
		} );

		this.RegisterInputHandler( "Toggle Iris", ( bool value ) =>
		{
			if ( value )
				ToggleIris();
		} );
	}

	public virtual PortType[] WireGetOutputs()
	{
		return new PortType[] {
			PortType.Entity("Gate"),
			PortType.Bool("Idle"),
			PortType.Bool("Active"),
			PortType.Bool("Dialing"),
			PortType.Bool("Opening"),
			PortType.Bool("Open"),
			PortType.Bool("Closing"),
			PortType.Bool("Inbound"),
			PortType.Int("Chevrons Encoded"),
			PortType.Bool("Last Chevron Locked"),
			PortType.String("Gate Address"),
			PortType.String("Gate Group"),
			PortType.String("Gate Point of Origin"),
			PortType.String("Gate Full Address"),
			PortType.String("Gate Full Address 8"),
			PortType.String("Gate Full Address 9"),
			PortType.String("Gate Name"),
			PortType.String("Gate Local"),
			PortType.String("Gate Private"),
			PortType.String("Dialing Address"),
			PortType.String("Dialing Symbol"),
			PortType.String("Dialed Symbol"),
			PortType.String("Ring Symbol"),
			PortType.Float("Ring Angle"),
			PortType.Bool("Iris Closed")
		};
	}

	[GameEvent.Tick.Server]
	public void WireThink()
	{
		this.WireTriggerOutput( "Gate", this );
		this.WireTriggerOutput( "Idle", Idle );
		this.WireTriggerOutput( "Active", Active );
		this.WireTriggerOutput( "Dialing", Dialing );
		this.WireTriggerOutput( "Opening", Opening );
		this.WireTriggerOutput( "Open", Open );
		this.WireTriggerOutput( "Closing", Closing );
		this.WireTriggerOutput( "Inbound", Inbound );
		this.WireTriggerOutput( "Chevrons Encoded", ActiveChevrons );
		this.WireTriggerOutput( "Last Chevron Locked", IsLocked || IsLockedInvalid );
		this.WireTriggerOutput( "Gate Address", GateAddress );
		this.WireTriggerOutput( "Gate Group", GateGroup );
		this.WireTriggerOutput( "Gate Point of Origin", PointOfOrigin );
		this.WireTriggerOutput( "Gate Full Address", GateAddress + PointOfOrigin );
		this.WireTriggerOutput( "Gate Full Address 8", GateAddress + GateGroup[0] + PointOfOrigin );
		this.WireTriggerOutput( "Gate Full Address 9", GateAddress + GateGroup + PointOfOrigin );
		this.WireTriggerOutput( "Gate Name", GateName );
		this.WireTriggerOutput( "Gate Local", GateLocal );
		this.WireTriggerOutput( "Gate Private", GatePrivate );
		this.WireTriggerOutput( "Dialing Address", DialingAddress );
		this.WireTriggerOutput( "Dialing Symbol", CurDialingSymbol.ToString() );
		this.WireTriggerOutput( "Dialed Symbol", DialingAddress.Length > 0 ? DialingAddress.Last().ToString() : " " );
		this.WireTriggerOutput( "Ring Symbol", CurRingSymbol.ToString() );
		this.WireTriggerOutput( "Ring Angle", GetRingAngle() );
		this.WireTriggerOutput( "Iris Closed", IsIrisClosed() );
	}

	[Net] public Vector3 SpawnOffset { get; private set; } = new( 0, 0, 95 );

	[Net]
	public IList<Chevron> Chevrons { get; set; } = new();

	[Net]
	public EventHorizon EventHorizon { get; private set; } = null;
	public int EventHorizonSkinGroup = 0;

	[Net]
	public StargateIris Iris { get; set; } = null;

	[Net]
	public Stargate OtherGate { get; set; } = null;
	public GateBearing Bearing;

	public float AutoCloseTime = -1;

	public Dictionary<string, string> SoundDict = new()
	{
		{ "gate_open", "baseValue" },
		{ "gate_close", "baseValue" },
		{ "chevron_open", "baseValue" },
		{ "chevron_close", "baseValue" },
		{ "dial_fail", "baseValue" },
		{ "dial_fail_noclose", "baseValue" },
	};

	[Net] public string GateAddress { get; set; } = "";
	[Net] public string GateGroup { get; protected set; } = "";
	[Net] public int GateGroupLength { get; set; } = 2;
	[Net] public string GateName { get; set; } = "";
	[Net] public bool AutoClose { get; set; } = true;
	[Net] public bool GatePrivate { get; set; } = false;
	[Net] public bool GateLocal { get; set; } = false;
	[Net] public GlyphType GateGlyphType { get; protected set; } = GlyphType.MILKYWAY;
	// Show Wormhole or not
	[Net] public bool ShowWormholeCinematic { get; set; } = false;

	[Net] public bool Busy { get; set; } = false; // this is pretty much used anytime the gate is busy to do anything (usually during animations/transitions)
	[Net] public bool Inbound { get; set; } = false;

	[Net] public bool ShouldStopDialing { get; set; } = false;
	[Net] public GateState CurGateState { get; set; } = GateState.IDLE;
	[Net] public DialType CurDialType { get; set; } = DialType.FAST;
	[Net] public bool IsManualDialInProgress { get; set; } = false;

	// gate state accessors
	public bool Idle { get => CurGateState is GateState.IDLE; }
	public bool Active { get => CurGateState is GateState.ACTIVE; }
	public bool Dialing { get => CurGateState is GateState.DIALING; }
	public bool Opening { get => CurGateState is GateState.OPENING; }
	public bool Open { get => CurGateState is GateState.OPEN; }
	public bool Closing { get => CurGateState is GateState.CLOSING; }

	[Net] public string DialingAddress { get; set; } = "";
	[Net] public int ActiveChevrons { get; set; } = 0;

	[Net] public bool IsLocked { get; set; } = false;
	[Net] public bool IsLockedInvalid { get; set; } = false;

	[Net] public char CurDialingSymbol { get; set; } = ' ';
	[Net] public char CurRingSymbol { get; set; } = ' ';
	[Net] public float CurRingSymbolOffset { get; set; } = 0;
	[Net] public bool CanOpenMenu { get; set; } = true;

	public TimeSince TimeSinceDialAction = 0f;
	public float InactiveDialShutdownTime = 20f;

	public IStargateRamp Ramp = null;

	private StargateWorldPanel WorldPanel;
	protected StargateEventManager StargateEventManager { get; set; }

	// SOUNDS
	public virtual string GetSound( string key )
	{
		return SoundDict.GetValueOrDefault( key, "" );
	}

	// VARIABLE RESET
	public virtual void ResetGateVariablesToIdle()
	{
		ShouldStopDialing = false;
		OtherGate = null;
		Inbound = false;
		Busy = false;
		CurGateState = GateState.IDLE;
		CurDialType = DialType.FAST;
		DialingAddress = "";
		ActiveChevrons = 0;
		IsLocked = false;
		IsLockedInvalid = false;
		AutoCloseTime = -1;
		CurDialingSymbol = ' ';
		IsManualDialInProgress = false;
	}

	// RING ANGLE
	public virtual float GetRingAngle()
	{
		return 0;
	}

	// USABILITY
	public bool IsUsable( Entity user )
	{
		return true; // we should be always usable
	}

	public bool OnUse( Entity user )
	{
		if (CanOpenMenu)
		{
			OpenStargateMenu( To.Single( user ) );
		}
		
		return false; // aka SIMPLE_USE, not continuously
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();
	}

	// EVENT HORIZON

	public void CreateEventHorizon()
	{
		EventHorizon = new EventHorizon();
		EventHorizon.Position = Position;
		EventHorizon.Rotation = Rotation;
		EventHorizon.Scale = Scale;
		EventHorizon.SetParent( this );
		EventHorizon.Gate = this;
		EventHorizon.EventHorizonSkinGroup = EventHorizonSkinGroup;
	}

	public void DeleteEventHorizon()
	{
		EventHorizon?.Delete();
	}

	public async Task EstablishEventHorizon(float delay = 0)
	{
		await GameTask.DelaySeconds( delay );
		if ( !this.IsValid() ) return;

		CreateEventHorizon();
		EventHorizon.Establish();

		await GameTask.DelaySeconds( 3f );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		EventHorizon.IsFullyFormed = true;
	}

	public async Task CollapseEventHorizon( float sec = 0 )
	{
		await GameTask.DelaySeconds( sec );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		EventHorizon.IsFullyFormed = false;
		EventHorizon.Collapse();

		await GameTask.DelaySeconds( sec + 2f );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		DeleteEventHorizon();
	}
  
	// IRIS
	public bool HasIris()
	{
		return Iris.IsValid();
	}

	public bool IsIrisClosed()
	{
		return HasIris() && Iris.Closed;
	}

	public void ToggleIris()
	{
		Iris?.Toggle();
	}

	// BEARING
	public bool HasBearing()
	{
		return Bearing.IsValid();
	}
  
	protected override void OnDestroy()
	{
		if ( Ramp != null ) Ramp.Gate.Remove( this );

		if ( Game.IsServer && OtherGate.IsValid() )
		{
			if (OtherGate.Inbound && !OtherGate.Dialing) OtherGate.StopDialing();
			if ( OtherGate.Open ) OtherGate.DoStargateClose();
		}

		KillAllPlayersInTransit();

		base.OnDestroy();
	}

	private void KillAllPlayersInTransit()
	{
		if ( !EventHorizon.IsValid() ) return;

		foreach (var ply in EventHorizon.InTransitPlayers)
		{
			EventHorizon.DissolveEntity( ply );
		}
	}

	// DIALING -- please don't touch any of these, dialing is heavy WIP

	public void MakeBusy( float duration )
	{
		Busy = true;
		AddTask( Time.Now + duration, () => Busy = false, TimedTaskCategory.SET_BUSY );
	}

	public bool CanStargateOpen()
	{
		return ( !Busy && !Opening && !Open && !Closing );
	}

	public bool CanStargateClose()
	{
		return ( !Busy && Open );
	}

	public bool CanStargateStartDial()
	{
		return ( Idle && !Busy && !Dialing && !Inbound && !Open && !Opening && !Closing && !IsLocked );
	}

	public bool CanStargateStopDial()
	{
		if (!Inbound) return (!Busy && Dialing);

		return ( !Busy && Active );
	}

	public bool CanStargateStartManualDial()
	{
		if (!Dialing)
			return CanStargateStartDial();

		return (!IsManualDialInProgress && !IsLocked);
	}

	public bool ShouldGateStopDialing()
	{
		return ShouldStopDialing;
	}

	public async void DoStargateOpen()
	{
		if ( !CanStargateOpen() ) return;

		OnStargateBeginOpen();

		await EstablishEventHorizon( 0.5f );
		if ( !this.IsValid() ) return;

		OnStargateOpened();
	}

	public async void DoStargateClose( bool alsoCloseOther = false )
	{
		if ( !CanStargateClose() ) return;

		if ( alsoCloseOther && OtherGate.IsValid() && OtherGate.Open ) OtherGate.DoStargateClose();

		OnStargateBeginClose();

		await CollapseEventHorizon( 0.25f );
		if ( !this.IsValid() ) return;

		OnStargateClosed();
	}

	public bool IsStargateReadyForInboundFast() // checks if the gate is ready to do a inbound anim for fast dial
	{
		if ( !Dialing )
		{
			return (!Busy && !Open && !Inbound);
		}
		else
		{
			return ( !Busy && !Open && !Inbound && (CurDialType is DialType.SLOW || CurDialType is DialType.DHD || CurDialType is DialType.MANUAL) );
		}
	}

	public bool IsStargateReadyForInboundFastEnd() // checks if the gate is ready to open when finishing fast dial?
	{
		return ( !Busy && !Open && !Dialing && Inbound );
	}

	public bool IsStargateReadyForInboundInstantSlow() // checks if the gate is ready to do inbound for instant or slow dial
	{
		return ( !Busy && !Open && !Inbound );
	}

	public bool IsStargateReadyForInboundDHD() // checks if the gate is ready to be locked onto by dhd dial
	{
		if ( !Dialing )
		{
			return (!Busy && !Open && !Inbound);
		}
		else
		{
			return (!Busy && !Open && !Inbound && (CurDialType == DialType.SLOW || CurDialType is DialType.MANUAL));
		}
	}

	public bool IsStargateReadyForInboundDHDEnd() // checks if the gate is ready to be opened while locked onto by a gate using dhd dial
	{
		if ( !Dialing )
		{
			return (!Busy && !Open && Inbound);
		}
		else
		{
			return (!Busy && !Open && Inbound && (CurDialType == DialType.SLOW || CurDialType is DialType.MANUAL));
		}
	}

	// begin dial
	public virtual void BeginDialFast( string address ) { }
	public virtual void BeginDialSlow( string address, float initialDelay=0 ) { }
	public virtual void BeginDialInstant( string address ) { } // instant gate open, with kawoosh
	public virtual void BeginDialNox( string address ) { } // instant gate open without kawoosh - asgard/ancient/nox style

	// begin inbound
	public virtual void BeginInboundFast( int numChevs )
	{
		if ( Inbound && !Dialing ) StopDialing();
	}

	public virtual void BeginInboundSlow( int numChevs ) // this can be used with Instant dial, too
	{
		if ( Inbound && !Dialing ) StopDialing();
	}

	// DHD DIAL
	public virtual void BeginOpenByDHD( string address ) { } // when dhd dial button is pressed
	public virtual void BeginInboundDHD( int numChevs ) { } // when a dhd dialing gate locks onto another gate


	// stop dial
	public async void StopDialing()
	{
		if ( !CanStargateStopDial() ) return;

		OnStopDialingBegin();

		await GameTask.DelaySeconds( 1.25f );

		OnStopDialingFinish();
	}

	public virtual void OnStopDialingBegin()
	{
		Busy = true;
		ShouldStopDialing = true; // can be used in ring/gate logic to to stop ring/gate rotation

		if (Inbound)
		{
			StargateEventManager.RunInboundBeginEvent( gate: this );
		}
		else
		{
			StargateEventManager.RunDialAbortEvent( gate: this );
		}

		ClearTasksByCategory( TimedTaskCategory.DIALING );

		if ( OtherGate.IsValid() )
		{
			OtherGate.ClearTasksByCategory( TimedTaskCategory.DIALING );

			if ( OtherGate.Inbound && !OtherGate.ShouldStopDialing ) OtherGate.StopDialing();
		}
	}

	public virtual void OnStopDialingFinish()
	{
		ResetGateVariablesToIdle();
		StargateEventManager.RunDialAbortFinishedEvent( gate: this );
	}

	// opening
	public virtual void OnStargateBeginOpen()
	{
		CurGateState = GateState.OPENING;
		Busy = true;
		StargateEventManager.RunGateOpeningEvent( gate: this );
	}

	public virtual void OnStargateOpened()
	{
		CurGateState = GateState.OPEN;
		Busy = false;
		StargateEventManager.RunGateOpenEvent( gate: this );
	}

	// closing
	public virtual void OnStargateBeginClose()
	{
		CurGateState = GateState.CLOSING;
		Busy = true;

		KillAllPlayersInTransit();
		StargateEventManager.RunGateClosingEvent( gate: this );
	}

	public virtual void OnStargateClosed()
	{
		ResetGateVariablesToIdle();
		StargateEventManager.RunGateClosedEvent( gate: this );
	}

	// reset
	public virtual void DoStargateReset()
	{
		ResetGateVariablesToIdle();
		ClearTasks();

		StargateEventManager.RunResetEvent( gate: this );
	}

	public virtual void EstablishWormholeTo(Stargate target)
	{
		target.OtherGate = this;
		OtherGate = target;

		target.Inbound = true;

		target.DoStargateOpen();
		DoStargateOpen();
	}

	// CHEVRON

	public virtual Chevron GetChevron( int num )
	{
		return (num <= Chevrons.Count) ? Chevrons[num - 1] : null;
	}

	public virtual Chevron GetTopChevron()
	{
		return GetChevron( 7 );
	}

	public bool IsChevronActive(int num)
	{
		var chev = GetChevron( num );

		if ( !chev.IsValid() )
			return false;

		return chev.On;
	}

	public virtual void SetChevronsGlowState( bool state, float delay = 0 )
	{
		foreach ( Chevron chev in Chevrons )
		{
			if ( state )
				chev.TurnOn( delay );
			else
				chev.TurnOff( delay );
		}
	}

	public Chevron GetChevronBasedOnAddressLength( int num, int len = 7 )
	{
		if ( len == 8 )
		{
			if ( num == 7 ) return GetChevron( 8 );
			else if ( num == 8 ) return GetChevron( 7 );
		}
		else if ( len == 9 )
		{
			if ( num == 7 ) return GetChevron( 8 );
			else if ( num == 8 ) return GetChevron( 9 );
			else if ( num == 9 ) return GetChevron( 7 );
		}
		return GetChevron( num );
	}

	public int GetChevronOrderOnGateFromChevronIndex( int index )
	{
		if ( index <= 3 ) return index;
		if ( index >= 4 && index <= 7 ) return index + 2;
		return index - 4;
	}

	// DHD/Fast Chevron Encode/Lock
	public virtual void DoDHDChevronEncode(char symbols)
	{
		if ( DialingAddress.Contains( symbols ) )
			return;

		// if we were already dialing but not via DHD, dont do anything
		if ( Dialing && CurDialType != DialType.DHD )
			return;

		TimeSinceDialAction = 0;

		if ( !Dialing ) // if gate wasnt dialing, begin dialing
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.DHD;
		}

		DialingAddress += symbols;
		StargateEventManager.RunDHDChevronEncodedEvent( gate: this, symbols );
	}

	public virtual void DoDHDChevronLock( char symbols )
	{
		if ( DialingAddress.Contains( symbols ) )
			return;

		// if we were already dialing but not via DHD, dont do anything
		if ( CurDialType != DialType.DHD )
			return;

		TimeSinceDialAction = 0;

		DialingAddress += symbols;

		var gate = FindDestinationGateByDialingAddress( this, DialingAddress );
		var isStargateReadyForInboundDhd = (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD());

		IsLocked = true;
		IsLockedInvalid = !isStargateReadyForInboundDhd;

		StargateEventManager.RunDHDChevronLockedEvent( gate: this, symbols, isStargateReadyForInboundDhd);
	}

	// Manual/Slow Chevron Encode/Lock
	public async virtual Task<bool> DoManualChevronEncode( char sym )
	{
		if ( !Symbols.Contains( sym ) )
			return false;

		// if we try to encode 9th symbol, do a lock instead
		if (DialingAddress.Length == 8)
		{
			DoManualChevronLock( sym );
			return false;
		}
			
		if ( !CanStargateStartManualDial() )
			return false;

		// if we were already dialing but not MANUAL, dont do anything
		if ( Dialing && CurDialType != DialType.MANUAL )
			return false;

		if ( DialingAddress.Contains( sym ) )
			return false;

		if ( DialingAddress.Length > 8 )
			return false;

		if ( !Dialing ) // if gate wasnt dialing, begin dialing
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.MANUAL;

			StargateEventManager.RunDialBeginEvent( gate: this, address: string.Empty );
		}

		TimeSinceDialAction = 0;

		return true;
	}

	public async virtual Task<bool> DoManualChevronLock( char sym )
	{
		if ( !Symbols.Contains( sym ) )
			return false;

		// if we try to lock sooner than 7th symbol, do nothing
		if ( DialingAddress.Length < 6 )
		{
			return false;
		}

		if ( !CanStargateStartManualDial() )
			return false;

		if ( !Dialing )
			return false;

		// if we were already dialing but not MANUAL, dont do anything
		if ( Dialing && CurDialType != DialType.MANUAL )
			return false;

		if ( DialingAddress.Contains( sym ) )
			return false;

		if ( DialingAddress.Length < 6 )
			return false;

		TimeSinceDialAction = 0;

		return true;
	}

	public virtual void BeginManualOpen( string address ) { } // when dialing manually, open the gate do the target address

	// THINK
	public void AutoCloseThink()
	{
		if ( AutoClose && AutoCloseTime != -1 && AutoCloseTime <= Time.Now && CanStargateClose() )
		{
			AutoCloseTime = -1;
			DoStargateClose( true );
		}
	}

	public void CloseIfNoOtherGate()
	{
		if ( Open && !OtherGate.IsValid() )
		{
			DoStargateClose();
		}
	}

	public void DhdDialTimerThink()
	{
		if ( Dialing && CurDialType is DialType.DHD && TimeSinceDialAction > InactiveDialShutdownTime )
		{
			StopDialing();
		}
	}

	public void ManualDialTimerThink()
	{
		if ( Dialing && CurDialType is DialType.MANUAL && TimeSinceDialAction > InactiveDialShutdownTime )
		{
			StopDialing();
		}
	}

	[GameEvent.Tick.Server]
	public void StargateTick()
	{
		AutoCloseThink();
		CloseIfNoOtherGate();
		DhdDialTimerThink();
		ManualDialTimerThink();
	}

	/*
	[GameEvent.Client.Frame]
	private void WorldPanelThink()
	{
		var isNearGate = Position.DistanceSquared( Camera.Position ) < (512 * 512);
		if ( isNearGate && !WorldPanel.IsValid() )
			WorldPanel = new StargateWorldPanel( this );
		else if ( !isNearGate && WorldPanel.IsValid() )
			WorldPanel.Delete();
	}
	*/

	// UI Related stuff

	[ClientRpc]
	public void OpenStargateMenu(Dhd dhd = null)
	{
		var hud = Game.RootPanel;
		var count = 0;
		foreach ( StargateMenuV2 menu in hud.ChildrenOfType<StargateMenuV2>() ) count++;

		// this makes sure if we already have the menu open, we cant open it again
		if ( count == 0 ) hud.AddChild( new StargateMenuV2( this, dhd ) );
	}

	[ClientRpc]
	public void RefreshGateInformation() {
		Event.Run("stargate.refreshgateinformation");
	}

	[ConCmd.Server]
	public static void RequestDial(DialType type, string address, int gate, float initialDelay=0) {
		if (FindByIndex( gate ) is Stargate g && g.IsValid()) {
			switch ( type ) {
				case DialType.FAST:
					g.BeginDialFast( address );
					break;

				case DialType.SLOW:
					g.BeginDialSlow( address, initialDelay );
					break;

				case DialType.INSTANT:
					g.BeginDialInstant( address );
					break;
			}
		}
	}

	[ConCmd.Server]
	public static void RequestClose(int gateID) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if ( g.Busy || ((g.Open || g.Active || g.Dialing) && g.Inbound) )
			{
				return;
			}

			if (g.Open)
			{
				g.DoStargateClose( true );
			}
			else if (g.Dialing)
			{
				g.StopDialing();
			}
		}
	}

	[ConCmd.Server]
	public static void ToggleIris(int gateID, int state) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.Iris.IsValid()) {
				if (state == -1)
					g.Iris.Toggle();

				if (state == 0)
					g.Iris.Close();

				if (state == 1)
					g.Iris.Open();
			}
		}
	}

	[ConCmd.Server]
	public static void RequestAddressChange(int gateID, string address) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.GateAddress == address || !IsValidAddressOnly( address ))
				return;

			g.GateAddress = address;

			g.RefreshGateInformation();
		}
	}

	[ConCmd.Server]
	public static void RequestGroupChange( int gateID, string group )
	{
		if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		{
			if ( g.GateGroup == group || !IsValidGroup( group ) || group.Length != g.GateGroupLength )
				return;

			g.GateGroup = group;

			g.RefreshGateInformation();
		}
	}

	[ConCmd.Server]
	public static void RequestNameChange(int gateID, string name) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.GateName == name)
				return;

			g.GateName = name;

			g.RefreshGateInformation();
		}
	}

	[ConCmd.Server]
	public static void SetAutoClose(int gateID, bool state) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.AutoClose == state)
				return;

			g.AutoClose = state;

			g.RefreshGateInformation();
		}
	}

	[ConCmd.Server]
	public static void SetGatePrivate(int gateID, bool state) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.GatePrivate == state)
				return;

			g.GatePrivate = state;

			g.RefreshGateInformation();
		}
	}

	[ConCmd.Server]
	public static void SetGateLocal( int gateID, bool state )
	{
		if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		{
			if ( g.GateLocal == state )
				return;

			g.GateLocal = state;

			g.RefreshGateInformation();
		}
	}

	[ConCmd.Server]
	public static void ToggleWormhole( int gateID, bool state )
	{
		if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		{
			if ( g.ShowWormholeCinematic == state )
				return;

			g.ShowWormholeCinematic = state;

			g.RefreshGateInformation();
		}
	}

	public Stargate FindClosestGate() {
		return Stargate.FindClosestGate(this.Position, 0, new Entity[] { this });
	}

	public static Stargate FindClosestGate(Vector3 postition, float max_distance = 0, Entity[] exclude = null) {
		Stargate current = null;
		float distance = float.PositiveInfinity;

		foreach ( Stargate gate in Entity.All.OfType<Stargate>() ) {
			if (exclude != null && exclude.Contains(gate))
				continue;

			float currDist = gate.Position.Distance(postition);
			if (distance > currDist) {

				if ( max_distance > 0 && currDist > max_distance )
					continue;

				distance = currDist;
				current = gate;
			}
		}

		return current;
	}
}
