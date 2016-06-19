using System;
using System.Collections.Generic;

public static class Extensions {
    public static void Insert<T>(this List<T> list, Tuple<int, T> t)
    {
        list.Insert(t.Item1, t.Item2);
    }
}  