rem set PATH=%PATH%;..\.nuget
rem PATH
..\.nuget\nuget.exe pack .\KiwiDb.csproj -build -prop Configuration=Release;Platform=AnyCPU;SolutionDir=${ProjectDir}\.. -OutputDirectory . -exclude license.txt