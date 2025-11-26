using System;

namespace MyTestTask.Abstraction.View.Layout
{
    public interface IFlexibleLayoutElement
    {
        float Size { get; }
        event Action<IFlexibleLayoutElement, float> OnSizeChanged;
    }
}