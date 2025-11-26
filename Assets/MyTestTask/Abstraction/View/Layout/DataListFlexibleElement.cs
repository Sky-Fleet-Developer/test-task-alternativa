using System;
using UnityEngine;

namespace MyTestTask.Abstraction.View.Layout
{
    public interface IDataListFlexibleElement<T> : IFlexibleLayoutElement, IDataViewOnComponent<T>
    {
        void SetPositionAsFirst();
        void SetPositionAfter(IDataListFlexibleElement<T> element);
        void SetPositionBefore(IDataListFlexibleElement<T> element);
        bool IsBelowThen(float worldPoint);
        bool IsAboveThen(float worldPoint);
        event Action<IDataListFlexibleElement<T>> OnSelected;
    }
}