# Commands

This file contains a list of all slash commands this bot has.

All commands are registered here alongside their description and a list of their parameters. All parameters that don't have a default value are required.

The commands that can only be used by admins are tagged as such.

Most commands that are also available for non-admins have a parameter `Private`, where the user can choose if they'd like the reply to be send ephemerally. The admin-only commands do not have this option as the reply from them can have information that could give players an unfair advantage for their next game.

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
| `name` | `text` | *none* | *none* | The name of the championship |
| `season` | `text` | *none* | Autocomplete provides a list of all available seasons | The championship's season |

### `/see-championships`

Shows all created championships.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/delete-championship` (*admin-only*)

Deletes a championship. This cannot be undone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `id` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to be deleted |

### `/start-championship` (*admin-only*)

Marks a championship as started and generates all rounds. Any team added after this command is ran will not be included in the games. Trying to run this command on a championship that has already started will result in an error.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to be started |

### `/restart-championship` (*admin-only*)

Deletes all games inside a championship and regenerates its rounds. This can't be undone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to be restarted |

### `/set-as-main` (*admin-only*)

Sets a championship as the main one. Commands related to reading from a championship that don't require telling which championship will read from the main one. This command will give an error if there is already a championship marked as main.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to be marked as the main one |

### `/unset-as-main` (*admin-only*)

Unsets a championship as the main one.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to be unmarked as the main one |

### `/see-standings`

Shows the standings of a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to get the standings from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/see-schedules`

Shows the times the games of the main championship's current round are scheduled. This uses timestamps so each Discord user will see it in their timezone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/see-full-leaderboard-json`

Returns the leaderboard of a championship as JSON.

An example of a possible output is:

```json
[
    {
        "country": "Brazil",
        "wins": 1,
        "ties": 0,
        "losses": 0,
        "hasBeaten": [
            "Singapore"
        ],
        "pointBalance": 2,
        "tryBalance": -2,
        "tries": 1,
        "points": 17,
        "matchesLeft": 0
    },
    {
        "country": "Singapore",
        "wins": 0,
        "ties": 0,
        "losses": 1,
        "hasBeaten": [],
        "pointBalance": -2,
        "tryBalance": 2,
        "tries": 3,
        "points": 15,
        "matchesLeft": 0
    }
]
```

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to get the leaderboard from |

### `/see-full-schedules-json`

Shows all rounds of a championship as JSON.

An example of a possible output is:

```json
{
    "1": [
        "Brazil vs Singapore",
        "Ireland vs Taiwan"
    ],
    "2": [
        "Brazil vs Ireland",
        "Singapore vs Taiwan"
    ],
    "3": [
        "Brazil vs Taiwan",
        "Ireland vs Singapore"
    ]
}
```

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to get the schedules from |

### `/see-championship-json`

Makes a JSON file containing all the championship's data. This includes the previous round, current round, leaderboard, teams, and schedules.

An example of a possible output is:

```json
{
	"previousRound": [],
	"currentRound": [
		{
			"teamA": "Brazil",
			"teamB": "Taiwan",
			"teamAHasMoraleBoost": false,
			"teamBHasMoraleBoost": false,
			"teamAGetsMoraleBoost": false,
			"teamBGetsMoraleBoost": false
		}
	],
	"leaderboard": [
		{
			"country": "Brazil",
			"wins": 0,
			"ties": 0,
			"losses": 0,
			"hasBeaten": [],
			"pointBalance": 0,
			"tryBalance": 0,
			"tries": 0,
			"points": 0,
			"matchesLeft": 1
		},
		{
			"country": "Taiwan",
			"wins": 0,
			"ties": 0,
			"losses": 0,
			"hasBeaten": [],
			"pointBalance": 0,
			"tryBalance": 0,
			"tries": 0,
			"points": 0,
			"matchesLeft": 1
		}
	],
	"teams": [
		{
			"country": "Brazil",
			"username": "RafaX9",
			"technique": 20,
			"insight": 50,
			"physique": 30,
			"coaches": [
				"Physique"
			],
			"cakes": [
				"Uranium"
			]
		},
		{
			"country": "Taiwan",
			"username": "Onko342",
			"technique": 21,
			"insight": 60,
			"physique": 19,
			"coaches": [
				"Technique"
			],
			"cakes": []
		}
	],
	"schedules": {
		"1": [
			"Brazil vs Taiwan"
		]
	}
}
```

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to get the JSON file from |

### `/calculate-all-odds` (*admin-only*)

