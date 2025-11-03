using System;

namespace GorillaTagScripts.Builder
{
	public class SharedBlocksKeyboardButton : GorillaKeyButton<SharedBlocksKeyboardBindings>
	{
		protected override void OnButtonPressedEvent()
		{
			GameEvents.OnSharedBlocksKeyboardButtonPressedEvent.Invoke(this.Binding);
		}
	}
}
