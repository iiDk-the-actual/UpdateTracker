using System;
using GorillaNetworking;
using GorillaTagScripts.Builder;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents
{
	public static UnityEvent<GorillaKeyboardBindings> OnGorrillaKeyboardButtonPressedEvent = new UnityEvent<GorillaKeyboardBindings>();

	public static UnityEvent<GorillaATMKeyBindings> OnGorrillaATMKeyButtonPressedEvent = new UnityEvent<GorillaATMKeyBindings>();

	internal static UnityEvent<string> ScreenTextChangedEvent = new UnityEvent<string>();

	internal static UnityEvent<Material[]> ScreenTextMaterialsEvent = new UnityEvent<Material[]>();

	internal static UnityEvent<string> FunctionSelectTextChangedEvent = new UnityEvent<string>();

	internal static UnityEvent<Material[]> FunctionTextMaterialsEvent = new UnityEvent<Material[]>();

	internal static UnityEvent LanguageEvent = new UnityEvent();

	internal static UnityEvent<string> ScoreboardTextChangedEvent = new UnityEvent<string>();

	internal static UnityEvent<Material[]> ScoreboardMaterialsEvent = new UnityEvent<Material[]>();

	public static UnityEvent<SharedBlocksKeyboardBindings> OnSharedBlocksKeyboardButtonPressedEvent = new UnityEvent<SharedBlocksKeyboardBindings>();
}
