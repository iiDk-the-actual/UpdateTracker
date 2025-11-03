using System;

namespace GorillaTag.Cosmetics
{
	public interface IFingerFlexListener
	{
		bool FingerFlexValidation(bool isLeftHand)
		{
			return true;
		}

		void OnButtonPressed(bool isLeftHand, float value);

		void OnButtonReleased(bool isLeftHand, float value);

		void OnButtonPressStayed(bool isLeftHand, float value);

		public enum ComponentActivator
		{
			FingerReleased,
			FingerFlexed,
			FingerStayed
		}
	}
}
