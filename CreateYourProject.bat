color 5
echo "if u install template error,pls connect QQ:3143422472"


color 3
dotnet new -i MultiAgentDevelop.Webapi.Template

set /p OP=Please set your project name(for example:BlogMicService):

md .1YourProject

cd .1YourProject

dotnet new blogcoretpl -n %OP%

cd ../


echo "Create Successfully!!!! ^ please see the folder .1YourProject"

dotnet new -u MultiAgentDevelop.Webapi.Template


echo "Delete Template Successfully"

pause
