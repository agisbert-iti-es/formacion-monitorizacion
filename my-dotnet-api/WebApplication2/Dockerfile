# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# PYROSCOPE
# 1. Binarios nativos 
COPY --from=pyroscope/pyroscope-dotnet:0.14.2-glibc /Pyroscope.Profiler.Native.so /app/Pyroscope.Profiler.Native.so
COPY --from=pyroscope/pyroscope-dotnet:0.14.2-glibc /Pyroscope.Linux.ApiWrapper.x64.so /app/Pyroscope.Linux.ApiWrapper.x64.so

# 2. Variables de entorno 
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={BD1A650D-AC5D-4896-B64F-D6FA25D6B26A}
ENV CORECLR_PROFILER_PATH=/app/Pyroscope.Profiler.Native.so
ENV LD_PRELOAD=/app/Pyroscope.Linux.ApiWrapper.x64.so
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/app

# 3. Configuración de envío 
ENV PYROSCOPE_APPLICATION_NAME=my-dotnet-api
ENV PYROSCOPE_SERVER_ADDRESS=http://pyroscope:4040
ENV PYROSCOPE_PROFILING_ENABLED=1

# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApplication2/WebApplication2.csproj", "WebApplication2/"]
RUN dotnet restore "./WebApplication2/WebApplication2.csproj"
COPY . .
WORKDIR "/src/WebApplication2"
RUN dotnet build "./WebApplication2.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApplication2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
# USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
# USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication2.dll"]