using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class CrittersLoudNoise : CrittersActor
{
	public override void OnEnable()
	{
		base.OnEnable();
		this.SetTimeEnabled();
	}

	public void SpawnData(float _soundVolume, float _soundDuration, float _soundMultiplier, bool _soundEnabled)
	{
		this.soundVolume = _soundVolume;
		this.volumeFearAttractionMultiplier = _soundMultiplier;
		this.soundDuration = _soundDuration;
		this.soundEnabled = _soundEnabled;
		this.Initialize();
	}

	public override bool ProcessLocal()
	{
		bool flag = base.ProcessLocal();
		if (!this.isEnabled)
		{
			return flag;
		}
		this.wasEnabled = base.gameObject.activeSelf;
		this.wasSoundEnabled = this.soundEnabled;
		if (PhotonNetwork.InRoom)
		{
			if (PhotonNetwork.Time > this.timeSoundEnabled + (double)this.soundDuration || this.timeSoundEnabled > PhotonNetwork.Time)
			{
				this.soundEnabled = false;
			}
		}
		else if ((double)Time.time > this.timeSoundEnabled + (double)this.soundDuration || this.timeSoundEnabled > (double)Time.time)
		{
			this.soundEnabled = false;
		}
		if (this.disableWhenSoundDisabled && !this.soundEnabled)
		{
			this.isEnabled = false;
			if (base.gameObject.activeSelf != this.isEnabled)
			{
				base.gameObject.SetActive(this.isEnabled);
			}
		}
		this.updatedSinceLastFrame = flag || this.wasSoundEnabled != this.soundEnabled || this.wasEnabled != this.isEnabled;
		return this.updatedSinceLastFrame;
	}

	public override void ProcessRemote()
	{
		if (!this.wasEnabled && this.isEnabled)
		{
			this.SetTimeEnabled();
		}
	}

	public void SetTimeEnabled()
	{
		if (PhotonNetwork.InRoom)
		{
			this.timeSoundEnabled = PhotonNetwork.Time;
			return;
		}
		this.timeSoundEnabled = (double)Time.time;
	}

	public override void CalculateFear(CrittersPawn critter, float multiplier)
	{
		if (this.soundEnabled)
		{
			if (this.soundDuration == 0f)
			{
				critter.IncreaseFear(this.soundVolume * this.volumeFearAttractionMultiplier * multiplier, this);
				return;
			}
			if ((PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) - this.timeSoundEnabled < (double)this.soundDuration)
			{
				critter.IncreaseFear(this.soundVolume * this.volumeFearAttractionMultiplier * Time.deltaTime * multiplier, this);
			}
		}
	}

	public override void CalculateAttraction(CrittersPawn critter, float multiplier)
	{
		if (this.soundEnabled)
		{
			if (this.soundDuration == 0f)
			{
				critter.IncreaseAttraction(this.soundVolume * this.volumeFearAttractionMultiplier * multiplier, this);
				return;
			}
			if ((PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) - this.timeSoundEnabled < (double)this.soundDuration)
			{
				critter.IncreaseAttraction(this.soundVolume * this.volumeFearAttractionMultiplier * Time.deltaTime * multiplier, this);
			}
		}
	}

	public override bool UpdateSpecificActor(PhotonStream stream)
	{
		float num;
		float num2;
		bool flag;
		float num3;
		if (!(base.UpdateSpecificActor(stream) & CrittersManager.ValidateDataType<float>(stream.ReceiveNext(), out num) & CrittersManager.ValidateDataType<float>(stream.ReceiveNext(), out num2) & CrittersManager.ValidateDataType<bool>(stream.ReceiveNext(), out flag) & CrittersManager.ValidateDataType<float>(stream.ReceiveNext(), out num3)))
		{
			return false;
		}
		this.soundVolume = num.GetFinite();
		this.soundDuration = num2.GetFinite();
		this.soundEnabled = flag;
		this.volumeFearAttractionMultiplier = num3.GetFinite();
		return true;
	}

	public override void SendDataByCrittersActorType(PhotonStream stream)
	{
		base.SendDataByCrittersActorType(stream);
		stream.SendNext(this.soundVolume);
		stream.SendNext(this.soundDuration);
		stream.SendNext(this.soundEnabled);
		stream.SendNext(this.volumeFearAttractionMultiplier);
	}

	public override int AddActorDataToList(ref List<object> objList)
	{
		base.AddActorDataToList(ref objList);
		objList.Add(this.soundVolume);
		objList.Add(this.soundDuration);
		objList.Add(this.soundEnabled);
		objList.Add(this.volumeFearAttractionMultiplier);
		return this.TotalActorDataLength();
	}

	public override int TotalActorDataLength()
	{
		return base.BaseActorDataLength() + 4;
	}

	public override int UpdateFromRPC(object[] data, int startingIndex)
	{
		startingIndex += base.UpdateFromRPC(data, startingIndex);
		float num;
		if (!CrittersManager.ValidateDataType<float>(data[startingIndex], out num))
		{
			return this.TotalActorDataLength();
		}
		float num2;
		if (!CrittersManager.ValidateDataType<float>(data[startingIndex + 1], out num2))
		{
			return this.TotalActorDataLength();
		}
		bool flag;
		if (!CrittersManager.ValidateDataType<bool>(data[startingIndex + 2], out flag))
		{
			return this.TotalActorDataLength();
		}
		float num3;
		if (!CrittersManager.ValidateDataType<float>(data[startingIndex + 3], out num3))
		{
			return this.TotalActorDataLength();
		}
		this.soundVolume = num.GetFinite();
		this.soundDuration = num2.GetFinite();
		this.soundEnabled = flag;
		this.volumeFearAttractionMultiplier = num3.GetFinite();
		return this.TotalActorDataLength();
	}

	public void PlayHandTapLocal(bool isLeft)
	{
		this.timeSoundEnabled = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		this.soundEnabled = true;
	}

	public void PlayHandTapRemote(double serverTime, bool isLeft)
	{
		this.timeSoundEnabled = serverTime;
		this.soundEnabled = true;
	}

	public void PlayVoiceSpeechLocal(double serverTime, float duration, float volume)
	{
		this.soundDuration = duration;
		this.timeSoundEnabled = serverTime;
		this.soundVolume = volume;
		this.soundEnabled = true;
	}

	public float soundVolume;

	public float volumeFearAttractionMultiplier;

	public float soundDuration;

	public double timeSoundEnabled;

	public bool soundEnabled;

	private bool wasSoundEnabled;

	public bool disableWhenSoundDisabled;
}
