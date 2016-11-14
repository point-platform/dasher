namespace Dasher.Schema
{
    internal static class EmptyArray<T>
    {
        public static T[] Instance { get; } = new T[0];
    }
}