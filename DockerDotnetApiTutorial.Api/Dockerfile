FROM microsoft/dotnet:2.1-sdk AS build
COPY ./ /opt/coreapp/

WORKDIR /opt/coreapp/
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /opt/coreapp/

COPY --from=build /opt/coreapp/out ./

ENTRYPOINT ["dotnet", "DockerDotnetApiTutorial.Api.dll"]
