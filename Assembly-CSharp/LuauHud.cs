using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GorillaGameModes;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class LuauHud : MonoBehaviour
{
	public static LuauHud Instance
	{
		get
		{
			return LuauHud._instance;
		}
	}

	private void Awake()
	{
		if (LuauHud._instance != null && LuauHud._instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			LuauHud._instance = this;
		}
		this.path = Path.Combine(Application.persistentDataPath, "script.luau");
	}

	private void OnDestroy()
	{
		if (LuauHud._instance == this)
		{
			LuauHud._instance = null;
		}
	}

	private void Start()
	{
		this.useLuauHud = true;
		DebugHudStats instance = DebugHudStats.Instance;
		instance.enabled = false;
		this.debugHud = instance.gameObject;
		this.text = instance.text;
		this.text.gameObject.SetActive(false);
		this.builder = new StringBuilder(50);
	}

	private void Update()
	{
		if (!CustomMapLoader.IsDevModeEnabled())
		{
			if (this.showLog && this.useLuauHud)
			{
				this.showLog = false;
				DebugHudStats instance = DebugHudStats.Instance;
				if (instance != null)
				{
					instance.gameObject.SetActive(false);
				}
				this.text.gameObject.SetActive(false);
			}
			return;
		}
		GorillaGameManager instance2 = GorillaGameManager.instance;
		if (instance2 == null || instance2.GameType() != GameModeType.Custom)
		{
			return;
		}
		bool flag = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
		bool flag2 = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
		if (flag != this.buttonDown && this.useLuauHud)
		{
			this.buttonDown = flag;
			if (!this.buttonDown)
			{
				if (!this.text.gameObject.activeInHierarchy)
				{
					DebugHudStats instance3 = DebugHudStats.Instance;
					if (instance3 != null)
					{
						instance3.gameObject.SetActive(true);
					}
					this.text.gameObject.SetActive(true);
					this.showLog = true;
				}
				else
				{
					DebugHudStats instance4 = DebugHudStats.Instance;
					if (instance4 != null)
					{
						instance4.gameObject.SetActive(false);
					}
					this.text.gameObject.SetActive(false);
					this.showLog = false;
				}
			}
		}
		if (!flag || !flag2)
		{
			this.resetTimer = Time.time;
		}
		if (Time.time - this.resetTimer > 2f && CustomGameMode.GameModeInitialized)
		{
			this.RestartLuauScript();
			this.resetTimer = Time.time;
		}
		if (this.useLuauHud && this.showLog)
		{
			this.builder.Clear();
			this.builder.AppendLine();
			for (int i = 0; i < this.luauLogs.Count; i++)
			{
				this.builder.AppendLine(this.luauLogs[i]);
			}
			this.text.text = this.builder.ToString();
		}
	}

	public void RestartLuauScript()
	{
		this.LuauLog("Restarting Luau Script");
		LuauScriptRunner gameScriptRunner = CustomGameMode.gameScriptRunner;
		if (gameScriptRunner != null && gameScriptRunner.ShouldTick)
		{
			CustomGameMode.StopScript();
		}
		this.script = this.LoadLocalScript();
		if (this.script != "")
		{
			this.LuauLog("Loaded script from: " + this.path);
			this.LuauLog("Loaded Script Text: \n" + this.script);
			CustomGameMode.LuaScript = this.script;
		}
		CustomGameMode.LuaStart();
	}

	public string LoadLocalScript()
	{
		string text = "";
		if (File.Exists(this.path))
		{
			text = File.ReadAllText(this.path);
		}
		return text;
	}

	public void LuauLog(string log)
	{
		Debug.Log(log);
		this.luauLogs.Add(log);
		if (this.luauLogs.Count > 6)
		{
			this.luauLogs.RemoveAt(0);
		}
	}

	private bool useLuauHud;

	private bool buttonDown;

	private bool showLog;

	private GameObject debugHud;

	private TMP_Text text;

	private StringBuilder builder;

	private float resetTimer;

	private string path = "";

	private string script = "";

	private static LuauHud _instance;

	private List<string> luauLogs = new List<string>();
}
