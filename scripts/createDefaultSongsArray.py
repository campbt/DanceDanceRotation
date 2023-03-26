import json
import os

#
# This script merges all the .json files in the defaultSongs/ folder into a single .json array
# and places it in the ref/ folder of the module
#

DEFAULT_SONGS_FOLDER = "../defaultSongs"
OUTPUT_FILE = "../DanceDanceRotationModule/ref/defaultSongs.json"

outputJson = []
for filename in os.listdir(DEFAULT_SONGS_FOLDER):
    if filename.endswith(".json"):
        print("Adding '" + filename + "'")
        with open(os.path.join(DEFAULT_SONGS_FOLDER, filename), 'r') as f:
            fileJson = json.load(f)
            outputJson.append(fileJson)

# Write out the array
prettyJson = json.dumps(
    outputJson,
    # sort_keys=True,
    indent=4
)
outputFile = open(OUTPUT_FILE, "w")
outputFile.write(
    prettyJson
)
outputFile.close()
