using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Pagination
{
    public partial class PagedRect
    {
        protected void OnMouseOver_Disable()
        {
            if (HighlightWhenMouseIsOver)
            {
                ShowHighlight();
            }
        }

        protected void OnMouseExit_Disable()
        {
            if (HighlightWhenMouseIsOver)
            {
                ClearHighlight();
            }
        }

        void SetupMouseEvents()
        {
            var eventTrigger = this.gameObject.AddComponent<EventTrigger>();

            var pointerEnter = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerEnter,
                callback = new EventTrigger.TriggerEvent()
            };
            pointerEnter.callback.AddListener((eventData) => { mouseIsOverPagedRect = true; OnMouseOver_Disable(); });

            var pointerExit = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerExit,
                callback = new EventTrigger.TriggerEvent()
            };
            pointerExit.callback.AddListener((eventData) => { mouseIsOverPagedRect = false; OnMouseExit_Disable(); });

            eventTrigger.triggers.Add(pointerEnter);
            eventTrigger.triggers.Add(pointerExit);
        }
    }
}
