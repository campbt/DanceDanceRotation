import json
import os

#
# This is a script that will validate the data in the blish module:
#   + All default songs are missing "Unknown"
#   + All abilityIds are known
#   + All palleteIds are known
#
# If anything is missing, it will be printed out (it's recommended to dump the output to a file)
#

# MARK: Resources in project

ALL_SKILLS_FILE = './allSkills.json'
DEFAULT_SONGS_FOLDER = '../defaultSongs'
ABILITY_INFO_FILES = [
    '../DanceDanceRotationModule/ref/abilityInfoApi.json',
    '../DanceDanceRotationModule/ref/abilityInfoCustom.json'
]
PALETTE_SKILL_LOOKUP_FILE = '../DanceDanceRotationModule/ref/paletteSkillLookup.json'

EXPECTED_KEYS_IN_NOTES = [
    "time",
    "duration",
    "noteType",
    "abilityId"
]

PROFESSIONS_BY_CODE = {
   1: "Guardian",
   2: "Warrior",
   3: "Engineer",
   4: "Ranger",
   5: "Thief",
   6: "Elementalist",
   7: "Mesmer",
   8: "Necromanger",
   9: "Revenant"
}

# MARK: Read in resources

with open(ALL_SKILLS_FILE, 'r') as f:
    allSkills = json.load(f)

abilityInfo = {}
for fileName in ABILITY_INFO_FILES:
    with open(fileName, 'r', encoding='utf-8-sig') as f:
        fileContents = json.load(f)
        for k,v in fileContents.items():
            abilityInfo[k] = v

with open(PALETTE_SKILL_LOOKUP_FILE, 'r') as f:
    paletteSkillLookup = json.load(f)


# MARK: Read in resources

knownSkillIds = {
    "-2": "<Weapon Swap>"
}


def badNoteInfo(note, notes):
    output_lines = []
    
    global knownSkillIds
    abilityId = str(note["abilityId"])
    time = note["time"]
    index = note["index"]
    
    output_lines.append(f"    abilityId: {abilityId}")
    output_lines.append(f"        index   :       {index}")
    output_lines.append(f"        time    :       {time} ({time//1000}.{time%1000:03d}s )")
    output_lines.append(f"        duration: {note['duration']}")
    output_lines.append(f"        total # : {note['count']}")
    if abilityId in allSkills:
        skill = allSkills[abilityId]
        output_lines.append(f"        name    : {skill.get('name')}")
        output_lines.append(f"        slot    : {skill.get('slot')}")
    else:
        output_lines.append("        name    : <Not in allskills>")
        
    # Nearby
    output_lines.append("        nearby notes [")
    for i in range(index - 2, index + 2):
        if i < 0 or i >= len(notes):
            break
        nearbyNote = notes[i]
        nearbyAbilityId = str(nearbyNote["abilityId"])
        if nearbyAbilityId in allSkills:
            nearbyAbility = allSkills[nearbyAbilityId]
            nearbyName = nearbyAbility["name"]
        elif nearbyAbilityId in knownSkillIds:
            nearbyName = knownSkillIds[nearbyAbilityId]
        else:
            nearbyName = "<Not in all skills>"
        nearbyTime = nearbyNote["time"]
        output_lines.append(f"            index    : {i}")
        output_lines.append(f"                abilityId : {nearbyAbilityId}")
        output_lines.append(f"                name      : {nearbyName}")
        output_lines.append(f"                time      :       {nearbyTime} ({nearbyTime//1000}.{nearbyTime%1000:03d}s )")
        output_lines.append(f"                duration  : {nearbyNote['duration']}")
        
    output_lines.append("        ]")
    return "\n".join(output_lines)

