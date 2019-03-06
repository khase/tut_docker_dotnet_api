# Developing a containerized dotnet api

## Prerequisites 

- Install a dotnet sdk
- Install postman
- Install docker for window or docker toolbox (depending on your windows version)
- Install Visual Studio Code
- Install pgAdmin (https://www.pgadmin.org/download/)

## Creating the dotnet project

    dotnet new sln DockerDotnetApiTutorial
    dotnet new web -n DockerDotnetApiTutorial.Api
    dotnet sln list
    dotnet sln add .\DockerDotnetApiTutorial.Api\DockerDotnetApiTutorial.Api.csproj
    dotnet sln list
    dotnet build


## Starting for the first time

    dotnet run

You will see "Hello World!" being shown in the browser.

## Configure your first controller

Startup.cs

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMvcWithDefaultRoute();

        // this was the original code, delete it
        // app.Run(async (context) =>
        // {
        //     await context.Response.WriteAsync("Hello World!");
        // });
    }

Controllers/HomeController.cs

    using Microsoft.AspNetCore.Mvc;

    public namespace DockerDotnetApiTutorial.Api.Controllers
    {
        public class HomeController: Controller 
        {
            public string Index()
            {
                return "Hello World from API!";
            }
        }
    }

Run the app with `dotnet run` and check that it returns your new greeting.

## Dockerize your app api

    dotnet publish

Create a dockerfile in the directory of the project and start up your docker machine.

Dockerfile

    FROM microsoft/dotnet:2.1-aspnetcore-runtime

    COPY ./bin/Debug/netcoreapp2.1/publish /opt/coreapp/

    WORKDIR /opt/coreapp/

    ENTRYPOINT ["dotnet", "DockerDotnetApiTutorial.Api.dll"]

Release vs Debug

    docker build .

Output will look similar to this

    Sending build context to Docker daemon  1.363MB
    Step 1/4 : FROM microsoft/dotnet:2.1-aspnetcore-runtime
    ---> beae224eb228
    Step 2/4 : COPY ./bin/Debug/netcoreapp2.1/publish /opt/coreapp/
    ---> 354912e67d9e
    Step 3/4 : WORKDIR /opt/coreapp/
    ---> Running in 62b0cc4e19e6
    Removing intermediate container 62b0cc4e19e6
    ---> 83c48f42a53e
    Step 4/4 : ENTRYPOINT ["dotnet", "DockerDotnetApiTutorial.Api.dll"]
    ---> Running in 816e25f076cc
    Removing intermediate container 816e25f076cc
    ---> 368c3e6d3eea
    Successfully built 368c3e6d3eea
    SECURITY WARNING: You are building a Docker image from Windows against a non-Windows Docker host. All files and directories added to build context will have '-rwxr-xr-x' permissions. It is recommended to double check and reset permissions for sensitive files and directories.

Get the id of your newly created image, in this example its `368c3e6d3eea`.

Start the docker container containing the api

    docker run 368c3e6d3eea -p 80:80

See that no ports are published

    docker ps

Correct the command

    docker run -p 80:80 368c3e6d3eea

Get the ip of your docker host to access the api in the browser

    docker-machine ip

Access your container in the browser. Usually its on http://192.168.99.100.

## Run a database for your api

Run a postgres image

    docker run postgress

aaah, forgot the ports

    docker run -p 5432:5432 postgres

Open pgadmin and access the database under 192.168.99.100:5432, user postgres/admin.
Create some database `Test`.

Stop the container.

    docker ps

You get output like this

    CONTAINER ID        IMAGE               COMMAND                  CREATED             STATUS              PORTS                    NAMES
    d2e914fdc5a1        postgres            "docker-entrypoint.s…"   2 minutes ago       Up 2 minutes        0.0.0.0:5432->5432/tcp   confident_feistel
    785c777720b8        postgres            "docker-entrypoint.s…"   5 minutes ago       Up 5 minutes        5432/tcp                 stupefied_hertz
    d5f0c2d70bb6        368c3e6d3eea        "dotnet DockerDotnet…"   11 minutes ago      Up 11 minutes       0.0.0.0:80->80/tcp       competent_tu
    af2873511879        368c3e6d3eea        "dotnet DockerDotnet…"   12 minutes ago      Up 12 minutes                                recursing_northcutt
    d4369bb3ebf9        368c3e6d3eea        "dotnet DockerDotnet…"   12 minutes ago      Up 12 minutes                                brave_heisenberg
    d9dfd91398fb        368c3e6d3eea        "dotnet DockerDotnet…"   14 minutes ago      Up 14 minutes                                wizardly_jang

Search and copy the id of the postgress database

    docker stop d2e914fdc5a1

Check that the database is gone

    docker ps

Start the container again.

    docker run -p 5432:5432 postgres

Check for your database. Oh no, its gone.

By befault docker containers do not have a persistent volume.

Details on how to configure the image can be found on https://hub.docker.com/_/postgres

## Persist your database volume

Create docker volume 

    docker volume create myDatabaseVolume
    docker volume ls

Run the postgres docker container with the folder being mounted as a volume.

    docker run -p 5432:5432 --mount 'type=volume,source=myDatabaseVolume,target=/var/lib/postgresql/data/pgdata' -e PGDATA=/var/lib/postgresql/data/pgdata postgres

Create a database on your db server again.

Stop and start the container and check that the database remained intact.

## Using Entity Framework in your api

Add postgres sql with entity framework core to your project (use the same version as your asp net project is)

    dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL -v 2.1.0

Create a data model class

Data/Person.cs

    namespace DockerDotnetApiTutorial.Api.Data
    {
        public class Person
        {
            public int Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Mobile { get; set; }

            public string Email { get; set; }
        }
    }

Add a data context class

Data/DbContext.cs

    using Microsoft.EntityFrameworkCore;

    namespace DockerDotnetApiTutorial.Api.Data
    {
        public class MyDbContext: DbContext
        {
            public MyDbContext(DbContextOptions options): base(options)
            {
                
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForNpgsqlUseIdentityAlwaysColumns();
            }

            public DbSet<Person> Persons { get;set; }
        }
    }

Extend Startup class

Startup.cs

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        var connectionString = "User ID=postgres;Password=admin;Host=192.168.99.100;Port=5432;Database=DockerDotnetApiTutorial;";
        services.AddDbContext<MyDbContext>(o => o.UseNpgsql(connectionString));
    }

HomeController.cs

    private readonly MyDbContext dbContext;

    public HomeController(MyDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Person[] GetPersons()
    {
        return dbContext.Persons.ToArray();
    }

Run the app and check the url https://localhost:5001/Home/GetPersons. You will receive an error that the database does not exist.

## Publish your database schema

Add a migration that contains your person class.

    dotnet ef migrations add AddedPersons

Apply the migrations to the database

    dotnet ef database update

Start your app again and check that you get an empty array.

## Create a person via postman api call

Controllers/HomeController.cs

    [HttpPost]
    public Person PostPerson([FromBody]Person person)
    {
        dbContext.Persons.Add(person);

        dbContext.SaveChanges();

        return person;
    }

Start postman and post a request to https://localhost:5001/Home/PostPerson with body containing a json like this:

    {
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@mhp.com"
    }

Check the person was created https://localhost:5001/Home/GetPersons.

## Get your api running in docker

    dotnet publish
    docker build .
    docker run -p 80:80 <imageid>

Check and be happy.

## Create a stack

