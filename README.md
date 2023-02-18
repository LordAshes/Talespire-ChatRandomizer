# Char Randomizer Plugin

This unofficial TaleSpire plugin for randomly determining items from a list.
Supports real time lists and named pre-configured lists.

This plugin, like all others, is free but if you want to donate, use: http://LordAshes.ca/TalespireDonate/Donate.php

## Change Log

```
3.4.0: Addes constent text support allowing use of result with other chat services
3.3.0: Added optional prompts
3.3.0: Fixed issue where randomization occurs on all devices and thus produces different results
3.2.1: Fixed issue with result being chopped off
3.2.1: Fixed issue with sample RPS and RPSLS samples
3.2.0: Added configurable display options
3.1.0: Added tally (total) support
3.0.0: Added multiple randomization per request
3.0.0: Added optional named randomization
3.0.0: Added added soft dependency for chat whisper so result can be whispered
2.1.3: Initial release
```

## Install

Use R2ModMan or similar installer to install this plugin.

## Display Options

The display options can be configured using R2ModMan's Edit Config for the plugin or using Config Manager
from the Talespire Settings menu. Please note that not all combinations of settings will produce usable
results.

### Show Roll Possibilities

This setting indicates if the possible outcomes are listed along with the roll. If so, the possible
outcomes are displayed, in brackets, after the roll name or the default roll name.

### Show All Names Instead Of Just First

This setting indicates if ``/cnr`` and ``/crnt`` commands expect a single name or a name for each
roll in the sequence. This not only affects the display but also the interpretation of the roll request.
If this setting is set to false then only the first entry in a multi roll request is expected to have
a name. Providing names for the other roll request parts will cause errors since the name will get
interpeted as a roll possibility.

Example: 

For Show All Names = True Use:

``/cnrt Name1 Roll1 / Name2 Roll2 / ...`` such as ``/cnrt Attack 1 2 3 4 5 6 7 9 10 / Damage 1 2 3 4 5 6``

For Show All NAmes = False Use:

``/cnrt Name1 Roll1 / Roll2 / ...`` such as ``/cnrt Attack 1 2 3 4 5 6 7 9 10 / 1 2 3 4 5 6``

### Show Result On Same Line

This setting determines if the result is place on the same like as the roll (true) or on new line
(false) with the Default Result Prefix (see below).

### Show Total On Same Line

This setting determines if the total is place on the same like as the roll (true) or on new line (false).

### Default Roll Prefix

This setting determines the prefix for non name rolls.

### Default Result Prefix

This setting determines the prefix for non name results.

### Use Chat Header For First Name

This setting determines if the first roll name is place in the header of the chat message (along with the
player name) with the result in the body.

## Usage

There are four types of rolls. Generic rolls, Named rolls, Generic Rolls with Tally and Named rolls with Tally.
Each of which can use pre-defined rolls (configured in a configuration file ahead of time) or real-time list
(specified when rolling).

To perform a generic roll, use the ``/cr`` (Custom Roll) function. For example:

``/cr RPS`` 

or

``/cr Rock Paper Scissors``

To perform a named roll, use the ``/cnr`` (Custom Named Roll) function. For example:

``/cnr Gesture RPS`` 

or

``/cnr Gecture Rock Paper Scissors``

When using the named functionality the first item in the list is treated as the name. This is what is displayed instead
of "Roll" when displaying the result. It should be noted that if the verbose setting is turned off the result is not
prefixed by the name but a cnr roll will still consume the first entry for the name.

To use the tally (total the value) version, add an ``t`` as the last letter of the command. When using the tally functions
the results need to be numeric. Non-numeric sides will be counted a 0.

``/crt 0 0 1 1 2 3`` 

or

``/cnrt Fluff 0 0 1 1 2 3`` 

### Multiple Rolls

Multiple rolls can be requested in a single request by separating the rolls with a ``/`` character. Note that if using
the custom named roll, a new name is used for each set of randomize options. For example:

``/cr Miss Hit / Low Med High``

or

``/cnr Attack Miss Hit / Damage Low Med High``

Multiple rolls are supported by all 4 of the functions (``/cr``, ``/cnr``, ``/crt`` and ``/cnrt``).
 
### Prompt Rolls

Prefixing a roll sequence with ? turns the sequence into a prompt pair. All entries in the seqeunce are treated as
key value plairs. The key is what is displayed in the prompt and the value is the value that is inserted if the
prompt is selected. Typically (since values are limited to a word) thsese select or not select pre-configured custom
dice seqeuences. The character back slash character is used to denote no text insertion (blank value).

For example:

``/cr ? Rock_Paper_Scissors RPS Boulder_Parchement_Shears BPS None \ / ? Bonus_1 1 None 0``

would generate a prompt for Rock_Paper_Scissors, Boulder_Parchement_Shears or None replacing the prompt with either RPS,
BPS or an empty string (back slash) depedning on what the roller chooses. Then it would process the next sequence which
would be a choice of Bonus_1 or None which would get replaced with 1 to 0 depending on the choice.

Prompts also work with ``/cnr``, ``crt`` and ``/cnrt``. If a name is used, it should be before the ? characters.

``/cnr Game ? Rock_Paper_Scissors RPS Boulder_Parchement_Shears BPS None \``
 
### Constent Text

Constent text can be inserted by making the text its own randomization and surrounding it in quotes. For example:

``/cr "I selected "/rock paper scissors``

would output ``I selected rock``, ``I selected paper`` or ``I selected scissors``.

### Whisper Rolls And Other Chat Services

Whispers and other chat services can now be triggered using the constent text option. For example:

``/cr "/w GM "/rock paper scissors``

would whisper the result of rock, paper or scissors to the GM. As can be seen the first part is a constent text which
triggers the whisper service and provides the target as GM. The slash from ``/w`` does not trip a Chat Roll divider
because it is part of the constent text. The slash after the GM and space is outside the quotes and thus Chat Randomizer
treats it as a divider. The randomization after the slash is processed and appended to the constent text. This total
result is then processed as a new chat messages tripping any chat services in the process (in this case the whisper
service).
 
### Pre-Configure Lists

To make rolling dice with a complex number of sides easier, custom dice can be defined in the ``Custom_Dice.json`` file.
The file is a JSON with the roll name as the key and the roll sides as the value. For example:
```
{
	"RPS": "Rock Paper Scissors",
	"Half6": "- - - 0 0 0",
	"TriTen": "0 0 0 0 1 1 1 1 1 2"
}
```
To roll a named roll, use the cr (Custom Roll) command. Type: ``/cr name`` such as ``/cr RPS``.
