using UnityEngine;

namespace MyTestTask.Abstraction.View.Layout
{
    public interface IDataListFlexibleElement<T> : IFlexibleLayoutElement, IDataViewOnComponent<T>
    {
        void SetPositionAsFirst();
        void SetPositionAfter(IDataListFlexibleElement<T> element);
        void SetPositionBefore(IDataListFlexibleElement<T> element);
        void AddPositionCorrection(float correction);
        bool IsBelowThen(Vector3 worldPoint);
        bool IsAboveThen(Vector3 worldPoint);
    }
}