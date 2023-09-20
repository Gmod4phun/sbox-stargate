using System.Collections.Generic;
using Sandbox;

public partial class Chevron : AnimatedEntity
{
	public bool UsesDynamicLight = true;

	public Stargate Gate;

	public PointLightEntity Light;

	public Dictionary<string, int> ChevronStateSkins = new() { { "Off", 0 }, { "On", 1 } };

	private float selfillumscale = 0;

	[Net]
	public bool On { get; private set; } = false;

	[Net]
	public bool Open { get; private set; } = false;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_sg1/chevron.vmdl" );

		CreateLight();
	}

	public void CreateLight()
	{
		var att = (Transform)GetAttachment( "light" );

		Light = new PointLightEntity();
		Light.Position = att.Position;
		Light.Rotation = att.Rotation;
		Light.SetParent( this, "light" );

		Light.SetLightColor( Color.Parse( "#FF6A00" ).GetValueOrDefault() );
		Light.Brightness = 0.6f;
		Light.Range = 12f;
		Light.Enabled = On;
	}

	// ANIMS

	public async void ChevronAnim( string name, float delay = 0 )
	{
		if ( delay > 0 )
		{
			await GameTask.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}

		CurrentSequence.Name = name;
	}

	public async void TurnOn( float delay = 0 )
	{
		if ( delay > 0 )
		{
			await GameTask.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}

		On = true;
	}

	public async void TurnOff( float delay = 0 )
	{
		if ( delay > 0 )
		{
			await GameTask.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}

		On = false;
	}

	public async void SetOpen( bool open, float delay = 0 )
	{
		if ( delay > 0 )
		{
			await GameTask.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}

		Open = open;
	}

	public void ChevronOpen( float delay = 0 )
	{
		ChevronAnim( "lock", delay );
		Stargate.PlaySound( this, Gate.GetSound( "chevron_open" ), delay );
		SetOpen( true, delay );
	}

	public void ChevronClose( float delay = 0 )
	{
		ChevronAnim( "unlock", delay );
		Stargate.PlaySound( this, Gate.GetSound( "chevron_close" ), delay );
		SetOpen( false, delay );
	}

	[GameEvent.Tick.Server]
	public void ChevronThink()
	{
		var group = ChevronStateSkins.GetValueOrDefault( On ? "On" : "Off", 0 );
		if ( GetMaterialGroup() != group ) SetMaterialGroup( group );
		if ( Light.IsValid() ) Light.Enabled = UsesDynamicLight && On;
	}

	[GameEvent.Client.Frame]
	public void LightLogic()
	{
		selfillumscale = selfillumscale.Approach( On ? 1 : 0, Time.Delta * 5 );
		SceneObject.Attributes.Set( "selfillumscale", selfillumscale );
		SceneObject.Batchable = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Light.IsValid() ) Light.Delete();
	}
}
