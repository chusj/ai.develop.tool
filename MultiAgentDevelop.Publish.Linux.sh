
find .PublishFiles/ -type f -and ! -path '*/wwwroot/images/*' ! -name 'appsettings.*' |xargs rm -rf
dotnet build;
rm -rf /home/MultiAgentDevelop/MultiAgentDevelop.Api/bin/Debug/.PublishFiles;
dotnet publish -o /home/MultiAgentDevelop/MultiAgentDevelop.Api/bin/Debug/.PublishFiles;
rm -rf /home/MultiAgentDevelop/MultiAgentDevelop.Api/bin/Debug/.PublishFiles/WMBlog.db;
# cp -r /home/MultiAgentDevelop/MultiAgentDevelop.Api/bin/Debug/.PublishFiles ./;
awk 'BEGIN { cmd="cp -ri /home/MultiAgentDevelop/MultiAgentDevelop.Api/bin/Debug/.PublishFiles ./"; print "n" |cmd; }'
echo "Successfully!!!! ^ please see the file .PublishFiles";