def parseSong(song):

    # Check palette
    unknownPaletteSkills = []
    profession = song["decodedBuildTemplate"]["profession"]
    # There are issues with the Revenant. Just ignore those
    if profession != 9:
        paletteSkills = song["decodedBuildTemplate"]["skills"]["terrestrial"]["utilities"]
        for paletteSkill in paletteSkills:
            if paletteSkill > 0 and str(paletteSkill) not in paletteSkillLookup:
                unknownPaletteSkills.append(paletteSkill)

    # Invalid format for notes
    invalidNotes = []

    # Unknown notes (abilityId is known, but the noteType was not determined)
    unknownNotes = {}

    # Unknown abilityIds ("abilityId" is missing or not known in the lookup table)
    unknownAbilities = {}

    # Parse Notes
    notes = song["notes"]
    time = 0
    for note, index in zip(notes, range(len(notes))):
        invalidNoteReason = ""
        for expectedKey in EXPECTED_KEYS_IN_NOTES:
            if expectedKey not in note:
                invalidNoteReason = "MISSING EXPECTED KEY: " + expectedKey

        if "time" in note and note["time"] < time:
            invalidNoteReason = "TIME IS BEFORE PREVIOUS NOTE: " + str(time)

        if invalidNoteReason != "":
            note["invalidReason"] = invalidNoteReason
            invalidNotes.append(note)
        else:
            time = note["time"]

            if str(note["abilityId"]) not in abilityInfo:
                abilityId = note["abilityId"]
                if abilityId not in unknownAbilities:
                    unknownAbilities[abilityId] = note
                    unknownAbilities[abilityId]["index"] = index
                    unknownAbilities[abilityId]["count"] = 1
                else:
                    unknownAbilities[abilityId]["count"] += 1
            elif note["noteType"] == "Unknown":
                abilityId = note["abilityId"]
                if abilityId not in unknownNotes:
                    unknownNotes[abilityId] = note
                    unknownNotes[abilityId]["index"] = index
                    unknownNotes[abilityId]["count"] = 1
                else:
                    unknownNotes[abilityId]["count"] += 1


    

    if len(invalidNotes) != 0 or len(unknownPaletteSkills) != 0 or len(unknownNotes) != 0 or len(unknownAbilities) != 0:
        songInfo = {}
        songInfo["song"] = song
        songInfo["unknownPaletteSkills"] = unknownPaletteSkills
        songInfo["invalidNotes"] = invalidNotes
        songInfo["unknownNotes"] = unknownNotes
        songInfo["unknownAbilities"] = unknownAbilities
        return songInfo
    else:
        return None

def checkInvalidNotes(songInfos):   
    invalidCount = 0
    badSongs = []
    for songInfo in songInfos:
        invalidNoteCount = len(songInfo["invalidNotes"])
        if invalidNoteCount > 0:
            invalidCount += invalidNoteCount
            badSongs.append(songInfo)
        
    if len(badSongs) == 0:
        return False
    
    print("\n\n")
    print("============================")      
    print("==  Invalid Notes         ==")
    print("============================")
    print("Severity: Extreme")
    print("  These are improbably formatted nots!")
    print("  Most likely a bug in the composer produced them, or manual edits did something wrong!")
    print("  These will probably break if loaded into the module, if it even allows them at all")
    print("")
    print("FIX:")
    print("  No easy fix. Have to debug the composer")
    print("")
    print(f"There were {len(badSongs)} songs that had invalid notes")
    print(f"There are {invalidCount} invalid notes total")
    print("")
    print("\nSONGS WITH INVALID NOTES:")
    for songInfo in badSongs:
        song = songInfo["song"]
        invalidNotes = songInfo["invalidNotes"]
        print("{")
        print("    Invalid Notes:")
        for note in invalidNotes:
            for k,v in note.items():
                print(f"    {k}: {v}")
        print("}")
        
    return True

