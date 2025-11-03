using System;
using GorillaTagScripts.UI;

namespace GorillaTagScripts.VirtualStumpCustomMaps.UI
{
	public class CustomMapsKeyboard : GorillaKeyWrapper<CustomMapKeyboardBinding>
	{
		public static string BindingToString(CustomMapKeyboardBinding binding)
		{
			return CustomMapsKeyButton.BindingToString(binding);
		}
	}
}
