# Commands

This file contains a list of all slash commands this bot has.

All commands are registered here alongside their description and a list of their parameters. All parameters that don't have a default value are required.

## Table of contents

* [Championship commands](#championship-commands)

* [Team commands](#team-commands)

* [Game commands](#game-commands)

* [Miscellaneous commands](#miscellaneous-commands)

## Championship commands

### `/add-championship` (*admin-only*)

Creates an empty championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Name` | `text` | — | — | The name to be used |
| `Season` | `text` | — | A list of all available seasons | The season to be used |

### `/calculate-all-odds` (*admin-only*)

Calculates the odds for each game of a championship. This command can be slow.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/delete-championship` (*admin-only*)

Deletes a championship. This can't be undone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/reset-championship-odds` (*admin-only*)

Recalculates the odds of a championship. It's recommended to use this after restarting a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/restart-championship` (*admin-only*)

Deletes all games inside a championship and regenerates its rounds. This can't be undone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/see-championship-json` 

Makes a JSON file containing all the championship's data. This includes the previous round, current round, leaderboard, teams, and schedules.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/see-championship-odds` 

Gets the odds of a championship. If its odds aren't calculated yet, or if it's outdated, this will return an error message and start calculating them.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-championships` 

Shows all created championships.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-full-leaderboard-json` 

Returns the leaderboard of a championship as JSON.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/see-full-schedules-json` 

Shows all rounds of a championship as JSON.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/see-schedules` 

Shows the times the games of the main championship's current round are scheduled. This uses timestamps so each Discord user will see it in their timezone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-standings` 

Shows the standings of a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/set-as-main` (*admin-only*)

Sets a championship as the main one. Commands related to reading from a championship that don't require telling which championship will read from the main one. This command will give an error if there is already a championship marked as main.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/start-championship` (*admin-only*)

Marks a championship as started and generates all its rounds. Any team added after this command is ran won't be included in the games. Trying to do this command on a championship that has already started will result in an error.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

### `/unset-as-main` (*admin-only*)

Unsets a championship as the main one.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

[Back to table of contents](#table-of-contents)

## Team commands

### `/add-cake` (*admin-only*)

Adds a cake to a team. The team can use the cake once in a future game. Adding 0 or a negative amount of cakes does nothing. Adding more than 1 cake will make multiple cakes with the name name or flavor.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Cake` | `text` | — | — | The name or flavor of the cake |
| `Amount` | `int` | `1` | — | The amount |

### `/add-coach` (*admin-only*)

Adds a coach to a team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Coach` | `text` | — | A list of all available coaches | The coach |

### `/add-team` (*admin-only*)

Adds a team to a championship. The team must follow the championship's season's rules.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |
| `PlayerUsername` | `text` | — | — | The username of the player |
| `Country` | `text` | — | — | The team's country |
| `Technique` | `int` | — | — | The team's Technique |
| `Insight` | `int` | — | — | The team's insight |
| `Physique` | `int` | — | — | The team's physique |
| `Coach` | `text` | `none` | A list of all coaches that can be chosen | The coach |

### `/add-to-stat` (*admin-only*)

Make a change in a team's stats.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Stat` | `text` | — | A list containing the three stats: `Insight`, `Physique`, and `Technique` | The team's stat |
| `Amount` | `int` | — | — | The amount, can be positive or negative |

### `/delete-team` (*admin-only*)

Deletes a team. This can't be undone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |

### `/remove-coach` (*admin-only*)

Removes a coach from a team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Coach` | `text` | — | A list of all available coaches | The coach |

### `/see-cakes` 

Shows all cakes a team has.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-team-stats` 

Shows the stats of a team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-teams` 

Shows all teams in a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-teams-json` 

Shows all teams in a championship as JSON.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |

[Back to table of contents](#table-of-contents)

## Game commands

### `/change-game-round` (*admin-only*)

Changes the round of a game. Use this for manually changing the order of games.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of all games | The game |
| `Round` | `int` | — | — | The round |

### `/cheer` 

Schedules a cheer for a team in the ongoing game. Anyone can cheer up to three times per game. After cheering, the user must wait two minutes before cheering again. In the scheduled time, the bot will announce that the user is cheering alongside their yell if one is provided, and give the respective team a small bonus.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | List containing both teams in the ongoing game. | The team |
| `Yell` | `text` | `null` | — | The yell; if given, the bot will say it when annoucing that the user is cheering |

### `/schedule-game` (*admin-only*)

Schedules the date and time a game will start. By default, the bot assumes Amsterdam's timezone, but you can specify it. **Note**: only one game can happen at a time.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |
| `Year` | `int` | — | — | The year |
| `Month` | `int` | — | — | The month |
| `Day` | `int` | — | — | The day |
| `Hour` | `int` | — | — | The hour |
| `Minute` | `int` | — | — | The minute |
| `Timezone` | `text` | `"Europe/Amsterdam"` | — | The IANA timezone the date and time are in; by default it's Amsterdam's |

### `/see-current-round` 

Shows all games from the main championship's current round.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-current-round-json` 

Shows the main championship's current round as JSON.

### `/see-game-details` (*admin-only*)

Shows a game including the teams' choices of cakes, tactics, and whether they have a morale boost. As this command shows information that could give players an advantage before a game, it is admin-only.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |

### `/see-games` 

Shows all games in a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | — | A list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-odds` 

Shows the odds for a game. If the odds for that game aren't yet calculated, this will return an error message and start calculating it. These odds don't take the teams' tactics nor cakes into account.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-previous-round-json` 

Shows the main championship's previous round, i.e. the latest round in which all games have been completed, as JSON.

### `/see-team-description` 

Describes a team from the ongoing game. This shows how many players the team has: on field, as replacement player, or out of the game, as well as their numbers.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | List containing both teams in the ongoing game. | The team |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/see-teams-games` 

Shows all games of a specific team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | — | A list of all created teams | The UUID of the team |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

### `/set-cake` (*admin-only*)

Sets the cake a team will use before their next game in the current round. Must be set before the game starts. The team must have the cake and not have used it yet.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |
| `Cake` | `text` | — | — | The name or flavor of the cake |
| `Team` | `text` | — | List containing the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team |

### `/set-coach` (*admin-only*)

Sets a team's coach for their game in the current round. Must be set before the game starts. The team must have that coach available.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |
| `Coach` | `text` | — | A list of all available coaches | The coach |
| `Team` | `text` | — | List containing the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team |

### `/set-morale-boost` (*admin-only*)

Adds or removes the team's morale boost for their next game in the current round.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |
| `Team` | `text` | — | List containing the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team |
| `MoraleBoost` | `bool` | — | — | Whether the team will have or not have the morale boost; `True` means they will have; `False` means they won't |

### `/set-tactic` (*admin-only*)

Sets a team's tactic choice for their game in the current round. Must be set before the scheduled time for their game.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | — | A list of the UUIDs of all games in the main championship's current round, displaying the game's teams and round | The UUID of the game |
| `Tactic` | `text` | — | A list of all available tactics | The tactic |
| `Team` | `text` | — | List containing the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team |

[Back to table of contents](#table-of-contents)

## Miscellaneous commands

### `/say` (*admin-only*)

Sends a message to the configured DailyRugby channel. This is meant to test if the channel was configured correctly.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Message` | `text` | `"Test message"` | — | The message to send |

### `/see-ram-usage` 

Shows how much RAM the bot is using.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | — | Whether the reply should be sent privately or not |

[Back to table of contents](#table-of-contents)
