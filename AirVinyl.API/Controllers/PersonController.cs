using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class PersonController : ODataController
    {
        private readonly AirVinylDbContext _ctx = new AirVinylDbContext();

        [HttpGet]
        [ODataRoute("Tim")]
        public IHttpActionResult GetSingletonKevin()
        {
            var personTim = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            return Ok(personTim);
        }

        [HttpGet]
        [ODataRoute("Tim/Email")]
        [ODataRoute("Tim/FirstName")]
        [ODataRoute("Tim/LastName")]
        [ODataRoute("Tim/DateOfBirth")]
        [ODataRoute("Tim/Gender")]
        public IHttpActionResult GetPersonProperty()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null) return NotFound();

            var propertyToGet = Url.Request.RequestUri.Segments.Last();
            if (person.HasProperty(propertyToGet) == false) return NotFound();

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null) return StatusCode(HttpStatusCode.NoContent);

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [ODataRoute("Tim/Email/$value")]
        [ODataRoute("Tim/FirstName/$value")]
        [ODataRoute("Tim/LastName/$value")]
        [ODataRoute("Tim/DateOfBirth/$value")]
        [ODataRoute("Tim/Gender/$value")]
        public IHttpActionResult GetPersonRawProperty()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null) return NotFound();

            var propertyToGet = Url.Request.RequestUri.Segments[Url.Request.RequestUri.Segments.Length - 2]
                .TrimEnd('/');
            if (person.HasProperty(propertyToGet) == false) return NotFound();

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null) return StatusCode(HttpStatusCode.NoContent);

            return this.CreateOKHttpActionResult(propertyValue.ToString());
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("Tim/Friends")]
        public IHttpActionResult GetPersonCollectionProperty()
        {
            var person = _ctx.People.Include(p => p.Friends).Include(p => p.VinylRecords)
                .FirstOrDefault(p => p.PersonId == 6);
            if (person == null) return NotFound();

            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            if (person.HasProperty(collectionPropertyToGet) == false) return NotFound();

            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("Tim/VinylRecords")]
        public IHttpActionResult GetVinylRecordsForPerson()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null) return NotFound();

            return Ok(_ctx.VinylRecords.Where(v => v.Person.PersonId == 6));
        }

        [HttpPatch]
        [ODataRoute("Tim")]
        public IHttpActionResult PartiallyUpdateTime(Delta<Person> patch)
        {
            if (ModelState.IsValid == false) return BadRequest(ModelState);

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (currentPerson == null) return NotFound();

            patch.Patch(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}