using System.Linq;
using Microsoft.AspNetCore.Mvc;

using DockerDotnetApiTutorial.Api.Data;

namespace DockerDotnetApiTutorial.Api.Controllers
{
    public class HomeController: Controller 
    {
        private readonly MyDbContext dbContext;

        public HomeController(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Index()
        {
            return "Hello World from API!";
        }

        public Person[] GetPersons()
        {
            return dbContext.Persons.ToArray();
        }

        [HttpPost]
        public Person PostPerson([FromBody]Person person)
        {
            dbContext.Persons.Add(person);

            dbContext.SaveChanges();

            return person;
        }
    }    
}