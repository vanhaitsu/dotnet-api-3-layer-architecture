using System.Reflection;
using System.Text;

namespace Services.Utils;

public static class CacheTools
{
    public static string GenerateCacheKey(object obj)
    {
        var cacheKey = new StringBuilder();

        // Get all properties of the object
        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(obj, null) ?? "null";
            cacheKey.Append($"{property.Name}:{value};");
        }

        // Get all fields of the object
        foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = field.GetValue(obj) ?? "null";
            cacheKey.Append($"{field.Name}:{value};");
        }

        return cacheKey.ToString();
    }
}