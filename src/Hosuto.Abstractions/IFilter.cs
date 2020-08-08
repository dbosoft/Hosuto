using System;

namespace Dbosoft.Hosuto
{
    public interface IFilter<T1>
    {
        Action<T1> Invoke(Action<T1> next);
    }

    public interface IFilter<T1, T2>
    {
        Action<T1, T2> Invoke(Action<T1, T2> next);
    }
}
