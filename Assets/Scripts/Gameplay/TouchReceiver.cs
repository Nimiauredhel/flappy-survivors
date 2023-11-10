using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class TouchReceiver : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event EventHandler<PointerEventData> PointerDown;
        public event EventHandler<PointerEventData> PointerUp;
    
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke(this, eventData);
        }
    }
}
