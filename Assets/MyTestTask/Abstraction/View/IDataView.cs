using UnityEngine;

namespace MyTestTask.Abstraction.View
{
    public interface IDataView
    {
    }

    public interface IDataView<T> : IDataView
    {
        T Data { get; }
        void SetData(T data);
        
        // we have to realize a Refresh method, but we wouldn't make any changes to the data
    }

    public interface IDataViewOnComponent<T> : IDataView<T>
    {
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get; }
        RectTransform RectTransform { get; }
    }
}