using System;

namespace GorillaNetworking
{
	public class GorillaATMKeyButton : GorillaKeyButton<GorillaATMKeyBindings>
	{
		protected override void OnButtonPressedEvent()
		{
			GameEvents.OnGorrillaATMKeyButtonPressedEvent.Invoke(this.Binding);
		}
	}
}
