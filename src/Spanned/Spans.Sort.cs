namespace Spanned;

public static partial class Spans
{
    internal static void Sort<T>(this Span<T> span, Comparison<T> comparison)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(comparison);

        if (span.Length < 2)
            return;

        QuickSort(span, comparison, 0, span.Length - 1);
    }

    internal static void Sort<T>(this Span<T> span, IComparer<T>? comparer = null)
    {
        if (span.Length < 2)
            return;

        QuickSort(span, comparer ?? Comparer<T>.Default, 0, span.Length - 1);
    }

    private static void QuickSort<T>(Span<T> span, IComparer<T> comparer, int leftIndex, int rightIndex)
    {
        int i = leftIndex;
        int j = rightIndex;
        T pivot = span[leftIndex];

        while (i <= j)
        {
            while (comparer.Compare(span[i], pivot) < 0)
            {
                i++;
            }

            while (comparer.Compare(span[j], pivot) > 0)
            {
                j--;
            }

            if (i <= j)
            {
                (span[j], span[i]) = (span[i], span[j]);

                i++;
                j--;
            }
        }

        if (leftIndex < j)
            QuickSort(span, comparer, leftIndex, j);

        if (i < rightIndex)
            QuickSort(span, comparer, i, rightIndex);
    }

    private static void QuickSort<T>(Span<T> span, Comparison<T> comparison, int leftIndex, int rightIndex)
    {
        int i = leftIndex;
        int j = rightIndex;
        T pivot = span[leftIndex];

        while (i <= j)
        {
            while (comparison(span[i], pivot) < 0)
            {
                i++;
            }

            while (comparison(span[j], pivot) > 0)
            {
                j--;
            }

            if (i <= j)
            {
                (span[j], span[i]) = (span[i], span[j]);

                i++;
                j--;
            }
        }

        if (leftIndex < j)
            QuickSort(span, comparison, leftIndex, j);

        if (i < rightIndex)
            QuickSort(span, comparison, i, rightIndex);
    }
}
