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
    [Route("api/Room/{action}", Name = "RoomsApi")]
    public class RoomsController : ApiController
    {
        private StopoverDbContext _context = new StopoverDbContext();

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var room = _context.Room.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.HotelId,
                i.RoomTypeId,
                i.FirstNightPrice,
                i.SecondNightPrice,
                i.FirstNightPriceNet,
                i.SecondNightPriceNet,
                i.PromoNetPrice,
                i.Comments
            }).Where(i => i.DeletedDate == null);
            return Request.CreateResponse(DataSourceLoader.Load(room, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Room();
            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedDate = null;
            Validate(model);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Room.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.Id);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Room.FirstOrDefault(item => item.Id == key);
            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Room not found");

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
            var model = _context.Room.FirstOrDefault(item => item.Id == key);

            if (model == null)
            {
                Request.CreateResponse(NotFound());
            }

            model.DeletedDate = DateTime.UtcNow;
            _context.SaveChanges();
        }


        [HttpGet]
        public HttpResponseMessage HotelLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Hotel
                         orderby i.NameCode
                         where i.DeletedDate == null
                         select new {
                             Value = i.Id,
                             Text = i.NameCode
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage RoomTypeLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.RoomType
                         orderby i.NameCode
                         where i.DeletedDate == null
                         select new {
                             Value = i.Id,
                             Text = (from t in _context.Translation
                                     where i.NameCode == t.NameCode && t.LangCode == "EN"
                                     select t.Text)
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        public HttpResponseMessage GetRoomsForHotel(int HotelId, DataSourceLoadOptions loadOptions)
        {
            var room = from r in _context.Room
                       where r.DeletedDate == null
                       where r.HotelId == HotelId
                       select new
                       {
                           r.Id,
                           r.CreatedDate,
                           r.UpdatedDate,
                           r.DeletedDate,
                           r.HotelId,
                           r.RoomTypeId,
                           r.FirstNightPrice,
                           r.SecondNightPrice,
                           r.FirstNightPriceNet,
                           r.SecondNightPriceNet,
                           r.PromoNetPrice,
                           r.Comments
                       };
            return Request.CreateResponse(DataSourceLoader.Load(room, loadOptions));
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