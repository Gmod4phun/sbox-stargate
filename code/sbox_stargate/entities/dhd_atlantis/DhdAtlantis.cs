using System.Collections.Generic;
using Sandbox;
using Editor;

[HammerEntity, SupportsSolid, EditorModel( Model )]
[Title( "DHD (Atlantis)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
public partial class DhdAtlantis : Dhd
{
	public const string Model = "models/sbox_stargate/dhd_atlantis/dhd_atlantis.vmdl";

	public DhdAtlantis()
	{
		Data = new(0, 1, "dhd.atlantis.press", "dhd.press_dial");
		DialIsLock = true;
	}

	protected override string ButtonSymbols => "ABCDEFGHIJKLMNOPQRST123456789UVW0XYZ";

	// Button positions for DhdWorldPanel
	protected override Dictionary<string, Vector3> ButtonPositions => new()
	{
		["A"] = new Vector3( -7.7f, -4.893f, 37.8f ),
		["B"] = new Vector3( -7.7f, -1.358f, 37.8f ),
		["C"] = new Vector3( -7.7f, 2.177f, 37.8f ),
		["D"] = new Vector3( -7.7f, 5.711f, 37.8f ),
		["E"] = new Vector3( -7.7f, 9.246f, 37.8f ),
		["F"] = new Vector3( -2.488f, -8.425f, 37.8f ),
		["G"] = new Vector3( -2.488f, -4.893f, 37.8f ),
		["H"] = new Vector3( -2.488f, -1.358f, 37.8f ),
		["I"] = new Vector3( -2.488f, 2.177f, 37.8f ),
		["J"] = new Vector3( -2.488f, 5.711f, 37.8f ),
		["K"] = new Vector3( -2.488f, 9.246f, 37.8f ),
		["L"] = new Vector3( -2.488f, 12.784f, 37.8f ),
		["M"] = new Vector3( 3.467f, -11.959f, 37.8f ),
		["N"] = new Vector3( 3.467f, -8.425f, 37.8f ),
		["O"] = new Vector3( 3.467f, -4.893f, 37.8f ),
		["P"] = new Vector3( 3.467f, -1.358f, 37.8f ),
		["DIAL"] = new Vector3( 3.467f, 2.177f, 37.8f ), // # + Dial button
		["Q"] = new Vector3( 3.467f, 5.711f, 37.8f ),
		["R"] = new Vector3( 3.467f, 9.246f, 37.8f ),
		["S"] = new Vector3( 3.467f, 12.784f, 37.8f ),
		["T"] = new Vector3( 3.467f, 16.314f, 37.8f ),
		["1"] = new Vector3( 8.403f, -11.959f, 37.8f ),
		["2"] = new Vector3( 8.403f, -8.425f, 37.8f ),
		["3"] = new Vector3( 8.403f, -4.893f, 37.8f ),
		["4"] = new Vector3( 8.403f, -1.358f, 37.8f ),
		["5"] = new Vector3( 8.403f, 2.177f, 37.8f ),
		["6"] = new Vector3( 8.403f, 5.711f, 37.8f ),
		["7"] = new Vector3( 8.403f, 9.246f, 37.8f ),
		["8"] = new Vector3( 8.403f, 12.784f, 37.8f ),
		["9"] = new Vector3( 8.403f, 16.314f, 37.8f ),
		["U"] = new Vector3( 14.292f, -8.425f, 37.8f ),
		["V"] = new Vector3( 14.292f, -4.893f, 37.8f ),
		["W"] = new Vector3( 14.292f, -1.358f, 37.8f ),
		["0"] = new Vector3( 14.292f, 2.177f, 37.8f ),
		["X"] = new Vector3( 14.292f, 5.711f, 37.8f ),
		["Y"] = new Vector3( 14.292f, 9.246f, 37.8f ),
		["Z"] = new Vector3( 14.292f, 12.784f, 37.8f ),
		["*"] = new Vector3( -9.13f, 20.55f, 37.84f ),
		["@"] = new Vector3( -4.07f, 20.52f, 37.85f ),
		["IRIS"] = new Vector3( 9.45f, -22.05f, 35.56f ),
	};

	protected override Vector3 ButtonPositionsOffset => new Vector3( 0, 0, -0.4f );

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( Model );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtons();
	}

	public override void CreateButtons() // visible models of buttons that turn on/off and animate
	{
		// SYMBOL BUTTONS

		for ( var i = 0; i < ButtonSymbols.Length; i++ )
		{
			var modelName = $"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_{i + 1}.vmdl";
			var actionName = ButtonSymbols[i].ToString();
			CreateSingleButton( modelName, actionName );
		}

		// CENTER DIAL BUTTON
		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_37.vmdl", "DIAL" );

		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_1.vmdl", "@" );
		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_2.vmdl", "*" );
		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_3.vmdl", "IRIS" );
		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_4.vmdl", "_", true );
		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_5.vmdl", ".", true );
		CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_6.vmdl", ",", true );
	}
}
