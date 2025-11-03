using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaGameModes;
using UnityEngine;

public class GameModeSpecificObject : MonoBehaviour
{
	public static event GameModeSpecificObject.GameModeSpecificObjectDelegate OnAwake;

	public static event GameModeSpecificObject.GameModeSpecificObjectDelegate OnDestroyed;

	public GameModeSpecificObject.ValidationMethod Validation
	{
		get
		{
			return this.validationMethod;
		}
	}

	public List<GameModeType> GameModes
	{
		get
		{
			return this.gameModes;
		}
	}

	private async void Awake()
	{
		this.gameModes = new List<GameModeType>(this._gameModes);
		await Task.Yield();
		if (GameModeSpecificObject.OnAwake != null)
		{
			GameModeSpecificObject.OnAwake(this);
		}
	}

	private void OnDestroy()
	{
		if (GameModeSpecificObject.OnDestroyed != null)
		{
			GameModeSpecificObject.OnDestroyed(this);
		}
	}

	public bool CheckValid(GameModeType gameMode)
	{
		if (this.validationMethod == GameModeSpecificObject.ValidationMethod.Exclusion)
		{
			return !this.gameModes.Contains(gameMode);
		}
		return this.gameModes.Contains(gameMode);
	}

	[SerializeField]
	private GameModeSpecificObject.ValidationMethod validationMethod;

	[SerializeField]
	private GameModeType[] _gameModes;

	private List<GameModeType> gameModes;

	public delegate void GameModeSpecificObjectDelegate(GameModeSpecificObject gameModeSpecificObject);

	[Serializable]
	public enum ValidationMethod
	{
		Inclusion,
		Exclusion
	}
}
