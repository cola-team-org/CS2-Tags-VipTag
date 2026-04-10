using System.Collections.Concurrent;

using VipTags.Models;

namespace VipTags.Managers;

public sealed class PlayerModelCache
{
    private readonly ConcurrentDictionary<ulong, TagSettings> _players = new();

    public IEnumerable<TagSettings> Players => _players.Values;

    public TagSettings? Get(ulong steamId) => _players.GetValueOrDefault(steamId);

    public TagSettings Set(ulong steamId, TagSettings tagSettings)
    {
        return _players[steamId] = tagSettings;
    }

    public void Clear(ulong steamId)
    {
        _players.TryRemove(steamId, out _);
    }
}