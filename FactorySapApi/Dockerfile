# ===============================
# Runtime image
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ===============================
# Build image
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["FactorySapApi/FactorySapApi.csproj", "FactorySapApi/"]
RUN dotnet restore "FactorySapApi/FactorySapApi.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/FactorySapApi"
RUN dotnet publish "FactorySapApi.csproj" -c Release -o /app/publish

# ===============================
# Final image
# ===============================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FactorySapApi.dll"]
