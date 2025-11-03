using System;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace UniLabs.Time
{
	[JsonObject(MemberSerialization.OptIn)]
	[Serializable]
	public class UDateTime : ISerializationCallbackReceiver, IComparable<UDateTime>, IComparable<DateTime>
	{
		[JsonProperty("DateTime")]
		public DateTime DateTime { get; set; }

		[JsonConstructor]
		public UDateTime()
		{
			this.DateTime = DateTime.UnixEpoch;
		}

		public UDateTime(DateTime dateTime)
		{
			this.DateTime = dateTime;
		}

		public static implicit operator DateTime(UDateTime udt)
		{
			return udt.DateTime;
		}

		public static implicit operator UDateTime(DateTime dt)
		{
			return new UDateTime
			{
				DateTime = dt
			};
		}

		public int CompareTo(DateTime other)
		{
			return this.DateTime.CompareTo(other);
		}

		public int CompareTo(UDateTime other)
		{
			if (this == other)
			{
				return 0;
			}
			if (other == null)
			{
				return 1;
			}
			return this.DateTime.CompareTo(other.DateTime);
		}

		protected bool Equals(UDateTime other)
		{
			return this.DateTime.Equals(other.DateTime);
		}

		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((UDateTime)obj)));
		}

		public override int GetHashCode()
		{
			return this.DateTime.GetHashCode();
		}

		public override string ToString()
		{
			return this.DateTime.ToString(CultureInfo.InvariantCulture);
		}

		public void OnAfterDeserialize()
		{
			DateTime dateTime;
			this.DateTime = (DateTime.TryParse(this._DateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime) ? dateTime : DateTime.MinValue);
		}

		public void OnBeforeSerialize()
		{
			this._DateTime = this.DateTime.ToString("o", CultureInfo.InvariantCulture);
		}

		[OnSerializing]
		internal void OnSerializing(StreamingContext context)
		{
			this.OnBeforeSerialize();
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			this.OnAfterDeserialize();
		}

		[HideInInspector]
		[SerializeField]
		private string _DateTime;
	}
}
