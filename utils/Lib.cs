namespace utils;
public class Dict<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
    public override string ToString()
    {
        return $"{{{string.Join(", ", this.Select(x => $"{x.Key}={x.Value}"))}}}";
    }
}

public class Stck<TValue> : Stack<TValue> where TValue : notnull
{
    public Stck() : base() { }
    public Stck(IEnumerable<TValue> values) : base(values) { }

    public override string ToString()
    {
        return $"[{string.Join(",", this)}]";
    }
}

public class Lst<TValue> : List<TValue> where TValue : notnull
{
    public Lst() : base() { }
    public Lst(IEnumerable<TValue> collection) : base(collection) { }

    public Lst(int capacity) : base(capacity)
    {
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", this)}]";
    }
}

public static class Extensions
{
    public static T Also<T>(this T self, Action<T> action)
    {
        action(self);
        return self;
    }

    public static K Let<T, K>(this T self, Func<T, K> func)
    {
        return func(self);
    }

    public static Lst<T> ToLst<T>(this IEnumerable<T> self) where T : notnull
    {
        return new(self);
    }
}
