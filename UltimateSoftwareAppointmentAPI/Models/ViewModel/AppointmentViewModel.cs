using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UltimateSoftwareAppointmentAPI.Models.ViewModel
{
    public class AppointmentViewModel
    {
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string comments { get; set; }
    }
}