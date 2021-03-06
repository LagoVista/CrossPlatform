#!/usr/bin/env bash

# Script to update files in android build to apply version numbers, icons and package name based on environment
# Expects environment variables setup as:
#  APPCENTER_APPID  Custom Variable with key for app center telemetry
#  APPBUNDLEID Custom variable with name of package used to identify application, we need different package names in all environments
#  APPCENTER_SOURCE_DIRECTORY Varialbe from build environment as the root directory where all source code files live
#  APPCENTER_BRANCH dev, stage or master, used to identify environment to set URI and grab correct icons

# Set to directory name within /src where native project lives
iOSProjectName=SeaWolf.iOS

# Set to directory name within /src where common Xamarin forms app lives
appProjectName=SeaWolf.iOS

# Version is pulled from a file in the root, it contains first two digits of full version, such as 1.0
version=$(<$APPCENTER_SOURCE_DIRECTORY/App/seawolf/version.txt)
version='1.0.0'
# Revision is four digit number, always zero padded HHMM
revision=`date +"%H%M"`

# Version to be displayed to the user 1.0.219.0818
fullVersion=$version.$APPCENTER_BUILD_ID

echo Setting Version
echo $fullVersion
cat $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/Info.plist

plutil -replace CFBundleShortVersionString -string $fullVersion $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/Info.plist
plutil -replace CFBundleVersion -string $fullVersion $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/Info.plist
plutil -replace CFBundleIdentifier -string $APPBUNDLEID $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/Info.plist

# within the script, the branch name identifies the server to be used, however it must always be upper case to match conditional compile in code
val=$(echo "$APPCENTER_BRANCH" | tr '[:lower:]' '[:upper:]' )
sed -i '' 's/#define ENV.*/#define ENV_'"$val"'/' $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$appProjectName/App.xaml.cs

# Set the unique mobile center key to capture telemetry
sed -i '' 's/MOBILE_CENTER_KEY = \"[0-9A-Fa-f\-]*\";/MOBILE_CENTER_KEY = \"'"$APPCENTER_APPID"'\";/' $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/AppDelegate.cs

# icons are setup by branch name so just copy everything over
cp -R $APPCENTER_SOURCE_DIRECTORY/BuildAssets/iOS/$APPCENTER_BRANCH/ $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/Resources/Media.xcassets/AppIcons.appiconset

cat $APPCENTER_SOURCE_DIRECTORY/Apps/seawolf/$iOSProjectName/Info.plist

