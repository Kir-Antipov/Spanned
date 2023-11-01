namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Performs a vectorized search for the extremum in a memory block.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TExtremum">The extremum type.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <returns>The extremum value.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    private static T Extremum<T, TExtremum>(ref T searchSpace, int length)
        where T : struct, IBinaryNumber<T>
        where TExtremum : IExtremum<T>
    {
        Debug.Assert(Vector<T>.IsSupported);

        if (length == 0)
            ThrowHelper.ThrowInvalidOperationException_NoElements();

        // Note, we use `<=` instead of `<`, because in the end we need to
        // manually process every lane of the resulting vector.
        // Therefore, when `length == Vector.Count` a vectorized solution
        // would just end up performing unnecessary operations, becoming
        // slower than the regular loop without those.
        if (!Vector128.IsHardwareAccelerated || length <= Vector128<T>.Count)
        {
            ref T current = ref searchSpace;
            ref T end = ref Unsafe.Add(ref current, length);

            T extremumValue = current;
            current = ref Unsafe.Add(ref current, (nint)1);

            while (Unsafe.IsAddressLessThan(ref current, ref end))
            {
                if (TExtremum.Compare(current, extremumValue))
                    extremumValue = current;

                current = ref Unsafe.Add(ref current, (nint)1);
            }
            return extremumValue;
        }
        else if (!Vector256.IsHardwareAccelerated || length <= Vector256<T>.Count)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, (nint)Vector128<T>.Count);
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector128<T>.Count);
            Vector128<T> extremum = Vector128.LoadUnsafe(ref searchSpace);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                extremum = TExtremum.Compare(extremum, Vector128.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, (nint)Vector128<T>.Count);
            }
            extremum = TExtremum.Compare(extremum, Vector128.LoadUnsafe(ref lastVectorStart));

            T extremumValue = extremum[0];
            for (int i = 1; i < Vector128<T>.Count; i++)
            {
                if (TExtremum.Compare(extremum[i], extremumValue))
                    extremumValue = extremum[i];
            }
            return extremumValue;
        }
        else if (!Vector512.IsHardwareAccelerated || length <= Vector512<T>.Count)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, (nint)Vector256<T>.Count);
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector256<T>.Count);
            Vector256<T> extremum = Vector256.LoadUnsafe(ref searchSpace);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                extremum = TExtremum.Compare(extremum, Vector256.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, (nint)Vector256<T>.Count);
            }
            extremum = TExtremum.Compare(extremum, Vector256.LoadUnsafe(ref lastVectorStart));

            T extremumValue = extremum[0];
            for (int i = 1; i < Vector256<T>.Count; i++)
            {
                if (TExtremum.Compare(extremum[i], extremumValue))
                    extremumValue = extremum[i];
            }
            return extremumValue;
        }
        else
        {
            ref T current = ref Unsafe.Add(ref searchSpace, (nint)Vector512<T>.Count);
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector512<T>.Count);
            Vector512<T> extremum = Vector512.LoadUnsafe(ref searchSpace);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                extremum = TExtremum.Compare(extremum, Vector512.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, (nint)Vector512<T>.Count);
            }
            extremum = TExtremum.Compare(extremum, Vector512.LoadUnsafe(ref lastVectorStart));

            T extremumValue = extremum[0];
            for (int i = 1; i < Vector512<T>.Count; i++)
            {
                if (TExtremum.Compare(extremum[i], extremumValue))
                    extremumValue = extremum[i];
            }
            return extremumValue;
        }
    }

    /// <summary>
    /// Defines methods for comparing values or vectors of type <typeparamref name="T"/>
    /// to determine the extremum (minimum or maximum).
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    private interface IExtremum<T> where T : struct, IBinaryNumber<T>
    {
        /// <summary>
        /// Compares two values of type <typeparamref name="T"/> to determine which one
        /// qualifies as the extremum.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The value to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> qualifies as the extremum;
        /// otherwise, <c>false</c>.
        /// </returns>
        static abstract bool Compare(T left, T right);

        /// <summary>
        /// Computes the extremum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>A vector whose elements are the extremum of the corresponding elements in left and right.</returns>
        static abstract Vector128<T> Compare(Vector128<T> left, Vector128<T> right);

        /// <inheritdoc cref="Compare(Vector128{T}, Vector128{T})"/>
        static abstract Vector256<T> Compare(Vector256<T> left, Vector256<T> right);

        /// <inheritdoc cref="Compare(Vector128{T}, Vector128{T})"/>
        static abstract Vector512<T> Compare(Vector512<T> left, Vector512<T> right);
    }
}
