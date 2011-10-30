$env:Path += ';..\.nuget'
nuget.exe pack .\KiwiDb.csproj -build -prop Configuration=Release;Platform=AnyCPU -OutputDirectory . -exclude license.txt