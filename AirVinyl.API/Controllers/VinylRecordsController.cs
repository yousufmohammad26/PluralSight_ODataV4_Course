﻿using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using AirVinyl.DataAccessLayer;

namespace AirVinyl.API.Controllers
{
    public class VinylRecordsController : ODataController
    {
        private readonly AirVinylDbContext _ctx = new AirVinylDbContext();

        [HttpGet]
        [ODataRoute("VinylRecords")]
        public IHttpActionResult GetAllVinylRecords()
        {
            return Ok(_ctx.VinylRecords);
        }
        [HttpGet]
        [ODataRoute("VinylRecords({key})")]
        public IHttpActionResult GetOneVinylRecord([FromODataUri] int key)
        {
            var vinylRecord = _ctx.VinylRecords.FirstOrDefault(p => p.VinylRecordId == key);
            if (vinylRecord == null) return NotFound();
            return Ok(vinylRecord);
        }

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}