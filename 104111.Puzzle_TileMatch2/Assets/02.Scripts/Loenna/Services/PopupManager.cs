#nullable enable

using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using NineTap.Common;

public class PopupManager
{
	#region Enums
	private enum PopupStage
	{
		None,
		Open,
		Close,
	}
	#endregion

	#region Fields
	private readonly GameObject? m_root;
	private readonly List<UIPopup> m_popups;
	private PopupStage m_popupStage;
	private bool m_isClosePopupRequested;
	public GameObject? Root => m_root;
	public UIPopup? Last => m_popups.LastOrDefault();
	#endregion

	#region Constructor
	public PopupManager(GameObject root)
	{
		m_root = root;
		m_popups = new();
		m_popupStage = PopupStage.None;
	}
	#endregion

	#region Methods
	public void ShowPopupUI<T>(UIParameter uiParameter)
		where T : UIPopup
	{
		ShowPopup<T>(uiParameter);
	}

	public void ClosePopupUI(UIPopup popup)
	{
		ClosePopup(popup);
	}

	public T? ShowPopup<T>(UIParameter uiParameter)
		where T : UIPopup
	{
		Assert.AreEqual(m_popupStage, PopupStage.None);

		T? popup = null;

		// 프리팹 인스턴싱에서도 호출될 수 있기 때문에 여기에서 상태를 설정한다.
		m_popupStage = PopupStage.Open;

		try
		{
			popup = InstantiatePopup<T>();
			if (popup == null)
			{
				return null;
			}

			AddPopup(popup);

			popup.OnSetup(uiParameter);
			popup.Show();
		}
#if DEBUG
		catch (Exception e)
		{
			Debug.LogException(e);
#else
		catch
		{
#endif

			m_isClosePopupRequested = false;
			RemovePopup(popup);

			if (popup != null)
			{
				GameObject.Destroy(popup.CachedGameObject);
			}

			return null;
		}
		finally
		{
			m_popupStage = PopupStage.None;
		}

		if (m_isClosePopupRequested)
		{
			m_isClosePopupRequested = false;
			ClosePopup(popup);

			return null;
		}

		return popup;
	}

	public void ClosePopup(UIPopup? popup)
	{
        Debug.Log(CodeManager.GetMethodName() + string.Format("Close Popup : {0}", popup?.name));
		ClosePopupInternal(popup, CancellationToken.None);
	}

	private void ClosePopupInternal(UIPopup? popup, CancellationToken token)
	{
		Assert.IsTrue(m_popupStage == PopupStage.None || m_popupStage == PopupStage.Open);

		if (popup == null)
		{
			return;
		}

		if (m_popupStage == PopupStage.Open && m_popups.LastOrDefault() == popup)
		{
			m_isClosePopupRequested = true;
			return;
		}

		if (!RemovePopup(popup))
		{
			return;
		}

		m_popupStage = PopupStage.Close;

		try
		{
			popup.Hide();
		}
		catch (Exception e)
		{
//#if DEBUG
			Debug.LogException(e);
//#endif
		}
		finally
		{
			m_popupStage = PopupStage.None;
			GameObject.Destroy(popup.CachedGameObject);
		}
	}

	private T? InstantiatePopup<T>()
		where T : UIPopup
	{
		GameObject? prefab = Load();
		return prefab == null ? null : GameObject.Instantiate(prefab, Root?.transform).GetComponent<T>();

		GameObject? Load()
		{
			string path = ResourcePathAttribute.GetPath<T>();
			if (string.IsNullOrWhiteSpace(path))
			{
				Debug.LogError($"{typeof(T)} 형식은 리소스 경로 값을 가지고 있지 않습니다.");
				return null;
			}

			GameObject prefab = Resources.Load<GameObject>(path);
			if (prefab == null)
			{
				Debug.LogError($"{typeof(T)} 형식의 프리팹을 로드할 수 없습니다.");
				return null;
			}

			return prefab;
		}
	}

	private void AddPopup<T>(T popup) where T: UIPopup
	{
		RemoveDestroyedPopups();

		if (CustomSortingOrderAttribute.TryGetSortingOrder<T>(out int sortingOrder))
		{
			popup.CachedCanvas.sortingOrder = sortingOrder;
			m_popups.Add(popup);

			return;
		}

		UIPopup? prevPopup = m_popups.LastOrDefault();

		if (prevPopup != null)
		{
			//popup.CachedCanvas.sortingOrder = prevPopup!.CachedCanvas.sortingOrder + 1;
			prevPopup.CachedRaycaster.enabled = false;
		}

		m_popups.Add(popup);
	}

	private bool RemovePopup(UIPopup? popup)
	{
		if (popup == null)
		{
			return false;
		}

		if (m_popups.Count < 1)
		{
			return false;
		}

		int index = m_popups.FindIndex(item => item == popup);
		if (index == -1)
		{
			return false;
		}

		var removedPoup = m_popups[index];
		m_popups.RemoveAt(index);

		RemoveDestroyedPopups();

		if (m_popups.Count > 0)
		{
			UIPopup topPopup = m_popups[^1]!;
			topPopup.CachedRaycaster.enabled = true;
		}

		return true;
	}

	private void RemoveDestroyedPopups()
	{
		for (int i = m_popups.Count - 1; i >= 0; --i)
		{
			if (ObjectUtility.IsNullOrDestroyed(m_popups[i]))
			{
				m_popups.RemoveAt(i);
			}
		}
	}

	public bool ClosePopupUI()
	{
		//	스택에 쌓여있는 씬이 없으면 뒤로가기 불가
		if (m_popups.Count == 0)
		{
			return false;
		}

        UIPopup lastPopup = m_popups.LastOrDefault();

        if(lastPopup.ignoreBackKey)
            return true;

        ClosePopup(lastPopup);

		return true;
	}

    public void ClosePopupUI_Force()
	{
		//	스택에 쌓여있는 씬이 없으면 뒤로가기 불가
		if (m_popups.Count == 0)
		{
			return;
		}
        
        ClosePopup(m_popups.LastOrDefault());
	}

    public void ClosePopupUI_ForceAll()
	{
		//	스택에 쌓여있는 씬이 없으면 뒤로가기 불가
		if (m_popups.Count == 0)
		{
			return;
		}

        for(int i = m_popups.Count - 1; i >= 0; i--)
        {
            ClosePopup(m_popups.LastOrDefault());
        }
	}
	#endregion
}
