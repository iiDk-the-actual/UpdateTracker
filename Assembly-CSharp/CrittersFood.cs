using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class CrittersFood : CrittersActor
{
	public override void Initialize()
	{
		base.Initialize();
		this.currentFood = this.maxFood;
	}

	public void SpawnData(float _maxFood, float _currentFood, float _startingSize)
	{
		this.maxFood = _maxFood;
		this.currentFood = _currentFood;
		this.startingSize = _startingSize;
		this.currentSize = this.currentFood / this.maxFood * this.startingSize;
		this.food.localScale = new Vector3(this.currentSize, this.currentSize, this.currentSize);
	}

	public override bool ProcessLocal()
	{
		bool flag = base.ProcessLocal();
		if (!this.isEnabled)
		{
			return flag;
		}
		this.wasEnabled = base.gameObject.activeSelf;
		this.ProcessFood();
		bool flag2 = Mathf.FloorToInt(this.currentFood) != this.lastFood;
		this.lastFood = Mathf.FloorToInt(this.currentFood);
		if (this.currentFood == 0f && this.disableWhenEmpty)
		{
			this.isEnabled = false;
		}
		if (base.gameObject.activeSelf != this.isEnabled)
		{
			base.gameObject.SetActive(this.isEnabled);
		}
		this.updatedSinceLastFrame = flag || flag2 || this.wasEnabled != this.isEnabled;
		return this.updatedSinceLastFrame;
	}

	public override void ProcessRemote()
	{
		base.ProcessRemote();
		if (!this.isEnabled)
		{
			return;
		}
		this.ProcessFood();
	}

	public void ProcessFood()
	{
		if (this.currentSize != this.currentFood / this.maxFood * this.startingSize)
		{
			this.currentSize = this.currentFood / this.maxFood * this.startingSize;
			this.food.localScale = new Vector3(this.currentSize, this.currentSize, this.currentSize);
			if (this.storeCollider != null)
			{
				this.storeCollider.radius = this.currentSize / 2f;
			}
		}
	}

	public void Feed(float amountEaten)
	{
		this.currentFood = Mathf.Max(0f, this.currentFood - amountEaten);
	}

	public override bool UpdateSpecificActor(PhotonStream stream)
	{
		int num;
		float num2;
		float num3;
		if (!(base.UpdateSpecificActor(stream) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num) & CrittersManager.ValidateDataType<float>(stream.ReceiveNext(), out num2) & CrittersManager.ValidateDataType<float>(stream.ReceiveNext(), out num3)))
		{
			return false;
		}
		this.currentFood = (float)num;
		this.maxFood = num2.GetFinite();
		this.startingSize = num3.GetFinite();
		return true;
	}

	public override void SendDataByCrittersActorType(PhotonStream stream)
	{
		base.SendDataByCrittersActorType(stream);
		stream.SendNext(Mathf.FloorToInt(this.currentFood));
		stream.SendNext(this.maxFood);
		stream.SendNext(this.startingSize);
	}

	public override int AddActorDataToList(ref List<object> objList)
	{
		base.AddActorDataToList(ref objList);
		objList.Add(Mathf.FloorToInt(this.currentFood));
		objList.Add(this.maxFood);
		objList.Add(this.startingSize);
		return this.TotalActorDataLength();
	}

	public override int TotalActorDataLength()
	{
		return base.BaseActorDataLength() + 3;
	}

	public override int UpdateFromRPC(object[] data, int startingIndex)
	{
		startingIndex += base.UpdateFromRPC(data, startingIndex);
		int num;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex], out num))
		{
			return this.TotalActorDataLength();
		}
		float num2;
		if (!CrittersManager.ValidateDataType<float>(data[startingIndex + 1], out num2))
		{
			return this.TotalActorDataLength();
		}
		float num3;
		if (!CrittersManager.ValidateDataType<float>(data[startingIndex + 2], out num3))
		{
			return this.TotalActorDataLength();
		}
		this.currentFood = (float)num;
		this.maxFood = num2.GetFinite();
		this.startingSize = num3.GetFinite();
		return this.TotalActorDataLength();
	}

	public float maxFood;

	public float currentFood;

	private int lastFood;

	public float startingSize;

	public float currentSize;

	public Transform food;

	public bool disableWhenEmpty = true;
}
