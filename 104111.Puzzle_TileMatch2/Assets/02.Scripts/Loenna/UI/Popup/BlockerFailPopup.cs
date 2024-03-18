using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using DG.Tweening;
using NineTap.Constant;
using NineTap.Common;

public record BlockerFailPopupParameter
(
	CurrentPlayState.Finished.State State,
	UITextButtonParameter ContinueButtonParameter,
	Action OnQuit,
    BlockerType BlockerType
) : DefaultParameterWithoutHUD();

/// <summary>
/// Bush / Chain
/// </summary>
[ResourcePath("UI/Popup/BlockerFailPopup")]
public class BlockerFailPopup : UIPopup
{
    [Header("★ [Live] Blocker Fail")]
    [SerializeField]	private BlockerType m_blockerType;
    [SerializeField]	private int m_popupIndex;

    [Header("★ [Reference] UI")]
    [SerializeField]	private UITextButton m_continueButton;
	[SerializeField]	private UITextButton m_quitButton;
    [SerializeField]	private UIImageButton m_exitButton;

    [Header("★ [Reference] Blocker")]
    [SerializeField]    private List<GameObject> m_blockerObject;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not BlockerFailPopupParameter parameter)
		{
            OnClickClose();
			return;
		}

        CurrentPlayState.Finished.State state = parameter.State;        
        m_blockerType = parameter.BlockerType;
        m_popupIndex = GetPopupIndex(m_blockerType);

        m_blockerObject.ForEach(ga => ga.SetActive(false));
        SetBlockerPopup(m_popupIndex);
		
        // [Undo]
		m_continueButton.OnSetup(
            new UITextButtonParameter {
                OnClick = () => {
                    SDKManager.SendAnalytics_C_Scene_Fail("Retry");
                    OnClickClose();
                    parameter.ContinueButtonParameter.OnClick?.Invoke();
                },
                ButtonText = parameter.ContinueButtonParameter.ButtonText,
                SubWidgetBuilder = parameter.ContinueButtonParameter.SubWidgetBuilder
            }
        );

        // [Give Up]
		m_quitButton.OnSetup(
			new UITextButtonParameter {
				OnClick = () => {
                    SDKManager.SendAnalytics_C_Scene_Fail("Close");
					OnClickClose();
					parameter.OnQuit?.Invoke();
				},
				ButtonText = Text.Button.GIVE_UP,
				SubWidgetBuilder = () => {
					var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
					widget.OnSetup("UI_Icon_Heart_Broken");
					return widget.CachedGameObject;
				}
			}
		);

        // [x] == [Give Up]
        m_exitButton.OnSetup(
			new UITextButtonParameter {
				OnClick = () => {
                    SDKManager.SendAnalytics_C_Scene_Fail("Close");
					OnClickClose();
                    parameter.OnQuit?.Invoke();
				}
			}
		);
	}

    private int GetPopupIndex(BlockerType blockerType)
    {
        return blockerType switch{
            BlockerType.Bush => 0,
            BlockerType.Chain => 1,
            _ => 0
        };
    }

    private void SetBlockerPopup(int index)
    {
        m_blockerObject[index].SetActive(true);
    }
}
