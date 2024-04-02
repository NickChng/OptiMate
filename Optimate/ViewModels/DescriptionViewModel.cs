using OptiMate;

namespace OptiMate.ViewModels
{
    public class DescriptionViewModel : ObservableObject
    {
        public string Id { get; set; }
        public string Description { get; set; } = "Default Description";

        public DescriptionViewModel(string id, string description)
        {
            Id = id;
            Description = description;
        }
       

    }
}
