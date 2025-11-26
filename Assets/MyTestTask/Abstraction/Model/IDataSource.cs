using System.Collections.Generic;

namespace MyTestTask.Abstraction.Model
{
    public interface IDataSource
    {
        int Count { get; }
    }

    public interface IDataSource<out T> : IDataSource
    {
        IEnumerable<T> EnumerateRange(int start, int count);
        T Get(int index);
        // this could realize data changed events, but it's not necessary for this task
    }
}