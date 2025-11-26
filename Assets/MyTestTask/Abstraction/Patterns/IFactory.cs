namespace MyTestTask.Abstraction.Patterns
{
    public interface IFactory<out T>
    {
        T Create();
    }
}