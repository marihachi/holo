if not exist ..\bin (
    mkdir ..\bin
)

dotnet publish -r win-x64 ..\Holoc\Holoc.csproj
copy ..\Holoc\bin\Release\net8.0\win-x64\publish\Holoc.exe ..\bin
