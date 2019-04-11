# Create build containber
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /opt/coreapp/

# Restore packages used in the project.
# Docker will cache this step until you change packege version or add new packages.
# (All changes in the .csproj will result in rerunning this step on the next docker build.
# Simple code changes will result in the docker deamon to reuse its cached intermediate container)
COPY *.csproj /opt/coreapp/
RUN dotnet restore

# Actually build the application
COPY ./ /opt/coreapp/
RUN dotnet publish -c Release -o out

# Create runtime container
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /opt/coreapp/

# Copy the build output from the build container
COPY --from=build /opt/coreapp/out ./

ENTRYPOINT ["dotnet", "DockerDotnetApiTutorial.Api.dll"]
