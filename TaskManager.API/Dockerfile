# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["TaskManager.API.csproj", "./"]
RUN dotnet restore "TaskManager.API.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
