using UnityEngine;

using System;

using Cysharp.Threading.Tasks;

public interface IDataImporter<T>
{
	T[] Import(TextAsset textAsset);
	UniTask<T[]> ImportAsync(string[] array, string index = "Key");
}
