using Dapper;

using Microsoft.Extensions.Logging;

using MySqlConnector;

using VipTags.Models;

namespace VipTags.Managers;

public sealed class DatabaseManager(
    ILogger<DatabaseManager> logger,
    VipTagsPlugin plugin,
    PlayerModelCache playerModelCache)
{
    public async Task InitializeConnection()
    {
        try
        {
            await using var connection = await CreateDbConnection();
            logger.LogInformation("Successfully connected to mysql database");

            string createTable = @"CREATE TABLE IF NOT EXISTS VipTags_Players (
                                    SteamID VARCHAR(255) NOT NULL,
                                    Tag VARCHAR(50)
                                        CHARACTER SET utf8mb4
                                        COLLATE utf8mb4_unicode_ci,
                                    TagColor VARCHAR(50)
                                        CHARACTER SET utf8mb4
                                        COLLATE utf8mb4_unicode_ci,
                                    NameColor VARCHAR(50)
                                        CHARACTER SET utf8mb4
                                        COLLATE utf8mb4_unicode_ci,
                                    ChatColor VARCHAR(50)
                                        CHARACTER SET utf8mb4
                                        COLLATE utf8mb4_unicode_ci,
                                    Visibility TINYINT(1),
                                    ChatVisibility TINYINT(1),
                                    ScoreVisibility TINYINT(1),
                                    PRIMARY KEY (SteamID)
                                )
                                ENGINE=InnoDB
                                DEFAULT CHARSET=utf8mb4
                                COLLATE=utf8mb4_unicode_ci;";
            await connection.QueryFirstOrDefaultAsync(createTable);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Error while trying to connect to database");
        }
    }

    private async Task<bool> UserExist(ulong steamId)
    {
        try
        {
            await using var connection = await CreateDbConnection();
            string sqlExists = "SELECT COUNT(1) FROM `VipTags_Players` WHERE `SteamID` = @SteamID";
            var exists = await connection.ExecuteScalarAsync<bool>(sqlExists, new { SteamID = steamId });
            logger.LogInformation("Player {SteamID} do exist", steamId);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "UserExists failed");
        }

        logger.LogInformation("Player {SteamID} does not exists", steamId);
        return false;
    }

    public async Task SaveTags(ulong steamId)
    {
        try
        {
            var userExists = await UserExist(steamId);
            await using var connection = await CreateDbConnection();
            var model = playerModelCache.Get(steamId) ?? throw new InvalidOperationException("Not player model");
            var parameters = new
            {
                SteamID = steamId,
                Tag = model.Tag,
                TagColor = model.TagColor ?? null,
                ChatColor = model.ChatColor ?? null,
                NameColor = model.NameColor ?? null,
                Visibility = model.Visibility ?? true,
                ChatVisibility = model.ChatVisibility ?? true,
                ScoreVisibility = model.ScoreVisibility ?? true,
            };
            if (userExists)
            {
                logger.LogInformation($"User exists! Updating tag!");
                string sqlUpdate =
                    "UPDATE `VipTags_Players` SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, `ChatColor` = @ChatColor, `Visibility` = @Visibility, `ChatVisibility` = @ChatVisibility, `ScoreVisibility` = @ScoreVisibility WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, parameters);
                return;
            }

            string sqlInsert =
                "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility, @ChatVisibility, @ScoreVisibility)";
            await connection.ExecuteAsync(sqlInsert, parameters);
        }
        catch (Exception err)
        {
            logger.LogInformation(err, "SaveTags failed");
        }

        return;
    }

    public async Task SaveAllTags()
    {
        try
        {
            await using var connection = await CreateDbConnection();
            foreach (var player in playerModelCache.Players)
            {
                var steamid = player.SteamId;
                var userExists = await UserExist(steamid);
                var parameters = new
                {
                    SteamID = steamid,
                    Tag = player.Tag,
                    TagColor = player.TagColor ?? null,
                    ChatColor = player.ChatColor ?? null,
                    NameColor = player.NameColor ?? null,
                    Visibility = player.Visibility ?? true,
                    ChatVis = player.ChatVisibility ?? true,
                    ScoreVis = player.ScoreVisibility ?? true
                };
                if (userExists)
                {
                    logger.LogInformation("Updating tag {SteamId}", steamid);
                    string sqlUpdate = @"
                    UPDATE `VipTags_Players`
                    SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, 
                        `ChatColor` = @ChatColor, `Visibility` = @Visibility, `ChatVisibility` = @ChatVis, `ScoreVisibility` = @ScoreVis
                    WHERE `SteamID` = @SteamID";
                    await connection.ExecuteAsync(sqlUpdate, parameters);
                }
                else
                {
                    logger.LogInformation("Inserting new tag {SteamId}", steamid);
                    string sqlInsert = @"
                    INSERT INTO `VipTags_Players` 
                    (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) 
                    VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility, @ChatVis, @ScoreVis)";
                    await connection.ExecuteAsync(sqlInsert, parameters);
                }
            }

            logger.LogInformation($"All players have been updated / inserted into DB");
        }
        catch (Exception err)
        {
            logger.LogInformation(err, "SaveAllTags failed");
        }
    }

    public async Task<TagSettings?> FetchPlayerInfo(ulong steamId)
    {
        await using var connection = await CreateDbConnection();
        // TODO: do in one call....
        var userExists = await UserExist(steamId);
        try
        {
            if (!userExists)
            {
                logger.LogInformation("No player in database with steamid: {SteamID}", steamId);
                return null;
            }

            string sqlSelect = $"SELECT * FROM `VipTags_Players` WHERE `SteamID` = {steamId}";
            var user = await connection.QueryFirstOrDefaultAsync<TagSettings>(sqlSelect);
            return user;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Fetchplayerinfo failed");
        }

        return null;
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
}