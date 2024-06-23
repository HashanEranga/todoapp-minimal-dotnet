namespace todoapp_minimal_dotnet.models {
    public class Todo {
        public int Id {get;set;}
        public required string Description {get;set;}
        public DateTime CreatedOn {get;set;}
        public bool IsCompleted {get;set;} = false;
    }
}