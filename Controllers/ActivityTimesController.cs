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
    [Route("api/ActivityTime/{action}", Name = "ActivityTimesApi")]
    public class ActivityTimesController : ApiController
    {
        private StopoverDbContext _context = new StopoverDbContext();

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var activitytime = _context.ActivityTime.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.Time,
                i.ActivityId,
                i.Comments
            }).Where(i => i.DeletedDate == null);
            return Request.CreateResponse(DataSourceLoader.Load(activitytime, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new ActivityTime();
           
            // Populating the model manually, since DateTime cannot be cast as TimeSpan
            // when using JSonConvert.PopulateObject()
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            
            //There was a bug when creating ActivityTime.Time for the first time,
            //it saved it in UTC Format, when we need a local time format, if/else statement fixes it
            DateTime activityTime = Convert.ToDateTime(values["Time"]);
            var kind = activityTime.Kind;
            DateTime localTime;
            TimeSpan time_needed;
            if (kind == DateTimeKind.Utc)
            {
                localTime = activityTime.ToLocalTime();
                time_needed = localTime.TimeOfDay;

            }
            else
            {
                time_needed = activityTime.TimeOfDay;
            }
            //should fix the bug 
            model.Time = time_needed;
            model.ActivityId = Convert.ToInt32(values["ActivityId"]);
            model.Comments = Convert.ToString(values["Comments"]);
            // end populate model
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedDate = null;

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.ActivityTime.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.Id);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.ActivityTime.FirstOrDefault(item => item.Id == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "ActivityTime not found");

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
            var model = _context.ActivityTime.FirstOrDefault(item => item.Id == key);

            if (model == null)
            {
                Request.CreateResponse(NotFound());
            }

            model.DeletedDate = DateTime.UtcNow;
            _context.SaveChanges();
        }


        [HttpGet]
        public HttpResponseMessage ActivityLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Activity
                         orderby i.NameCode
                         where i.DeletedDate == null
                         select new {
                             Value = i.Id,
                             Text = i.NameCode
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage GetTimesForActivity(int activityId, DataSourceLoadOptions loadOptions)
        {
            var activitytime = _context.ActivityTime.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.Time,
                i.ActivityId,
                i.Comments
            }).Where(i => i.DeletedDate == null).Where(i => i.ActivityId == activityId);
            return Request.CreateResponse(DataSourceLoader.Load(activitytime, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage GetTimeForOrderActivity(int activityTimeId, DataSourceLoadOptions loadOptions)
        {
            var activitytime = _context.ActivityTime.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.Time,
                i.ActivityId,
                i.Comments
            }).Where(i => i.DeletedDate == null).Where(i => i.Id == activityTimeId);
            return Request.CreateResponse(DataSourceLoader.Load(activitytime, loadOptions));
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