namespace BTL_LTW_DOTJOB.Models.Mock
{
    public class CateMockData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<CateMockData> GetMockCategories()
        {
            return new List<CateMockData>
            {
                new CateMockData { Id = "1", Name = "IT", Description = "Information Technology" },
                new CateMockData { Id = "2", Name = "Finance", Description = "Financial Services" },
                new CateMockData { Id = "3", Name = "Healthcare", Description = "Medical and Health Services" },
                new CateMockData { Id = "4", Name = "Education", Description = "Educational Services" },
                new CateMockData { Id = "5", Name = "Marketing", Description = "Marketing and Advertising" },
                new CateMockData { Id = "6", Name = "IT2", Description = "Information Technology" },
                new CateMockData { Id = "7", Name = "Finance2", Description = "Financial Services" },
                new CateMockData { Id = "8", Name = "Healthcare2", Description = "Medical and Health Services" },
                new CateMockData { Id = "9", Name = "Education2", Description = "Educational Services" },
                new CateMockData { Id = "10", Name = "Marketing2", Description = "Marketing and Advertising" },
                new CateMockData { Id = "11", Name = "IT3", Description = "Information Technology" },
                new CateMockData { Id = "12", Name = "Finance3", Description = "Financial Services" },
                new CateMockData { Id = "13", Name = "Healthcare3", Description = "Medical and Health Services" },
                new CateMockData { Id = "14", Name = "Education3", Description = "Educational Services" },
                new CateMockData { Id = "15", Name = "Marketing3", Description = "Marketing and Advertising" }
            };
        }
    }
}
