using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigin",
    builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });
});



builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddDbContext<DbCont>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors("AllowMyOrigin");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name v1");
});

async Task<List<User>> GetUsers(DbCont context)=> await context.Users.ToListAsync();

app.MapGet("get/users", async (DbCont dbContext)=> await dbContext.Users.ToListAsync());
app.MapGet("get/chapters", async (DbCont dbContext)=> await dbContext.Chapters.ToListAsync());
app.MapGet("get/documents", async (DbCont dbContext)=> await dbContext.Documents.ToListAsync());

app.MapGet("get/documents/chapters", async (DbCont dbContext, int id)=>
{       

        var item = await dbContext.Chapters.Where(e => e.DocumentID ==id).ToListAsync();
        return Results.Ok(item);

});


app.MapPost("/add/user", async (User user, DbCont dbContext) =>
{
        if(user.UserID.GetType()==typeof(int)&&user.UserName.GetType()==typeof(string))
        {    
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Results.Ok(await GetUsers(dbContext));
        } else
        {
            return Results.NoContent();
        }
});


app.MapPost("/add/document/{id}/chapter", async (Chapter chapter, DbCont dbContext,int id)=>
{
     
        var maxPositionChapter = await dbContext.Chapters
        .OrderByDescending(c => c.Position)
        .FirstOrDefaultAsync();
        var chapterCount = await dbContext.Chapters
        .Where(c => c.DocumentID == id)
        .CountAsync();
        if (chapterCount==0){
            chapter.DocumentID=id;
            chapter.Position=0;
        } else
        {
        chapter.DocumentID=id;
        chapter.Position=maxPositionChapter.Position+1;
        }
        dbContext.Chapters.Add(chapter);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
       
});


app.MapPost("/add/document", async (Document document, DbCont dbContext) =>
{
    document.TimeAdded = DateTime.Now;
    if(document.DocumentID is int && document.Title is string)
    {
    dbContext.Documents.Add(document);
    await dbContext.SaveChangesAsync();

    return Results.Ok();
    } else
    {
        return Results.NoContent();
    }
});






app.MapPut("/change/chapter/{id}", async (Chapter chapter, DbCont dbContext, int id)=>
{
        var item=await dbContext.Chapters.FindAsync(id);
        if (item==null) return Results.NotFound();
        if(item.Title is string&&item.Position is int
        &&item.Text is string&&item.DocumentID is int &&item.ChapterID is int)
        {
        item.Position=chapter.Position;
        dbContext.Chapters.Update(item);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
        } else
        {
            return Results.NoContent();
        }
});






app.MapPut("/change/user/{id}", async (User user, DbCont dbContext,int id)=>
{
        var item=await dbContext.Users.FindAsync(id);
        if (item==null) return Results.NotFound();

        if(item.UserName is string&&item.UserID is int)
        {
        item.UserName=user.UserName;
       
        dbContext.Users.Update(item);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
        } else
        {
            return Results.NoContent();
        }

});

app.MapPut("/down/chapter/{id}/position", async (DbCont dbContext, int id)=>
{
    var item= await dbContext.Chapters.FindAsync(id);
           var item2 = await dbContext.Chapters
        .Where(c => c.Position == item.Position + 1)
        .FirstOrDefaultAsync();
    if(item==null || item2==null) return Results.NotFound();
    if(item.Position is int &&item2.Position is int)
    {
        item.Position++;
        item2.Position--;
        dbContext.Chapters.Update(item);
        dbContext.Chapters.Update(item2);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    } else
    {
        return Results.NoContent();
    }
});

app.MapPut("/up/chapter/{id}/position", async (DbCont dbContext, int id)=>
{
    var item= await dbContext.Chapters.FindAsync(id);
    

       var item2 = await dbContext.Chapters
        .Where(c => c.Position == item.Position - 1)
        .FirstOrDefaultAsync();

    if(item==null || item2==null) return Results.NotFound();
    if(item.Position is int &&item2.Position is int)
    {
   

        item.Position--;
        item2.Position++;
        dbContext.Update(item);
        dbContext.Update(item2);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    } else
    {
        return Results.NoContent();
    }
});

app.MapDelete("/delete/chapter{id}", async (DbCont dbContext, int id)=>
{
    var item=await dbContext.Chapters.FindAsync(id);
    
    if (item==null) return Results.NotFound();

    dbContext.Chapters.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/delete/document/{id}",async (DbCont dbContext, int id)=>
{
    var item=await dbContext.Documents.FindAsync(id);
    if(item==null) return Results.NotFound();
    
    dbContext.Documents.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});


app.MapGet("get/documents/chapters/{documentID}", async (DbCont dbContext, int documentID) =>
{
    var chapters = await dbContext.Chapters
        .Where(chapter => chapter.DocumentID == documentID)
        .OrderBy(chapter => chapter.Position)
        .ToListAsync();

    return Results.Ok(chapters);
});




app.Run("https://localhost:5001");
