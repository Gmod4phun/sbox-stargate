using Sandbox;
using Sandbox.UI;

public class SGCProgram : Panel
{
	public SGCMonitor Monitor { get; set; }
	public SGCComputer Computer { get; set; }
	protected Stargate Gate { get; set; }

	public virtual void UpdateProgram( SGCMonitor monitor, SGCComputer computer )
	{
		Monitor = monitor;
		Computer = computer;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Computer.IsValid() )
			return;

		if ( Gate != Computer.Gate )
			Gate = Computer.Gate;
	}

	[Event.Hotload]
	private void UpdateProgram()
	{
		UpdateProgram( Monitor, Computer );
	}
}
