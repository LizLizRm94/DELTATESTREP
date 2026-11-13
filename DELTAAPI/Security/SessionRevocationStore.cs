using System.Collections.Concurrent;

namespace DELTAAPI.Security;

/// <summary>
/// Mantiene en memoria los identificadores de sesión revocados (logout).
/// La cookie se elimina en el cliente, pero este registro impide reutilizar
/// una cookie capturada después del cierre de sesión.
/// </summary>
public sealed class SessionRevocationStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revoked = new(StringComparer.Ordinal);

    public void Revoke(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        _revoked[sessionId.Trim()] = DateTimeOffset.UtcNow;
    }

    public bool IsRevoked(string? sessionId)
    {
        return !string.IsNullOrWhiteSpace(sessionId) && _revoked.ContainsKey(sessionId.Trim());
    }

    /// <summary>
    /// Elimina entradas antiguas para que el diccionario no crezca indefinidamente.
    /// </summary>
    public void RemoveOlderThan(TimeSpan age)
    {
        var limit = DateTimeOffset.UtcNow - age;
        foreach (var entry in _revoked)
        {
            if (entry.Value < limit)
                _revoked.TryRemove(entry.Key, out _);
        }
    }
}
