using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public partial class TileItem
{
    private bool ShuffleMove;
    private float _shuffleAngle;
    private float _shuffleRadius;
    private float _shuffleSpeed;
    private Transform _shuffleCenter;
    private Vector3 _myPosition;

#region Shuffle Effect

    private void Update()
    {
        if (ShuffleMove)
            CircleMove();
    }

    public void ShuffleStart(float radiusMin, float radiusMax, float speed)
    {
        _myPosition = transform.position;
        _shuffleAngle = Random.Range(0, 360);
        _shuffleRadius = Random.Range(radiusMin, radiusMax);
        _shuffleSpeed = speed;
        ShuffleMove = true;
    }

    public void ShuffleStop()
    {
        ShuffleMove = false;

        UniTask.Void(
            async () =>
            {
                await transform
                    .DOMove(_myPosition, 0.25f)
                    .OnComplete(() => {
                        transform.position = _myPosition;
                    })
                    .ToUniTask()
                    .AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();
            }
        );
    }

    private void CircleMove()
    {    
        _shuffleAngle -= _shuffleSpeed * Time.deltaTime;
    
        var offset = new Vector3(Mathf.Sin(_shuffleAngle), Mathf.Cos(_shuffleAngle)) * _shuffleRadius;
        transform.position = _shuffleCenter.position + offset;
    }

#endregion Shuffle Effect
}