def checkPaletteSkills(songInfos):
    missingPaletteIds = {}
    for songInfo in songInfos:
        for unknownSkillId in songInfo["unknownPaletteSkills"]:
            if unknownSkillId not in missingPaletteIds:
                missingPaletteIds[unknownSkillId] = songInfo
    
    if len(missingPaletteIds) == 0:
        return False
      
    print("\n\n")
    print("============================")      
    print("== MISSING PALETTE SKILLS ==")
    print("============================")
    print("Severity: High")
    print("  Palette skills IDs are used to hook up the Utility skills to appropriate hotkeys")
    print("  If these are mapped incorrectly, then the Utility skills probably won't have the correct Hotkey,")
    print("  but they will probably display correctly.")
    print("")
    print("Fix:")
    print("  Need to update palette mapping files, PaletteSkills.js, with these IDs.")
    print("  Go to here to get full mapping: https://wiki.guildwars2.com/wiki/Chat_link_format/skill_palette_table")
    print("  Go to here to load the buildChatCode: https://en.gw2skills.net/editor/")
    print("  The missing IDs are probably not on the wiki webpage, because there is already a script that has these mapped in")
    print("  Typically, the missing number will need to be associated to the ability found on the editor page, pointing to the ability defined in the wiki")
    print("")
    print(f"MISSING PALETTE SKILLS: {len(missingPaletteIds)}")
    for unknownSkillId in missingPaletteIds:
        print(f"  {unknownSkillId}")
    
    print("\nSONGS WITH MISSING PALETTES:")
    for unknownSkillId in missingPaletteIds:
        songInfo = missingPaletteIds[unknownSkillId]
        song = songInfo["song"]
        unknownPaletteSkills = songInfo["unknownPaletteSkills"]
        print("{")
        print(f"  {song['name']}")
        print(f"  {song['logUrl']}")
        print(f"  {song['buildChatCode']}")
        print(f"  {song['buildUrl']}")
        print("  Palette Skills:")
        profession = song["decodedBuildTemplate"]["profession"]
        paletteSkills = song["decodedBuildTemplate"]["skills"]["terrestrial"]["utilities"]
        for paletteSkill in paletteSkills:
            if paletteSkill == 0:
                print("    0: <Not Defined/Flex Slot. Ignore>")
            elif str(paletteSkill) in paletteSkillLookup:
                abilityId = str(paletteSkillLookup[str(paletteSkill)])
                if abilityId in allSkills:
                    ability = allSkills[abilityId]
                    print("    " + str(paletteSkill) + " : " + str(abilityId) + " - " + ability["name"])
                else:
                    print("    " + str(paletteSkill) + " : " + str(abilityId) + " - !! This abilityId is not in allSkills though !!")
            else:
                print("    " + str(paletteSkill) + ": !!UNKNOWN!!")
        print("}")
    return True
        
def checkUnknownAbilities(songInfos):
    unknownAbilities = {}
    for songInfo in songInfos:
        unknownAbility = songInfo["unknownAbilities"]
        for abilityId in unknownAbility:
            if abilityId not in unknownAbilities:
                unknownAbilities[abilityId] = songInfo
    
    if len(unknownAbilities) == 0:
        return False
        
    sortedUnknownAbilities = sorted(unknownAbilities.keys())
      
    print("\n\n")
    print("============================")      
    print("== UNKNOWN INFO SKILLS    ==")
    print("============================")
    print("Severity: High")
    print("  Some notes have unknown Ability IDs")
    print("  Specifically, the note's 'abilityId' is not in abilityInfo files")
    print("  This typically means the DDR module would show an Unknown icon there.")
    print("  But, this often means the 'note' is actually a relic or instant effect, not something the user has to press a hotkey for")
    print("FIX:")
    print("  First check if this note even needs to exist. It might just be a relic or instant effect")
    print("  Update the CustomSkills.js file in DDRComposer:")
    print("    If not a note:")
    print('       	"<abilityId>": {')
    print('               "isNote": false')
    print('         },')
    print("    If it is a note:")
    print('       	"<abilityId>": {')
    print('               "isNote": true,')
    print('               "noteType": "Profession1"')
    print('         },')
    print("  Also, if this is a note:")
    print("     Update the abilityInfoCustom.json file in DDR with the icon and/or name")
    print("     Use https://search.gw2dat.com/ to find the assetId")
    print("")
    print(f"UNKNOWN ABILITIES: {len(unknownAbilities)}")
    for abilityId in sortedUnknownAbilities:
        print(f"  {abilityId}")
        
    print("")
    print("CustomSkills.js 'fix':")
    print("Use this as a GUIDE, since *most* unknowns are not notes, but you need to double check")
    for abilityId in sortedUnknownAbilities:
        print("// ")
        print('"' + str(abilityId) + '": {')
        print('    "isNote": false')
        print('},')
    
    print("\nSONGS WITH UNKNOWN ABILITIES:")
    for abilityId in sortedUnknownAbilities:
        songInfo = unknownAbilities[abilityId]
        song = songInfo["song"]
        unknownAbility = songInfo["unknownAbilities"]
        print("{")
        print(f"  {song['name']}")
        print(f"  {song['logUrl']}")
        print(f"  Unknown Ability ID: {abilityId}")
        print(badNoteInfo(unknownAbility[abilityId], song["notes"]))
        print("}")
        
    return True
        
