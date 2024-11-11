FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_TOKEN
ARG NUGET_USERNAME
WORKDIR /src
COPY ["nuget.config", "nuget.config"]
COPY ["Rfsmart.Phoenix.Licensing.Web/Rfsmart.Phoenix.Licensing.Web.csproj", "Rfsmart.Phoenix.Licensing.Web/"]
COPY ["Rfsmart.Phoenix.Licensing/Rfsmart.Phoenix.Licensing.csproj", "Rfsmart.Phoenix.Licensing/"]
RUN dotnet nuget update source github --password "$NUGET_TOKEN" --username "$NUGET_USERNAME" --store-password-in-clear-text
RUN dotnet restore "./Rfsmart.Phoenix.Licensing.Web/Rfsmart.Phoenix.Licensing.Web.csproj"
COPY ["Rfsmart.Phoenix.Licensing.Web", "Rfsmart.Phoenix.Licensing.Web"]
COPY ["Rfsmart.Phoenix.Licensing", "Rfsmart.Phoenix.Licensing"]
WORKDIR "/src/Rfsmart.Phoenix.Licensing.Web"
RUN dotnet build "./Rfsmart.Phoenix.Licensing.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG VERSION=
ARG VERSION_SUFFIX=
RUN semVer=${VERSION:=0.0.0} \
    && if [ ! -z "$VERSION_SUFFIX" ]; then semVer="$semVer"-"$VERSION_SUFFIX"; fi \
    && dotnet publish "./Rfsmart.Phoenix.Licensing.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:Version="$semVer"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Run the createLogPath script on Linux to ensure the automatic instrumentation logs are genereated without permission isues
USER root
RUN /app/datadog/createLogPath.sh
USER app
ENV ASPNETCORE_HTTP_PORTS=80
ENTRYPOINT ["dotnet", "Rfsmart.Phoenix.Licensing.Web.dll"]
