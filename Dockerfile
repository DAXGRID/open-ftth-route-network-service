FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./OpenFTTH.RouteNetworkService/*.csproj ./OpenFTTH.RouteNetworkService/
COPY ./OpenFTTH.RouteNetworkService.Queries/*.csproj ./OpenFTTH.RouteNetworkService.Queries/

COPY ./OpenFTTH.RouteNetworkService.Tests/*.csproj ./OpenFTTH.RouteNetworkService.Tests/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/OpenFTTH.RouteNetworkService
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /app

COPY --from=build-env /app/OpenFTTH.RouteNetworkService/out .
ENTRYPOINT ["dotnet", "OpenFTTH.RouteNetworkService.dll"]

ENV ASPNETCORE_URLS=https://+443;http://+80
ENV ASPNETCORE_HTTPS_PORT=443
EXPOSE 80
