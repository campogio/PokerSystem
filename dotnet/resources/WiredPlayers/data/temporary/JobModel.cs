using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class JobModel
    {
        public string DescriptionMale { get; set; }
        public string DescriptionFemale { get; set; }
        public PlayerJobs Job { get; set; }
        public int Salary { get; set; }

        public JobModel(string descriptionMale, string descriptionFemale, PlayerJobs job, int salary)
        {
            DescriptionMale = descriptionMale;
            DescriptionFemale = descriptionFemale;
            Job = job;
            Salary = salary;
        }
    }
}
