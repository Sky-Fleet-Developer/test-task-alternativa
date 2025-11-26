using System;
using System.Collections.Generic;
using MyTestTask.Abstraction.View;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MyTestTask.Misc.View
{
    public class SelectionProvider : MonoBehaviour
    {
        [SerializeField]
        
        private readonly HashSet<ISelectable> _selectables = new();
        private ISelectable _selected;
        public ISelectable Selected => _selected;
        public event Action<ISelectable> OnSelectionChanged;
        private LinkedList<ISelectable> _elements = new();
        
        public void Register(ISelectable selectable)
        {
            _selectables.Add(selectable);
            if (_elements.Count == 0)
            {
                _elements.AddFirst(selectable);
                return;
            }
            var pointer = _elements.First;
            while (pointer != null)
            {
                if (pointer.Value.Order < selectable.Order)
                {
                    var next = pointer.Next;
                    _elements.AddAfter(pointer, selectable);
                    selectable.SetPrevNeighbour(pointer.Value);
                    pointer.Value.SetNextNeighbour(selectable);
                    if(next != null)
                    {
                        selectable.SetNextNeighbour(next.Value);
                        next.Value.SetPrevNeighbour(selectable);
                    }
                    return;
                }
                pointer = pointer.Next;
            }
        }

        public void Unregister(ISelectable selectable)
        {
            _selectables.Remove(selectable);
            if(_selected == selectable)
            {
                SetSelected(null);
            }
            
            var element = _elements.Find(selectable);
            if(element == null) return;
            
            var prev = element.Previous;
            var next = element.Next;
            if (next != null)
            {
                next.Value.SetPrevNeighbour(prev?.Value);
            }
            if(prev != null)
            {
                prev.Value.SetNextNeighbour(next?.Value);
            }
            
            _elements.Remove(selectable);
        }
        
        public void SetSelected(ISelectable selectable)
        {
            if(_selected != null)
            {
                _selected.IsSelected = false;
            }
            _selected = selectable;
            if(_selected != null)
            {
                _selected.IsSelected = true;
            }
            OnSelectionChanged?.Invoke(selectable);
        }
    }
}