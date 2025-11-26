using System;
using MyTestTask.Abstraction.View;
using MyTestTask.Abstraction.View.Layout;
using MyTestTask.Misc;
using MyTestTask.Misc.View;
using MyTestTask.Ranks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyTestTask.Ranks.View
{
    public class RankView : Selectable, IDataListFlexibleElement<Rank>, ISelectable, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI sequenceIndexText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private RectTransform dropDownArrow;
        [SerializeField] private float minSize;
        [SerializeField] private float maxSize;
        [Header("Size detection")]
        [SerializeField] private RectTransform topAnchor;
        [SerializeField] private RectTransform bottomAnchor;
        private float _currentSize;
        private SelectionProvider _selectionProvider;
        private Rank _data;
        private bool _isSelected;
        private RectTransform _rectTransform;
        private bool _selectionRegistered;
        private ISelectable _prev;
        private ISelectable _next;
        private bool _isInSelectionState;

        public float Size => _currentSize;
        public event Action<IFlexibleLayoutElement, float> OnSizeChanged;
        public Rank Data => _data;
        public RectTransform RectTransform => _rectTransform;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (EventSystem.current.currentSelectedGameObject != gameObject && !_isInSelectionState)
                {
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
                selectionIndicator.gameObject.SetActive(value);
                _isSelected = value;
            }
        }

        public int Order => transform.GetSiblingIndex();
        public bool IsValid => gameObject.activeSelf;

        protected override void Awake()
        {
            _rectTransform = (RectTransform) transform;
            _currentSize = topAnchor.position.y - bottomAnchor.position.y;
            base.Awake();
        }

        protected override void OnEnable()
        {
            selectionIndicator.gameObject.SetActive(false);
            EnsureSelectionProvider();
            EnsureSelectionRegistered();
            _isSelected = false;
            base.OnEnable();
        }

        private void EnsureSelectionRegistered()
        {
            if (_selectionRegistered || !_selectionProvider)
            {
                return;
            }
            _selectionProvider.Register(this);
            _selectionRegistered = true;
        }

        protected override void OnDisable()
        {
            _selectionProvider?.Unregister(this);
            _selectionRegistered = false;
            _prev = null;
            _next = null;
            base.OnDisable();
        }

        private void EnsureSelectionProvider()
        {
            if (_selectionProvider)
            {
                return;
            }
            _selectionProvider = GetComponentInParent<SelectionProvider>();
        }
        
        public void SetData(Rank data)
        {
            _data = data;
            EnsureSelectionProvider();
            EnsureSelectionRegistered();
            sequenceIndexText.text = (_data.SequenceIndex+1).ToString("00") + ".";
            nameText.text = LocalizationService.Localize($"rank_{_data.Id}_name");
            descriptionText.text = LocalizationService.Localize($"rank_{_data.Id}_description");
            LoadIcon(_data.Id);
        }

        private async void LoadIcon(string id) // I wouldn't realize cancellation token
        {
            try
            {
                icon.sprite = Resources.Load<Sprite>("rank_loading_icon"); // just imagine that we have the resources caching system and we use it here
                var loadingOperation = Resources.LoadAsync<Sprite>($"rank_{id}_icon");
                await loadingOperation;
                icon.sprite = loadingOperation.asset as Sprite;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            _isInSelectionState = true;
            try
            {
                _selectionProvider.SetSelected(this);
                base.OnSelect(eventData);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _isInSelectionState = false;
            }
        }

        public void SetPrevNeighbour(ISelectable prev)
        {
            _prev = prev;
        }
        public void SetNextNeighbour(ISelectable next)
        {
            _next = next;
        }

        //public override void OnMove(AxisEventData eventData)
        //{
        //    base.OnMove(eventData);
        //    if (_isSelected)
        //    {
        //        if (eventData.moveDir == MoveDirection.Down && _prev != null)
        //        {
        //            _selectionProvider.SetSelected(_prev);
        //        }
        //        else if (eventData.moveDir == MoveDirection.Up && _next != null)
        //        {
        //            _selectionProvider.SetSelected(_next);
        //        }
        //    }
        //}
        
        public void SetPositionAsFirst()
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            RefreshWidth();
        }

        public void SetPositionAfter(IDataListFlexibleElement<Rank> element)
        {
            _rectTransform.anchoredPosition = element.RectTransform.anchoredPosition + Vector2.down * element.Size;
            RefreshWidth();
        }

        public void SetPositionBefore(IDataListFlexibleElement<Rank> element)
        {
            _rectTransform.anchoredPosition = element.RectTransform.anchoredPosition + Vector2.down * Size;
            RefreshWidth();
        }

        public void AddPositionCorrection(float correction)
        {
            _rectTransform.anchoredPosition += Vector2.up * correction;
        }

        private void RefreshWidth()
        {
            _rectTransform.sizeDelta = new Vector2(0, _rectTransform.sizeDelta.y);
        }

        public bool IsBelowThen(Vector3 worldPoint)
        {
            return topAnchor.position.y < worldPoint.y;
        }

        public bool IsAboveThen(Vector3 worldPoint)
        {
            return bottomAnchor.position.y > worldPoint.y;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _selectionProvider.SetSelected(this);
        }
    }
}