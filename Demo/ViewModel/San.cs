using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Demo.Models;
namespace Demo.ViewModels
{
    public class San
    {
        public List<SanTheThao> SanNoiBat { get; set; }

        public List<TranDau> TranDauMoi { get; set; }

        public List<TinTuc> TinTucMoi { get; set; }
    }
}