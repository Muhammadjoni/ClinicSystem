using System;
using System.Collections.Generic;
using System.Linq;
using ClinicSystem.Models;
using ClinicSystem.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.Controllers
{
  [Authorize]
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

      string docRoleID = context.Roles.Where(d => d.Name.Equals("Doctor")).Select(x => x.Id).Single();
      List<String> drIds = context.UserRoles.Where(d => d.RoleId.Equals(docRoleID)).Select(x => x.UserId).ToList();
      List<String> drs = new List<string>();

          foreach (var n in drIds)
          {
            drs.Add(context.Users.Where(d => d.Id.Equals(n)).Select(x => x.UserName).Single());
          }

      return drs;

    }

//----------------------------------------------------------------------------------------------------------------------------------------------------------

    //View doc info
    [HttpGet("{id}")]
    public DocInfo GetDoctorByID(string id)
    {
      string docRoleID = context.Roles.Where(d => d.Name.Equals("Doctor")).Select(x => x.Id).Single();
      List<String> drIds = context.UserRoles.Where(d => d.RoleId.Equals(docRoleID)).Select(x => x.UserId).ToList();

      DocInfo info = new DocInfo();
      info.DoctorId = id;
      info.Username = context.Users.Where(d => d.Id.Equals(id)).Select(x => x.UserName).Single();

      return info;

    }

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
        dr.DoctorId = ids[i];
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
