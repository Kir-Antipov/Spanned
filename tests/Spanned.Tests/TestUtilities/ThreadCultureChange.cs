using System.Globalization;

namespace Spanned.Tests.TestUtilities;

internal sealed class ThreadCultureChange : IDisposable
{
    private readonly CultureInfo _origCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _origUICulture = CultureInfo.CurrentUICulture;

    public ThreadCultureChange(string? cultureName)
        : this(cultureName != null ? new CultureInfo(cultureName) : null)
    {
    }

    public ThreadCultureChange(CultureInfo? newCulture)
        : this(newCulture, null)
    {
    }

    public ThreadCultureChange(CultureInfo? newCulture, CultureInfo? newUICulture)
    {
        if (newCulture is not null)
        {
            _origCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = newCulture;
        }

        if (newUICulture is not null)
        {
            _origUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = newUICulture;
        }
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _origCulture;
        CultureInfo.CurrentUICulture = _origUICulture;
    }
}