def checkUnknownNotes(songInfos, hasError):
    global allSkills
    global paletteSkillLookup
    
    songsWithUnknownNotes = []
    unknownNotes = {}
    for songInfo in songInfos:
        unknownNote = songInfo["unknownNotes"]
        if len(unknownNote):
            songsWithUnknownNotes.append(songInfo)
        for abilityId in unknownNote:
            if abilityId not in unknownNotes:
                unknownNotes[abilityId] = songInfo
    
    if len(unknownNotes) == 0:
        return False
        
    sortedSongs = sorted(songsWithUnknownNotes, key=lambda song: song["song"]["name"])
    sortedUnknownNotes = sorted(unknownNotes.keys())
      
    print("\n\n")
    print("============================")      
    print("== UNKNOWN NOTES          ==")
    print("============================")
    print("Severity: Medium")
    print("  AbilityID is known but, noteType is marked as Unknown")
    print("  This means the note will have no hotkey hookup.")
    print("  It should be able to show a name/icon, though.")
    print("")
    print("FIX:")
    print("  There are two common cases for this. Best thing is to look at the 'slot' it is given in AbilityInfo")
    print("")
    print("  Slot: None")
    print("  If slot is none, you need to check the wiki to see if this is a real skill or a special effect from a trait.")
    print("  The script will try and generate the wiki link, but it is just a guess")
    print("  If the wiki confirms it, then add the skill to the CustomSkills.js as \"isNote\": false")
    print("")
    print("  Slot: Utility")
    print("  Most common reason is that this is a Utility skill, and the palette skills do not link it")
    print("  This generally means that the buildChatCode in Snow Crows does not match what the dps report used")
    print("  While this will cause problems for the default songs, it shouldn't affect people using the Composer tool")
    print("")
    print("  To confirm it's just a mismatch between the buildChatCode and the dps report:")
    print("    Load: https://en.gw2skills.net/editor/")
    print("    Enter in the buildChatCode")
    print("    Check if the Unknown note's name (which is known) is one of the Utility skills")
    print("    If not, it's a buildChatCode mismatch")
    print("    To fix this, you have to update the buildChatCode to match the dps report")
    print("")
    print("  ==> You may be able to just ignore these! <==")
    print("")
    print(f"SONGS WITH UNKNOWN NOTES: {len(songsWithUnknownNotes)}")
    for songInfo in sortedSongs:
        songName = songInfo["song"]["name"]
        unknownNotesInSong = len(songInfo["unknownNotes"])
        print(f"  {songName} : {unknownNotesInSong}")
    
    # There is a special prompt at the end to delete the songs that have mismatch problems
    mismatchSongs = [] # Will be the songInfo for songs with mismatch problems
    
    print("\nUNKNOWN NOTES:")
    for abilityId in sortedUnknownNotes:
        songInfo = unknownNotes[abilityId]
        song = songInfo["song"]
        unknownNotesInSong = songInfo["unknownNotes"]
        print("{")
        print(f"  {song['name']}")
        print(f"  {song['logUrl']}")
        print(f"  {song['buildChatCode']}")
        print(f"  {song['buildUrl']}")
        print(f"  Ability ID: {abilityId}")
        skill = allSkills[str(abilityId)]
        
        skillName = skill.get('name')
        slot = skill.get('slot')
        professionCode = song["decodedBuildTemplate"]["profession"]
    
        if slot == None:
            profession = ""
            if professionCode in PROFESSIONS_BY_CODE:
                profession = PROFESSIONS_BY_CODE[professionCode] + " - "
            
            print("    +-------------------")
            print("    | !! ANALYSIS !!")
            print("    | Slot: None")
            print("    | Check wiki to see if this is a note")
            print("    | https://wiki.guildwars2.com/wiki/" + skillName.replace(" ", "_"))
            print("    | Add info to AllSkills.js (example below if this is not a note)")
            print('    // ' + profession + skillName + ' (Trait)')
            print('    "' + str(abilityId) + '": {')
            print('        "isNote": false')
            print('    },')
            print("    +--------------------")
        elif slot == "Utility":
            isMissingPaletteLookup = False
            utilityAbilityNames = []
            paletteInfo = ""
            if professionCode != 9:
                paletteSkills = song["decodedBuildTemplate"]["skills"]["terrestrial"]["utilities"]
                for paletteSkill in paletteSkills:
                    if paletteSkill <= 0:
                        paletteInfo += "    |   " + str(paletteSkill) + " : <Flex Slot>\n"
                    else:
                        if str(paletteSkill) in paletteSkillLookup:
                            utilityAbilityId = paletteSkillLookup[str(paletteSkill)]
                            if str(utilityAbilityId) in allSkills:
                                utilityAbility = allSkills[str(utilityAbilityId)]
                                utilityAbilityName = utilityAbility.get('name')
                                utilityAbilityNames.append(utilityAbilityName)
                                paletteInfo += "    |   " + str(paletteSkill) + " : " + utilityAbilityName + "\n"
                            else:
                                paletteInfo += "    |   " + str(paletteSkill) + " : <Unknown Ability Id: " + str(utilityAbilityId) + ">\n"
                        else:
                            paletteInfo += "    |   " + str(paletteSkill) + " : <Missing Palette>\n"
                            isMissingPaletteLookup = True
            if skillName != None and skillName not in utilityAbilityNames:
                print("    +-------------------")
                print("    | !! ANALYSIS !!")
                print("    | Slot: Utility")
                print("    | DPS Report has Skill '" + str(skillName) + "' but it's not defined in the palette skills:")
                print(paletteInfo)
                print("    | This is *probably* a mismatch with the buildChatCode and dps report.")
                print("    | To fix you'll have to fix the buildChatCode to match the dps report,")
                print("    | but, it might be better to just remove this")
                print("    +-------------------")
                mismatchSongs.append(songInfo)
            elif isMissingPaletteLookup:
                print("    +-------------------")
                print("    | !! ANALYSIS !!")
                print("    | Slot: Utility")
                print("    | Failed to look up a Utility skill from the build. The Unknown note may be this missing palette skill.")
                print(paletteInfo)
                print("    | You have to fix the PaletteSkills.js file")
                print("    +-------------------")
    
        unknownNote = unknownNotesInSong[abilityId]
        print(badNoteInfo(unknownNote, song["notes"]))
        print("}")
        
    if len(mismatchSongs) > 0:
        promptToDeleteUnknownNoteSongs(mismatchSongs)
        
    return True
    
