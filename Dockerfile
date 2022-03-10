FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./OpenFTTH.RouteNetwork.Service/*.csproj ./OpenFTTH.RouteNetwork.Service/
COPY ./OpenFTTH.RouteNetwork.API/*.csproj ./OpenFTTH.RouteNetwork.API/
COPY ./OpenFTTH.RouteNetwork.Business/*.csproj ./OpenFTTH.RouteNetwork.Business/
COPY ./OpenFTTH.RouteNetwork.Tests/*.csproj ./OpenFTTH.RouteNetwork.Tests/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/OpenFTTH.RouteNetwork.Service
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app

COPY --from=build-env /app/OpenFTTH.RouteNetwork.Service/out .
ENTRYPOINT ["dotnet", "OpenFTTH.RouteNetwork.Service.dll"]

ENV ASPNETCORE_URLS=https://+443;http://+80
ENV ASPNETCORE_HTTPS_PORT=443
EXPOSE 80
