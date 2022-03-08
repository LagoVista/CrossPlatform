using LagoVista.Core.Models;

namespace BugLog.Models
{
    public class TimeEntry
    {
        public string UserId { get; set; }
        public double Hours { get; set; }
        public string Notes { get; set; }
        public string Date { get; set; }
        public EntityHeader Project { get; set; }
        public EntityHeader WorkTask { get; set; }
    }
}
