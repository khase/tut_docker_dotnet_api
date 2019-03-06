FROM microsoft/dotnet:2.1-aspnetcore-runtime

COPY ./bin/Debug/netcoreapp2.1/publish /opt/coreapp/

WORKDIR /opt/coreapp/

ENTRYPOINT ["dotnet", "DockerDotnetApiTutorial.Api.dll"]