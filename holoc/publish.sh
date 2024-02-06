rm -rf dist
mkdir dist

dotnet publish -r linux-x64 -c Release
cp -r bin/Release/net8.0/linux-x64/publish/* dist

rm -rf bin obj
