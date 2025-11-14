using System;
using UnityEngine;

public class GorillaDevButton : GorillaPressableButton
{
	public bool on
	{
		get
		{
			return this.isOn;
		}
		set
		{
			if (this.isOn != value)
			{
				this.isOn = value;
				this.UpdateColor();
			}
		}
	}

	public new void OnEnable()
	{
		this.UpdateColor();
	}

	public DevButtonType Type;

	public LogType levelType;

	public DevConsoleInstance targetConsole;

	public int lineNumber;

	public bool repeatIfHeld;

	public float holdForSeconds;

	private Coroutine pressCoroutine;
}
