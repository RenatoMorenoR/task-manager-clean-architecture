FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy projects and restore dependencies
COPY ["TaskManager.Domain/TaskManager.Domain.csproj", "TaskManager.Domain/"]
COPY ["TaskManager.Application/TaskManager.Application.csproj", "TaskManager.Application/"]
COPY ["TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "TaskManager.Infrastructure/"]
COPY ["TaskManager.API/TaskManager.API.csproj", "TaskManager.API/"]

RUN dotnet restore "TaskManager.API/TaskManager.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TaskManager.API"
RUN dotnet build "TaskManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
