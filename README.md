# Dance Dance Rotation

Learn Guild Wars 2 Rotations to the Beat!

This [Blish](https://blishhud.com/) module will add a window that can play Guild Wars 2 skill rotations as "songs" in a rhythm game.

![danceDanceRotation](https://user-images.githubusercontent.com/1414123/227801325-cb166989-11eb-4e59-965c-1b91457f31d9.gif)


The module comes pre-loaded with a variety of songs already, and users can add more or even create their own using just [arcdps logs](https://www.deltaconnected.com/arcdps/).

## Install Instructions

Download and install [Blish](https://blishhud.com/)

In Blish, select **Module Repo** and find Dance Dance Rotation. Click **Install**
![ddrInstall](https://github.com/campbt/DanceDanceRotation/assets/1414123/6d322434-f21e-4aa7-a8b6-86fff2c0ccb1)

It should now appear under **Manage Modules** in Blish. Make sure **Enable Module** is selected here.

## Setup and Usage

Once enabled, DanceDanceRotation will show up under in the Blish window under modules and include a variety of settings. Make sure all Ability Hotkeys are set and match the ones in game.

![ddrHotkeys](https://github.com/campbt/DanceDanceRotation/assets/1414123/7af62d38-fdca-4261-b301-f750d128c2ff)

*(Mouse Keys can not be bound in Blish at this time)*

### Module Settings

Module settings are accessed from the main Blish window. Select the **Manage Modules** section, then find **Dance Dance Rotation** and it should load them in the main window.

#### Notes Orientation

This setting determines the direction notes move. You can use:

**Horizontal** (Right to Left / Left to Right)

![ddrOrientationHorizontal](https://github.com/campbt/DanceDanceRotation/assets/1414123/30f657d5-68d7-4bec-9150-bf78c68c4c28)

**Vertical** (Top to Bottom / Bottom to Top)

![ddrOrientationVertical](https://github.com/campbt/DanceDanceRotation/assets/1414123/5bb20471-5a71-453e-8cd1-7e7f7a967be2)

**Ability Bar**

![ddrOrientationAbilityBar](https://github.com/campbt/DanceDanceRotation/assets/1414123/cd495fe0-57ab-4cc0-8e86-79321417860b)

*(The intention for this one is to resize it and overlay it over your actual ability bar)*

#### Background Transparency

Sets the transparency of the main window.

#### Start with first skill

When enabled, the first note to be played will be shifted so it is on the perfect position. The song can then be started by simply pressing that hotkey.

If the song's start time is not 0s, then the first note after the desired start time will be shifted.

#### Auto Hit Weapon 1

If enabled, the Weapon 1 ability will automatically be hit in the module, even if you don't hit the hotkey for it, which is what happens if Weapon 1 is set to auto cast (which it generally should be). Some skills, like Mirage Ambushes, still must be pressed because the game does not auto cast them.

When enabled, the Weapon 1 notes that will be auto pressed will not show a hotkey text and the icon will be smaller.

#### Show ability icon as note

If enabled, the module will load the actual ability icon and display that as a note. If this is too cluttered for you, you can disable it to display generic icons for all notes:

![ddrNoAbilityIcons](https://github.com/campbt/DanceDanceRotation/assets/1414123/e13a78a8-6630-471b-9dfb-22b6ba47fb9c)

#### Show ability hotkeys

If enabled, the hotkey set in the module's setting will be overlaid on top of the note.

#### Only show current profession songs

If enabled, the Song List will only show songs of the loaded character's profession

#### Compact Mode

Allows contracting the number of displayed lanes and where notes are placed.

**Regular**: 6 lanes. Notes are placed in a lane based on their ability type.                                                                                                                                                       |

**Compact**: 3 lanes. Notes are placed in the first lane, but may be shifted to other lanes to prevent overlap.                                                                                                                     |
![ddrCompactMode](https://github.com/user-attachments/assets/75e8315a-0216-4b8d-813a-6c4a82597678)

**Ultra Compact**: 1 lane. Notes are placed in the first lane. There may be significant overlap. This is better if using the Next Abilities feature, or practicing with No Miss Mode. Adjust note pace to higher values to lower overlap. |
![ddrCompactMode](https://github.com/user-attachments/assets/f430d682-a9d9-4f1d-8ebb-7ca71c27d66e)

#### Show next abilities

This setting will place the next X ability icons that need to be pressed in its own section. It does not move like standard notes. Use the slider to select how many abilities to show. 

![ddrNextAbilities](https://github.com/campbt/DanceDanceRotation/assets/1414123/5c939bdb-afc7-46f6-a50a-028673cf551c)

----------

### Song Settings

Each song has additional settings that can be adjusted for just that song. You can enter this by clicking on the name of the song in the main window, or pressing the gear icon.

#### Adjusting Playback Speed

To slow down the song, open the song details window and find the "Practice Settings" section. Setting this slider allows changing the playback rate, speed of the notes (animation speed), and the start time.

![ddrUsage1](https://github.com/user-attachments/assets/f6e851ab-e440-4cff-b373-87f978f6f37c)

**No Miss Mode**: When enabled, the song will automatically pause when a note reaches the "perfect" position. It will then resume when that note's hotkey is pressed. This is a great way to practice a new rotation and learn the hotkey order to press without having to worry about getting the timing right.

![noMissMode](https://github.com/user-attachments/assets/a09f2a18-d371-4bee-a14d-6ce420033a63)

**Note Speed**: How fast the notes *spawn*, so adjusting this down will slow down how fast you have to press the keys. 100% is performing the rotation at max speed.

**Note Pace**: Determines how fast the notes *move*. This will cause more/less notes to appear in the module at one time. It does not affect how fast you have to press keys.

**Start At**: Sets the song to start the first note at a specific time. Good for practicing the main rotation, rather than always having to start on the opener.

#### Remapping utilities

If you prefer having utilities in different slots than those used by the song, you can remap them. Click on the name of the song in the main window to bring up the Song Info page. Under here is the section "Remap Utility Skills". By default, this will show the utilities skills used in the positions defined in the build of song. If yours are different, click on the arrow circle icon, which should shift the utility skills displayed. Keep clicking the icon until the utility skills in the Song Info window match your utility skills.

![ddr2Usage2](https://user-images.githubusercontent.com/1414123/227801878-ed5d5cf4-f4a0-43a2-ab69-4f541fe78cc0.png)

*Currently, remapping *Revenant* utility skills is not supported.*

## Adding Songs

You can create your own songs using the  [Dance Dance Rotation Composer](https://campbt.github.io/DanceDanceRotationComposer/create.html)

This requires recording a golem rotation and uploading it to dps.report. Include the link to the report, and the exact build template code used when performing the rotation. The tool will attempt to construct a song given this info, but it is not perfect because arcdps can not record everything (pre cast skills and some instant cast things may not be caught).

Using the tool will give you .json for the song. This can then be imported into the module in two ways:

### Copying .json

* Copy the .json for the song to your clipboard.
* Select the list icon on the DDR main window.
* Select Add from Clipboard

A notification text should display saying it was added successfully. If this fails, the JSON may be malformed.

![ddrSongList](https://github.com/campbt/DanceDanceRotation/assets/1414123/ca3de354-64a1-4867-83df-40117935c11b)

### Adding the file directly

* Open the Blish settings
* Select DanceDanceRotation module
* Select the Gear icon in the top right and select "Open songs"
* Place all .json files in the **customSongs** folder.
* The song list should automatically populate with them if they're valid.

## Credits

Module created by **Shooper.7129**
Big thanks to **Falson.5284** and **xRemainNameless.3842** for feedback and help with creating the song generation tool.

**Initial Songs Rotation Credits**
* [Snow Crows](https://snowcrows.com/en/home) 
* [MrMystic](https://www.youtube.com/channel/UCRheAtCtIXM2JKBzmCtA8xg)

## License

Licensed under the [MIT License](https://choosealicense.com/licenses/mit/)
