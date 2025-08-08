# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . ./

# Restore packages
RUN dotnet restore "./FitPick_EXE201.csproj"

# Build and publish
RUN dotnet publish "./FitPick_EXE201.csproj" -c Release -o /app/publish

# ===== STAGE 2: Run =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy build output
COPY --from=build /app/publish .

# Expose the default port
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "FitPick_EXE201.dll"]
