using System;
using UnityEngine;

public class PlayerPrefFlags
{
	internal static bool Check(PlayerPrefFlags.Flag flag)
	{
		return (PlayerPrefs.GetInt("PlayerPrefFlags0", 1) & (int)flag) == (int)flag;
	}

	internal static void Touch(PlayerPrefFlags.Flag flag)
	{
		bool flag2 = (PlayerPrefs.GetInt("PlayerPrefFlags0", 1) & (int)flag) == (int)flag;
		if (PlayerPrefFlags.OnFlagChange != null)
		{
			PlayerPrefFlags.OnFlagChange(flag, flag2);
		}
	}

	internal static void TouchIf(PlayerPrefFlags.Flag flag, bool value)
	{
		int @int = PlayerPrefs.GetInt("PlayerPrefFlags0", 1);
		if (value == ((@int & (int)flag) == (int)flag) && PlayerPrefFlags.OnFlagChange != null)
		{
			PlayerPrefFlags.OnFlagChange(flag, value);
		}
	}

	internal static void Set(PlayerPrefFlags.Flag flag, bool value)
	{
		int num = PlayerPrefs.GetInt("PlayerPrefFlags0", 1);
		if (value)
		{
			num |= (int)flag;
		}
		else
		{
			num &= (int)(~(int)flag);
		}
		PlayerPrefs.SetInt("PlayerPrefFlags0", num);
		if (PlayerPrefFlags.OnFlagChange != null)
		{
			PlayerPrefFlags.OnFlagChange(flag, value);
		}
	}

	internal static bool Flip(PlayerPrefFlags.Flag flag)
	{
		int num = PlayerPrefs.GetInt("PlayerPrefFlags0", 1);
		bool flag2 = (num & (int)flag) != (int)flag;
		if (flag2)
		{
			num |= (int)flag;
		}
		else
		{
			num &= (int)(~(int)flag);
		}
		PlayerPrefs.SetInt("PlayerPrefFlags0", num);
		if (PlayerPrefFlags.OnFlagChange != null)
		{
			PlayerPrefFlags.OnFlagChange(flag, flag2);
		}
		return flag2;
	}

	public static Action<PlayerPrefFlags.Flag, bool> OnFlagChange;

	private const int defaultValue = 1;

	public enum Flag
	{
		SHOW_1P_COSMETICS = 1,
		SWAP_HELD_COSMETICS
	}
}
