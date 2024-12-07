import requests
import json
import time

#
# This is a script that will generate the required metadata files used by this project.
#

# A json object of ability -> jsonInfo. Taken from repeated api requests
ALL_SKILLS_FILENAME = "./allSkills.json"
# A json object of paletteSkillId -> abilityId. Taken from wiki
# PALETTE_SKILL_LOOKUP_FILENAME = "./paletteSkillLookup.json"

# Output filename for generated info used by the module
ABILITY_INFO_FILENAME = "../DanceDanceRotationModule/ref/abilityInfoApi.json"

# Don't want to hammer the guild wars server, so sleep a bit between requests
SLEEP_TIME = 5.0


##########################################



## MARK: allSkills.json generation methods

#
# Makes an API request and returns an array of all ability IDs in the API
#
def fetchSkillIds():
    # Returns an array of ints, each int is a skill ID
    response = requests.get(url="https://api.guildwars2.com/v2/skills")
    return response.json()

#
# Creates a request chunk to avoid hitting 414 (Request-URI Too Long) errors
# This is a helper function for fetchSkills
#
def createRequestChunk(ids, startIndex, endIndex):
    idsToRequest = ""
    for index in range(startIndex,endIndex):
        if index != startIndex:
            idsToRequest += ","
        idsToRequest += str(ids[index])
    return idsToRequest

#
# Makes multiple requests to fetch the requested ability ID jsons and writes the json
# into an output file
#
def fetchSkills(ids, outputFile):
    # The API allows requesting multiple IDs at once, so block it with this stepSize
    # This is the step size used by the wiki tool
    stepSize = 200
    
    outputFile = open(ALL_SKILLS_FILENAME, "w")
    maxIndex = len(ids) - 1

    isFirstSkill = True
    outputFile.write("{\n")
    
    for startIndex in range(0, len(ids), stepSize):
        idsToRequest = []  # Use a list instead of string concatenation
        endIndex = min(startIndex + stepSize, maxIndex + 1)  # Fix the endIndex calculation
        
        # Collect IDs for this batch
        idsToRequest = ids[startIndex:endIndex]
        
        # Join IDs with commas
        ids_string = ",".join(str(id) for id in idsToRequest)
        
        print(f"Making Fetch: {ids[startIndex]} -> {ids[endIndex-1]}")
        
        response = requests.get(url=f"https://api.guildwars2.com/v2/skills?ids={ids_string}")
        
        if response.status_code != 200:
            print(f"Error with request: {response.status_code}")
            print(f"Response: {response.text[:200]}")
            continue
            
        try:
            jsonResponse = response.json()
            professionSkillsFound = 0
            for skillJson in jsonResponse:
                skillId = skillJson.get("id")
                if not skillId:
                    print("Skill Has No ID:\n" + str(skillJson))
                    continue

                professions = skillJson.get("professions", [])
                if not professions:
                    continue

                if len(professions) > 0:
                    professionSkillsFound += 1
                    prettyJson = ""
                    if not isFirstSkill:
                        prettyJson += ","
                    isFirstSkill = False
                    prettyJson += f'\n"{skillId}": '
                    prettyJson += json.dumps(
                        skillJson,
                        sort_keys=True,
                        indent=4,
                        separators=(',', ': ')
                    )
                    outputFile.write(prettyJson)
            
            print(f"Found {professionSkillsFound} Profession Skills")
            time.sleep(SLEEP_TIME)
            
        except json.JSONDecodeError as e:
            print(f"JSON Decode Error: {e}")
            continue

    outputFile.write("\n}")
    outputFile.close()

# MARK: AbilityInfo Lookup Table Functions

#
# Creates the abilityInfo.json lookup info for the module
# This requires an allSkills.json file, which is NOT
#
def createAbilityInfoTable():
    abilityIdToImageId = {}
    with open(ALL_SKILLS_FILENAME, 'r') as f:
        allSkillsJson = json.load(f)
        for abilityId, info in allSkillsJson.items():
            abilityInfo = {}
            if "icon" in info:
                iconUrl = info["icon"]
                imageName = getImageFileName(iconUrl)
                abilityInfo["assetId"] = imageName
            if "name" in info:
                abilityInfo["name"] = info["name"]
            abilityIdToImageId[abilityId] = abilityInfo

    # DON'T DO THIS - paletteIds conflict with real ability IDs
    # with open(PALETTE_SKILL_LOOKUP_FILENAME, 'r') as f:
    #     paletteSkillLookupJson = json.load(f)
    #     for paletteId,abilityIdInt in paletteSkillLookupJson.items():
    #         abilityId = str(abilityIdInt)
    #         if abilityId in abilityIdToImageId:
    #             abilityIdToImageId[paletteId] = abilityIdToImageId[abilityId]
                # print("PALETTE: " + str(paletteId) + ": " + str(abilityIdToImageId[abilityId]))

    prettyJson = json.dumps(
        abilityIdToImageId,
        sort_keys=True,
        indent=4
        # separators=(',', ': ')
    )
    outputFile = open(ABILITY_INFO_FILENAME, "w")
    outputFile.write(
        prettyJson
    )
    outputFile.close()

# MARK: Utility Functions

#
# Convert an icon url from the allSkills.json to the ID used by the Blish
# DatAssetCache
#
def getImageFileName(iconUrl):
    startIndex = iconUrl.rindex("/") + 1
    return int(iconUrl[
        startIndex:len(iconUrl)
    ].replace("/","_").split(".")[0])


###########################

# MARK: Main

allIds = fetchSkillIds()
print("Found " + str(len(allIds)) + " IDs. Starting fetch")
fetchSkills(allIds, ALL_SKILLS_FILENAME)

createAbilityInfoTable()
