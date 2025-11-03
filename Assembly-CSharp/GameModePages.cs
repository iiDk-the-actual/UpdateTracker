using System;
using System.Collections.Generic;
using System.Text;
using GorillaGameModes;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.UI;

public class GameModePages : BasePageHandler
{
	protected override int pageSize
	{
		get
		{
			return this.buttons.Length;
		}
	}

	protected override int entriesCount
	{
		get
		{
			return GameMode.gameModeNames.Count;
		}
	}

	private void Awake()
	{
		GameModePages.gameModeSelectorInstances.Add(this);
		this.buttons = base.GetComponentsInChildren<GameModeSelectButton>();
		for (int i = 0; i < this.buttons.Length; i++)
		{
			this.buttons[i].buttonIndex = i;
			this.buttons[i].selector = this;
		}
	}

	protected override void Start()
	{
		base.Start();
		base.SelectEntryFromIndex(GameModePages.sharedSelectedIndex);
		this.initialized = true;
	}

	private void OnEnable()
	{
		if (this.initialized)
		{
			base.SelectEntryFromIndex(GameModePages.sharedSelectedIndex);
		}
	}

	private void OnDestroy()
	{
		GameModePages.gameModeSelectorInstances.Remove(this);
	}

	protected override void ShowPage(int selectedPage, int startIndex, int endIndex)
	{
		GameModePages.textBuilder.Clear();
		for (int i = startIndex; i < endIndex; i++)
		{
			GameModePages.textBuilder.AppendLine(GameMode.gameModeNames[i]);
		}
		this.gameModeText.text = GameModePages.textBuilder.ToString();
		if (base.selectedIndex >= startIndex && base.selectedIndex <= endIndex)
		{
			this.UpdateAllButtons(this.currentButtonIndex);
		}
		else
		{
			this.UpdateAllButtons(-1);
		}
		int num = ((selectedPage == base.pages - 1 && base.maxEntires > endIndex) ? (base.maxEntires - endIndex) : 0);
		this.EnableEntryButtons(num);
	}

	protected override void PageEntrySelected(int pageEntry, int selectionIndex)
	{
		if (selectionIndex >= this.entriesCount)
		{
			return;
		}
		GameModePages.sharedSelectedIndex = selectionIndex;
		this.UpdateAllButtons(pageEntry);
		this.currentButtonIndex = pageEntry;
		GorillaComputer.instance.OnModeSelectButtonPress(GameMode.gameModeNames[selectionIndex], false);
	}

	private void UpdateAllButtons(int onButton)
	{
		for (int i = 0; i < this.buttons.Length; i++)
		{
			if (i == onButton)
			{
				this.buttons[onButton].isOn = true;
				this.buttons[onButton].UpdateColor();
			}
			else if (this.buttons[i].isOn)
			{
				this.buttons[i].isOn = false;
				this.buttons[i].UpdateColor();
			}
		}
	}

	private void EnableEntryButtons(int buttonsMissing)
	{
		int num = this.buttons.Length - buttonsMissing;
		int i;
		for (i = 0; i < num; i++)
		{
			this.buttons[i].gameObject.SetActive(true);
		}
		while (i < this.buttons.Length)
		{
			this.buttons[i].gameObject.SetActive(false);
			i++;
		}
	}

	public static void SetSelectedGameModeShared(string gameMode)
	{
		GameModePages.sharedSelectedIndex = GameMode.gameModeNames.IndexOf(gameMode);
		if (GameModePages.sharedSelectedIndex < 0)
		{
			return;
		}
		for (int i = 0; i < GameModePages.gameModeSelectorInstances.Count; i++)
		{
			GameModePages.gameModeSelectorInstances[i].SelectEntryFromIndex(GameModePages.sharedSelectedIndex);
		}
	}

	private int currentButtonIndex;

	[SerializeField]
	private Text gameModeText;

	[SerializeField]
	private GameModeSelectButton[] buttons;

	private bool initialized;

	private static int sharedSelectedIndex = 0;

	private static StringBuilder textBuilder = new StringBuilder(50);

	[OnEnterPlay_Clear]
	private static List<GameModePages> gameModeSelectorInstances = new List<GameModePages>(7);
}
