using System;
using UnityEngine;

public class TestScreen : ArcadeGame
{
	public override byte[] GetNetworkState()
	{
		return null;
	}

	public override void SetNetworkState(byte[] b)
	{
	}

	private int buttonToLightIndex(int player, ArcadeButtons button)
	{
		int num = 0;
		if (button <= ArcadeButtons.RIGHT)
		{
			switch (button)
			{
			case ArcadeButtons.GRAB:
				num = 0;
				break;
			case ArcadeButtons.UP:
				num = 1;
				break;
			case ArcadeButtons.GRAB | ArcadeButtons.UP:
				break;
			case ArcadeButtons.DOWN:
				num = 2;
				break;
			default:
				if (button != ArcadeButtons.LEFT)
				{
					if (button == ArcadeButtons.RIGHT)
					{
						num = 4;
					}
				}
				else
				{
					num = 3;
				}
				break;
			}
		}
		else if (button != ArcadeButtons.B0)
		{
			if (button != ArcadeButtons.B1)
			{
				if (button == ArcadeButtons.TRIGGER)
				{
					num = 7;
				}
			}
			else
			{
				num = 6;
			}
		}
		else
		{
			num = 5;
		}
		return (player * 8 + num) % this.lights.Length;
	}

	protected override void ButtonUp(int player, ArcadeButtons button)
	{
		this.lights[this.buttonToLightIndex(player, button)].color = Color.red;
	}

	protected override void ButtonDown(int player, ArcadeButtons button)
	{
		this.lights[this.buttonToLightIndex(player, button)].color = Color.green;
	}

	public override void OnTimeout()
	{
	}

	[SerializeField]
	private SpriteRenderer[] lights;

	[SerializeField]
	private Transform dot;
}
