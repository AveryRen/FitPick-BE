# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files from subfolder
COPY FitPick_EXE201/ ./FitPick_EXE201/

# Set working directory to the project
WORKDIR /src/FitPick_EXE201

# Restore packages
RUN dotnet restore "FitPick_EXE201.csproj"

# Build and publish
RUN dotnet publish "FitPick_EXE201.csproj" -c Release -o /app/publish

# ===== STAGE 2: Run =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "FitPick_EXE201.dll"]
