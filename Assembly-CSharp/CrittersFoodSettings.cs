using System;
using UnityEngine;

public class CrittersFoodSettings : CrittersActorSettings
{
	public override void UpdateActorSettings()
	{
		base.UpdateActorSettings();
		CrittersFood crittersFood = (CrittersFood)this.parentActor;
		crittersFood.maxFood = this._maxFood;
		crittersFood.currentFood = this._currentFood;
		crittersFood.startingSize = this._startingSize;
		crittersFood.currentSize = this._currentSize;
		crittersFood.food = this._food;
		crittersFood.disableWhenEmpty = this._disableWhenEmpty;
		crittersFood.SpawnData(this._maxFood, this._currentFood, this._startingSize);
	}

	public float _maxFood;

	public float _currentFood;

	public float _startingSize;

	public float _currentSize;

	public Transform _food;

	public bool _disableWhenEmpty;
}
