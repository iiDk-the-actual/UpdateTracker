using System;

namespace GorillaNetworking
{
	public class GorillaKeyboardButton : GorillaKeyButton<GorillaKeyboardBindings>
	{
		protected override void OnButtonPressedEvent()
		{
			GameEvents.OnGorrillaKeyboardButtonPressedEvent.Invoke(this.Binding);
		}
	}
}
