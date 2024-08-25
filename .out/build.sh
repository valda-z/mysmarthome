#/bin/bash

cp -rf MySmartHomeCore/ .out/MySmartHomeCore
cp -f .out/appsettings.json .out/bin

cd .out/
dotnet restore "MySmartHomeCore/MySmartHomeCore.csproj"
dotnet build "MySmartHomeCore/MySmartHomeCore.csproj" -c Release -o ../.out/bin

cd bin
dotnet MySmartHomeCore.dll