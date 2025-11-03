using System;
using System.Globalization;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GRDistillery : MonoBehaviour
{
	public void Init(GhostReactor reactor)
	{
		this.reactor = reactor;
		this.sentientCoreDeposit.Init(reactor);
		this.cores = PlayerPrefs.GetInt("_grDistilleryCore", -1);
		if (this.cores == -1)
		{
			this.cores = 0;
		}
		this.RestoreStartTime();
		this.InitializeGauges();
	}

	private void SaveStartTime(DateTime time)
	{
		string text = time.ToString("O");
		PlayerPrefs.SetString("_grDistilleryStartTime", text);
		PlayerPrefs.Save();
	}

	private void RestoreStartTime()
	{
		string @string = PlayerPrefs.GetString("_grDistilleryStartTime", string.Empty);
		if (@string != string.Empty)
		{
			this.startTime = DateTime.ParseExact(@string, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}
	}

	public void StartResearch()
	{
		if (this.cores > 0)
		{
			this.startTime = GorillaComputer.instance.GetServerTime();
			this.SaveStartTime(this.startTime);
			this.bProcessing = true;
			this.InitializeGauges();
		}
	}

	public double CalculateRemaining()
	{
		return (double)this.secondsToResearchACore - (GorillaComputer.instance.GetServerTime() - this.startTime).TotalSeconds;
	}

	private void FirstUpdate()
	{
		double num = this.CalculateRemaining();
		while (this.cores > 0 && num < (double)(-(double)this.secondsToResearchACore))
		{
			if (num < (double)(-(double)this.secondsToResearchACore))
			{
				this.CompleteResearchingCore();
				num += (double)this.secondsToResearchACore;
			}
		}
		if (this.cores > 0 && num < 0.0)
		{
			this.startTime = GorillaComputer.instance.GetServerTime().AddSeconds(num);
			num = this.CalculateRemaining();
			this.SaveStartTime(this.startTime);
		}
		if (this.cores > 0)
		{
			this.bProcessing = true;
			this.currentGaugeCore = this.cores - 1;
		}
		else
		{
			this.currentGaugeCore = 0;
		}
		if (this.cores >= 4)
		{
			this.depositDoor.transform.position = this.depositClosePosition.position;
		}
		else
		{
			this.depositDoor.transform.position = this.depositOpenPosition.position;
		}
		this.UpdateGauges();
	}

	public void Update()
	{
		if (!this.firstUpdate)
		{
			this.FirstUpdate();
			this.firstUpdate = true;
		}
		this.UpdateDoorPosition();
		this.UpdateGauges();
		if (!this.bProcessing)
		{
			return;
		}
		this.remaingTime = this.CalculateRemaining();
		if (this.remaingTime <= 0.0)
		{
			this.CompleteResearchingCore();
		}
	}

	private void UpdateDoorPosition()
	{
		if (this.cores >= 4)
		{
			this.depositDoor.transform.position = Vector3.MoveTowards(this.depositDoor.transform.position, this.depositClosePosition.transform.position, this.depositDoorCloseSpeed * Time.deltaTime);
			return;
		}
		this.depositDoor.transform.position = Vector3.MoveTowards(this.depositDoor.transform.position, this.depositOpenPosition.transform.position, this.depositDoorCloseSpeed * Time.deltaTime);
	}

	private void CompleteResearchingCore()
	{
		this.cores = Math.Max(this.cores - 1, 0);
		this.currentGaugeCore = Math.Max(this.cores - 1, 0);
		PlayerPrefs.SetInt("_grDistilleryCore", this.cores);
		PlayerPrefs.Save();
		if (this.cores > 0)
		{
			this.startTime = GorillaComputer.instance.GetServerTime().AddSeconds(this.remaingTime);
			this.SaveStartTime(this.startTime);
			this.remaingTime = this.CalculateRemaining();
		}
		if (this.cores == 0)
		{
			this.bProcessing = false;
		}
		this.UpdateGauges();
	}

	public void DepositCore()
	{
		if (this.cores < this.maxCores)
		{
			this.cores++;
			if (!this.bFillingGauge)
			{
				this.bFillingGauge = true;
				this.fillTime = 0f;
			}
			PlayerPrefs.SetInt("_grDistilleryCore", this.cores);
			PlayerPrefs.Save();
			if (this.cores == 1)
			{
				this.StartResearch();
			}
		}
	}

	public void DebugFinishDistill()
	{
	}

	private void OnEnable()
	{
		if (this._applyMaterialgauge1)
		{
			this._applyMaterialgauge1.mode = ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock;
		}
		if (this._applyMaterialgauge2)
		{
			this._applyMaterialgauge2.mode = ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock;
		}
		if (this._applyMaterialgauge3)
		{
			this._applyMaterialgauge3.mode = ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock;
		}
		if (this._applyMaterialgauge4)
		{
			this._applyMaterialgauge4.mode = ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock;
		}
		this.InitializeGauges();
	}

	private void InitializeGauges()
	{
		for (int i = 0; i < this.gaugesFill.Length - 1; i++)
		{
			this.gaugesFill[i] = ((this.cores >= i + 1) ? this.gaugeFullFillAmount : this.gaugeEmptyFillAmount);
		}
		this.researchGaugeFill = this.gaugesFill[0];
		this.currentGaugeFillAmount = this.gaugeEmptyFillAmount;
	}

	private void UpdateGauges()
	{
		for (int i = 0; i < this.gaugesFill.Length; i++)
		{
			if (i + 1 > this.cores)
			{
				this.gaugesFill[i] = this.gaugeEmptyFillAmount;
			}
		}
		if (this.bFillingGauge)
		{
			this.fillTime += Time.deltaTime;
			float num = this.fillTime / this.gaugeDrainTime;
			if (this.currentGaugeCore == this.cores - 1)
			{
				if (num > 1f)
				{
					this.bFillingGauge = false;
				}
				else
				{
					this.gaugesFill[this.currentGaugeCore] = Mathf.Lerp(this.currentGaugeFillAmount, Mathf.Lerp(this.gaugeEmptyFillAmount, this.gaugeFullFillAmount, (float)this.remaingTime / (float)this.secondsToResearchACore), num);
				}
			}
			else
			{
				this.gaugesFill[this.currentGaugeCore] = Mathf.Lerp(this.currentGaugeFillAmount, this.gaugeFullFillAmount, num);
			}
			if (this.bFillingGauge && num > 1f)
			{
				this.currentGaugeCore++;
				this.currentGaugeFillAmount = this.gaugeEmptyFillAmount;
				this.fillTime = 0f;
			}
		}
		else if (this.bProcessing)
		{
			this.gaugesFill[this.currentGaugeCore] = Mathf.Lerp(this.gaugeEmptyFillAmount, this.gaugeFullFillAmount, (float)this.remaingTime / (float)this.secondsToResearchACore);
			this.currentGaugeFillAmount = this.gaugesFill[this.currentGaugeCore];
		}
		this._applyMaterialgauge1.SetFloat("_LiquidFill", this.gaugesFill[0]);
		this._applyMaterialgauge1.Apply();
		this._applyMaterialgauge2.SetFloat("_LiquidFill", this.gaugesFill[1]);
		this._applyMaterialgauge2.Apply();
		this._applyMaterialgauge3.SetFloat("_LiquidFill", this.gaugesFill[2]);
		this._applyMaterialgauge3.Apply();
		this._applyMaterialgauge4.SetFloat("_LiquidFill", this.gaugesFill[3]);
		this._applyMaterialgauge4.Apply();
		this._applyMaterialCurrentResearch.SetFloat("_LiquidFill", this.researchGaugeFill);
		this._applyMaterialCurrentResearch.Apply();
	}

	[SerializeField]
	private GRCurrencyDepositor sentientCoreDeposit;

	[SerializeField]
	private ApplyMaterialProperty _applyMaterialgauge1;

	[SerializeField]
	private ApplyMaterialProperty _applyMaterialgauge2;

	[SerializeField]
	private ApplyMaterialProperty _applyMaterialgauge3;

	[SerializeField]
	private ApplyMaterialProperty _applyMaterialgauge4;

	[SerializeField]
	private ApplyMaterialProperty _applyMaterialCurrentResearch;

	[FormerlySerializedAs("emptyFillAmount")]
	public float gaugeEmptyFillAmount = 0.44f;

	[FormerlySerializedAs("fullFillAmount")]
	public float gaugeFullFillAmount = 0.56f;

	[SerializeField]
	private Transform depositClosePosition;

	[SerializeField]
	private Transform depositOpenPosition;

	[SerializeField]
	private GameObject depositDoor;

	[SerializeField]
	private float depositDoorCloseSpeed = 0.5f;

	[SerializeField]
	private TextMeshPro currentResearchPoints;

	public float researchGaugeEmptyFillAmount = 0.44f;

	public float researchGaugeFullFillAmount = 0.56f;

	public int secondsToResearchACore;

	public float gaugeDrainTime = 2f;

	public int maxCores = 4;

	public AudioSource feedbackSound;

	private DateTime startTime;

	private bool bProcessing;

	private int cores;

	private bool bFillingGauge;

	private int currentGaugeCore;

	private float currentGaugeFillAmount;

	private double remaingTime;

	private float fillTime;

	private float[] gaugesFill = new float[4];

	private float researchGaugeFill;

	private bool firstUpdate;

	[NonSerialized]
	public GhostReactor reactor;

	private const string grDistilleryCorePrefsKey = "_grDistilleryCore";

	private const string grDistilleryStartTimePrefsKey = "_grDistilleryStartTime";
}
