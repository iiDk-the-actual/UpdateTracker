using System;
using System.Linq;
using System.Reflection;

namespace BuildSafe
{
	public static class Reflection
	{
		public static Assembly[] AllAssemblies
		{
			get
			{
				return Reflection.PreFetchAllAssemblies();
			}
		}

		public static Type[] AllTypes
		{
			get
			{
				return Reflection.PreFetchAllTypes();
			}
		}

		static Reflection()
		{
			Reflection.PreFetchAllAssemblies();
			Reflection.PreFetchAllTypes();
		}

		private static Assembly[] PreFetchAllAssemblies()
		{
			if (Reflection.gAssemblyCache != null)
			{
				return Reflection.gAssemblyCache;
			}
			return Reflection.gAssemblyCache = (from a in AppDomain.CurrentDomain.GetAssemblies()
				where a != null
				select a).ToArray<Assembly>();
		}

		private static Type[] PreFetchAllTypes()
		{
			if (Reflection.gTypeCache != null)
			{
				return Reflection.gTypeCache;
			}
			return Reflection.gTypeCache = (from t in Reflection.PreFetchAllAssemblies().SelectMany((Assembly a) => a.GetTypes())
				where t != null
				select t).ToArray<Type>();
		}

		public static MethodInfo[] GetMethodsWithAttribute<T>() where T : Attribute
		{
			return (from m in Reflection.AllTypes.SelectMany((Type t) => t.GetRuntimeMethods())
				where m.GetCustomAttributes(typeof(T), false).Length != 0
				select m).ToArray<MethodInfo>();
		}

		private static Assembly[] gAssemblyCache;

		private static Type[] gTypeCache;
	}
}