Calculates the odds for each game of a championship. This command can be slow.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to get the schedules from |

### `/see-championship-odds`

Gets the odds of a championship. If its odds aren't calculated yet, or if it's outdated, this will return an error message and start calculating them.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to get the odds from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |


[Back to table of contents.](#table-of-contents)

## Team commands

### `/add-team` (*admin-only*)

Adds a team to a championship. If the data of the team is invalid as per the championship's season's rules, the bot will return an error message saying what's wrong.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to add a team to |
| `PlayerUsername` | `text` | *none* | *none* | The username of the player representing the country |
| `Insight` | `int` | *none* | *none* | A non-negative number representing the team's Insight |
| `Physique` | `int` | *none* | *none* | A non-negative number representing the team's Physique |
| `Technique` | `int` | *none* | *none* | A non-negative number representing the team's Technique |
| `InitialCoach` | `text` | *none* | Autocomplete provides a list of all coaches that can be chosen | The team's initial coach |

### `/see-teams`

Shows all teams in a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to see the teams from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/delete-team` (*admin-only*)

Deletes a team. This can't be undone.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to delete |

### `/see-team-stats`

Shows the stats of a team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to see the stats from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/add-to-stat` (*admin-only*)

Add a certain number to a team's stats.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to add a stat to |
| `Stat` | `text` | *none* | Autocomplete provides a list containing `Insight`, `Physique`, and `Technique` | The stat that should be changed |
| `Amount` | `int` | *none* | *none* | The amount to add to the stat; can be positive or negative |

### `/add-coach` (*admin-only*)

Adds a coach to a team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to add a coach to |
| `Coach` | `text` | *none* | Autocomplete provides a list of all coaches available | The coach to add |

### `/remove-coach` (*admin-only*)

Removes a coach from a team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to remove a coach from |
| `Coach` | `text` | *none* | Autocomplete provides a list of all coaches available | The coach to remove |

### `/see-teams-json`

Shows all teams in a championship as JSON.

An example of a possible output is:

```json
[
	{
		"country": "Brazil",
		"username": "RafaX9",
		"technique": 20,
		"insight": 50,
		"physique": 30,
		"coaches": [
			"Physique"
		],
		"cakes": [
			"Uranium"
		]
	},
	{
		"country": "Taiwan",
		"username": "Onko342",
		"technique": 21,
		"insight": 60,
		"physique": 19,
		"coaches": [
			"Technique"
		],
		"cakes": []
	}
]
```

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to see the teams from |

### `/add-cake` (*admin-only*)

Adds a cake to a team they can use once in any future game.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to add a cake to |
| `Cake` | `text` | *none* | *none* | The name or flavor of the cake |
| `Amount` | `int` | `1` | *none* | The amount of cakes to add. Inputting a negative number or 0 will do nothing. The same name or flavor is repeated for all cakes.

### `/see-cakes`

Shows all cakes a team has.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to add a cake to |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

[Back to table of contents.](#table-of-contents)

## Game commands

### `/see-games`

Shows all games in a championship.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Championship` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created championships, displaying the championship's name | The UUID of the championship to see the games from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/see-teams-games`

Shows all games of a specific team.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides a list of the UUIDs of all created teams, displaying the team's name | The UUID of the team to see the games from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/see-game-details` (*admin-only*)

Shows a game including the teams' choices of cakes, tactics, and whether they have a morale boost. As this command shows information that could help opponents before a game, it is admin-only.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to see |

### `/schedule-game` (*admin-only*)

Schedules the date and time a game will start. The date and time must be in UTC.

**Note:** only one game can happen in a given moment. If, by the time a new game should start, one is still happening, the current game will end before the next one starts.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to schedule |
| `yearUtc` | `int` | *none* | *none* | The year the game must start in |
| `monthUtc` | `int` | *none* | *none* | The month the game must start in |
| `dayUtc` | `int` | *none* | *none* | The day the game must start in |
| `hourUtc` | `int` | *none* | *none* | The hour the game must start at |
| `minuteUtc` | `int` | *none* | *none* | The minute the game must start in |

### `/see-current-round`

Shows all games from the main championship's current round.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/set-tactic` (*admin-only*)

Sets a team's tactic choice for their game in the current round. Must be set before the scheduled time for their game.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to set a tactic to |
| `Tactic` | `text` | *none* | Autocomplete provides a list of all available tactics | The tactic to set |
| `Team` | `text` | *none* | Autocomplete provides the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team to set the tactic to |

### `/set-coach` (*admin-only*)

Sets a team's coach for their game in their game in the current round. The team must have that coach.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to set a coach to |
| `Coach` | `text` | *none* | Autocomplete provides a list of all possible coaches | The coach to set |
| `Team` | `text` | *none* | Autocomplete provides the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team to set the tactic to |

### `/set-cake` (*admin-only*)

Sets the cake a team will use right before their next game in the current round. The team must have the cake and not have used it yet.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to set a cake to |
| `Cake` | `text` | *none* | *none* | The name or flavor of the cake the team will use |
| `Team` | `text` | *none* | Autocomplete provides the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team to set the tactic to |

### `/set-morale-boost` (*admin-only*)

Adds or removes the team's morale boost for their next game in the current round.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to set a coach to |
| `Team` | `text` | *none* | Autocomplete provides the options `TeamA` and `TeamB`, referring, respectively, to the team shown on the left and on the right in `Game`'s display name | The team to set the tactic to |
| `MoraleBoost` | `bool` | *none* | *none* | Whether the team will have or not have a morale boost for the game; `True` means they will have the boost, `False` means they won't |

### `/see-odds` (*admin-only*)

Shows the odds for a game. If the odds for that game aren't yet calculated, this will return an error message and start calculating it. This odds do not take the team's tactic nor cake choices into account.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Game` | `text` | *none* | Autocomplete provides a list of the UUIDs of all games in the **main championship's current round**, displaying the game's teams and round | The UUID of the game to see the odds from |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/cheer`

Schedules a cheer for a team in the ongoing game. Anyone can cheer up to three times during a game. After cheering, the user must wait at least two minutes before cheering again.

In the following minute, the bot will announce that the user is cheering for that team alongside their yell if they provide one. After this announcement, the team gets a small boost in their stats for a certain amount of time as per the championship's rules.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides the names of both teams in the ongoing game | The team to cheer for |
| `Yell` | `text` | `null` | *none* | The yell; if given, the bot will say the yell when it announces the user is cheering |

### `/see-current-round-json`

Shows the main championship's current round as JSON.

An example of a possible output is:

```json
[
    {
        "teamA": "Singapore",
        "teamAGetsMoraleBoost": false,
        "teamAHasMoraleBoost": false,
        "teamB": "Soviet Union",
        "teamBGetsMoraleBoost": false,
        "teamBHasMoraleBoost": false
    },
    {
        "teamA": "Brazil",
        "teamAGetsMoraleBoost": false,
        "teamAHasMoraleBoost": false,
        "teamB": "Taiwan",
        "teamBGetsMoraleBoost": false,
        "teamBHasMoraleBoost": false
    }
]
```

### `/see-previous-round-json`

Shows the main championship's previous round (the latest round in which all games have been completed) as JSON.

An example of a possible output is:

```json
[
    {
        "teamA": "Brazil",
        "teamAGotMoraleBoost": false,
        "teamAHadMoraleBoost": false,
        "teamAScore": 17,
        "teamATactic": "None",
        "teamAUsedCake": false,
        "teamB": "Singapore",
        "teamBGotMoraleBoost": false,
        "teamBHadMoraleBoost": false,
        "teamBScore": 15,
        "teamBTactic": "None",
        "teamBUsedCake": false
    },
    {
        "teamA": "Ireland",
        "teamAGotMoraleBoost": false,
        "teamAHadMoraleBoost": false,
        "teamAScore": 21,
        "teamATactic": "Physique",
        "teamAUsedCake": false,
        "teamB": "Taiwan",
        "teamBGotMoraleBoost": false,
        "teamBHadMoraleBoost": false,
        "teamBScore": 23,
        "teamBTactic": "General",
        "teamBUsedCake": false
    }
]
```

### `/see-team-description`

Describes a team from the ongoing game. This shows how many players the team has on field, as replacement players, or out of the game, as well as their numbers.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Team` | `text` | *none* | Autocomplete provides the names of both teams in the ongoing game | The team to describe |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

[Back to table of contents.](#table-of-contents)

## Miscellaneous commands

### `/see-ram-usage`

Shows how much RAM the bot is using.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Private` | `bool` | `true` | *none* | Whether the reply should be sent privately or not |

### `/say` (*admin-only*)

Sends a message to the configured DailyRugby channel. This is meant to test if the channel was configured correctly.

Parameters:

| Parameter | Type | Default | Autocomplete | Description |
| --------- | ---- | ------- | ------------ | ----------- |
| `Message` | `text` | `"Test message"` | *none* | The message to send |

[Back to table of contents.](#table-of-contents)