using System;

namespace GorillaTag
{
	public interface ObjectPoolEvents
	{
		void OnTaken();

		void OnReturned();
	}
}
