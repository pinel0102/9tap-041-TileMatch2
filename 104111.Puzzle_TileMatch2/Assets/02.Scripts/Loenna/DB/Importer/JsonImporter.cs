using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json.UnityConverters.Math;

using Cysharp.Threading.Tasks;

public class JsonImporter<T> : IDataImporter<T>
{
	private readonly JsonSerializerSettings m_settings;
	public JsonImporter()
	{
		m_settings = new JsonSerializerSettings {
			Converters = new [] {
				new Vector3Converter()
			},
			Formatting = Formatting.Indented,
			ContractResolver = new UnityTypeContractResolver()
		};
	}

	public T[] Import(TextAsset textAsset)
	{
		var objects = JsonConvert.DeserializeObject<List<T>>(textAsset.text);

		return objects?.ToArray() ?? Array.Empty<T>();
	}

	public UniTask<T[]> ImportAsync(string[] array, string index = "")
	{
		if (array == null)
		{
			return UniTask.FromResult(Array.Empty<T>());
		}

		var jsons = array.Where(json => !string.IsNullOrWhiteSpace(json)).ToArray();
		int max = jsons.Count();

		return UniTask.FromResult(
			jsons
			.Where (json => !string.IsNullOrWhiteSpace(json))
			.Select(
				(json, index) => {
					return JsonConvert.DeserializeObject<T>(json, m_settings);
				}
			)
			.ToArray()
		);
	}
}
