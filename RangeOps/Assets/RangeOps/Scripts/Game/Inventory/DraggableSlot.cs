using UnityEngine;
using UnityEngine.EventSystems;
using Game.Items;

namespace Game.Inventory
{
    public enum DragSlotType { Primary, Secondary, Component, Attachment }

    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private DragSlotType slotType;
        [SerializeField] private int slotIndex = -1;
        [SerializeField] private int chunkAmount = -1;
        [SerializeField] private AttachmentType attachmentType = AttachmentType.None;
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private RectTransform dropZone;

        private RectTransform rect;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Configure(DragSlotType type, int index, int amount, InventoryUI ui, RectTransform zone)
        {
            slotType = type;
            slotIndex = index;
            chunkAmount = amount;
            inventoryUI = ui;
            dropZone = zone;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalPosition = rect.anchoredPosition;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rect.anchoredPosition += eventData.delta / rect.lossyScale.x;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            rect.anchoredPosition = originalPosition;

            bool droppedInZone = dropZone != null &&
                RectTransformUtility.RectangleContainsScreenPoint(dropZone, eventData.position, eventData.pressEventCamera);

            if (droppedInZone)
                inventoryUI.HandleSlotDropped(slotType, slotIndex, chunkAmount, attachmentType);
        }
    }
}