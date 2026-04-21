using System;

public static class EventBus<T> where T : struct
{
    static Action<T> _handlers;

    public static void Subscribe(Action<T> handler)   => _handlers += handler;
    public static void Unsubscribe(Action<T> handler) => _handlers -= handler;
    public static void Raise(T evt)                   => _handlers?.Invoke(evt);
}
