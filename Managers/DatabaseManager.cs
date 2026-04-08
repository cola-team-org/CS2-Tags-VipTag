using CounterStrikeSharp.API.Core;

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
    private string _dbConnection = string.Empty;
    public async Task InitializeConnection()
    {
        var config = plugin.Config;
        if (config.DbHost.Length < 1 || config.DbName.Length < 1 || config.DbPassword.Length < 1 || config.DbUsername.Length < 1)
        {
            logger.LogInformation("You need to setup a mysql database!");
        }

        MySqlConnectionStringBuilder builder = new()
        {
            Server = config.DbHost,
            UserID = config.DbUsername,
            Port = config.DbPort,
            Password = config.DbPassword,
            Database = config.DbName,
            CharacterSet = "utf8mb4"
        };

        _dbConnection = builder.ConnectionString;

        try
        {
            var connection = new MySqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
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
            /*
            string createTable = @"CREATE TABLE IF NOT EXISTS VipTags_Players(
            SteamID VARCHAR(255) PRIMARY KEY,
            Tag VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
            TagColor VARCHAR(50),
            NameColor VARCHAR(50),
            ChatColor VARCHAR(50),
            Visibility TINYINT(1),
            ChatVisibility TINYINT(1),
            ScoreVisibility TINYINT(1)
            );";
            */
            await connection.QueryFirstOrDefaultAsync(createTable);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Error while trying to connect to database");
            return;
        }
    }

    private async Task<bool> UserExist(ulong steamId)
    {
        try
        {
            using var connection = new MySqlConnection(_dbConnection);
            await connection.OpenAsync();
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
    public async Task AddTag(CCSPlayerController player, string tag)
    {
        try
        {
            ulong steamId = 0;
            await Task.Run(() =>
            {
                steamId = player.AuthorizedSteamID!.SteamId64;
            });
            var userExists = await UserExist(steamId);
            await using var connection = new MySqlConnection(_dbConnection);
            await Task.Run(() => userExists);
            if (userExists)
            {
                logger.LogInformation($"User exists! Updating tag!");
                await connection.OpenAsync();
                string sqlUpdate = "UPDATE `VipTags_Players` SET `Tag` = @tag WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, new { SteamID = steamId, tag });
                return;
            }
            await connection.OpenAsync();
            string sqlInsert = "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) VALUES (@SteamID, @tag, true, true, true)";
            await connection.ExecuteAsync(sqlInsert, new { SteamID = steamId, tag });
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "AddTag failed");
        }
        return;
    }

    public async Task SaveTags(ulong steamId)
    {
        try
        {
            var userExists = await UserExist(steamId);
            await using var connection = new MySqlConnection(_dbConnection);
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
                await connection.OpenAsync();
                string sqlUpdate = "UPDATE `VipTags_Players` SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, `ChatColor` = @ChatColor, `Visibility` = @Visibility, `ChatVisibility` = @ChatVisibility, `ScoreVisibility` = @ScoreVisibility WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, parameters);
                return;
            }
            await connection.OpenAsync();
            string sqlInsert = "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility, @ChatVisibility, @ScoreVisibility)";
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
            await using var connection = new MySqlConnection(_dbConnection);
            await connection.OpenAsync();
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
        return;
    }
    public async Task ChangeColor(CCSPlayerController player, string color, int type)
    {
        var steamId = player!.AuthorizedSteamID!.SteamId64;
        var userExists = await UserExist(steamId);
        string? type1 = null;
        switch (type)
        {
            case 1:
                type1 = "TagColor";
                break;
            case 2:
                type1 = "ChatColor";
                break;
            case 3:
                type1 = "NameColor";
                break;
        }
        try
        {
            await using var connection = new MySqlConnection(_dbConnection);
            await Task.Run(() => userExists);
            if (userExists)
            {
                logger.LogInformation("User exists! Updating color - {Type}!", type);
                await connection.OpenAsync();
                string sqlUpdate = $"UPDATE `VipTags_Players` SET {type1} = @color WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, new { color, SteamID = steamId });
                return;
            }
            player.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["SetupTag"]}");
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "ChangeColor Method failed");
        }
    }

    public async Task<PlayerModel?> FetchPlayerInfo(ulong steamId)
    {
        await using var connection = new MySqlConnection(_dbConnection);
        var userExists = await UserExist(steamId);
        try
        {
            await Task.Run(() => userExists);
            if (!userExists)
            {
                logger.LogInformation("No player in database with steamid: {SteamID}", steamId);
                return null;
            }
            await connection.OpenAsync();
            string sqlSelect = $"SELECT * FROM `VipTags_Players` WHERE `SteamID` = {steamId}";
            var user = await connection.QueryFirstOrDefaultAsync<PlayerModel>(sqlSelect);
            return user;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Fetchplayerinfo failed");
        }
        return null;
    }

}