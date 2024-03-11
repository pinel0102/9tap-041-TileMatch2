using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class PlaySceneCommandReceiver : Command.Receiver<CommandResource.PlayScene>
{
	private readonly GameManager m_gameManager;

	public PlaySceneCommandReceiver(GameManager gameManager)
	{
		m_gameManager = gameManager;
	}

	public override UniTask Execute(CommandResource.PlayScene resource)
	{
        if (m_gameManager.IsBasketEnable())
        {
            var (type, tileItem) = resource;

            //UnityEngine.Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}] {1} : {2}", type, tileItem.BlockerType, tileItem.BlockerICD));

            var list = m_gameManager.MoveTo(tileItem, type.GetLocationType());
            var result = tileItem with { Location = type.GetLocationType()};

            switch (type)
            {
                case CommandType.PlayScene.ROLLBACK_TILE_TO_BOARD:
                    m_gameManager.AddToBoard(result, list);
                    break;
                case CommandType.PlayScene.MOVE_TILE_IN_BOARD_TO_BASKET:
                case CommandType.PlayScene.MOVE_TILE_IN_STASH_TO_BASKET:
                    m_gameManager.AddToBasket(result, list);
                    break;
                case CommandType.PlayScene.ROLLBACK_TILE_TO_STASH:
                    m_gameManager.AddToStash(result, list);
                    break;
                case CommandType.PlayScene.CHANGE_BLOCKER_ICD:
                    m_gameManager.ChangeICD(result, list, -1);
                    break;
                case CommandType.PlayScene.ROLLBACK_CHANGE_BLOCKER_ICD:
                    m_gameManager.ChangeICD(result, list, 1);
                    break;
            }
        }

		return UniTask.CompletedTask;
	}

	public override UniTask UnExecute(CommandResource.PlayScene resource)
	{
		return Execute(resource.Undo);
	}
}
