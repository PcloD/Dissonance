import os

# replaceWith = "idleFatSide"
replaceWith = os.path.basename(os.getcwd())[5:]
replace = "untitled"
for filename in os.listdir("."):
	if filename.startswith(replace):
		extension = ''
		extensionLoc = filename.find(".")
		if extensionLoc == -1:
			extensionLoc = 0
		else:
			extensionLoc = -(extensionLoc+1)
			extension = filename[-(len(filename)+extensionLoc+1):]
		pureName = filename[:-len(extension)]
		os.rename(filename, replaceWith + pureName.replace(replace, '').zfill(3) + extension)