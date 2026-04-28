using Demo.Models;
using System.Collections.Generic;

namespace Demo.ViewModels
{
    public class MyBookingsViewModel
    {
        public List<DatSan> UpcomingBookings { get; set; }
        public List<DatSan> PastBookings { get; set; }
        public int TotalBookingsCount { get; set; }

        public string HoTen { get; set; }
        public string AnhDaiDien { get; set; }
    }

}