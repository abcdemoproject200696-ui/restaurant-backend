# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Resturent-MobileApp-Backend.csproj" --source https://api.nuget.org/v3/index.json
RUN dotnet publish "Resturent-MobileApp-Backend.csproj" -c Release -o /app

# ===== Run stage =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Render PORT env var deta hai; app usi pe sunega (Program.cs me handle)
ENTRYPOINT ["dotnet", "Resturent-MobileApp-Backend.dll"]
