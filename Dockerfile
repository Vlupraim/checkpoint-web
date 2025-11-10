# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY checkpoint-web/checkpoint-web.csproj checkpoint-web/
RUN dotnet restore "checkpoint-web/checkpoint-web.csproj"

# Copiar todo el código y compilar
COPY checkpoint-web/ checkpoint-web/
WORKDIR "/src/checkpoint-web"
RUN dotnet build "checkpoint-web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "checkpoint-web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar herramientas de EF Core para migraciones en runtime
RUN dotnet tool install --global dotnet-ef --version 8.*
ENV PATH="${PATH}:/root/.dotnet/tools"

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Crear script de inicio que aplica migraciones
RUN echo '#!/bin/bash\n\
dotnet ef database update --no-build\n\
dotnet checkpoint-web.dll' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
