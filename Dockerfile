FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY checkpoint-web/checkpoint-web.csproj checkpoint-web/
RUN dotnet restore checkpoint-web/checkpoint-web.csproj
COPY . .
WORKDIR /app/checkpoint-web
RUN dotnet publish -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "checkpoint-web.dll"]