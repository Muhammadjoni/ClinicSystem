using System;
using System.Collections.Generic;
using System.Linq;
using ClinicSystem.Models;
using ClinicSystem.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections;

namespace Clinic.Controllers
{
  // [Authorize]
  [ApiController]
  [Route("api/doctors")]
  public class DoctorController : ControllerBase
  {
    private readonly ApplicationDbContext context;
    public DoctorController(ApplicationDbContext context)
    {
      this.context = context;
    }

    [HttpGet]
    public IEnumerable<String> GetAllDoctors()
    {

      string docRoleID = context.Roles.Where(d => d.Name.Equals("Doctor")).Select(x => x.Id).SingleOrDefault();
      List<String> docIDs = context.UserRoles.Where(d => d.RoleId.Equals(docRoleID)).Select(x => x.UserId).ToList();
      List<String> docs = new List<string>();

          foreach (var n in docIDs)
          {
            docs.Add(context.Users.Where(d => d.Id.Equals(n)).Select(x => x.Id).Single());
          }

      return docs;

    }

//----------------------------------------------------------------------------------------------------------------------------------------------------------

    //View doc info
    [HttpGet("{id}")]
    public async Task GetDoctorByID(string id)
    {
      try
        {

        // result.Id = id;
          // string docRoleID = context.Roles.Where(d => d.Name.Equals("Doctor")).Select(x => x.Id).Single();
          // List<String> docIDs = context.UserRoles.Where(d => d.RoleId.Equals(docRoleID)).Select(x => x.UserId).ToList();

        // DocInfo result = new DocInfo();


        // // Adding elements in Hashtable
        //   result.Add("Username", context.Users.Where(d => d.Id.Equals(id)).Select(x => x.UserName).SingleOrDefault());

        // // Get a collection of the keys.
        // ICollection c = result.key;

        // // Displaying the contents
        // foreach (string str in c)
        //   Console.WriteLine(str + ": " + result[str]);
          var result = context.Users.Where(d => d.Id.Equals(id)).ToList();

          Response.StatusCode = 200;
          Response.ContentType = "application/json";
          await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));
        }
      catch (Exception e)
        {
          Response.StatusCode = 400;
          Response.ContentType = "application/json";
          await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e)));
        }
    }

      // string doc = context.DocInfo.Where(d => d.Id.Equals(id)).Select(x => x.Id).Single();
   //   List<String> docIDs = context.UserRoles.Where(d => d.RoleId.Equals(docRoleID)).Select(x => x.UserId).ToList();

      // DocInfo info = new DocInfo();
      // // info.Username = context.Users.Where(d => d.Id.Equals(id)).Select(x => x.UserName).SingleOrDefault(); // need to fix this issue => username
      // info.Username = context.DocInfo.Where(d => d.DoctorId.Equals(id)).SingleOrDefault();

      // return info;


//---------------------------------------------------------------------------------------------------------------------------------------------------------

    //View doctor => available slots
    [HttpGet("{id}/slots")]
    public IEnumerable<String> GetDoctorAvailableSlots(string id)
    {

      int count = context.Appointment.Where(s => s.DoctorID.Equals(id)).Count();
      List<DateTime> startDuration = context.Appointment.Where(s => s.DoctorID.Equals(id)).Select(d => d.StartTime).ToList();
      List<DateTime> endDuration = context.Appointment.Where(s => s.DoctorID.Equals(id)).Select(d => d.EndTime).ToList();

      double totalDuration = 0.0;

      List<String> open = new List<String>();//available
      List<String> closed = new List<String>();// not available
      List<DateTime> sortedSlots = new List<DateTime>();//sorted list

      var duration = startDuration.Zip(endDuration, (str, end) => new { startDuration = str, endDuration = end });
      foreach (var strend in duration)
      {
        //time difference
        System.TimeSpan timeSpan = strend.endDuration.Subtract(strend.startDuration);
        double mins = timeSpan.TotalMinutes;
        totalDuration += mins; // total of mins allocated from all appointments

        string unAvailableSlot = strend.startDuration.ToString("H:mm") + "-" + strend.endDuration.ToString("H:mm");   //MM/DD/YYYY H:mm

        closed.Add(unAvailableSlot);

        //storing open in a list (start,end)
        sortedSlots.Add(strend.startDuration);
        sortedSlots.Add(strend.endDuration);
      }

      // sort the list
      sortedSlots.Sort((a, b) => a.CompareTo(b));

      // if endtime1 != starttime2 then available slot is endtime1-starttime

      for (int i = 0; i < sortedSlots.Count / 2; i++)
      {
        if (sortedSlots[i + 1] != sortedSlots[i + 2])// giving an error
        {
          if (count != 12)
          {
            string slot = sortedSlots[i + 1].ToString("H:mm") + "-" + sortedSlots[i + 2].ToString("H:mm");
            open.Add(slot);
            count++;
          }
        }
      }
      //to get the time in hrs
      totalDuration /= 60;

      if (count == 12 || totalDuration >= 8)
        return closed;

      else
        return open;

    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    //View availability of all doctors
    [HttpGet("available")]
    public IEnumerable<DocSlot> GetAllDoctorsAvailableSlots()
    {

      List<string> ids = context.Appointment.Select(x => x.DoctorID).Distinct().ToList();
      int totalDr = ids.Count();

      List<DocSlot> response = new List<DocSlot>();

      for (int i = 0; i < ids.Count; i++)
      {
        DocSlot dr = new DocSlot();
        dr.Id = ids[i];
        dr.Slots = GetDoctorAvailableSlots(ids[i]).ToList();
        response.Add(dr);
      }
      return response;

    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    //View doctors with the most appointments in a given day
    [HttpGet("mostSlots")]
    public IEnumerable<DocSlot> DoctorMostSlots()
    {

      List<DocSlot> dr = GetAllDoctorsAvailableSlots().ToList();

      dr.Sort(delegate (DocSlot x, DocSlot y)
      {
        return y.Slots.Count().CompareTo(x.Slots.Count());
      });

      return dr;

    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    //View doctors who have 6+ hours total appointments in a day
    [HttpGet("sixHoursPlus")]
    public IEnumerable<DocSlot> DoctorSixHours()
    {

      List<DocSlot> dr = DoctorMostSlots().ToList();

      List<DocSlot> sixHoursList = new List<DocSlot>();

      for (int i = 0; i < dr.Count(); i++)
      {
        if (dr[i].Slots.Count() > 6)
        {

          sixHoursList.Add(dr[i]);
        }
      }

      return sixHoursList;
    }
  }
}
