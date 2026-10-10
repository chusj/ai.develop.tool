#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#这种模式是直接在构建镜像的内部编译发布dotnet项目。
#注意下容器内输出端口是9291
#如果你想先手动dotnet build成可执行的二进制文件，然后再构建镜像，请看.Api层下的dockerfile。


#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制 Directory.Build.props 文件
COPY ["Directory.Build.props", "."]

# 复制所有项目文件
COPY ["MultiAgentDevelop.Api/MultiAgentDevelop.Api.csproj", "MultiAgentDevelop.Api/"]
COPY ["MultiAgentDevelop.Extensions/MultiAgentDevelop.Extensions.csproj", "MultiAgentDevelop.Extensions/"]
COPY ["MultiAgentDevelop.EventBus/MultiAgentDevelop.EventBus.csproj", "MultiAgentDevelop.EventBus/"]
COPY ["MultiAgentDevelop.Common/MultiAgentDevelop.Common.csproj", "MultiAgentDevelop.Common/"]
COPY ["MultiAgentDevelop.Model/MultiAgentDevelop.Model.csproj", "MultiAgentDevelop.Model/"]
COPY ["MultiAgentDevelop.Serilog.Es/MultiAgentDevelop.Serilog.Es.csproj", "MultiAgentDevelop.Serilog.Es/"]
COPY ["Ocelot.Provider.Nacos/Ocelot.Provider.Nacos.csproj", "Ocelot.Provider.Nacos/"]
COPY ["MultiAgentDevelop.Services/MultiAgentDevelop.Services.csproj", "MultiAgentDevelop.Services/"]
COPY ["MultiAgentDevelop.IServices/MultiAgentDevelop.IServices.csproj", "MultiAgentDevelop.IServices/"]
COPY ["MultiAgentDevelop.Repository/MultiAgentDevelop.Repository.csproj", "MultiAgentDevelop.Repository/"]
COPY ["MultiAgentDevelop.Tasks/MultiAgentDevelop.Tasks.csproj", "MultiAgentDevelop.Tasks/"]
COPY ["build", "build/"]

# 恢复依赖项
RUN dotnet restore "MultiAgentDevelop.Api/MultiAgentDevelop.Api.csproj"

# 复制其余的源代码
COPY . .

# 构建项目
WORKDIR "/src/MultiAgentDevelop.Api"
RUN dotnet build "MultiAgentDevelop.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiAgentDevelop.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 9291 
ENTRYPOINT ["dotnet", "MultiAgentDevelop.Api.dll"]
