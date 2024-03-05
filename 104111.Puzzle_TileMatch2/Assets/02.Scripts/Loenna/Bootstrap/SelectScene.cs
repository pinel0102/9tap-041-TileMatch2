using UnityEngine;

using Cysharp.Threading.Tasks;

using NineTap.Common;

using static UnityEngine.SceneManagement.SceneManager;

public class SelectScene : CachedBehaviour
{
	[SerializeField]
	private UITextButton m_moveEditorButton;

	[SerializeField]
	private UITextButton m_moveClientButton;

	private void Start()
	{
		//var numbers = new List<int>();

		//for (int i = 0; i < 25; i++)
		//{
		//	numbers.Add(i);
		//}

		//for (int x = 0; x < 7; x++)
		//{
		//	var result = numbers.Shuffle();

		//	StringBuilder stringBuilder = new StringBuilder();
		//	stringBuilder.AppendJoin(',', result);
		//	Debug.Log($"[{stringBuilder.ToString()}]");
		//}

		m_moveEditorButton.OnSetup(
			new UITextButtonParameter{
				ButtonText = Constant.Scene.EDITOR,
				OnClick = () => {
                    GlobalDefine.SetEditorMode(Constant.Scene.EDITOR);
					LoadScene(Constant.Scene.EDITOR);
				}
			}
		);

		var temp = new AsyncReactiveProperty<bool>(true);

		m_moveClientButton.OnSetup(
			new UITextButtonParameter{
				ButtonText = Constant.Scene.CLIENT,
				Binder = temp,
				OnClick = () => {
                    GlobalDefine.SetEditorMode(Constant.Scene.CLIENT);
					LoadScene(Constant.Scene.CLIENT);
				}
			}
		);
	}
}
