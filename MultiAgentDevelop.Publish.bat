color B

del  .PublishFiles\*.*   /s /q

dotnet restore

dotnet build

cd MultiAgentDevelop.Api

dotnet publish -o ..\MultiAgentDevelop.Api\bin\Debug\net7.0\

md ..\.PublishFiles

xcopy ..\MultiAgentDevelop.Api\bin\Debug\net7.0\*.* ..\.PublishFiles\ /s /e 

echo "Successfully!!!! ^ please see the file .PublishFiles"

cmd