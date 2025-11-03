using System;
using System.Linq;
using System.Reflection;

namespace BuildSafe
{
	public static class Reflection<T>
	{
		public static Type Type { get; } = typeof(T);

		public static EventInfo[] Events
		{
			get
			{
				return Reflection<T>.PreFetchEvents();
			}
		}

		public static MethodInfo[] Methods
		{
			get
			{
				return Reflection<T>.PreFetchMethods();
			}
		}

		public static FieldInfo[] Fields
		{
			get
			{
				return Reflection<T>.PreFetchFields();
			}
		}

		public static PropertyInfo[] Properties
		{
			get
			{
				return Reflection<T>.PreFetchProperties();
			}
		}

		private static EventInfo[] PreFetchEvents()
		{
			if (Reflection<T>.gEventsCache != null)
			{
				return Reflection<T>.gEventsCache;
			}
			return Reflection<T>.gEventsCache = Reflection<T>.Type.GetRuntimeEvents().ToArray<EventInfo>();
		}

		private static PropertyInfo[] PreFetchProperties()
		{
			if (Reflection<T>.gPropertiesCache != null)
			{
				return Reflection<T>.gPropertiesCache;
			}
			return Reflection<T>.gPropertiesCache = Reflection<T>.Type.GetRuntimeProperties().ToArray<PropertyInfo>();
		}

		private static MethodInfo[] PreFetchMethods()
		{
			if (Reflection<T>.gMethodsCache != null)
			{
				return Reflection<T>.gMethodsCache;
			}
			return Reflection<T>.gMethodsCache = Reflection<T>.Type.GetRuntimeMethods().ToArray<MethodInfo>();
		}

		private static FieldInfo[] PreFetchFields()
		{
			if (Reflection<T>.gFieldsCache != null)
			{
				return Reflection<T>.gFieldsCache;
			}
			return Reflection<T>.gFieldsCache = Reflection<T>.Type.GetRuntimeFields().ToArray<FieldInfo>();
		}

		private static Type gCachedType;

		private static MethodInfo[] gMethodsCache;

		private static FieldInfo[] gFieldsCache;

		private static PropertyInfo[] gPropertiesCache;

		private static EventInfo[] gEventsCache;
	}
}
