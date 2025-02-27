FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG ACCESS_TOKEN
WORKDIR /src
COPY ["NGrid.Customer.GraphQl.FusionGateway/NGrid.Customer.GraphQl.FusionGateway.csproj", "nuget.config", "NGrid.Customer.GraphQl.FusionGateway/"]
RUN dotnet nuget update source internal-feed \
                --source https://pkgs.dev.azure.com/NGRID/US-4750J-ADOProject-01/_packaging/uwp2/nuget/v3/index.json \
                --username gitHub \
                --configfile NGrid.Customer.GraphQl.FusionGateway/nuget.config \
                --password $ACCESS_TOKEN \
                --store-password-in-clear-text
RUN dotnet restore "NGrid.Customer.GraphQl.FusionGateway/NGrid.Customer.GraphQl.FusionGateway.csproj"
COPY . .
WORKDIR "/src/NGrid.Customer.GraphQl.FusionGateway"
RUN dotnet build "NGrid.Customer.GraphQl.FusionGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NGrid.Customer.GraphQl.FusionGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "NGrid.Customer.GraphQl.FusionGateway.dll"]