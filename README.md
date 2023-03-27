# Dance Dance Rotation

Learn Guild Wars 2 Rotations to the Beat!

This [Blish](https://blishhud.com/) module will add a window that can play Guild Wars 2 skill rotations as "songs" in a rhythm game.

![danceDanceRotation](https://user-images.githubusercontent.com/1414123/227801325-cb166989-11eb-4e59-965c-1b91457f31d9.gif)


The module comes pre-loaded with a variety of songs already, and users can add more or even create their own using just [arcdps logs](https://www.deltaconnected.com/arcdps/).

## Install Instructions

Download and install [Blish](https://blishhud.com/)

Download the latest [release](https://github.com/campbt/DanceDanceRotation/releases/latest) .bhm file and place in:
```
<My Documents>/Guild Wars 2/addons/blishhud/modules
```
(Full instructions [here](https://new.blishhud.com/docs/user/installing-modules/#manually-installing-modules)

In game, launch blish and go to the Blish settings and enable the module. 

## Setup and Usage

Once enabled, DanceDanceRotation will show up under in the Blish window under modules and include a variety of settings. Make sure all Ability Hotkeys are set and match the ones in game.

### Adjusting Playback Speed

To slow down the song, click on the name of the song in the main window to bring up the Song Info page. Under here is a "Pracitce Settings" section that allows changing the playback rate, speed of the notes (animation speed), and the start time.

![ddrUsage1](https://user-images.githubusercontent.com/1414123/227801872-0946f253-cc06-4302-a6ab-da4be194c9d9.png)

### Remapping utilities

If you prefer having utilities in different slots than those used by the song, you can remap them. Click on the name of the song in the main window to bring up the Song Info page. Under here is the section "Remap Utility Skills". By default, this will show the utilities skills used in the positions defined in the build of song. If yours are different, click on the arrow circle icon, which should shift the utility skills displayed. Keep clicking the icon until the utility skills in the Song Info window match your utility skills.

![ddr2Usage2](https://user-images.githubusercontent.com/1414123/227801878-ed5d5cf4-f4a0-43a2-ab69-4f541fe78cc0.png)

*Currently, remapping *Revenant* utility skills is not supported.*

## Adding Songs

### Copying .json

* Copy the .json for the song to your clipboard.
* Select the list icon on the DDR main window.
* Select Add from Clipboard

A notification text should display saying it was added successfully. If this fails, the JSON may be malformed.

### Adding the file directly

* Open the Blish settings
* Select DanceDanceRotation module
* Select the Gear icon in the top right and select "Open songs"
* Place all .json files in this folder.
* The list should automatically populate with them if they're valid.

## Credits

Module created by **Shooper.7129**
Big thanks to **Falson.5284** and **xRemainNameless.3842** for feedback and help with creating the song generation tool.

**Initial Songs Rotation Credits**
Benchmark rotations by a variety of creators at [Snow Crows](https://snowcrows.com/en/home)
[MrMystic](https://www.youtube.com/channel/UCRheAtCtIXM2JKBzmCtA8xg)

## License

Licensed under the [MIT License](https://choosealicense.com/licenses/mit/)
