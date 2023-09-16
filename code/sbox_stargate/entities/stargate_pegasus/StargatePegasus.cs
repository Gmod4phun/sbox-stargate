using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Editor;
using Sandbox;

[HammerEntity, SupportsSolid, EditorModel( MODEL )]
[Title( "Stargate (Pegasus)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class StargatePegasus : Stargate
{
	public const string MODEL = "models/sbox_stargate/sg_peg/sg_peg_gate.vmdl";

	public StargateRingPegasus Ring;
	public List<Chevron> EncodedChevronsOrdered = new ();

	public StargatePegasus()
	{
		SoundDict = new()
		{
			{ "gate_open", "stargate.atlantis.open" },
			{ "gate_close", "stargate.milkyway.close" },
			{ "gate_roll_fast", "stargate.atlantis.roll" },
			{ "gate_roll_slow", "stargate.atlantis.roll_slow" },
			{ "chevron", "stargate.atlantis.chevron_roll" },
			{ "chevron_inbound", "stargate.atlantis.chevron_roll_incoming" },
			{ "chevron_inbound_longer", "stargate.atlantis.chevron_roll_incoming_long" },
			{ "chevron_inbound_shorter", "stargate.atlantis.chevron_roll_incoming_short" },
			{ "chevron_lock", "stargate.atlantis.chevron_lock" },
			{ "chevron_lock_inbound", "stargate.atlantis.chevron_lock_incoming" },
			{ "chevron_dhd", "stargate.atlantis.chevron" },
			{ "dial_fail", "stargate.atlantis.dial_fail" }
		};

		GateGlyphType = GlyphType.PEGASUS;

		EventHorizonSkinGroup = 3;
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( MODEL );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateRing();
		CreateAllChevrons();

		GateGroup = "P@";
		GateAddress = GenerateGateAddress( GateGroup );
	}

	public override void ResetGateVariablesToIdle()
	{
		base.ResetGateVariablesToIdle();

		EncodedChevronsOrdered.Clear();
	}

	// RING
	public void CreateRing()
	{
		Ring = new();
		Ring.Position = Position;
		Ring.Rotation = Rotation;
		Ring.SetParent( this );
		Ring.Gate = this;
		Ring.Transmit = TransmitType.Always;
	}

	// CHEVRONS

	public virtual Chevron CreateChevron( int n )
	{
		var chev = new Chevron();
		chev.SetModel( "models/sbox_stargate/sg_peg/sg_peg_chevron.vmdl" );
		chev.Position = Position;
		chev.Rotation = Rotation.Angles().WithRoll( -ChevronAngles[n-1] ).ToRotation();
		chev.SetParent( this );
		chev.Transmit = TransmitType.Always;
		chev.Gate = this;

		//chev.Light.SetLightColor( Color.Parse( "#00A9FF" ).GetValueOrDefault() );

		chev.UsesDynamicLight = false;

		return chev;
	}

	public void CreateAllChevrons()
	{
		for (int i = 1; i <= 9; i++ )
		{
			var chev = CreateChevron( i );
			Chevrons.Add( chev );
		}
	}

	// DIALING

	public override void OnStopDialingBegin()
	{
		base.OnStopDialingBegin();

		PlaySound( this, GetSound( "dial_fail" ) );
		Ring?.StopRollSound();
		ClearTasksByCategory( TimedTaskCategory.SYMBOL_ROLL_PEGASUS_DHD );
		ClearTasksByCategory( TimedTaskCategory.DIALING );
	}

	public override void OnStopDialingFinish()
	{
		base.OnStopDialingFinish();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
		Ring?.SetRingState( true );
	}

	public override void OnStargateBeginOpen()
	{
		base.OnStargateBeginOpen();

		PlaySound( this, GetSound( "gate_open" ) );
	}

	public override void OnStargateOpened()
	{
		base.OnStargateOpened();
	}

	public override void OnStargateBeginClose()
	{
		base.OnStargateBeginClose();

		PlaySound( this, GetSound( "gate_close" ) );
	}

	public override void OnStargateClosed()
	{
		base.OnStargateClosed();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
		Ring?.SetRingState( true );
	}

	public override void DoStargateReset()
	{
		if ( Dialing ) ShouldStopDialing = true;

		base.DoStargateReset();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
		Ring?.StopRollSound();
		Ring?.SetRingState( true );
	}


	// CHEVRON ANIMS & SOUNDS

	public void ChevronActivate( Chevron chev, float delay = 0, bool turnon = true, bool chevLock = false, bool longer = false, bool shorter = false, bool nosound = false )
	{
		if ( chev.IsValid() )
		{
			if (!nosound) Stargate.PlaySound( chev, GetSound( "chevron" + (chevLock ? "_lock" : "") + (Inbound ? "_inbound" : "") + ( longer ? "_longer" : "" ) + (shorter ? "_shorter" : "") ), delay );
			if (turnon) chev.TurnOn( delay );
		}
	}

	public void ChevronDeactivate( Chevron chev, float delay = 0 )
	{
		if ( chev.IsValid() )
		{
			chev.TurnOff( delay );
		}
	}

	public void ChevronActivateDHD( Chevron chev, float delay = 0, bool turnon = true )
	{
		if ( chev.IsValid() )
		{
			Stargate.PlaySound( chev, GetSound( "chevron_dhd" ), delay );
			if ( turnon ) chev.TurnOn( delay );
		}
	}

	public void ChevronLightup( Chevron chev, float delay = 0 )
	{
		if ( chev.IsValid() ) chev.TurnOn( delay );
	}

	// INDIVIDUAL DIAL TYPES

	// FAST DIAL
	public override void BeginDialFast(string address)
	{
		base.BeginDialFast( address );

		if ( !CanStargateStartDial() ) return;

		Event.Run( StargateEvent.DialBegin, this, address );

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.FAST;

			if ( !IsValidFullAddress( address ) ) { StopDialing(); return; }

			var target = FindDestinationGateByDialingAddress( this, address );
			var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

			if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
			{
				wasTargetReadyOnStart = true;
				target.BeginInboundFast( address.Length );
				OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
				OtherGate.OtherGate = this;
			}

			var startTime = Time.Now;
			var addrLen = address.Length;

			bool gateValidCheck() { return wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd(); }

			Ring.RollSymbolsDialFast( address, gateValidCheck );

			async void openOrStop()
			{
				if ( gateValidCheck() ) // if valid, open both gates
				{
					EstablishWormholeTo( target );
				}
				else
				{
					await GameTask.DelaySeconds( 0.25f ); // otherwise wait a bit, fail and stop dialing
					StopDialing();
				}
			}

			AddTask( startTime + 7, openOrStop, TimedTaskCategory.DIALING );

		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// FAST INBOUND
	public override void BeginInboundFast( int numChevs )
	{
		base.BeginInboundFast( numChevs );

		if ( !IsStargateReadyForInboundFast() ) return;

		Event.Run( StargateEvent.InboundBegin, this );

		try
		{
			if ( Dialing )
			{
				OtherGate?.StopDialing();
				DoStargateReset();
			}

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			PlaySound( this, GetSound("gate_roll_fast"), 0.35f );

			Ring.RollSymbolsInbound( 5.5f, 1f, numChevs );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// SLOW DIAL
	public async override void BeginDialSlow( string address, float initialDelay=0 )
	{
		base.BeginDialSlow( address, initialDelay );

		if ( !CanStargateStartDial() ) return;

		Event.Run( StargateEvent.DialBegin, this, address );

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.SLOW;

			if ( !IsValidFullAddress( address ) )
			{
				StopDialing();
				return;
			}

			if ( initialDelay > 0 )
				await GameTask.DelaySeconds( initialDelay );

			if ( ShouldStopDialing || !Dialing )
				return;

			var startTime = Time.Now;
			var addrLen = address.Length;

			var target = FindDestinationGateByDialingAddress( this, address );

			bool gateValidCheck() { return target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd(); }

			Ring.RollSymbolsDialSlow(address, gateValidCheck);

			var dialTime = (addrLen == 9) ? 36f : ((addrLen == 8) ? 32f : 26f);
			
			void startInboundAnim()
			{
				//var target = FindDestinationGateByDialingAddress( this, address );
				if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
				{
					target.BeginInboundFast( address.Length );
					OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
					OtherGate.OtherGate = this;
				}
			}

			AddTask( startTime + dialTime - 7f, startInboundAnim, TimedTaskCategory.DIALING );

			void openOrStop()
			{
				Ring.StopRollSound();

				if ( ShouldStopDialing || !Dialing )
				{
					StopDialing();
					return;
				}

				Busy = false;

				if ( gateValidCheck() )	EstablishWormholeTo( target ); else StopDialing();
			}

			AddTask( startTime + dialTime, openOrStop, TimedTaskCategory.DIALING );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// SLOW INBOUND
	public override void BeginInboundSlow( int numChevs )
	{
		base.BeginInboundSlow( numChevs );

		if ( !IsStargateReadyForInboundInstantSlow() ) return;

		Event.Run( StargateEvent.InboundBegin, this );

		try
		{
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			for ( var i = 1; i <= numChevs; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				ChevronLightup( chev );
			}

			PlaySound( this, GetSound( "chevron_lock_inbound" ) );
			Ring.LightupSymbols();

			ActiveChevrons = numChevs;
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	public async override void BeginDialInstant( string address )
	{
		base.BeginDialInstant( address );

		if ( !CanStargateStartDial() ) return;

		Event.Run( StargateEvent.DialBegin, this, address );

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.INSTANT;

			if ( !IsValidFullAddress( address ) )
			{
				StopDialing();
				return;
			}

			var otherGate = FindDestinationGateByDialingAddress( this, address );
			if ( !otherGate.IsValid() || otherGate == this || !otherGate.IsStargateReadyForInboundInstantSlow() )
			{
				StopDialing();
				return;
			}

			otherGate.BeginInboundSlow( address.Length );

			for ( var i = 1; i <= address.Length; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, address.Length );
				ChevronActivate( chev, nosound: true );

				Ring.SetSymbolState( Ring.GetSymbolNumFromChevron( GetChevronOrderOnGateFromChevronIndex( Chevrons.IndexOf( chev ) + 1 ) ), true );
			}

			PlaySound( this, GetSound( "chevron_lock_inbound" ) );

			await GameTask.DelaySeconds( 0.5f );

			EstablishWormholeTo( otherGate );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// DHD DIAL

	public async override void BeginOpenByDHD( string address )
	{
		base.BeginOpenByDHD( address );

		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.DHD;

			await GameTask.DelaySeconds( 0.35f );

			var otherGate = FindDestinationGateByDialingAddress( this, address );
			if ( otherGate.IsValid() && otherGate != this && otherGate.IsStargateReadyForInboundDHD() )
			{
				otherGate.BeginInboundSlow( address.Length );
			}
			else
			{
				StopDialing();
				return;
			}

			await GameTask.DelaySeconds( 0.15f );

			EstablishWormholeTo( otherGate );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	public async override void BeginInboundDHD( int numChevs )
	{
		base.BeginInboundDHD( numChevs );

		if ( !IsStargateReadyForInboundDHD() ) return;

		Event.Run( StargateEvent.InboundBegin, this );

		try
		{
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			for ( var i = 1; i <= numChevs; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				ChevronActivate( chev, 0, true, true );
				Ring.DoSymbolsInboundInstant();
			}
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// CHEVRON STUFF - DHD DIALING
	public override void DoDHDChevronEncode(char sym)
	{
		base.DoDHDChevronEncode( sym );

		var clampLen = Math.Clamp( DialingAddress.Length + 1, 7, 9 );

		var chev = GetChevronBasedOnAddressLength(DialingAddress.Length, clampLen );
		EncodedChevronsOrdered.Add( chev );

		Ring.RollSymbolDHDFast( clampLen, () => true, DialingAddress.Length, 0.6f );
	}

	public override void DoDHDChevronLock( char sym ) // only the top chevron locks, always
	{
		base.DoDHDChevronLock( sym );

		var chev = GetTopChevron();
		EncodedChevronsOrdered.Add( chev );

		bool validCheck()
		{
			var gate = FindDestinationGateByDialingAddress( this, DialingAddress );
			return (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD());
		}

		var rollTime = 0.6f;
		Ring.RollSymbolDHDFast( DialingAddress.Length, validCheck, DialingAddress.Length, rollTime );
		MakeBusy( rollTime );
	}

	// Manual encode/lock
	public async override Task<bool> DoManualChevronEncode( char sym )
	{
		if ( !await base.DoManualChevronEncode( sym ) )
			return false;

		IsManualDialInProgress = true;

		var chevNum = DialingAddress.Length + 1;

		CurDialingSymbol = sym;

		var success = await Ring.RollSymbolSlow( sym, chevNum ); // wait for ring to rotate to the target symbol
		if ( !success || ShouldStopDialing )
		{
			StopDialing();
			return false;
		}

		DialingAddress += sym;
		ActiveChevrons++;

		var chev = GetChevronBasedOnAddressLength( chevNum, DialingAddress.Length + 1 ); // addrLen+1 since we are not locking
		//var chev = GetChevronBasedOnAddressLength( 7, 8 );
		ChevronActivateDHD( chev, 0, true );

		Event.Run( StargateEvent.ChevronEncoded, this, chevNum );

		IsManualDialInProgress = false;

		TimeSinceDialAction = 0;

		return true;
	}

	public async override Task<bool> DoManualChevronLock( char sym )
	{
		if ( !await base.DoManualChevronLock( sym ) )
			return false;

		IsManualDialInProgress = true;

		var chevNum = DialingAddress.Length + 1;

		CurDialingSymbol = sym;

		var success = await Ring.RollSymbolSlow( sym, chevNum, true ); // wait for ring to rotate to the target symbol
		if ( !success || ShouldStopDialing )
		{
			StopDialing();
			return false;
		}

		DialingAddress += sym;

		var target = FindDestinationGateByDialingAddress( this, DialingAddress );

		bool gateValidCheck() { return target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow(); }

		var isValid = gateValidCheck();
		IsLocked = true;
		IsLockedInvalid = !isValid;

		var chev = GetChevronBasedOnAddressLength( chevNum, DialingAddress.Length );
		ChevronActivate( chev, 0, isValid, true );

		if (isValid)
			ActiveChevrons++;

		Event.Run( StargateEvent.ChevronLocked, this, chevNum, isValid );

		TimeSinceDialAction = 0;

		BeginManualOpen( DialingAddress );

		return true;
	}

	public async override void BeginManualOpen( string address )
	{
		try
		{
			var otherGate = FindDestinationGateByDialingAddress( this, address );
			if ( otherGate.IsValid() && otherGate != this && otherGate.IsStargateReadyForInboundInstantSlow() )
			{
				otherGate.BeginInboundSlow( address.Length );
				IsManualDialInProgress = false;

				await GameTask.DelaySeconds( 0.5f );

				EstablishWormholeTo( otherGate );
			}
			else
			{
				await GameTask.DelaySeconds( 1f );

				StopDialing();
				return;
			}
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	public static void DrawGizmos( EditorContext context )
	{
		Gizmo.Draw.Model( "models/sbox_stargate/sg_peg/sg_peg_ring.vmdl" );

		for ( var i = 0; i < 9; i++ )
		{
			Gizmo.Draw.Model( "models/sbox_stargate/sg_peg/sg_peg_chevron.vmdl", new Transform( Vector3.Zero, Rotation.FromRoll( i * 40 ) ) );
		}
	}
}
