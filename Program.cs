
using GCalendarSyncWithDB;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalendarQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "Google Calendar API .NET CalendarQuickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                    HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            String calendarId = "primary";
            EventsResource.ListRequest request = service.Events.List(calendarId);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            

            Events events = request.Execute();
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    //compare with items in the database 
                    using (var db = new SyncContext())
                    {
                        foreach (var item in db.EventTable)
                        {
                            //create db entry obj corresponding to event in the calendar
                            EventTable E = new EventTable() { Title = eventItem.Summary, Description = " ", Date = Convert.ToDateTime(eventItem.Start.Date) };
                            db.SaveChanges();
                            //if entry in db is not same as in the calendar, copy events from db to the calendar
                            if (item != E)
                            {
                                //event not in db
                                db.EventTable.Add(E);
                                Console.WriteLine("Event added to database: {0}", E.Title);
                                //create event obj corresponding to entry in the db

                                Event newEvent = new Event()
                                {
                                    Summary = item.Title,
                                    Description = item.Description,
                                    Start = new EventDateTime()
                                    {
                                        DateTime = item.Date,
                                    },
                                    End = new EventDateTime()
                                    {
                                        DateTime = item.Date,
                                    }
                                };

                                //check not in calendar
                                EventsResource.InsertRequest request2 = service.Events.Insert(newEvent, calendarId);
                                Event createdEvent = request2.Execute();
                                Console.WriteLine("Event created in calendar: {0}, {1}", newEvent.Summary,createdEvent.HtmlLink);
                                Console.Read();
                            }
                        }
                    }        
                    
                }
            }         
        }
    }
}