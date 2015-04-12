using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using UltimateSoftwareAppointmentAPI.Models;
using UltimateSoftwareAppointmentAPI.Models.ViewModel;

namespace UltimateSoftwareAppointmentAPI.Controllers
{
    
    public class AppointmentController : ApiController
    {
        private us_dbEntities db = new us_dbEntities();
        //Get: api/controller
        /// <summary>
        /// Get all appointments.
        /// </summary>
        /// <returns></returns>
        public IQueryable<appointment> Getappointments()
        {
            return db.appointments;
        }

        /// <summary>
        /// Get appointment from the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [ResponseType(typeof(appointment))]
        public IHttpActionResult Getappointment(int id)
        {
            appointment appointment = db.appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        /// <summary>
        /// Get appointments using the specified params: Start Date (yyyy-mm-dd) and End Date (yyyy-mm-dd)
        /// </summary>
        /// <param name="startdate">The start date.</param>
        /// <param name="enddate">The end date.</param>
        /// <returns></returns>
        [ResponseType(typeof(appointment))]
        [Route("api/appointment/startdate={startdate}/enddate={enddate}")]
        public IHttpActionResult Getappointment(string startdate, string enddate)
        {
            if (startdate == null ||enddate == null)
            {
                return BadRequest();
            }
            DateTime sd = Convert.ToDateTime(startdate);
            DateTime ed = Convert.ToDateTime(enddate);

            //Combine the Dates and Times together
            DateTime sdt = new DateTime(sd.Year, sd.Month, sd.Day, 0 , 0, 0);
            DateTime edt = new DateTime(ed.Year, ed.Month, ed.Day,23, 59,59);

            //Find the appointments that match the filter
            var appointment = db.appointments.Where(time => time.start_time > sdt && time.end_time < edt );
            
            return Ok(appointment);
        }

        /// <summary>
        /// Get appointments with the specified params: Start Date (yyyy-mm-dd), Start Time (hhmm), End Date (yyyy-mm-dd) and End Time (hhmm).
        /// </summary>
        /// <param name="startdate">The start date.</param>
        /// <param name="starttime">The start time.</param>
        /// <param name="enddate">The end date.</param>
        /// <param name="endtime">The end time.</param>
        /// <returns></returns>
        [ResponseType(typeof(appointment))]
        [Route("api/appointment/startdate={startdate}/starttime={starttime}/enddate={enddate}/endtime={endtime}")]
        public IHttpActionResult Getappointment(string startdate, int starttime, string enddate, int endtime)
        {
            if (startdate == null || enddate == null )
            {
                return BadRequest();
            }
            //Convert Date parameters to DateTime
            var sd = Convert.ToDateTime(startdate);
            var ed = Convert.ToDateTime(enddate);

            //Convert Military time to Date Time
            int Hours = starttime / 100;
            int Minutes = starttime - Hours * 100;
            var st = DateTime.MinValue;
            st = st.AddHours(Hours);
            st = st.AddMinutes(Minutes);

            Hours = endtime / 100;
            Minutes = endtime - Hours * 100;
            var et = DateTime.MinValue;
            et = et.AddHours(Hours);
            et = et.AddMinutes(Minutes);
            
            //Combine the Dates and Times together
            var sdt = new DateTime(sd.Year, sd.Month, sd.Day, st.Hour, st.Minute, st.Second);
            var edt = new DateTime(ed.Year, ed.Month, ed.Day, et.Hour, et.Minute, et.Second);

            //Find the appointments that match the filter
            var appointment = db.appointments.Where(time => time.start_time > sdt && time.end_time < edt );
            
            return Ok(appointment);
        }


        /// <summary>
        /// Updates the appointment with the the specified input. The appointment can't be made in the past or overlap with a current appointment.
        /// </summary>
        /// <param name="appointment">The appointment.</param>
        /// <returns></returns>
        [ResponseType(typeof(void))]
        public IHttpActionResult Putappointment(appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var availability = CheckAvailability(appointment);
            if (availability != null)
            {
                return availability;
            }

            if (0 == appointment.idappointment)
            {
                return BadRequest();
            }

            db.Entry(appointment).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!appointmentExists(appointment.idappointment))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Creates appointment for the specified input. The appointment can't be made in the past or overlap with a current appointment.
        /// </summary>
        /// <param name="newappointment">The appointment.</param>
        /// <returns></returns>
        [ResponseType(typeof(appointment))]
        public IHttpActionResult Postappointment(AppointmentViewModel newappointment)
        {
            appointment appointment = new appointment()
            {
                comments = newappointment.comments,
                start_time = newappointment.start_time,
                end_time = newappointment.end_time,
                first_name = newappointment.first_name,
                last_name = newappointment.last_name
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var availability = CheckAvailability(appointment);
            if (availability != null)
            {
                return availability;
            }

            db.appointments.Add(appointment);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = appointment.idappointment }, appointment);
        }

        
        /// <summary>
        /// Delete appointment for the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [ResponseType(typeof(appointment))]
        public IHttpActionResult Deleteappointment(int id)
        {
            appointment appointment = db.appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            db.appointments.Remove(appointment);
            db.SaveChanges();

            return Ok(appointment);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Checks the availability of appointment.
        /// </summary>
        /// <param name="appointment">The appointment.</param>
        /// <returns></returns>
        private IHttpActionResult CheckAvailability(appointment appointment)
        {
            //Check if end time is earlier than start time
            if (appointment.start_time > appointment.end_time)
            {
                return BadRequest("Start is after the end time, please correct the appointment time.");
            }

            /*When creating or updating a resource, it should only be considered valid if the 
             * start and end times are in the future and do not overlap an existing 
             * appointment on the same day
             */
            if (appointment.start_time < DateTime.Now || appointment.end_time < DateTime.Now)
            {
                return BadRequest("Start/End time is in the past. Please update your post.");
            }

            //Pull all appointments
            var dayOfAppointments = db.appointments.AsNoTracking().ToList();
            var samedayappointments = new List<appointment>();

            //Remove all appointments that don't fall on the same day
            foreach (var singleappointment in dayOfAppointments)
            {
                if (singleappointment.start_time.Date == appointment.start_time.Date)
                    samedayappointments.Add(singleappointment);
            }
            
            //Check each appointment that day to see if there is an overlap in schedules
            foreach (var singleappointment in samedayappointments)
            {
                if ((appointment.start_time >= singleappointment.start_time &&
                    appointment.start_time <= singleappointment.end_time) ||
                    (appointment.end_time <= singleappointment.end_time &&
                    appointment.end_time >= singleappointment.start_time))
                    return BadRequest("There is already a booking between " +
                                        singleappointment.start_time.TimeOfDay +
                                        " and " +
                                        singleappointment.end_time.TimeOfDay +
                                        " Please submit another time");
            }
            return null;

        }
        
        private bool appointmentExists(int id)
        {
            return db.appointments.Count(e => e.idappointment == id) > 0;
        }
    }
}