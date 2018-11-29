using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace StopoverAdminPanel.Models.Controllers
{
    [Route("api/Hotel/{action}", Name = "HotelsApi")]
    public class HotelsController : ApiController
    {
        private StopoverDbContext _context = new StopoverDbContext();

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var hotel = _context.Hotel.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.NameCode,
                i.CityId,
                i.Email,
                i.IsPromo,
                i.Enabled,
                i.Comments
            }).Where(i => i.DeletedDate == null);

            return Request.CreateResponse(DataSourceLoader.Load(hotel, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Hotel();
            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedDate = null;

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Hotel.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.Id);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Hotel.FirstOrDefault(item => item.Id == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Hotel not found");

            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.UpdatedDate = DateTime.UtcNow;

            //Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Hotel.FirstOrDefault(item => item.Id == key);

            if (model == null)
            {
                Request.CreateResponse(NotFound());
            }

            model.DeletedDate = DateTime.UtcNow;
            _context.SaveChanges();
        }

        [HttpGet]
        public HttpResponseMessage NameCodeLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.Translation
                         orderby i.NameCode
                         where i.DeletedDate == null && i.LangCode == "EN"
                         select new
                         {
                             Value = i.NameCode,
                             Text = i.Text
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }


        [HttpGet]
        public HttpResponseMessage CityLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.City
                         orderby i.IataCode
                         where i.DeletedDate == null
                         select new {
                             Value = i.Id,
                             Text = i.IataCode
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }


        [HttpGet]
        public HttpResponseMessage GetHotelsForOrderStopover(int hotelId, DataSourceLoadOptions loadOptions)
        {
            var hotel = _context.Hotel.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.NameCode,
                i.CityId,
                i.Email,
                i.IsPromo,
                i.Enabled,
                i.Comments
            }).Where(i => i.DeletedDate == null).Where(i => i.Id == hotelId);
            return Request.CreateResponse(DataSourceLoader.Load(hotel, loadOptions));
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}