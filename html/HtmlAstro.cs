namespace nscore;
//using System.Collections.Generic;

public class HtmlAstro
{

    public static IResult Index()
    {
        return Results.Extensions.Html(@$"<!doctype html>
<html>
    <head><title>miniHTML</title></head>
    <body>
        <h1>Hello World</h1>
        <p>The time on the server is {DateTime.Now:O}</p>
    </body>

</html>");

    }
}