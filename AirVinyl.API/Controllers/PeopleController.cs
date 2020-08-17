using System;
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
    public class PeopleController : ODataController
    {
        private readonly AirVinylDbContext _ctx = new AirVinylDbContext();

        [EnableQuery(MaxExpansionDepth = 3, MaxSkip = 10, MaxTop = 5, PageSize = 4)]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.People);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            var people = _ctx.People.Where(p => p.PersonId == key);
            if (people.Any() == false) return NotFound();

            return Ok(SingleResult.Create(people));
        }

        [HttpGet]
        [ODataRoute("People({key})/Email")]
        [ODataRoute("People({key})/FirstName")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/DateOfBirth")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonProperty([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            var propertyToGet = Url.Request.RequestUri.Segments.Last();
            if (person.HasProperty(propertyToGet) == false) return NotFound();

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null) return StatusCode(HttpStatusCode.NoContent);

            return Created(propertyValue);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetVinylRecordsForPerson([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            return Ok(_ctx.VinylRecords.Where(v => v.Person.PersonId == key));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult GetVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            var vinylRecords =
                _ctx.VinylRecords.Where(v => v.Person.PersonId == key && v.VinylRecordId == vinylRecordKey);
            if (vinylRecords.Any() == false) return NotFound();

            return Ok(SingleResult.Create(vinylRecords));
        }

        [HttpPost]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult CreateVinylRecordForPerson([FromODataUri] int key, VinylRecord vinylRecord)
        {
            if (ModelState.IsValid == false) return BadRequest(ModelState);

            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            vinylRecord.Person = person;

            _ctx.VinylRecords.Add(vinylRecord);
            _ctx.SaveChanges();

            return Created(vinylRecord);
        }

        [HttpPatch]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult PartiallyUpdateVinylRecordForPerson([FromODataUri] int key,
            [FromODataUri] int vinylRecordKey, Delta<VinylRecord> patch)
        {
            if (ModelState.IsValid == false) return BadRequest(ModelState);

            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            var currentVinylRecord =
                _ctx.VinylRecords.FirstOrDefault(v => v.Person.PersonId == key && v.VinylRecordId == vinylRecordKey);
            if (currentVinylRecord == null) return NotFound();

            patch.Patch(currentVinylRecord);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult DeleteVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            var currentVinylRecord =
                _ctx.VinylRecords.FirstOrDefault(v => v.Person.PersonId == key && v.VinylRecordId == vinylRecordKey);
            if (currentVinylRecord == null) return NotFound();

            _ctx.VinylRecords.Remove(currentVinylRecord);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ODataRoute("People({key})/Friends")]
        [EnableQuery]
        public IHttpActionResult GetPersonCollectionProperty([FromODataUri] int key)
        {
            var person = _ctx.People.Include(p => p.Friends).Include(p => p.VinylRecords)
                .FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            if (person.HasProperty(collectionPropertyToGet) == false) return NotFound();

            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [ODataRoute("People({key})/Email/$value")]
        [ODataRoute("People({key})/FirstName/$value")]
        [ODataRoute("People({key})/LastName/$value")]
        [ODataRoute("People({key})/DateOfBirth/$value")]
        [ODataRoute("People({key})/Gender/$value")]
        public IHttpActionResult GetPersonPropertyRawValue([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null) return NotFound();

            var propertyToGet = Url.Request.RequestUri.Segments[Url.Request.RequestUri.Segments.Length - 2]
                .TrimEnd('/');
            if (person.HasProperty(propertyToGet) == false) return NotFound();

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null) return StatusCode(HttpStatusCode.NoContent);

            return this.CreateOKHttpActionResult(propertyValue.ToString());
        }

        public IHttpActionResult Post(Person person)
        {
            if (ModelState.IsValid == false) return BadRequest(ModelState);

            _ctx.People.Add(person);
            _ctx.SaveChanges();

            return Created(person);
        }

        public IHttpActionResult Put([FromODataUri] int key, Person person)
        {
            if (ModelState.IsValid == false) return BadRequest(ModelState);

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null) return NotFound();

            person.PersonId = currentPerson.PersonId;
            _ctx.Entry(currentPerson).CurrentValues.SetValues(person);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> patch)
        {
            if (ModelState.IsValid == false) return BadRequest(ModelState);

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null) return NotFound();

            patch.Patch(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var currentPerson = _ctx.People.Include(p => p.Friends).FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null) return NotFound();

            var peopleWithCurrentPersonAsFriends = _ctx.People
                .Include(p => p.Friends)
                .Where(p => p.Friends.Select(f => f.PersonId).AsQueryable().Contains(key));

            foreach (var person in peopleWithCurrentPersonAsFriends) person.Friends.Remove(currentPerson);

            _ctx.People.Remove(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST odata/People('key')/Friends/$ref
        [HttpPost]
        [ODataRoute("People({key})/Friends/$ref")]
        public IHttpActionResult CreateLinkToFriend([FromODataUri] int key, [FromBody] Uri link)
        {
            var currentPerson = _ctx.People.Include(p => p.Friends).FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null) return NotFound();

            var keyOfFriendToAdd = Request.GetKeyValue<int>(link);

            if (currentPerson.Friends.Any(f => f.PersonId == keyOfFriendToAdd))
                return BadRequest(
                    $"The person with id {key} is already linked to the person with id {keyOfFriendToAdd}");

            var friendToLinkTo = _ctx.People.FirstOrDefault(f => f.PersonId == keyOfFriendToAdd);
            if (friendToLinkTo == null) return NotFound();

            currentPerson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult UpdateLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey,
            [FromBody] Uri link)
        {
            var currentPerson = _ctx.People.Include(p => p.Friends).FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null) return NotFound();

            var currentFriend = _ctx.People.Include(p => p.Friends).FirstOrDefault(p => p.PersonId == relatedKey);
            if (currentFriend == null) return NotFound();

            var keyOfFriendToAdd = Request.GetKeyValue<int>(link);

            if (currentPerson.Friends.Any(f => f.PersonId == keyOfFriendToAdd))
                return BadRequest(
                    $"The person with id {key} is already linked to the person with id {keyOfFriendToAdd}");

            var friendToLinkTo = _ctx.People.FirstOrDefault(f => f.PersonId == keyOfFriendToAdd);
            if (friendToLinkTo == null) return NotFound();

            currentPerson.Friends.Remove(currentPerson);
            currentFriend.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult DeleteLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey)
        {
            var currentPerson = _ctx.People.Include(p => p.Friends).FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null) return NotFound();

            var currentFriend = _ctx.People.Include(p => p.Friends).FirstOrDefault(p => p.PersonId == relatedKey);
            if (currentFriend == null) return NotFound();

            currentPerson.Friends.Remove(currentFriend);
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