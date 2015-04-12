using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web.Http.Results;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltimateSoftwareAppointmentAPI.Controllers;
using UltimateSoftwareAppointmentAPI.Models;
using UltimateSoftwareAppointmentAPI.Models.ViewModel;


namespace UltimateSoftwareAppointmentAPI.Tests.Controllers
{
    /// <summary>
    /// Summary description for AppointmentControllerTest
    /// </summary>
    [TestClass]
    public class AppointmentControllerTest
    {

        [TestMethod]
        public void Get()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();

            // Act
            IQueryable<appointment> result = controller.Getappointments();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotSame(0, result.Count());
            
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();

            // Act
            var result = controller.Getappointment(64);
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<appointment>));
        }

        [TestMethod]
        public void GetByStartandEndDate()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();
            string start = "2014-04-11";
            string end = "2014-04-11";

            // Act
            var result = controller.Getappointment(start, end);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<IQueryable<appointment>>));
        }

        [TestMethod]
        public void GetByStartandEndDateTime()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();
            string start = "2014-04-11";
            string end = "2014-04-11";
            int starttime = 600;
            int endtime = 2000;

            // Act
            var result = controller.Getappointment(start, starttime, end, endtime);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<IQueryable<appointment>>));
        }


        [TestMethod]
        public void Post()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();
            AppointmentViewModel app = new AppointmentViewModel()
            {
                comments="",
                end_time =  DateTime.Now.AddSeconds(500),
                start_time = DateTime.Now.AddSeconds(400),
                first_name = "Ultimate",
                last_name = "Software"
            };
            

            // Act
            var result = controller.Postappointment(app);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtRouteNegotiatedContentResult<appointment>));

        }

        [TestMethod]
        public void Put()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();
            appointment app = new appointment()
            {
                comments = "",
                end_time = DateTime.Now.AddSeconds(50),
                start_time = DateTime.Now.AddSeconds(40),
                first_name = "Ultimate",
                last_name = "Software",
                idappointment = 100
            };
            // Act
            var result = controller.Putappointment(app);

            // Assert
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            AppointmentController controller = new AppointmentController();

            // Act
            var result = controller.Deleteappointment(106);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<appointment>));
        }
    }
}
