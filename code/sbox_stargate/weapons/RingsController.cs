using Sandbox;

[Spawnable]
[Library( "weapon_stargate_ringscontroller", Title = "Rings Controller", Description = "", Group = "Stargate.Weapons" )]
public partial class StargateRingsController : Weapon
{
	//later add a hand model
	//public override string ViewModelPath => "hand model";
	public override float PrimaryRate => 15.0f;
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

		using ( Prediction.Off() )
		{
			Rings ring;

			var ray = Owner.AimRay;

			var tr = Trace.Ray( ray.Position, ray.Position + ray.Forward * 500f ).Ignore( Owner ).Run();

			if ( tr.Hit && tr.Entity is Rings )
				ring = tr.Entity as Rings;
			else
				ring = Rings.GetClosestRing( Owner.Position, null, 500f );

			if ( ring is null || !ring.IsValid() )
				return;

			ring.DialClosest();
		}
	}
}
