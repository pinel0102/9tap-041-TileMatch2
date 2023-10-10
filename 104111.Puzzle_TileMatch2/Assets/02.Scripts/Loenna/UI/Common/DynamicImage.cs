using UnityEngine;
using UnityEngine.UI;

public class DynamicImage : Image
{
	[SerializeField]
	private string m_pathParameter;

	[ContextMenu("Set Current Path")]
	private void ResetPath()
	{
		m_pathParameter = ObjectUtility.GetRawObject(this)?.sprite?.name ?? string.Empty;
	}

	public void OnSetup(string atlasPath)
	{

	}

	public void ChangeSprite(params string[] param)
	{
		sprite = SpriteManager.GetSprite(GetPath(param));

		SetAllDirty();

		string GetPath(params string[] param) 
		{
			if (param == null)
			{
				return m_pathParameter;
			}

			return string.Format(m_pathParameter, param);
		}
	}

	public void ChangeSprite(string param0)
	{
		sprite = SpriteManager.GetSprite(string.Format(m_pathParameter, param0));
		SetAllDirty();
	}
}
