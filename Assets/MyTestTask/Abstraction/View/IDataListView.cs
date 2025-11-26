using MyTestTask.Abstraction.Model;

namespace MyTestTask.Abstraction.View
{
    public interface IDataListView
    {
    }

    public interface IDataListView<in T> : IDataListView where T : IDataSource
    {
        void SetDataSource(T dataSource);
    }
}