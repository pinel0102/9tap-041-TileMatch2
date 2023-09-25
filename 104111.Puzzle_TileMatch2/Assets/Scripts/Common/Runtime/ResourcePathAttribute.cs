#nullable enable

using UnityEngine;

using System;
using System.Reflection;

namespace NineTap.Common
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class ResourcePathAttribute : Attribute
	{
		public string Path { get; set; }

		public ResourcePathAttribute(string path)
		{
			Path = path;
		}

		public static string GetPath<T>()
		{
			ResourcePathAttribute attribute = typeof(T).GetCustomAttribute<ResourcePathAttribute>(inherit: false);
			return attribute?.Path ?? "";
		}

		public static T? GetResource<T>()
			where T: UnityEngine.Object
		{
			string path = GetPath<T>();
			return !string.IsNullOrWhiteSpace(path) ? Resources.Load<T>(path) : default(T);
		}
	}
}
