# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HeimdallWeb/HeimdallWeb.csproj", "HeimdallWeb/"]
RUN dotnet restore "HeimdallWeb/HeimdallWeb.csproj"

COPY . .
WORKDIR "/src/HeimdallWeb"
RUN dotnet build "HeimdallWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HeimdallWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
EXPOSE 8081

COPY --from=publish --chown=appuser /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HeimdallWeb.dll"]
