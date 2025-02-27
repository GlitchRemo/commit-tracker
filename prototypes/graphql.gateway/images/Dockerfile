FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG ACCESS_TOKEN

WORKDIR /src
COPY ["NGrid.Customer.GraphQl.Gateway/NGrid.Customer.GraphQl.Gateway.csproj", "nuget.config", "NGrid.Customer.GraphQl.Gateway/"]
RUN dotnet nuget update source internal-feed \
                --source https://pkgs.dev.azure.com/NGRID/US-4750J-ADOProject-01/_packaging/uwp2/nuget/v3/index.json \
                --username gitHub \
                --configfile NGrid.Customer.GraphQl.Gateway/nuget.config \
                --password $ACCESS_TOKEN \
                --store-password-in-clear-text
RUN dotnet restore "NGrid.Customer.GraphQl.Gateway/NGrid.Customer.GraphQl.Gateway.csproj"
COPY . .
WORKDIR "/src/NGrid.Customer.GraphQl.Gateway"
RUN dotnet build "NGrid.Customer.GraphQl.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NGrid.Customer.GraphQl.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "NGrid.Customer.GraphQl.Gateway.dll"]