using System;
using System.Collections;
using System.Threading.Tasks;
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
    public class RankView : Selectable, IDataListFlexibleElement<Rank>, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI sequenceIndexText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private RectTransform dropDownArrow;
        [SerializeField] private float minSize;
        [SerializeField] private float maxSize;
        [SerializeField] private float expansionDuration;
        [Header("Size detection")]
        [SerializeField] private RectTransform topAnchor;
        [SerializeField] private RectTransform bottomAnchor;
        private float _currentSize;
        private Rank _data;
        private RectTransform _rectTransform;
        private bool _isExpanded;
        private bool _targetExpanded;
        private Coroutine _expandingCoroutine;

        public event Action<IFlexibleLayoutElement, float> OnSizeChanged;
        public event Action<IDataListFlexibleElement<Rank>> OnSelected;
        public float Size => _currentSize;
        public Rank Data => _data;
        public RectTransform RectTransform => _rectTransform;

        public bool IsSelected { get; set; }
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
            if (Application.isPlaying)
            {
                SetCollapsed();
                selectionIndicator.gameObject.SetActive(false);
            }

            base.OnEnable();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }

        
        public void SetData(Rank data)
        {
            _data = data;
            sequenceIndexText.text = (_data.SequenceIndex+1).ToString("00") + ".";
            nameText.text = LocalizationService.Localize($"rank_{_data.Id}_name");
            descriptionText.text = string.Format(LocalizationService.Localize($"next_rank_description"), _data.ExperienceLevel, _data.ExtToNextLevel);
            LoadIcon(_data.Id);
        }

        private async void LoadIcon(string id) // I wouldn't realize cancellation token
        {
            try
            {
                icon.gameObject.SetActive(false);
                // just imagine that we have the resources caching system, and we use it right here
                var loadingOperation = Resources.LoadAsync<Sprite>($"rank_{id}_icon");
                await loadingOperation;
                icon.gameObject.SetActive(true);
                icon.sprite = loadingOperation.asset as Sprite;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            selectionIndicator.gameObject.SetActive(true);
            base.OnSelect(eventData);
            OnSelected?.Invoke(this);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            selectionIndicator.gameObject.SetActive(false);
            TeySetExpanded(false);
            base.OnDeselect(eventData);
        }


        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            if (eventData.moveDir == MoveDirection.Right && !_isExpanded && _expandingCoroutine == null)
            {
                TeySetExpanded(true);
            }
            else if (eventData.moveDir == MoveDirection.Left && _isExpanded)
            {
                TeySetExpanded(false);
            }
        }
        
        private void TeySetExpanded(bool value)
        {
            _targetExpanded = value;
            if (_expandingCoroutine == null)
            {
                _expandingCoroutine = StartCoroutine(SetExpanded(value));
            }
        }

        private IEnumerator SetExpanded(bool value)
        {
            Vector3 arrowRotation = Vector3.forward * (value ? -90 : 90);
            Vector3 initialArrowRotation = dropDownArrow.localEulerAngles;
            Vector2 currentSize = _rectTransform.sizeDelta;
            Vector2 targetSize = new Vector2(currentSize.x, value ? maxSize : minSize);
            
            foreach (float progress in TweenUtility.Tween(expansionDuration))
            {
                yield return null;
                dropDownArrow.localEulerAngles = Vector3.Lerp(initialArrowRotation, arrowRotation, progress);
                _rectTransform.sizeDelta = Vector2.Lerp(currentSize, targetSize, progress);
                _currentSize = _rectTransform.sizeDelta.y;
                OnSizeChanged?.Invoke(this, _currentSize);
            }

            _isExpanded = value;
            if (_targetExpanded == _isExpanded)
            {
                _expandingCoroutine = null;
            }
            else
            {
                _expandingCoroutine = StartCoroutine(SetExpanded(value));
            }
        }

        private void SetCollapsed()
        {
            dropDownArrow.localEulerAngles = Vector3.forward * 90;
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, minSize);;
            _currentSize = minSize;
            OnSizeChanged?.Invoke(this, _currentSize);
            _expandingCoroutine = null;
            _isExpanded = false;
        }
        
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
            _rectTransform.anchoredPosition = element.RectTransform.anchoredPosition + Vector2.up * Size;
            RefreshWidth();
        }

        private void RefreshWidth()
        {
            _rectTransform.sizeDelta = new Vector2(0, _rectTransform.sizeDelta.y);
        }

        public bool IsBelowThen(float worldPoint)
        {
            return topAnchor.position.y < worldPoint;
        }

        public bool IsAboveThen(float worldPoint)
        {
            return bottomAnchor.position.y > worldPoint;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_expandingCoroutine == null)
            {
                TeySetExpanded(!_isExpanded);
            }
        }
    }
}