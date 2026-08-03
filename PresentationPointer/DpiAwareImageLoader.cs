using System.Reflection;

namespace PresentationPointer;

internal static class DpiAwareImageLoader
{
    public static Image? Load(Assembly assembly, string baseResourceName, Control? dpiSource = null)
    {
        float dpiScale = GetDpiScale(dpiSource);

        foreach (string candidate in GetCandidateResourceNames(baseResourceName, dpiScale))
        {
            using Stream? stream = assembly.GetManifestResourceStream(candidate);
            if (stream != null)
            {
                return Image.FromStream(stream);
            }
        }

        return null;
    }

    private static float GetDpiScale(Control? dpiSource)
    {
        try
        {
            if (dpiSource != null)
            {
                if (dpiSource.IsHandleCreated)
                {
                    return Math.Max(1f, dpiSource.DeviceDpi / 96f);
                }

                using Graphics graphics = dpiSource.CreateGraphics();
                return Math.Max(1f, graphics.DpiX / 96f);
            }
        }
        catch
        {
        }

        return 1f;
    }

    private static IEnumerable<string> GetCandidateResourceNames(string baseResourceName, float dpiScale)
    {
        string[] suffixes = dpiScale switch
        {
            >= 2.75f => ["@3x", "@2x", "@1.5x", string.Empty],
            >= 2.25f => ["@2x", "@3x", "@1.5x", string.Empty],
            >= 1.75f => ["@2x", "@1.5x", "@3x", string.Empty],
            >= 1.25f => ["@1.5x", "@2x", "@3x", string.Empty],
            _ => [string.Empty, "@1.5x", "@2x", "@3x"]
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string suffix in suffixes)
        {
            string candidate = AppendSuffix(baseResourceName, suffix);
            if (seen.Add(candidate))
            {
                yield return candidate;
            }
        }
    }

    private static string AppendSuffix(string resourceName, string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
        {
            return resourceName;
        }

        int extensionIndex = resourceName.LastIndexOf(".png", StringComparison.OrdinalIgnoreCase);
        return extensionIndex >= 0 ? resourceName.Insert(extensionIndex, suffix) : resourceName + suffix;
    }
}
