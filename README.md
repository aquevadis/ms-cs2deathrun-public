## CS2 Deathrun Manager for ModSharp | [Modules](https://github.com/CS2-Deathrun-Modules)

### Description

Deathrun Manager for CS2 servers. This is port of [this](https://forums.alliedmods.net/showthread.php?t=78197) plugin with few changes and improvements.

### Commands
#### Chat
```config

!respawn, /respawn - Respawns the caller if they are dead and have enough extra lives

```

```config

!kill, /kill - Kills the caller if they are alive

```

### Installation

1. Download the latest release from [Releases](https://github.com/aquevadis/ms-deathrun-manager-release/releases).
2. Extract the `.zip` archive or Copy-Paste it directly into your server's root directory.
3. Restart server to generate config files.
4. Adjust the configuration files to your linking and restart the server again to reflect all changes.

### Configuration

The configurations values do exactly what they are labeled, so I believe it doesn't need further explanation.

#### deathrun.json(default)
```json
{
  "GiveWeaponToCTs": true,
  "RemoveBuyZones": true,
  "RemoveMoneyFromGameAndHud": true,
  "SetRoundTimeOneHour": true,
  "EnableClippingThroughTeamMates": true,
  "EnableAutoBunnyHopping": true,
  "EnableKillCommandForCTs": true,
  "EnableKillCommandForTs": false,
  "Prefix": "{GREEN}[Deathrun]{DEFAULT}"
}
```

#### lives_system.json(default)
```json
{
  "EnableLivesSystem": false,
  "StartLivesNum": 1,
  "ShowLivesCounter": true,
  "SaveLivesToDatabase": false,
  "Spacer": "// If SaveLivesToDatabase is true, you have to configure the database connection details below too.",
  "Host": "localhost",
  "Database": "database_name",
  "User": "database_user",
  "Password": "database_password",
  "Port": 3306,
  "TableName": "deathrun_players"
}
```

#### game_cvars.json(default)
```json
{
  "Cash": [
    //commands from this block are only executed if config var RemoveMoneyFromGameAndHud = true
  ],
  "Teams": [
    "mp_limitteams 0",
    "mp_autoteambalance false",
    "mp_autokick 0",
    "bot_quota_mode fill",
    "bot_join_team ct",
    "mp_ct_default_melee weapon_knife",
    "mp_ct_default_secondary weapon_usp_silencer",
    "mp_ct_default_primary",
    "mp_t_default_melee weapon_knife",
    "mp_t_default_secondary",
    "mp_t_default_primary",
    "sv_alltalk 0"
  ],
  "Movement": [
    "sv_enablebunnyhopping 1",
    "sv_airaccelerate 99999",
    "sv_wateraccelerate 50",
    "sv_accelerate_use_weapon_speed 0",
    "sv_maxspeed 9999",
    "sv_stopspeed 0",
    "sv_backspeed 0.1",
    "sv_accelerate 50",
    "sv_staminamax 0",
    "sv_maxvelocity 9000",
    "sv_staminajumpcost 0",
    "sv_staminalandcost 0",
    "sv_staminarecoveryrate 0"
  ],
  "RoundTimer": [
    //commands from this block are only executed if config var SetRoundTimeOneHour = true
  ],
  "PlayerClipping": [
    //commands from this block are only executed if config var EnableClippingThroughTeamMates = true
  ]
}
```

### Shared API
- The plugin focuses more on the API it provides, that you can use to create and plug-in all kinds of addons and features.

#### Be sure to check the ModSharp's documentation about Shared APIs [here](https://docs.modsharp.net/docs/en-us/examples/module-api.html).

#### Getting started
##### 1. First download the Nuget package from [**here**](https://www.nuget.org/packages/DeathrunManager.Shared).

##### 2. Add Project Reference, or manually inside your .csproj:
```csharp
dotnet add reference DeathrunManager.Shared.csproj
```
or
```csharp
<ProjectReference Include="..\DeathrunManager.Shared\DeathrunManager.Shared.csproj" />
```

---

If you'd like more exposed methods/events/hooks in the API, feel free to raise issue, open pull request, etc.. 
 
