using System.Collections.Concurrent;

using VipTags.Models;

namespace VipTags.Managers;

public sealed class PlayerModelCache
{
    private readonly ConcurrentDictionary<ulong, PlayerModel> _players = new();

    public IEnumerable<PlayerModel> Players => _players.Values;

    public PlayerModel? Get(ulong steamId) => _players.GetValueOrDefault(steamId);

    public PlayerModel Set(ulong steamId, PlayerModel playerModel)
    {
        return _players[steamId] = playerModel;
    }

    public void Clear(ulong steamId)
    {
        _players.TryRemove(steamId, out _);
    }
}