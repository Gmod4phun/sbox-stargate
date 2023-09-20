using System.Collections.Generic;
using System.Linq;
using Sandbox;

[Title( "SGC Monitor" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class SGCMonitor : ModelEntity, IUse
{
	private SGCMonitorHUDPanel _hudPanel;
	private SGCMonitorWorldPanel _worldPanel;

	[Net]
	public SGCComputer Computer { get; private set; } = null;

	public List<SGCProgram> Programs { get; } = new();
	public SGCProgram CurrentProgram { get; private set; } = null;

	[Net]
	public Entity CurrentUser { get; private set; } = null;

	[Net]
	public string DialProgramCurrentAddress { get; private set; } = "";

	[ConCmd.Server]
	public static void KickCurrentUser( int monitorIdent )
	{
		var monitor = FindByIndex( monitorIdent ) as SGCMonitor;
		monitor.OnUse( monitor.CurrentUser );
	}

	public static bool IsPointBehindPlane( Vector3 point, Vector3 planeOrigin, Vector3 planeNormal )
	{
		return (point - planeOrigin).Dot( planeNormal ) < 0;
	}

	// networking for program shit
	[ConCmd.Server]
	public static void ProgramDialUpdateAddressOnServer( int monitorIdent, string address )
	{
		var monitor = FindByIndex( monitorIdent ) as SGCMonitor;
		if ( !monitor.IsValid() )
			return;

		monitor.DialProgramCurrentAddress = address;
		monitor.ProgramDialUpdateAddressOnClient( To.Everyone, monitor, address );
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/tech/nec_multisync_lcd2080ux/nec_multisync_lcd2080ux.vmdl" );
		Scale = 0.6f;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		RenderColor = Color.Black;

		Tags.Add( "solid" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		CreatePrograms();
		UpdatePrograms( Computer, this );

		CurrentProgram = Programs.First();
		_worldPanel = new(this, CurrentProgram);
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is SGCComputer computer && Computer != computer )
		{
			Computer = computer;
			Computer.AddMonitor( this );

			UpdatePrograms( To.Everyone, Computer, this );
		}
	}

	[ClientRpc]
	public void ViewPanelOnHud()
	{
		CurrentProgram.Parent = null;
		_hudPanel = new(this, CurrentProgram);
		Game.RootPanel.AddChild( _hudPanel );
	}

	[ClientRpc]
	public void ViewPanelOnWorld()
	{
		CurrentProgram.Parent = null;
		_hudPanel?.Delete( true );
		_worldPanel.AddProgram( CurrentProgram );
	}

	[ClientRpc]
	public void DeleteBothPanels()
	{
		CurrentProgram?.Delete( true );
		_hudPanel?.Delete( true );
		_worldPanel?.Delete( true );
	}

	public bool OnUse( Entity user )
	{
		if ( !user.IsValid() )
			return false;

		if ( CurrentUser.IsValid() )
		{
			if ( CurrentUser == user )
			{
				ViewPanelOnWorld( To.Single( CurrentUser ) );
				CurrentUser = null;
			}
		}
		else
		{
			CurrentUser = user;
			ViewPanelOnHud( To.Single( CurrentUser ) );
		}

		return false;
	}

	public bool IsWorldPanelInScreen()
	{
		var bounds = _worldPanel.PanelBounds;
		var scaleFactor = (_worldPanel.ActualSize / _worldPanel.RenderSize) / 40.0f;
		var heightScaleModifier = 0.65f;

		var p1 = _worldPanel.Position + _worldPanel.Rotation.Up * (bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (bounds.Width * scaleFactor);
		var p2 = _worldPanel.Position + _worldPanel.Rotation.Up * (bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (-bounds.Width * scaleFactor);
		var p3 = _worldPanel.Position + _worldPanel.Rotation.Up * (-bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (bounds.Width * scaleFactor);
		var p4 = _worldPanel.Position + _worldPanel.Rotation.Up * (-bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (-bounds.Width * scaleFactor);

		var p1s = p1.ToScreen();
		var p2s = p2.ToScreen();
		var p3s = p3.ToScreen();
		var p4s = p4.ToScreen();

		// check if all points are not in screen, on the same side (so that if we stare at the center and points will be offscreen it won't hide our panel)
		if ( (p1s.x < 0 && p2s.x < 0 && p3s.x < 0 && p4s.x < 0) ||
		    (p1s.x > 1 && p2s.x > 1 && p3s.x > 1 && p4s.x > 1) ||
		    (p1s.y < 0 && p2s.y < 0 && p3s.y < 0 && p4s.y < 0) ||
		    (p1s.y > 1 && p2s.y > 1 && p3s.y > 1 && p4s.y > 1) )
		{
			return false;
		}

		return true;
	}

	public bool IsWorldPanelBehindScreen()
	{
		var bounds = _worldPanel.PanelBounds;
		var scaleFactor = (_worldPanel.ActualSize / _worldPanel.RenderSize) / 40.0f;
		var heightScaleModifier = 0.65f;

		var p1 = _worldPanel.Position + _worldPanel.Rotation.Up * (bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (bounds.Width * scaleFactor);
		var p2 = _worldPanel.Position + _worldPanel.Rotation.Up * (bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (-bounds.Width * scaleFactor);
		var p3 = _worldPanel.Position + _worldPanel.Rotation.Up * (-bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (bounds.Width * scaleFactor);
		var p4 = _worldPanel.Position + _worldPanel.Rotation.Up * (-bounds.Height * heightScaleModifier * scaleFactor) + _worldPanel.Rotation.Right * (-bounds.Width * scaleFactor);

		var p1s = p1.ToScreen();
		var p2s = p2.ToScreen();
		var p3s = p3.ToScreen();
		var p4s = p4.ToScreen();

		if ( p1s.z < 0 && p2s.z < 0 && p3s.z < 0 && p4s.z < 0 )
			return true;

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	[ClientRpc]
	public void ProgramDialUpdateAddressOnClient( SGCMonitor monitor, string address )
	{
		var program = Programs.OfType<SGCProgram_Dialing>().FirstOrDefault();
		if ( !program.IsValid() )
			return;

		program.UpdateProgram( this, Computer );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Computer.IsValid() )
			Computer.RemoveMonitor( this );

		DeleteBothPanels( To.Everyone );
	}

	private void CreatePrograms()
	{
		Programs.Add( new SGCProgram_Dialing() );
		Programs.Add( new SGCProgram_Screensaver() );
	}

	[ClientRpc]
	private void UpdatePrograms( SGCComputer computer, SGCMonitor monitor )
	{
		foreach ( var program in Programs )
		{
			program.UpdateProgram( monitor, computer );
		}
	}

	[ClientRpc]
	private void DisableWorldPanel()
	{
		CurrentProgram.Parent = null;
	}

	[GameEvent.Tick.Server]
	private void CurrentUserThink()
	{
		if ( (!CurrentUser.IsValid() || CurrentUser.Health <= 0 || CurrentUser.Position.DistanceSquared( Position ) > 120 * 120) && CurrentUser != null )
		{
			CurrentUser = null;
		}
	}

	[Event.Hotload]
	private void Hotloaded()
	{
		CurrentUser = null;
	}

	[GameEvent.Client.Frame]
	private void RenderLogic()
	{
		if ( !_worldPanel.IsValid() )
			return;

		// if we are viewing it on HUD, we don't care about hiding world panel, and also remove HUDPanel if we arent active user
		if ( _hudPanel.IsValid() )
		{
			if ( CurrentUser != Game.LocalPawn )
				ViewPanelOnWorld();

			return;
		}

		var screenPos = _worldPanel.Position;
		var screenDir = Rotation.Forward;

		bool isWorldPanelBehindScreen = IsWorldPanelBehindScreen();
		bool isPlayerBehindMonitor = IsPointBehindPlane( Camera.Position, screenPos, screenDir );
		bool isPlayerFarAway = Camera.Position.DistanceSquared( screenPos ) > (512 * 512);
		bool isWorldPanelOffScreen = !IsWorldPanelInScreen();

		if ( (isPlayerBehindMonitor || isPlayerFarAway || isWorldPanelOffScreen || isWorldPanelBehindScreen) && CurrentUser != Game.LocalClient && CurrentProgram.Parent.IsValid() )
		{
			CurrentProgram.Parent = null;
		}

		if ( (!isPlayerBehindMonitor && !isPlayerFarAway && !isWorldPanelOffScreen && !isWorldPanelBehindScreen) && CurrentUser != Game.LocalClient && !CurrentProgram.Parent.IsValid() )
		{
			_worldPanel.AddProgram( CurrentProgram );
		}
	}
}
