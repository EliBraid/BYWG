# 使用官方.NET 8运行时镜像作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 使用官方.NET 8 SDK镜像进行构建
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["BYWG.Gateway/BYWG.Gateway.csproj", "BYWG.Gateway/"]
COPY ["BYWG.Shared/BYWG.Shared.csproj", "BYWG.Shared/"]
COPY ["BYWGLib/BYWGLib.csproj", "BYWGLib/"]

# 恢复依赖
RUN dotnet restore "BYWG.Gateway/BYWG.Gateway.csproj"

# 复制所有源代码
COPY . .

# 构建应用
WORKDIR "/src/BYWG.Gateway"
RUN dotnet build "BYWG.Gateway.csproj" -c Release -o /app/build

# 发布应用
FROM build AS publish
RUN dotnet publish "BYWG.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 最终镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建日志目录
RUN mkdir -p /app/logs

# 设置环境变量
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# 启动应用
ENTRYPOINT ["dotnet", "BYWG.Gateway.dll"]
