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

def printBadNote(note, notes):
    abilityId = str(note["abilityId"])
    time = note["time"]
    index = note["index"]
    print("    abilityId: ", abilityId)
    print("        index   :     ", index)
    print("        time    :     ", time, "(", str(time//1000) + "." + str(time%1000) +"s )")
    print("        duration: ", note["duration"])
    print("        total # : ", note["count"])
    if abilityId in allSkills:
        skill = allSkills[abilityId]
        print("        name    : ", skill["name"])
        print("        slot    : ", skill["slot"])
    else:
        print("        name    : <Not in allskills>")
    # Nearby
    print("        nearby notes [")
    for i in range(index - 2, index + 2):
        if i < 0 or i >= len(notes):
            break
        nearbyNote = notes[i]
        nearbyAbilityId = str(nearbyNote["abilityId"])
        if nearbyAbilityId in allSkills:
            nearbyAbility = allSkills[nearbyAbilityId]
            nearbyName = nearbyAbility["name"]
        else:
            nearbyName = "<Not in all skills>"
        nearbyTime = nearbyNote["time"]
        print("           index    : ", i)
        print("               abilityId : ", nearbyAbilityId)
        print("               name      : ", nearbyName)
        print("               time      :     ", nearbyTime, "(", str(nearbyTime//1000) + "." + str(nearbyTime%1000) +"s )")
        print("               duration  : ", nearbyNote["duration"])
    print("        ]")

def parseSong(song):

    # Check palette
    unknownPaletteSkills = []
    profession = song["decodedBuildTemplate"]["profession"]
    # There are issues with the Revenant. Just ignore those
    if profession != 9:
        paletteSkills = song["decodedBuildTemplate"]["skills"]["terrestrial"]["utilities"]
        for paletteSkill in paletteSkills:
            if str(paletteSkill) not in paletteSkillLookup:
                unknownPaletteSkills.append(paletteSkill)

    # Invalid format for notes
    invalidNotes = []

    # Unknown notes (literally, notes with noteType: "Unknown"
    unknownNotes = {}

    # Unknown abilityIds ("abilityId" is missing or not known in the lookup table)
    unknownInfo = {}

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

            if note["noteType"] == "Unknown":
                abilityId = note["abilityId"]
                if abilityId not in unknownNotes:
                    unknownNotes[abilityId] = note
                    unknownNotes[abilityId]["index"] = index
                    unknownNotes[abilityId]["count"] = 1
                else:
                    unknownNotes[abilityId]["count"] += 1

            if str(note["abilityId"]) not in abilityInfo:
                abilityId = note["abilityId"]
                if abilityId not in unknownInfo:
                    unknownInfo[abilityId] = note
                    unknownInfo[abilityId]["index"] = index
                    unknownInfo[abilityId]["count"] = 1
                else:
                    unknownInfo[abilityId]["count"] += 1


    if len(invalidNotes) != 0 or len(unknownPaletteSkills) != 0 or len(unknownNotes) != 0 or len(unknownInfo) != 0:
        print("{")
        print("  ", song["name"])
        print("  ", song["logUrl"])

        if len(invalidNotes) != 0:
            print("   Invalid Notes:")
            for note in invalidNotes:
                for k,v in note.items():
                    print("    ", k, ": ", v)

        if len(unknownPaletteSkills) != 0:
            print("   Unknown Palette Skills:")
            for paletteSkill in unknownPaletteSkills:
                print("    paletteId: ", paletteSkill)

        if len(unknownNotes) != 0:
            print("   Unknown Notes:")
            print("     (noteType is \"Unknown\" - If <Not in allskills>, Composer needs to be updated. If ability is known, it's probably the build not matching the dps report file):")
            for note in unknownNotes.values():
                abilityId = str(note["abilityId"])
                if abilityId in allSkills:
                    # Known
                    print("   > Ability ID is known | Build code probably doesn't match snowcrows:")
                    print("      buildURL     : " + song["buildUrl"])
                    print("      buildChatCode: " + song["buildChatCode"])
                    printBadNote(note, notes)
                else:
                    print("   > Ability ID is not known | Update composer")
                    printBadNote(note, notes)

        if len(unknownInfo) != 0:
            print("   Unknown Ability IDs (note abilityId not in abilityInfo files):")
            print("     (Add name/assetId for abilityId to abilityInfoCustom.json. Use https://search.gw2dat.com/ to find the assetId)")
            for note in unknownInfo.values():
                printBadNote(note, notes)

        print("}")
        return True
    else:
        return False


def parseFolder(folder):
    noErrors = False
    for fileName in os.listdir(folder):
        with open(os.path.join(folder, fileName), 'r') as f:
            song = json.load(f)
            if parseSong(song):
                noErrors = True
    if noErrors == False:
        print("All Data is Valid!")

parseFolder(DEFAULT_SONGS_FOLDER)
