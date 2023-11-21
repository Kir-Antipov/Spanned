using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Spanned.Tests.Spans;

public class UnsafeCastTests
{
    [Fact]
    public void UnsafeCast_UIntToUShort()
    {
        Span<uint> source = BitConverter.IsLittleEndian ? [0x44332211, 0x88776655] : [0x22114433, 0x66558877];
        Span<ushort> sourceAsUShort = source.UnsafeCast<uint, ushort>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<uint, ushort>(ref MemoryMarshal.GetReference(source)), ref MemoryMarshal.GetReference(sourceAsUShort)));
        Assert.Equal([0x2211, 0x4433, 0x6655, 0x8877], sourceAsUShort.ToArray());

        // ---------------------------------

        ReadOnlySpan<uint> readOnlySource = BitConverter.IsLittleEndian ? [0x44332211, 0x88776655] : [0x22114433, 0x66558877];
        ReadOnlySpan<ushort> readOnlySourceAsUShort = readOnlySource.UnsafeCast<uint, ushort>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<uint, ushort>(ref MemoryMarshal.GetReference(readOnlySource)), ref MemoryMarshal.GetReference(readOnlySourceAsUShort)));
        Assert.Equal([0x2211, 0x4433, 0x6655, 0x8877], readOnlySourceAsUShort.ToArray());
    }

    struct EmptyStruct { }

    [Fact]
    public void UnsafeCast_UIntToEmptyStruct()
    {
        Span<uint> source = new uint[] { 1 };
        Span<EmptyStruct> sourceAsEmptyStruct = source.UnsafeCast<uint, EmptyStruct>();

        Assert.Equal(1, Unsafe.SizeOf<EmptyStruct>());
        Assert.Equal(sizeof(uint), sourceAsEmptyStruct.Length);

        // ---------------------------------

        ReadOnlySpan<uint> readOnlySource = new uint[] { 1 };
        ReadOnlySpan<EmptyStruct> readOnlySourceAsEmptyStruct = readOnlySource.UnsafeCast<uint, EmptyStruct>();

        Assert.Equal(1, Unsafe.SizeOf<EmptyStruct>());
        Assert.Equal(sizeof(uint), readOnlySourceAsEmptyStruct.Length);
    }

    [Fact]
    public void UnsafeCast_ShortToLong()
    {
        Span<short> source = BitConverter.IsLittleEndian ? [0x1234, 0x2345, 0x3456, 0x4567, 0x5678] : [0x4567, 0x3456, 0x2345, 0x1234, 0x5678];
        Span<long> sourceAsLong = source.UnsafeCast<short, long>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<short, long>(ref MemoryMarshal.GetReference(source)), ref MemoryMarshal.GetReference(sourceAsLong)));
        Assert.Equal([0x4567345623451234], sourceAsLong.ToArray());

        // ---------------------------------

        ReadOnlySpan<short> readOnlySource = BitConverter.IsLittleEndian ? [0x1234, 0x2345, 0x3456, 0x4567, 0x5678] : [0x4567, 0x3456, 0x2345, 0x1234, 0x5678];
        ReadOnlySpan<long> readOnlySourceAsLong = readOnlySource.UnsafeCast<short, long>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<short, long>(ref MemoryMarshal.GetReference(readOnlySource)), ref MemoryMarshal.GetReference(readOnlySourceAsLong)));
        Assert.Equal([0x4567345623451234], readOnlySourceAsLong.ToArray());
    }

    [Fact]
    public void UnsafeCast_ReferenceTypeToReferenceType()
    {
        Span<Func<int>> source = [() => 42];
        Span<Delegate> sourceAsDelegate = source.UnsafeCast<Func<int>, Delegate>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<Func<int>, Delegate>(ref MemoryMarshal.GetReference(source)), ref MemoryMarshal.GetReference(sourceAsDelegate)));
        Assert.Equal(1, sourceAsDelegate.Length);
        Assert.Equal(42, sourceAsDelegate[0].DynamicInvoke());

        // ---------------------------------

        ReadOnlySpan<Func<int>> readOnlySource = [() => 42];
        ReadOnlySpan<Delegate> readOnlySourceAsDelegate = readOnlySource.UnsafeCast<Func<int>, Delegate>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<Func<int>, Delegate>(ref MemoryMarshal.GetReference(readOnlySource)), ref MemoryMarshal.GetReference(readOnlySourceAsDelegate)));
        Assert.Equal(1, readOnlySourceAsDelegate.Length);
        Assert.Equal(42, readOnlySourceAsDelegate[0].DynamicInvoke());
    }

    [Fact]
    public void UnsafeCast_ReferenceTypeToNInt()
    {
        Span<string> source = ["Zebra"];
        Span<nint> sourceAsNInt = source.UnsafeCast<string, nint>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<string, nint>(ref MemoryMarshal.GetReference(source)), ref MemoryMarshal.GetReference(sourceAsNInt)));
        Assert.Equal(1, sourceAsNInt.Length);
        Assert.NotEqual(0, sourceAsNInt[0]);

        // ---------------------------------

        ReadOnlySpan<string> readOnlySource = ["Zebra"];
        ReadOnlySpan<nint> readOnlySourceAsNInt = readOnlySource.UnsafeCast<string, nint>();

        Assert.True(Unsafe.AreSame(ref Unsafe.As<string, nint>(ref MemoryMarshal.GetReference(readOnlySource)), ref MemoryMarshal.GetReference(readOnlySourceAsNInt)));
        Assert.Equal(1, readOnlySourceAsNInt.Length);
        Assert.NotEqual(0, readOnlySourceAsNInt[0]);
    }
}
