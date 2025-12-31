using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoAppV01.Models
{
    [Table("ToDoItem")]
    public class ToDoItem
    {
        public int Id { get; set; }
        public string ItemTitle { get; set; }
        public bool IsCompleted { get; set; }
        // Foreign key to associate with ToDoList
        public int ToDoListId { get; set; }
        public bool IsRepetitive { get; set; } // gibt an, ob die Aufgabe wiederholend ist
        public TimeSpan RepetitiveInterval { get; set; } // wiederholender Intervall e.g., daily, weekly
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        // Navigation back to parent
        public ToDoList? ToDoList { get; set; }
    }
}
