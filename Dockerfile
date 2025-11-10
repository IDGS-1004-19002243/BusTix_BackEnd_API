# Usa la imagen del SDK de .NET 9.0 para compilar la aplicación (etapa de compilación)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia los archivos de proyecto/solución y restaura las dependencias.
# Se copia por separado para aprovechar el cache de capas de Docker.
# **IMPORTANTE**: Asegúrate de que la ruta y el nombre del archivo .csproj sean correctos.
COPY ["AuthAPI/AuthAPI.csproj", "AuthAPI/"]
RUN dotnet restore "AuthAPI/AuthAPI.csproj"

# Copia el resto de los archivos del proyecto.
COPY . .
WORKDIR "/src/AuthAPI"
RUN dotnet publish "AuthAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

# Usa la imagen de ASP.NET Core runtime para ejecutar la aplicación (etapa final)
# Esta imagen es más ligera que la del SDK.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

# Copia la aplicación publicada desde la etapa 'build'.
COPY --from=build /app/publish .

# Expone el puerto 8080 (y 8081 para https). Ajusta estos puertos si tu API usa otros.
EXPOSE 8080
EXPOSE 8081

# Define el punto de entrada para ejecutar la aplicación.
# **IMPORTANTE**: Asegúrate de que el nombre del archivo .dll sea correcto.
ENTRYPOINT ["dotnet", "AuthAPI.dll"]