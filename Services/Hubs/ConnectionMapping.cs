namespace Services.Hubs;

public class ConnectionMapping<T> where T : notnull
{
    private static readonly Dictionary<T, HashSet<string>> Connections = new();

    public void Add(T key, string connectionId)
    {
        lock (Connections)
        {
            if (!Connections.TryGetValue(key, out var connections))
            {
                connections = new HashSet<string>();
                Connections.Add(key, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        lock (Connections)
        {
            if (Connections.TryGetValue(key, out var connections)) return connections;
        }

        return Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetConnections(List<T> keys)
    {
        lock (Connections)
        {
            var result = new HashSet<string>();
            foreach (var key in keys)
                if (Connections.TryGetValue(key, out var connections))
                    result.UnionWith(connections);

            return result;
        }
    }

    public void Remove(T key, string connectionId)
    {
        lock (Connections)
        {
            if (!Connections.TryGetValue(key, out var connections)) return;

            lock (connections)
            {
                connections.Remove(connectionId);
                if (!connections.Any()) Connections.Remove(key);
            }
        }
    }
}