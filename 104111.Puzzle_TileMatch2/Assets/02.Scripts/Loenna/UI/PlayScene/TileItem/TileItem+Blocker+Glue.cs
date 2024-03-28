using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public partial class TileItem
{
#region Glue FX

    public async UniTask WaitGlueAnimation(bool existPair, TileItem pairTile, Action onFinished)
    {
        if (existPair)
        {
            bool aniFinished = false;

            RefreshBlockerState(blockerType, blockerICD);
            pairTile.RefreshBlockerState(pairTile.blockerType, pairTile.blockerICD);

            SetBasketParent();
            pairTile.SetBasketParent();

            SetDim(0);
            pairTile.SetDim(0);

            SetMoving(true);
            pairTile.SetMoving(true);

            switch(blockerType)
            {
                case BlockerType.Glue_Left:
                    pairTile.m_blockerImage.gameObject.SetActive(false);
                    pairTile.PlayBlockerEffect(pairTile.blockerType, pairTile.blockerICD, pairTile.m_blockerRect.position);
                    await DoGlueMove(transform, pairTile.transform, GlobalDefine.GlueFX_Duration, OnAniFinished);
                    break;
                case BlockerType.Glue_Right:
                    m_blockerImage.gameObject.SetActive(false);
                    PlayBlockerEffect(blockerType, blockerICD, m_blockerRect.position);
                    await DoGlueMove(pairTile.transform, transform, GlobalDefine.GlueFX_Duration, OnAniFinished);
                    break;
                default:
                    aniFinished = true;
                    break;
            }

            await UniTask.WaitUntil(() => aniFinished);

            void OnAniFinished()
            {
                RollBackParent();
                pairTile.RollBackParent();

                aniFinished = true;
            }
        }

        onFinished?.Invoke();
    }

    private UniTask DoGlueMove(Transform left, Transform right, float duration, Action onComplete = null)
    {
        Sequence seqGlue = DOTween.Sequence();

        if(globalData.glueSimple)
        {
            var (leftPathLast, rightPathLast) = GlobalDefine.GluePathLast(left.transform.localPosition, right.transform.localPosition);
            var (leftRotationLast, rightRotationLast) = GlobalDefine.GlueRotationLast();

            seqGlue
                .Append(left.DOLocalMove(leftPathLast, duration))
                .Join(left.DOLocalRotate(leftRotationLast, duration))
                .Join(right.DOLocalMove(rightPathLast, duration))
                .Join(right.DOLocalRotate(rightRotationLast, duration));
        }
        else
        {
            var (leftPath, rightPath) = GlobalDefine.GluePathArray(left.transform.localPosition, right.transform.localPosition);
            var (leftRotation, rightRotation) = GlobalDefine.GlueRotationArray();

            float eDuration = duration * GlobalDefine.GlueFX_Count_Inverse;

            for(int i = 0; i < GlobalDefine.GlueFX_Count; i++)
            {
                seqGlue
                    .Append(left.DOLocalMove(leftPath[i], eDuration))
                    .Join(left.DOLocalRotate(leftRotation[i], eDuration))
                    .Join(right.DOLocalMove(rightPath[i], eDuration))
                    .Join(right.DOLocalRotate(rightRotation[i], eDuration));
            }
        }

        return seqGlue
            .SetEase(globalData.glueEase)
            .SetAutoKill()
            .OnComplete(() => {
                left.DOLocalRotate(Vector3.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                    .OnComplete(() => left.transform.localRotation = Quaternion.identity);
                right.DOLocalRotate(Vector3.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                    .OnComplete(() => right.transform.localRotation = Quaternion.identity);
                onComplete?.Invoke();
            })
            .Play()
            .ToUniTask();
    }

#endregion Glue FX

}