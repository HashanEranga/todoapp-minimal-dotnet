using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;
using todoapp_minimal_dotnet.models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "todos/$1"));
app.Use(async (context, next) => {
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow} started]");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow} ended]");
});

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

app.MapGet("/todos/complete", Results<Ok<List<Todo>>, NoContent> () => {
    var completedTodos = todos.Where(todo => todo.IsCompleted).ToList();
    if(completedTodos.Count < 0) return TypedResults.NoContent();
    return TypedResults.Ok(completedTodos);
});

app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => {
    var targetTodo = todos.SingleOrDefault(todo => todo.Id == id);
    return targetTodo is null ?
    TypedResults.NotFound() : 
    TypedResults.Ok(targetTodo);
});

app.MapPost("/todos", (Todo task) => {
    todos.Add(task);
    return TypedResults.Created("todos/{id}", task);
})
.AddEndpointFilter(async (context, next) => {
    var taskArgument = context.GetArgument<Todo>(0);
    var errors = new Dictionary<string, string[]>();
    if (taskArgument.CreatedOn < DateTime.UtcNow) errors.Add(nameof(Todo.CreatedOn), ["Cannot have due date in the past"]);
    if (taskArgument.IsCompleted) errors.Add(nameof(Todo.IsCompleted), ["Cannot add completed todo"]);
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    return await next(context);
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
