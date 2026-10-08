# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore

WORKDIR /src

COPY TaxiPark.sln ./

COPY src/TaxiPark.Api/TaxiPark.Api.csproj \
     src/TaxiPark.Api/

COPY src/TaxiPark.Application/TaxiPark.Application.csproj \
     src/TaxiPark.Application/

COPY src/TaxiPark.Domain/TaxiPark.Domain.csproj \
     src/TaxiPark.Domain/

COPY src/TaxiPark.Infrastructure/TaxiPark.Infrastructure.csproj \
     src/TaxiPark.Infrastructure/

RUN dotnet restore TaxiPark.sln


FROM restore AS build

COPY . .

RUN dotnet publish src/TaxiPark.Api/TaxiPark.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaxiPark.Api.dll"]