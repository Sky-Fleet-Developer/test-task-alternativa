using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyTestTask.Abstraction.Injection;
using MyTestTask.Abstraction.Model;
using MyTestTask.Abstraction.Patterns;
using MyTestTask.Abstraction.View;
using MyTestTask.Abstraction.View.Layout;
using MyTestTask.Ranks.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MyTestTask.Ranks.View
{
    public class RanksListView : MonoBehaviour, IDataListView<IDataSource<Rank>>, IInjectionTarget
    {
        [SerializeField] private ScrollRect scroll;
        
        [Header("Size detection")]
        [Tooltip("it seems like a crutch, but it is the best, fastest and safest way to get global position of ui element border")]
        [SerializeField] private RectTransform topScrollViewAnchor;
        [SerializeField] private RectTransform bottomScrollViewAnchor;
        [SerializeField] private RectTransform scrollViewCenterAnchor;
        [SerializeField] private float anchorsTolerance;
        [SerializeField] private float centringTolerance;

        private IDataSource<Rank> _dataSource;
        private IFactory<IDataListFlexibleElement<Rank>> _factory;
        private List<IDataListFlexibleElement<Rank>> _pool = new();
        private LinkedList<IDataListFlexibleElement<Rank>> _items = new();
        private DefaultInputActions _inputActions;
        private float _currentHeight;
        private int _currentRangeMin;
        private int _currentRangeMax;
        private bool _refreshLoopInProgress;
        private bool _isDirty;
        private void Awake()
        {
            scroll.onValueChanged.AddListener(OnScrollValueChanged);
            _inputActions = new DefaultInputActions();
            _inputActions.UI.Enable();
        }

        public void OnScrollValueChanged(Vector2 value)
        {
            if (_inputActions.UI.Click.IsPressed())
            {
                return;
            }

            RefreshView();
        }

        private void Update()
        {
            if (_inputActions.UI.Click.IsPressed())
            {
                _isDirty = true;
                return;
            }
            
            if (_isDirty)
            {
                _isDirty = false;
                RefreshView();
            }
        }

        public void Inject(DiContainer container)
        {
            if (_factory == null)
            {
                _factory = container.Resolve<IFactory<IDataListFlexibleElement<Rank>>>();
            }
        }

        public void SetDataSource(IDataSource<Rank> dataSource)
        {
            _dataSource = dataSource;
            RefreshLoop();
        }

        // apparently, transform.position of new items is not update immediately. We have to wait and check again 
        private async void RefreshLoop()
        {
            if (_refreshLoopInProgress)
            {
                return;
            }
            _refreshLoopInProgress = true;
            float prevSize = 0;
            do
            {
                float t = Time.time;
                while (Time.time - t < Time.deltaTime)
                {
                    await Task.Yield();
                }
                prevSize = _currentHeight;
                OnDimensionsChanged();
            }
            while (!Mathf.Approximately(prevSize, _currentHeight) || _currentHeight < (topScrollViewAnchor.position.y - bottomScrollViewAnchor.position.y) * 0.5f);
            _refreshLoopInProgress = false;
        }

        private void RefreshView()
        {
            if (_dataSource == null)
            {
                Debug.LogError("DataSource is null");
                return;
            }
            OnDimensionsChanged();
            if (_items.Count > 0 && !Mathf.Approximately(_items.First.Value.RectTransform.localPosition.y, 0))
            {
                MoveAllToZero();
                RecalculateHeight();
            }
        }

        private void OnItemSizeChanged(IFlexibleLayoutElement item, float size)
        {
            float oldHeight = _currentHeight;
            if (RecalculateHeight())
            {
                OnDimensionsChanged();
            }
            
            float delta = _currentHeight - oldHeight;
            var element = _items.Find((IDataListFlexibleElement<Rank>)item);
            if (element == null)
            {
                Debug.LogError("Some element was not unsubscribed from size changes callback and was now removed");
                item.OnSizeChanged -= OnItemSizeChanged;
                return;
            }
            var pointer = element.Next;
            while (pointer != null)
            {
                pointer.Value.SetPositionAfter(pointer.Previous!.Value);
                pointer = pointer.Next;
            }
        }

        private void SetItemLast(IDataListFlexibleElement<Rank> item, Rank data)
        {
            item.SetData(data);
            item.SetPositionAfter(_items.Last.Value);
            item.RectTransform.SetAsLastSibling();
            _items.AddLast(item);
            RecalculateHeight();
        }

        private void SetItemFirst(IDataListFlexibleElement<Rank> item, Rank data)
        {
            item.SetData(data);
            item.SetPositionBefore(_items.First.Value);
            item.RectTransform.SetAsFirstSibling();
            _items.AddFirst(item);
            RecalculateHeight();
        }

        private void RemoveFirstItem()
        {
            _currentRangeMin++;
            RemoveItem(_items.First.Value);
            RecalculateHeight();
        }

        private void RemoveLastItem()
        {
            _currentRangeMax--;
            RemoveItem(_items.Last.Value);
            RecalculateHeight();
        }

        private void MoveAllToZero()
        {
            var firstElementPosition = _items.First.Value.RectTransform.position.y;
            _items.First.Value.SetPositionAsFirst();
            var pointer = _items.First.Next;
            while (pointer != null)
            {
                pointer.Value.SetPositionAfter(pointer.Previous!.Value);
                pointer = pointer.Next;
            }
            float delta = _items.First.Value.RectTransform.position.y - firstElementPosition;
            scroll.content.anchoredPosition -= new Vector2(0, delta);
        }

        private void RemoveItem(IDataListFlexibleElement<Rank> item)
        {
            item.OnSizeChanged -= OnItemSizeChanged;
            item.OnSelected -= OnItemSelected;

            _items.Remove(item);
            item.gameObject.SetActive(false);
            _pool.Add(item);
        }

        private bool RecalculateHeight()
        {
            float prevHeight = _currentHeight;
            _currentHeight = 0;
            var pointer = _items.First;
            while (pointer != null)
            {
                _currentHeight += pointer.Value.Size;
                pointer = pointer.Next;
            }
            scroll.content.sizeDelta = new Vector2(0, _currentHeight);

            return !Mathf.Approximately(prevHeight, _currentHeight);
        }

        private void OnDimensionsChanged()
        {
            if (_items.Count == 0)
            {
                var newItem = GetFromPoolOrCreate();
                newItem.SetData(_dataSource.Get(_currentRangeMin));
                newItem.SetPositionAsFirst();
                _items.AddFirst(newItem);
                RecalculateHeight();
            }
            Vector3 topAnchor = topScrollViewAnchor.position;
            while (_items.First.Value.IsBelowThen(topAnchor.y - anchorsTolerance) && _currentRangeMin > 0) // try to add first
            {
                var newItem = GetFromPoolOrCreate();
                SetItemFirst(newItem, _dataSource.Get(--_currentRangeMin));
            }
            while (_items.First.Value.IsAboveThen(topAnchor.y + anchorsTolerance) && _currentRangeMin < _currentRangeMax) //try to remove first
            {
                RemoveFirstItem();
            }
            
            var bottomAnchor = bottomScrollViewAnchor.position;
            while (_items.Last.Value.IsAboveThen(bottomAnchor.y + anchorsTolerance) && _currentRangeMax < _dataSource.Count - 1) // try to add last
            {
                var newItem = GetFromPoolOrCreate();
                SetItemLast(newItem, _dataSource.Get(++_currentRangeMax));
            }
            while (_items.Last.Value.IsBelowThen(bottomAnchor.y - anchorsTolerance) && _currentRangeMax > _currentRangeMin) // try to remove last
            {
                RemoveLastItem();
            }
        }

        private IDataListFlexibleElement<Rank> GetFromPoolOrCreate()
        {
            IDataListFlexibleElement<Rank> result;
            if (_pool.Count == 0)
            {
                result = CreateItem();
            }
            else
            {

                result = _pool[^1];
                _pool.RemoveAt(_pool.Count - 1);
                result.gameObject.SetActive(true);
            }
            result.OnSizeChanged += OnItemSizeChanged;
            result.OnSelected += OnItemSelected;
            return result;
        }

        private void OnItemSelected(IDataListFlexibleElement<Rank> item)
        {
            var delta = item.RectTransform.position.y - scrollViewCenterAnchor.position.y;
            if (Mathf.Abs(delta) > centringTolerance)
            {
                float deltaWithTolerance = delta - Mathf.Sign(delta) * centringTolerance;
                scroll.content.anchoredPosition -= new Vector2(0, deltaWithTolerance);
            }
        }

        private IDataListFlexibleElement<Rank> CreateItem()
        {
            var item = _factory.Create();
            item.RectTransform.SetParent(scroll.content.transform);
            return item;
        }

        private void OnDestroy()
        {
            var pointer = _items.Last;
            while (pointer != null)
            {
                var prev = pointer;
                pointer = pointer.Previous;
                RemoveItem(prev.Value);
            }

            foreach (IDataListFlexibleElement<Rank> element in _pool)
            {
                Destroy(element.gameObject); // we could move method out from here, for example, to the factory
            }
            _inputActions.UI.Disable();
            _inputActions.Dispose();
        }
    }
}