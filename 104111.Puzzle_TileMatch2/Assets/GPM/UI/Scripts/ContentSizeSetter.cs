namespace Gpm.Ui
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ContentSizeSetter : UIBehaviour
    {
        [System.NonSerialized]
        private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        public Vector2 margin;
        public RectTransform[] target;

        protected override void OnEnable()
        {
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            Vector2 sizeDelta = new Vector2(rectTransform.rect.size.x + margin.x, rectTransform.rect.size.y + margin.y);

            if (target != null)
            {
                for (int i = 0; i < target.Length; i++)
                {
                    target[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);
                    target[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeDelta.y);

                    if (target[i].TryGetComponent<LayoutElement>(out var element))
                    {
                        element.minWidth = sizeDelta.x;
                        element.minHeight = sizeDelta.y;
                        element.preferredHeight = sizeDelta.y;
                        element.preferredWidth = sizeDelta.x;
                    }

                    LayoutRebuilder.MarkLayoutForRebuild(target[i]);
                }
            }
        }

        protected void SetDirty(bool force = false)
        {
            if (IsActive() == false)
            {
                return;
            }

            if (force == true)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
            else
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty(false);
        }
#endif
    }
}