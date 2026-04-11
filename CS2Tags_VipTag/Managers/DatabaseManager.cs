using Dapper;

using Microsoft.Extensions.Logging;

using MySqlConnector;

using VipTags.Models;

namespace VipTags.Managers;

public sealed class DatabaseManager(
    ILogger<DatabaseManager> logger,
    VipTagsPlugin plugin)
{
    public async Task InitializeConnection()
    {
        await using var connection = await CreateDbConnection();
        logger.LogInformation("Successfully connected to mysql database");

        string createTable = """
                             CREATE TABLE IF NOT EXISTS VipTags_Players
                             (
                                 SteamID   VARCHAR(255) NOT NULL,
                                 Tag       VARCHAR(50)
                                               CHARACTER SET utf8mb4
                                               COLLATE utf8mb4_unicode_ci,
                                 TagColor  VARCHAR(50)
                                               CHARACTER SET utf8mb4
                                               COLLATE utf8mb4_unicode_ci,
                                 NameColor VARCHAR(50)
                                               CHARACTER SET utf8mb4
                                               COLLATE utf8mb4_unicode_ci,
                                 ChatColor VARCHAR(50)
                                               CHARACTER SET utf8mb4
                                               COLLATE utf8mb4_unicode_ci,
                                 PRIMARY KEY (SteamID)
                             )
                                 ENGINE = InnoDB
                                 DEFAULT CHARSET = utf8mb4
                                 COLLATE = utf8mb4_unicode_ci;
                             """;
        await connection.QueryFirstOrDefaultAsync(createTable);
        logger.LogInformation("Ensured database has been setup");
    }

    public async Task SaveTags(TagSettings settings)
    {
        logger.LogInformation("Saving tag settings for {SteamId}", settings.SteamId);
        await using var connection = await CreateDbConnection();
        await SaveTagsInternal(connection, settings);
    }

    public async Task DeleteTags(ulong steamId)
    {
        logger.LogInformation("Deleting tag settings for {SteamId}", steamId);

        await using var connection = await CreateDbConnection();
        await connection.ExecuteAsync("DELETE FROM VipTags_Players WHERE SteamID = @steamId", new { steamId });
    }

    public async Task<TagSettings?> FetchPlayerInfo(ulong steamId)
    {
        await using var connection = await CreateDbConnection();
        var user = await connection.QueryFirstOrDefaultAsync<TagSettings>(
            "SELECT * FROM `VipTags_Players` WHERE `SteamID` = @steamId",
            new { steamId });

        if (user is null)
        {
            logger.LogInformation("Player with {SteamID} was not found", steamId);
        }
        else
        {
            logger.LogInformation("Player with {SteamID} was found", steamId);
        }

        return user;
    }

    private async Task<MySqlConnection> CreateDbConnection()
    {
        if (plugin.Config.DbHost.Length < 1 || plugin.Config.DbName.Length < 1 || plugin.Config.DbPassword.Length < 1 ||
            plugin.Config.DbUsername.Length < 1)
        {
            throw new InvalidOperationException("You need to setup a mysql database!");
        }

        MySqlConnectionStringBuilder builder = new()
        {
            Server = plugin.Config.DbHost,
            UserID = plugin.Config.DbUsername,
            Port = plugin.Config.DbPort,
            Password = plugin.Config.DbPassword,
            Database = plugin.Config.DbName,
            CharacterSet = "utf8mb4"
        };

        var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task SaveTagsInternal(MySqlConnection connection, TagSettings settings)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`)
            VALUES (@SteamId, @Tag, @TagColor, @NameColor, @ChatColor)
            ON DUPLICATE KEY UPDATE `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, `ChatColor` = @ChatColor
            """,
            settings);
    }
}