# Spanned

[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/Kir-Antipov/Spanned/build.yml?style=flat&logo=github&cacheSeconds=3600)](https://github.com/Kir-Antipov/Spanned/actions/workflows/build.yml)
[![Version](https://img.shields.io/github/v/release/Kir-Antipov/Spanned?sort=date&style=flat&label=version&cacheSeconds=3600)](https://github.com/Kir-Antipov/Spanned/releases/latest)
[![License](https://img.shields.io/github/license/Kir-Antipov/Spanned?style=flat&cacheSeconds=36000)](https://github.com/Kir-Antipov/Spanned/blob/HEAD/LICENSE.md)

<img alt="Spanned Icon" src="https://raw.githubusercontent.com/Kir-Antipov/Spanned/HEAD/media/icon.png" width="128">

`Spanned` is a high-performance, zero-allocation .NET library that introduces span-compatible alternatives to popular BCL types and provides vectorized solutions for common operations on spans.

----

## Getting Started

### Installation

To get started, first add the [Spanned](https://nuget.org/packages/Spanned) package to your project. You can do this by running the following command:

```sh
dotnet add package Spanned
```

Alternatively, you can install it via the Package Manager Console with this command:

```sh
Install-Package Spanned
```

Note that .NET has accumulated numerous optimization routes over time, both framework- and runtime-dependent. Consequently, it has become exceedingly challenging to neighbor highly optimized code for different frameworks in a single codebase. Therefore, I made a difficult decision to support only one framework per version of this library. You can find a list of library versions and the corresponding supported framework versions in the table below:

|   | **.NET Standard 2.0** | **.NET Standard 2.1** | **.NET 8+** |
|:--|:---------------------:|:---------------------:|:-----------:|
| [![Spanned](https://img.shields.io/nuget/v/Spanned?style=flat&logo=nuget&label=Spanned&cacheSeconds=3600)](https://nuget.org/packages/Spanned/ "Download Spanned from NuGet.org") | ❌ | ❌ | ✅ |
| [![Spanned v0.1.0](https://img.shields.io/badge/Spanned-v0.1.0-blue?style=flat&logo=nuget&cacheSeconds=3600)](https://nuget.org/packages/Spanned/0.1.0 "Download Spanned v0.1.0 from NuGet.org") | ❌ | ✅ | ⚠️ |
| [![Spanned v0.0.1](https://img.shields.io/badge/Spanned-v0.0.1-blue?style=flat&logo=nuget&cacheSeconds=3600)](https://nuget.org/packages/Spanned/0.0.1 "Download Spanned v0.0.1 from NuGet.org") | ✅ | ⚠️ | ⚠️ |

 - ✅ - Fully supported with the best performance possible.
 - ⚠️ - Partially supported with noticeably degraded performance compared to fully supported alternatives. Can be used as a polyfill.
 - ❌ - Not supported.

While it may be tempting to use `v0.0.1` for all your needs, given its support for .NET Standard 2.0 and the widespread adoption of .NET Standard 2.0 nowadays, I strongly recommend against doing so. There are virtually no optimizations one can perform using this legacy framework. Even our beloved `Span`, accessible in .NET Standard 2.0 via the `System.Memory` NuGet package, is known as "slow Span", because that `Span` is nothing more than a re-invented `ArraySegment`, lacking proper support on the Runtime/JIT side. Therefore, please choose the best package version for your environment, not the one that seems to fit them all.

### Usage Guidelines

Before we get into the details, let's discuss some common edge cases you may encounter while using this library, and answer the question: "Why isn't X part of .NET?" In short, everything you can find here is both easy to use **and** easy to misuse.

#### Premature Optimization

Let's begin with an obvious point. This library is designed specifically for scenarios where you fight tooth and nail for every allocated byte and every nanosecond of execution time in highly critical paths. It's not meant to be a one-size-fits-all solution for your entire codebase. Mindlessly incorporating types from this library in every conceivable scenario may degrade the overall performance of your application, rather than enhancing it.

Remember, don't dive head first into the nano-optimization ocean until you're sure it's necessary.

#### ValueTypes as Parameters

A common mistake to avoid is passing any of the types provided by this library by value *(yeah, this alone should help you understand why something like this should not and will never be a part of the BCL)*. For instance, while the following code may appear fine, it's actually disastrous:

```csharp
ValueStringBuilder sb = new ValueStringBuilder(16);
sb.AppendUserName(user);

// ...

public static void AppendUserName(this ValueStringBuilder sb, User user)
{
    sb.Append(user.FirstName);
    sb.Append(' ');
    sb.Append(user.LastName);
}
```

Appending to a string builder may enlarge its internal buffer. However, since we passed our `ValueStringBuilder` by value *(i.e., copied it)*, the original `ValueStringBuilder` won't be aware of it and will continue to use an already disposed buffer.

While this approach may seem to work with sanitized inputs during testing, it will occasionally fail, breaking not only your code but also some random parts of your app's runtime by interfering with a buffer that a copy of `ValueStringBuilder` has already returned to the pool, so it can be reused by something else.

You might try to be smart about it and address the issue by re-writing the problematic extension method as follows:

```csharp
public static void AppendUserName(this in ValueStringBuilder sb, User user)
{
    sb.Append(user.FirstName);
    sb.Append(' ');
    sb.Append(user.LastName);
}
```

Now, `ValueStringBuilder` is passed by reference, so there should be no problems, right? Well, no. The `in` modifier exists to reduce the costs of copying the entirety of a value type instance by passing its reference to a method while preserving the semantics as if it was passed by value. This means any state modifications of the provided `ValueStringBuilder` won't be propagated to the original instance. So, we still have the same problem on our hands. The only correct way to implement a method that may modify a value type instance's internal state is by actually passing it by reference:

```csharp
ValueStringBuilder sb = new ValueStringBuilder(16);
AppendUserName(ref sb, user);

// ...

public static void AppendUserName(ref ValueStringBuilder sb, User user)
{
    sb.Append(user.FirstName);
    sb.Append(' ');
    sb.Append(user.LastName);
}
```

While not as fancy as some would like it to be, this solution has the benefit of actually working.

#### Dispose Pattern

Most of the types provided by this library define a `Dispose()` method, enabling their use with the `using` keyword, as can be seen below:

```csharp
using ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
Foo(ref sb);
return sb.ToString();
```

However, this doesn't mean that they **should** be used with the `using` keyword. It's quite important to remember how the code above is actually lowered:

```csharp
ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);

try
{
    Foo(ref sb);
    return sb.ToString();
}
finally
{
    sb.Dispose();
}
```

Creating and managing protected regions is not free. Considering our focus on nano-optimizations, the impact here is noticeable. Therefore, it's preferable to manually call `Dispose()`:

```csharp
ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
Foo(ref sb);
string result = sb.ToString();

sb.Dispose();
return result;
```

Alternatively, check if the last method you call on a given type has an overload that performs cleanup automatically:

```csharp
ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
Foo(ref sb);
return sb.ToString(dispose: true);
```

### Usage

#### SpanOwner

In modern .NET, it's common to encounter the following pattern:

 - If data length is less than some arbitrary value, allocate a buffer on the stack.
 - Otherwise, rent it from the pool.
 - Use the buffer for data processing.
 - In case we rented the buffer from the pool, return it.

Or, expressing the same concept in code:

```csharp
const int StackAllocByteLimit = 1024;

T[]? spanSource;
scoped Span<T> span;

if (sizeof(T) * length > StackAllocByteLimit)
{
    spanSource = ArrayPool<T>.Shared.Rent(length);
    span = spanSource.AsSpan(0, length);
}
else
{
    spanSource = null;
    span = stackalloc T[length];
}

DoSomeWorkWithSpan(span);

if (spanSource is not null)
{
    ArrayPool<T>.Shared.Return(spanSource);
}
```

Not the prettiest piece of boilerplate, is it? The actual logic often ends up buried by it, which is far from ideal. This is the exact problem `SpanOwner` aims to solve. Here's the same logic, but all the boilerplate has been hidden behind the `SpanOwner`:

```csharp
SpanOwner<T> owner = SpanOwner<T>.ShouldRent(length) ? SpanOwner<T>.Rent(length) : stackalloc T[length];
Span<T> span = owner.Span;

DoSomeWorkWithSpan(span);

owner.Dispose();
```

Much easier to write, much easier to read, and, most importantly, this approach provides the exact same performance because `SpanOwner` is designed to be fully inlinable. It can be completely eliminated from your code by JIT:

| Method                  | Mean     | StdDev    | Ratio | Code Size |
|------------------------ |---------:|----------:|------:|----------:|
| Without_SpanOwner_Int32 | 5.134 ns | 0.0425 ns |  1.00 |     315 B |
| With_SpanOwner_Int32    | 4.908 ns | 0.0168 ns |  0.96 |     310 B |

#### ValueStringBuilder

`ValueStringBuilder` is a re-implementation of `StringBuilder` designed to support stack-allocated buffers. It's capable of utilizing a shared array pool to expand its internal buffer when necessary. `ValueStringBuilder` is perfect for building compact strings that can fit on the stack; however, it shouldn't be used for anything else, because operations on larger strings may and will degrade the overall performance of your app. The true brilliance of `ValueStringBuilder` emerges when you need to create a short character sequence that doesn't need to be materialized as a string at all.

`ValueStringBuilder` mirrors all the features of `StringBuilder` and successfully passes the same set of unit tests, allowing it to seamlessly serve as a drop-in replacement in most scenarios.

```csharp
// Note that providing a capacity instead of a buffer will force
// the builder to rent an array from `ArrayPool<char>.Shared`.
ValueStringBuilder sb = new(stackalloc char[256]);

// `ValueStringBuilder` provides a custom interpolated string handler,
// ensuring such operations do not allocate any new strings.
sb.Append($"Hello, {user.Name}! Your ID is: {user.id}");

// Unlike `StringBuilder`, `ValueStringBuilder` can be represented
// as a readonly span. Thus, you don't need to actually materialize
// the string you've built in lots of cases.
DisplayWelcome((ReadOnlySpan<char>)sb);

// Remember to dispose of the builder to return
// a rented buffer, if any, back to the pool.
sb.Dispose();
```

#### ValueList

`ValueList<T>` is a re-implementation of `List<T>` designed to support stack-allocated buffers. It's capable of utilizing a shared array pool to expand its internal buffer when necessary. `ValueList<T>` is perfect for processing small amounts of data that can fit on the stack; however, it shouldn't be used for anything else, because operations on larger datasets may and will degrade the overall performance of your app.

`ValueList<T>` mirrors all the features of `List<T>` and successfully passes the same set of unit tests, allowing it to seamlessly serve as a drop-in replacement in most scenarios.

```csharp
// Note that providing a capacity instead of a buffer will force
// the list to rent an array from `ArrayPool<T>.Shared`.
ValueList<int> list = new(stackalloc int[10]);

list.Add(0);
list.Add(1);
list.Add(2);

DoSomethingWithIntegers((ReadOnlySpan<int>)list);

// Remember to dispose of the list to return
// a rented buffer, if any, back to the pool.
list.Dispose();
```

#### ValueStack

`ValueStack<T>` is a re-implementation of `Stack<T>` designed to support stack-allocated buffers. It's capable of utilizing a shared array pool to expand its internal buffer when necessary. `ValueStack<T>` is perfect for processing small amounts of data that can fit on the stack; however, it shouldn't be used for anything else, because operations on larger datasets may and will degrade the overall performance of your app.

`ValueStack<T>` mirrors all the features of `Stack<T>` and successfully passes the same set of unit tests, allowing it to seamlessly serve as a drop-in replacement in most scenarios.

```csharp
// Note that providing a capacity instead of a buffer will force
// the stack to rent an array from `ArrayPool<T>.Shared`.
ValueStack<int> stack = new(stackalloc int[10]);

stack.Push(0);
stack.Push(1);
stack.Push(2);

DoSomethingWithIntegers((ReadOnlySpan<int>)stack);

// Remember to dispose of the stack to return
// a rented buffer, if any, back to the pool.
stack.Dispose();
```

#### ValueQueue

`ValueQueue<T>` is a re-implementation of `Queue<T>` designed to support stack-allocated buffers. It's capable of utilizing a shared array pool to expand its internal buffer when necessary. `ValueQueue<T>` is perfect for processing small amounts of data that can fit on the stack; however, it shouldn't be used for anything else, because operations on larger datasets may and will degrade the overall performance of your app.

`ValueQueue<T>` mirrors all the features of `Queue<T>` and successfully passes the same set of unit tests, allowing it to seamlessly serve as a drop-in replacement in most scenarios.

```csharp
// Note that providing a capacity instead of a buffer will force
// the queue to rent an array from `ArrayPool<T>.Shared`.
ValueQueue<int> queue = new(stackalloc int[10]);

queue.Enqueue(0);
queue.Enqueue(1);
queue.Enqueue(2);

DoSomethingWithIntegers((ReadOnlySpan<int>)queue);

// Remember to dispose of the queue to return
// a rented buffer, if any, back to the pool.
queue.Dispose();
```

#### ValueSet

`ValueSet<T>` is a re-implementation of `HashSet<T>` designed to support stack-allocated buffers. It's capable of utilizing a shared array pool to expand its internal buffer when necessary. `ValueSet<T>` is perfect for processing small amounts of data that can fit on the stack; however, it shouldn't be used for anything else, because operations on larger datasets may and will degrade the overall performance of your app.

`ValueSet<T>` mirrors all the features of `HashSet<T>` and successfully passes the same set of unit tests, allowing it to seamlessly serve as a drop-in replacement in most scenarios.

```csharp
// Note that providing a capacity instead of a buffer will force
// the set to rent an array from `ArrayPool<T>.Shared`.
ValueSet<int> set = new(stackalloc int[10]);

set.Add(0);
set.Add(1);
set.Add(2);

DoSomethingWithIntegers((ReadOnlySpan<int>)set);

// Remember to dispose of the set to return
// a rented buffer, if any, back to the pool.
set.Dispose();
```

#### ValueDictionary

`ValueDictionary<TKey, TValue>` is a re-implementation of `Dictionary<TKey, TValue>` designed to support stack-allocated buffers. It's capable of utilizing a shared array pool to expand its internal buffer when necessary. `ValueDictionary<TKey, TValue>` is perfect for processing small amounts of data that can fit on the stack; however, it shouldn't be used for anything else, because operations on larger datasets may and will degrade the overall performance of your app.

`ValueDictionary<TKey, TValue>` mirrors all the features of `Dictionary<TKey, TValue>` and successfully passes the same set of unit tests, allowing it to seamlessly serve as a drop-in replacement in most scenarios.

```csharp
// Note that providing a capacity instead of a buffer will force
// the dictionary to rent an array from `ArrayPool<T>.Shared`.
ValueDictionary<int, string> dictionary = new(10);

dictionary.Add(0, "zero");
dictionary.Add(1, "one");
dictionary.Add(2, "two");

DoSomethingWithPairs((ReadOnlySpan<KeyValuePair<int, string>>)dictionary);

// Remember to dispose of the dictionary to return
// a rented buffer, if any, back to the pool.
dictionary.Dispose();
```

#### Spans.Min & Spans.Max

`.Min()` and `.Max()` are extension methods that can help you find the minimum/maximum value in a span. They are vectorized for all supported types, unlike `Enumerable.Min()` and `Enumerable.Max()`, which don't provide any optimizations for floating-point numbers.

However, there is a slight problem with floating-point numbers (i.e., `float` and `double`), and the name of this problem is `NaN`. As you may know, `NaN` is neither greater than nor less than any number, and it is not equal to any number, even to itself. Thus, if a `NaN` is present in the provided sequence, it can disrupt a naive implementation that relies solely on the result of regular comparison operations. Thus, if a `NaN` is present in the provided sequence, it can disrupt a naive implementation that relies solely on the result of regular comparison operations. Therefore, not accounting for this bane of floating-point comparisons is not an option.

`Spanned` manages to employ all the `NaN`-related checks in a highly efficient manner, providing a significant performance boost over non-optimized solutions. However, the performance could be even better if we didn't need to account for `NaN`s. This is why `.UnsafeMin()` and `.UnsafeMax()` exist. These methods are specific to spans containing floating-point numbers, and they perform comparison operations without acknowledging the existence of `NaN`, eliminating all related checks. So, if you are absolutely sure a span of floating-point numbers is sanitized and cannot contain any `NaN`s, you can squeeze even more performance out of `.Min()` and `.Max()` operations.

While the difference between `.Min()` and `.UnsafeMin()` may not be very noticeable:

| Method                | Mean       | StdDev   | Ratio | Code Size |
|---------------------- |-----------:|---------:|------:|----------:|
| Min_Loop_Single       | 3,919.5 ns | 15.75 ns |  1.00 |     207 B |
| Min_Linq_Single       | 4,030.3 ns | 37.38 ns |  1.03 |     570 B |
| Min_Span_Single       |   611.1 ns |  8.55 ns |  0.16 |     534 B |
| UnsafeMin_Span_Single |   569.0 ns |  1.82 ns |  0.15 |     319 B |

The performance gap becomes quite substantial between `.Max()` and `.UnsafeMax()`:

| Method                | Mean       | StdDev   | Ratio | Code Size |
|---------------------- |-----------:|---------:|------:|----------:|
| Max_Loop_Single       | 3,849.2 ns | 36.97 ns |  1.00 |     215 B |
| Max_Linq_Single       | 3,936.4 ns | 53.51 ns |  1.02 |     643 B |
| Max_Span_Single       |   901.7 ns |  7.12 ns |  0.23 |     606 B |
| UnsafeMax_Span_Single |   551.8 ns |  3.06 ns |  0.14 |     321 B |

#### Spans.Sum

`.Sum()` is an extension method that can help you compute the sum of all values in a span. It is vectorized for all supported types, unlike `Enumerable.Sum()`, which not just lacks vectorization, but doesn't provide overloads for most numeric types out of the box at all.

Similar to `.Min()` and `.Max()`, `.Sum()` has the evil twin that goes by the name `.UnsafeSum()`. The base method will throw an `OverflowException` if the sum computation results in integer overflow/underflow. Overflow guards, of course, come at a cost, and it's not a negligible one. Therefore, if your input is sanitized and cannot cause an overflow, or if integer overflow is the expected behavior in your working context, feel free to use `.UnsafeSum()`. It's twice as fast as `.Sum()`, 34 times faster than computing the sum within a loop, and casual 130 times faster than computing the sum via Linq:

| Method               | Mean        | StdDev    | Ratio | Code Size |
|--------------------- |------------:|----------:|------:|----------:|
| Sum_Loop_Int16       |  3,820.0 ns |   7.04 ns |  1.00 |     128 B |
| Sum_Linq_Int16       | 14,472.6 ns | 281.83 ns |  3.80 |     732 B |
| Sum_Span_Int16       |    214.6 ns |   2.43 ns |  0.06 |     413 B |
| UnsafeSum_Span_Int16 |    111.8 ns |   1.00 ns |  0.03 |     200 B |

#### Spans.LongSum

`.LongSum()` is an extension method that can help you compute the sum of all values in a span using a 64-bit accumulator *(i.e., `long` for signed integers, `ulong` for unsigned integers, and `double` for `float`)*, capable of storing a result larger than the maximum/minimum value of the original type *(e.g., you cannot store the mathematically correct result of `int.MaxValue + int.MaxValue` in a variable of type `int`)*. It is vectorized for all supported types and has no proper alternatives in Linq *(thus, the benchmark below is a little bit unfair)*.

`.LongSum()` doesn't have an "unsafe" counterpart, because even the largest possible span that stores `int.MaxValue` elements of type `int` cannot cause an overflow of a 64-bit accumulator (`(long)int.MaxValue * (long)int.MaxValue < long.MaxValue`).

| Method             | Mean        | StdDev    | Ratio | Code Size |
|------------------- |------------:|----------:|------:|----------:|
| LongSum_Loop_Int16 |  2,537.1 ns |  21.30 ns |  1.00 |      98 B |
| LongSum_Linq_Int16 | 14,372.0 ns | 130.00 ns |  5.67 |     734 B |
| LongSum_Span_Int16 |    251.0 ns |   2.38 ns |  0.10 |     390 B |

#### Spans.Average

`.Average()` is an extension method that can help you compute the average of all values in a span. It is vectorized for all supported types, unlike `Enumerable.Average()`, which only provides some level of optimization for 32-bit signed integers (i.e., `int`s).

Under the hood, `.Average()` uses `.LongSum()` to compute the sum of all elements while avoiding integer overflows. However, if your input is sanitized and cannot cause one, you can switch to `.UnsafeAverage()`, which uses `.UnsafeSum()` and does not spend the precious execution time on overflow guards.

| Method                   | Mean        | StdDev   | Ratio | Code Size |
|------------------------- |------------:|---------:|------:|----------:|
| Average_Loop_Int16       |  2,482.1 ns | 20.04 ns |  1.00 |     241 B |
| Average_Linq_Int16       | 13,198.2 ns | 97.67 ns |  5.31 |   1,016 B |
| Average_Span_Int16       |    257.8 ns |  3.61 ns |  0.10 |     593 B |
| UnsafeAverage_Span_Int16 |    116.7 ns |  1.27 ns |  0.05 |     128 B |

#### Spans.FillSequential

`.FillSequential()` is an extension method that can help you fill a given span with sequential numeric values. It is vectorized for all supported types and has no alternatives in Linq.

| Method                    | Mean       | StdDev   | Ratio | Code Size |
|-------------------------- |-----------:|---------:|------:|----------:|
| FillSequential_Loop_Int16 | 2,499.4 ns | 28.47 ns |  1.00 |     118 B |
| FillSequential_Span_Int16 |   169.2 ns |  0.18 ns |  0.07 |     660 B |

#### Spans.IndexOf & Spans.LastIndexOf & Spans.Contains

`.IndexOf()`, `.LastIndexOf()`, and `.Contains()` may seem familiar to you, because these methods are provided by `MemoryExtensions`. However, there are two problems with them:

 1) They require the input values to implement `IEquatable<T>`, making them inaccessible in the unbound generic context.
 2) They do not support custom `IEqualityComparer<T>` implementations.

The implementations of these methods provided by this library address both of these issues.

----

## License

Licensed under the terms of the [MIT License](https://github.com/Kir-Antipov/Spanned/blob/master/LICENSE.md).
