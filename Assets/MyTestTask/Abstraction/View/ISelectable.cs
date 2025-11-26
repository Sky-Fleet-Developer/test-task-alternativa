namespace MyTestTask.Abstraction.View
{
    public interface ISelectable
    {
        bool IsSelected { get; set; }
        int Order { get; }
        void SetPrevNeighbour(ISelectable prev);
        void SetNextNeighbour(ISelectable next);
    }
}