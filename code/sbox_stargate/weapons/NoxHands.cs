using Sandbox;

[Spawnable]
[Library( "weapon_stargate_noxhands", Title = "Nox Hands", Description = "Instant dialling of the gate, without kawoosh effect.", Group = "Stargate.Weapons" )]
public partial class StargateNoxHands : Weapon
{
	/// <summary>
	/// in the future when all settings are globalized, there is this value so that the gate opening distance can be adjusted.
	/// </summary>
	public static float MaxDistance { get; set; } = 1500;

	//later add a hand model
	//public override string ViewModelPath => "hand model";
	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 1.0f;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.PrimaryAttack );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		var gate = Stargate.FindClosestGate( Owner.Position, MaxDistance );
		gate?.OpenStargateMenu();
	}

	public override void AttackSecondary()
	{
		TimeSinceSecondaryAttack = 0;

		//var gate = Stargate.FindClosestGate( Owner.Position, MaxDistance );
		//if ( gate is not null )
		//{
		//	if ( gate.IsValid() )
		//	{
		//		var _Hud = Local.Hud.ChildrenOfType<StargateMenuV2>().ToArray();

		//		if ( _Hud[0] is not null )
		//		{
		//			_Hud[0].Delete();
		//		}
		//	}
		//}
	}
}
