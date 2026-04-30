using System.Text.Json;

namespace KidSafeApp.Helpers;

/// <summary>
/// Provides JSON serialization and deserialization helpers.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Gets the default JSON serializer options with camelCase naming policy.
    /// </summary>
    public static JsonSerializerOptions DefaultOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">Optional serializer options.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string Serialize(object value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional serializer options.</param>
    /// <returns>The deserialized object, or default if deserialization fails.</returns>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }
}