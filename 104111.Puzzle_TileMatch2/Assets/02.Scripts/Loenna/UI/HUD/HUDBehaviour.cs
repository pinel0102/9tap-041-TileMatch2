using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NineTap.Common;
using TMPro;

[ResourcePath("UI/HUD/HUDBehaviour")]
public class HUDBehaviour : CachedBehaviour
{
	[SerializeField]
	private List<HUD_Field> m_fields;
    private Dictionary<HUDType, HUD_Field> m_fieldDic = new();
    public List<HUD_Field> Fields => m_fields;

	public void OnSetup(params HUDFieldParameter[] parameters)
	{
        //Debug.Log(CodeManager.GetMethodName());
        
		if (parameters == null)
		{
			return;
		}

		m_fieldDic = parameters
			.Take(Mathf.Min(parameters.Count(), m_fields.Count))
			.Select(
				(param, index) => {
					m_fields[index].OnSetup(param);
					return (param.Type, m_fields[index]);
				}
			)
			.ToDictionary(keySelector: tuple => tuple.Type, elementSelector: tuple => tuple.Item2);
	}

	public void OnShow(params HUDType[] types)
	{
		foreach (var (type, field) in m_fieldDic)
		{
			field.SetVisible(types.Any(hudType => hudType.HasFlag(type)));
		}
	}

	public Transform GetAttractorTarget(HUDType hudType)
	{
		if (m_fieldDic.TryGetValue(hudType, out var target))
		{
			return target.AttractorTarget;
		}

		return null;
	}

    public void AddListener(params KeyValuePair<HUDType, System.Action>[] OnClick)
    {
        for(int i=0; i < OnClick.Length; i++)
        {
            m_fieldDic.FirstOrDefault(item => item.Key == OnClick[i].Key).Value?.AddListener(OnClick[i].Value);
        }
    }
}
