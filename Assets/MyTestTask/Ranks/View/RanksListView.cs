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
        
        private IDataSource<Rank> _dataSource;
        private IFactory<IDataListFlexibleElement<Rank>> _factory;
        private List<IDataListFlexibleElement<Rank>> _pool = new();
        private LinkedList<IDataListFlexibleElement<Rank>> _items = new();
        private float _scrollValue;
        private float _currentHeight;
        private int _currentRangeMin;
        private int _currentRangeMax;

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
            RefreshView();
        }

        // apparently, transform.position of new items is not update immediately. We have to wait and check again 
        private async void RefreshLoop()
        {
            float prevSize = 0;
            do
            {
                float t = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t < Time.deltaTime)
                {
                    await Task.Yield();
                }
                prevSize = _currentHeight;
                OnDimensionsChanged();
            }
            while (!Mathf.Approximately(prevSize, _currentHeight));
        }

        private void RefreshView()
        {
            if (_dataSource == null)
            {
                Debug.LogError("DataSource is null");
                return;
            }
            RefreshLoop();
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
                pointer.Value.AddPositionCorrection(-delta);
                pointer = pointer.Next;
            }
        }

        private void SetItemLast(IDataListFlexibleElement<Rank> item, Rank data)
        {
            item.SetData(data);
            item.SetPositionAfter(_items.Last.Value);
            item.OnSizeChanged += OnItemSizeChanged;
            item.RectTransform.SetAsLastSibling();
            _items.AddLast(item);
            RecalculateHeight();
        }

        private void SetItemFirst(IDataListFlexibleElement<Rank> item, Rank data)
        {
            item.SetData(data);
            item.SetPositionBefore(_items.First.Value);
            item.OnSizeChanged += OnItemSizeChanged;
            item.RectTransform.SetAsFirstSibling();
            _items.AddFirst(item);
            RecalculateHeight();
        }

        private void RemoveItem(IDataListFlexibleElement<Rank> item)
        {
            item.OnSizeChanged -= OnItemSizeChanged;
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

            return !Mathf.Approximately(prevHeight, _currentHeight);
        }

        private  void OnDimensionsChanged()
        {
            if (_items.Count == 0)
            {
                var newItem = GetFromPoolOrCreate();
                newItem.SetData(_dataSource.Get(_currentRangeMin));
                newItem.SetPositionAsFirst();
                newItem.OnSizeChanged += OnItemSizeChanged;
                _items.AddFirst(newItem);
                RecalculateHeight();
            }
            Vector3 topAnchor = topScrollViewAnchor.position;
                while (_items.First.Value.IsBelowThen(topAnchor) && _currentRangeMin > 1)
            {
                var newItem = GetFromPoolOrCreate();
                SetItemFirst(newItem, _dataSource.Get(--_currentRangeMin));
            }
            var bottomAnchor = bottomScrollViewAnchor.position;
            while (_items.Last.Value.IsAboveThen(bottomAnchor) && _currentRangeMax < _dataSource.Count)
            {
                var newItem = GetFromPoolOrCreate();
                SetItemLast(newItem, _dataSource.Get(++_currentRangeMax));
            }
        }

        private IDataListFlexibleElement<Rank> GetFromPoolOrCreate()
        {
            if (_pool.Count == 0)
            {
                return CreateItem();
            }
            
            var result = _pool[^1];
            _pool.RemoveAt(_pool.Count - 1);
            return result;
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
        }
    }
}