using System.Collections.Generic;
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
    public class RecordStoresController : ODataController
    {
        // context
        private readonly AirVinylDbContext _ctx = new AirVinylDbContext();

        // GET odata/RecordStores
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.RecordStores);
        }

        // GET odata/RecordStores(key)
        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            var recordStores = _ctx.RecordStores.Where(p => p.RecordStoreId == key);

            if (!recordStores.Any()) return NotFound();

            return Ok(SingleResult.Create(recordStores));
        }

        [HttpGet]
        [ODataRoute("RecordStores({key})/Tags")]
        [EnableQuery]
        public IHttpActionResult GetRecordStoreTagsProperty([FromODataUri] int key)
        {
            // no Include necessary for EF - Tags isn't a navigation property 
            // in the entity model.  
            var recordStore = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);

            if (recordStore == null) return NotFound();

            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var collectionPropertyValue = recordStore.GetValue(collectionPropertyToGet);

            // return the collection of tags
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [ODataRoute("RecordStores({key})/AirVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
        public bool IsHighRated([FromODataUri] int key, int minimumRating)
        {
            var recordStore = _ctx.RecordStores.FirstOrDefault(rs =>
                rs.RecordStoreId == key &&
                rs.Ratings.Any() &&
                rs.Ratings.Sum(rating => rating.Value) / rs.Ratings.Count >= minimumRating);

            return recordStore != null;
        }

        [HttpGet]
        [ODataRoute("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={personIds})")]
        public IHttpActionResult AreRatedBy([FromODataUri] IEnumerable<int> personIds)
        {
            var recordStores =
                _ctx.RecordStores.Where(rs =>
                    rs.Ratings.Any(rating =>
                        personIds.Contains(rating.RatedBy.PersonId)));

            return this.CreateOKHttpActionResult(recordStores);
        }

        [HttpGet]
        [ODataRoute("GetHighRatedRecordStores(minimumRating={minimumRating})")]
        public IHttpActionResult IsHighRated(int minimumRating)
        {
            var recordStores = _ctx.RecordStores.Where(rs =>
                rs.Ratings.Any() &&
                rs.Ratings.Sum(rating => rating.Value) / rs.Ratings.Count >= minimumRating);

            return this.CreateOKHttpActionResult(recordStores);
        }

        [HttpPost]
        [ODataRoute("RecordStores({key})/AirVinyl.Actions.Rate")]
        public IHttpActionResult Rate([FromODataUri] int key, ODataActionParameters parameters)
        {
            var recordStore = _ctx.RecordStores.FirstOrDefault(rs => rs.RecordStoreId == key);
            if (recordStore == null) return NotFound();

            if (parameters.TryGetValue("rating", out var outputFromDictionary) == false) return NotFound();
            if (int.TryParse(outputFromDictionary.ToString(), out var rating) == false) return NotFound();

            if (parameters.TryGetValue("personId", out outputFromDictionary) == false) return NotFound();
            if (int.TryParse(outputFromDictionary.ToString(), out var personId) == false) return NotFound();

            var person = _ctx.People.FirstOrDefault(rs => rs.PersonId == personId);
            if (person == null) return NotFound();

            recordStore.Ratings.Add(new Rating {RatedBy = person, Value = rating});

            return _ctx.SaveChanges() > -1 ? this.CreateOKHttpActionResult(true) : this.CreateOKHttpActionResult(false);
        }

        [HttpPost]
        [ODataRoute("RecordStores/AirVinyl.Actions.RemoveRatings")]
        public IHttpActionResult RemoveRatings(ODataActionParameters parameters)
        {
            if (parameters.TryGetValue("personId", out var outputFromDictionary) == false) return NotFound();
            if (int.TryParse(outputFromDictionary.ToString(), out var personId) == false) return NotFound();

            var person = _ctx.People.FirstOrDefault(rs => rs.PersonId == personId);
            if (person == null) return NotFound();

            var recordStoresRatedByCurrentPerson =
                _ctx.RecordStores
                    .Include("Ratings")
                    .Include("Ratings.RatedBy")
                    .Where(rs => rs.Ratings.Any(rating => rating.RatedBy.PersonId == personId))
                    .ToList();

            foreach (var store in recordStoresRatedByCurrentPerson)
            {
                var ratingsByCurrentPerson =
                    store.Ratings.Where(rating => rating.RatedBy.PersonId == personId).ToList();

                foreach (var rating in ratingsByCurrentPerson) store.Ratings.Remove(rating);
            }

            return _ctx.SaveChanges() > -1 ? this.CreateOKHttpActionResult(true) : this.CreateOKHttpActionResult(false);
        }

        [HttpPost]
        [ODataRoute("RemoveRecordStoreRatings")]
        public IHttpActionResult RemoveRecordStoreRatings(ODataActionParameters parameters)
        {
            if (parameters.TryGetValue("personId", out var outputFromDictionary) == false) return NotFound();
            if (int.TryParse(outputFromDictionary.ToString(), out var personId) == false) return NotFound();

            var person = _ctx.People.FirstOrDefault(rs => rs.PersonId == personId);
            if (person == null) return NotFound();

            var recordStoresRatedByCurrentPerson =
                _ctx.RecordStores
                    .Include("Ratings")
                    .Include("Ratings.RatedBy")
                    .Where(rs => rs.Ratings.Any(rating => rating.RatedBy.PersonId == personId))
                    .ToList();

            foreach (var store in recordStoresRatedByCurrentPerson)
            {
                var ratingsByCurrentPerson =
                    store.Ratings.Where(rating => rating.RatedBy.PersonId == personId).ToList();

                foreach (var rating in ratingsByCurrentPerson) store.Ratings.Remove(rating);
            }

            return _ctx.SaveChanges() > -1
                ? StatusCode(HttpStatusCode.NoContent)
                : StatusCode(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        [ODataRoute("RecordStores/AirVinyl.Model.SpecializedRecordStore")]
        [EnableQuery]
        public IHttpActionResult GetSpecializedRecordStores()
        {
            var specializedStores = _ctx.RecordStores.OfType<SpecializedRecordStore>();
            return Ok(specializedStores);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult GetSpecializedRecordStore([FromODataUri] int key)
        {
            var specializedStores = _ctx.RecordStores.Where(r => r is SpecializedRecordStore && r.RecordStoreId == key)
                .OfType<SpecializedRecordStore>();
            if (specializedStores.Any() == false) return NotFound();

            return Ok(SingleResult.Create(specializedStores));
        }

        [HttpPost]
        [ODataRoute("RecordStores")]
        public IHttpActionResult CreateRecordStore(RecordStore recordStore)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // add the RecordStore
            _ctx.RecordStores.Add(recordStore);
            _ctx.SaveChanges();

            // return the created RecordStore 
            return Created(recordStore);
        }

        [HttpPatch]
        [ODataRoute("RecordStores({key})")]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult UpdateRecordStorePartially([FromODataUri] int key, Delta<RecordStore> patch)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // find a matching record store
            var currentRecordStore = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);

            // if the record store isn't found, return NotFound
            if (currentRecordStore == null) return NotFound();

            patch.Patch(currentRecordStore);
            _ctx.SaveChanges();

            // return NoContent
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("RecordStores({key})")]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult DeleteRecordStore([FromODataUri] int key)
        {
            var currentRecordStore = _ctx.RecordStores.Include("Ratings")
                .FirstOrDefault(p => p.RecordStoreId == key);
            if (currentRecordStore == null) return NotFound();

            currentRecordStore.Ratings.Clear();
            _ctx.RecordStores.Remove(currentRecordStore);
            _ctx.SaveChanges();

            // return NoContent
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            // dispose the context
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}