namespace WorkflowExample
{
    public class DataContext
    {
        public string BoardPassData { get; set; }

        public bool HasValidBoardPass => !string.IsNullOrEmpty(BoardPassData);

        public bool IsDomestic { get; set; }
    }
}
