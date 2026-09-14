# CS2Tags_VipTag
This plugin allows to someone with specific permission to set up their own scoreboard tag, chat tag and colors. This plugin uses MySQL database to store information about tags.<br/>

## [📌] Dependiencies:
- [CounterStrikeSharp (tested on v294)](https://github.com/roflmuffin/CounterStrikeSharp)  
- [CS2-Tags (at least v1.4)](https://github.com/schwarper/cs2-tags)
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)

## [📋] Commands:
- `!settag`  | Sets up tag. Usage: `!settag ExampleTag`,
- `!tagmenu` | Displays menu,

## [📋] Functions:
- Changing scoreboard and chat tag,
- Changing color of tag, chat text color, name color,
- Resetting tags & colors,
- Storing all data in MySQL database,

## [📌] Setup
- Install all dependencies listed upwards,
- Download latest release,
- Drag files to /plugins/
- Restart your server,
- Config file should be created in configs/plugins/CS2Tags_VipTag,
- Edit to your liking,

```
{
  "Vip_VipSetTagFlag": "@vip/vipsettag", // Flag that allows players to set tags. 
  "Vip_TagColorFlag": "@vip/tagcolor", // Flag that allows players to change Tag color
  "Vip_ChatColorFlag": "@vip/chatcolor", // Flag that allows players to change Chat color
  "Vip_NameColorFlag": "@vip/namecolor", // Flag that allows players to change Name color
  "DBHost": "", //MySQL Host
  "DBPort": 3306, //MySQL Port
  "DBUsername": "", //MySQL Username
  "DBName": "", //MySQL database name
  "DBPassword": "", //MySQL database password
  "CustomTagOnScoreboard": true, // Whether to show custom tags on the scoreboard
  "ConfigVersion": 1
}
```

### [🩷] Thanks to:
- [CS2-Tags (at least v1.4)](https://github.com/schwarper/cs2-tags) for api,
- [CS2-Ranks](https://github.com/partiusfabaa/cs2-ranks) how to manage things such as keeping player information from database,
- CounterStrikeSharp discord,
- Probably some other open-source projects that I forgot to mention,
- This its forked from Letaryat
<br><img src="https://i.imgur.com/TQP4lYn.gif" height="200px">

### [🚨] Plugin might be poorly written and have some issues. I have no idea what I am doing. Even so, when tested it worked as intended.