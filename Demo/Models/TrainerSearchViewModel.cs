namespace Demo.ViewModels
{
    using System.Collections.Generic;
    using Demo.Models;

    public class TrainerSearchViewModel
    {
        public string SearchTuKhoa { get; set; }
        public string MonHienTai { get; set; }
        public string Service { get; set; }
        public string AgeGroup { get; set; }
        public bool? IsAcademy { get; set; }
        public List<Trainer> Trainers { get; set; }
        public List<LoaiTheThao> TatCaMonTheThao { get; set; }
    }
}