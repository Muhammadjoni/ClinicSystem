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
  public class DoctorsController : ControllerBase
  {
    private readonly ApplicationDbContext context;
    public DoctorsController(ApplicationDbContext context)
    {
      this.context = context;
    }

    //View list of doctors
    [HttpGet]
    public IEnumerable<String> GetAllDoctors()
    {

      string drRoleID = context.Roles.Where(d => d.Name.Equals("Doctor")).Select(x => x.Id).Single();
      List<String> drIds = context.UserRoles.Where(d => d.RoleId.Equals(drRoleID)).Select(x => x.UserId).ToList();
      List<String> drs = new List<string>();

      foreach (var n in drIds)
      {
        drs.Add(context.Users.Where(d => d.Id.Equals(n)).Select(x => x.UserName).Single());
      }

      return drs;
    }


    //View doctor information
    [HttpGet("{id}")]
    public DocInfo GetDoctorByID(string id)
    {
      DocInfo info = new DocInfo();
      info.DoctorId = id;
      info.Username = context.Users.Where(d => d.Id.Equals(id)).Select(x => x.UserName).Single();
      // info.Slots = context.Appointments.Where(d => d.Id.Equals(id)).ToList();
      return info;
    }

    //View doctor => available slots
    [HttpGet("{id}/slots")]
    public IEnumerable<String> GetDoctorAvailableSlots(string id)
    {

      int count = context.Appointment.Where(s => s.DoctorID.Equals(id)).Count();// slot counting
      List<DateTime> startDuration = context.Appointment.Where(s => s.DoctorID.Equals(id)).Select(d => d.StartTime).ToList();
      List<DateTime> endDuration = context.Appointment.Where(s => s.DoctorID.Equals(id)).Select(d => d.EndTime).ToList();
      double totalDuration = 0.0;
      List<String> slots = new List<String>();//available
      List<String> uslots = new List<String>();// unavailable
      List<DateTime> sortedSlots = new List<DateTime>();//sorted list

      var duration = startDuration.Zip(endDuration, (s, e) => new { startDuration = s, endDuration = e });
      foreach (var se in duration)
      {
        //time difference
        System.TimeSpan timeSpan = se.endDuration.Subtract(se.startDuration);
        double mins = timeSpan.TotalMinutes;
        totalDuration += mins; // total of mins allocated from all appointments

        string unAvailableSlot = se.startDuration.ToString("H:mm") + "-" + se.endDuration.ToString("H:mm");
        uslots.Add(unAvailableSlot);

        //storing slots in a list (start,end)
        sortedSlots.Add(se.startDuration);
        sortedSlots.Add(se.endDuration);
      }

      // sort the list
      sortedSlots.Sort((a, b) => a.CompareTo(b));

      // if endtime1 != starttime2 then available slot is endtime1-starttime

      for (int i = 0; i < sortedSlots.Count / 2; i++)
      {
        if (sortedSlots[i + 1] != sortedSlots[i + 2])
        {
          if (count != 12)
          {
            string slot = sortedSlots[i + 1].ToString("H:mm") + "-" + sortedSlots[i + 2].ToString("H:mm");
            slots.Add(slot);
            count++;
          }
        }
      }
      //to get the time in hrs
      totalDuration /= 60;

      if (count == 12 || totalDuration >= 8)
        return uslots;

      else
        return slots;

    }
    //----------------------------------------------------------------------------------------------------------
    //--------------------------------                ----------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------
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
