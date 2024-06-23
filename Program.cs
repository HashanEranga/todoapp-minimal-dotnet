using Microsoft.AspNetCore.Http.HttpResults;
using todoapp_minimal_dotnet.models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var todos = new List<Todo>(){
    new(){
        Id = 1,
        Description = "Todo item 1",
        CreatedOn = DateTime.Now,
    },
    new(){
        Id = 2,
        Description = "Todo item 2",
        CreatedOn = DateTime.Now
    }
};

app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => {
    var targetTodo = todos.SingleOrDefault(todo => todo.Id == id);
    return targetTodo is null ?
    TypedResults.NotFound() : 
    TypedResults.Ok(targetTodo);
});

app.MapPost("/todos", (Todo task) => {
    todos.Add(task);
    return TypedResults.Created("todos/{id}", task);
});

app.MapGet("/todos", () => todos);

app.MapDelete("/todos/{id}", (int id) => {
    todos.RemoveAll(todo => todo.Id == id);
    return TypedResults.NoContent();
});

app.MapPatch("/todos/{id}", Results<Ok<Todo>, NoContent> (int id, Todo newTask) => {
    var currentTask = todos.SingleOrDefault(todo => todo.Id == id);
    todos.RemoveAll(todo => todo.Id == id);
    if(currentTask is not null)
    {
        currentTask.CreatedOn = newTask.CreatedOn;
        currentTask.Description = newTask.Description;
        currentTask.IsCompleted = newTask.IsCompleted;
        todos.Add(currentTask);
        return TypedResults.Ok(currentTask);
    }    
    return TypedResults.NoContent();
});

app.Run();
