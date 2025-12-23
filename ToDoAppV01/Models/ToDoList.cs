namespace ToDoAppV01.Models
{
    public class ToDoList
    {
        public int Id { get; set; }
        public string ListTitle { get; set; }
        public string ListDescription { get; set; }
        public List<ToDoItem> Items { get; set; }
        // public int UserId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }
}