def promptToDeleteUnknownNoteSongs(songInfos):
    print("")
    print("-- Verify Results --")
    print("  PARTIAL SUCCESS: There are errors, but they are all mismatch errors")
    print("  The following files have buildChatCodes that don't match their DPS reports")
    print("  This happens on some Snow Crows builds either due to accident or because they bench with something different (like Signet instead of Flame Elemental on Elementalist")
    print("")
    
    files = []
    for songInfo in songInfos:
        fullFileName = songInfo["fullFileName"]
        if fullFileName not in files:
            files.append(songInfo["fullFileName"])
            print(songInfo["fileName"])
        
    print("")
    confirmation = input("Delete these files? (y/yes): ")
    if confirmation in ["y", "yes"]:
        for fullFileName in files:
            print("Deleting: " + fullFileName)
            os.remove(fullFileName)

def parseFolder(folder):
    songInfos = []
    for fileName in os.listdir(folder):
        fullFileName = folder + "/" + fileName
        with open(fullFileName, 'r') as f:
            song = json.load(f)
            info = parseSong(song)
            if info != None:
                info["fileName"] = fileName
                info["fullFileName"] = fullFileName
                songInfos.append(info)
                
    if len(songInfos) == 0:
        print("All Data is Valid!")
    else:
        # There was at least one error!
        
        hasError = False
        hasError = hasError or checkInvalidNotes(songInfos)
        hasError = hasError or checkPaletteSkills(songInfos)
        hasError = hasError or checkUnknownAbilities(songInfos)
        hasError = hasError or checkUnknownNotes(songInfos, hasError)
        
        if hasError:
            print("")
            print("-- Verify Results --")
            print("!! There were errors !!")
        

parseFolder(DEFAULT_SONGS_FOLDER)
