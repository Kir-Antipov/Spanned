using System.Numerics;

namespace Spanned.Tests.TestUtilities;

public static class Shuffler
{
    public static T[] Shuffle<T>(T[] array)
    {
        new Random(42).Shuffle(array);
        return array;
    }

    public static T[] Shuffle<T>(this IEnumerable<T> enumerable) => Shuffle(enumerable.ToArray());

    public static U[] Shuffle<T, U>(this IEnumerable<T> enumerable, Func<T, U> selector) => Shuffle(enumerable.Select(selector).ToArray());

    public static T[] Range<T>(T start, int count) where T : INumberBase<T>
    {
        T[] array = new T[count];

        if (array.Length != 0)
            array[0] = start;

        for (int i = 1; i < count; i++)
        {
            array[i] = array[i - 1] + T.One;
        }

        return Shuffle(array);
    }

    public static string[] Range(string start, int count)
    {
        string[] array = new string[count];

        if (array.Length != 0)
            array[0] = start;

        for (int i = 1; i < count; i++)
        {
            string previous = array[i - 1];
            array[i] = previous.Substring(0, previous.Length - 1) + (char)(previous[^1] + 1);
        }

        return Shuffle(array);
    }
}
