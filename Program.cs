namespace DotNetRuntime;

using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using Appwrite.Extensions;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Collections;
using System.Xml.Schema;

public class Handler {

    string apikey = Environment.GetEnvironmentVariable("APPWRITE_KEY");
    string endpoint = "https://api.segeln.social/v1";
    string projectId = Environment.GetEnvironmentVariable("APPWRITE_FUNCTION_PROJECT_ID");

    string result = "false";

    Client client = new Client();
        

    // This is your Appwrite function
    // It is executed each time we get a request
    public async Task<RuntimeOutput> Main(RuntimeContext Context) 
    {
        //initialize the client
        client
            .SetEndpoint(endpoint)
            .SetProject(projectId)
            .SetKey(apikey)
        ;

        // You can log messages to the console
        // if environment variable LOG_REQUESTS is set to true logs the request details
        if (Environment.GetEnvironmentVariable("LOG_REQUESTS") == "true") {
            Context.Log("---> at " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));  // Current time in UTC
            Context.Log(Context.Req.BodyRaw);                                                   // Raw request body, contains request data
            Context.Log(JsonSerializer.Serialize<object>(Context.Req.Body));                    // Object from parsed JSON request body, otherwise string
            Context.Log(JsonSerializer.Serialize<object>(Context.Req.Headers));                 // String key-value pairs of all request headers, keys are lowercase
            Context.Log(Context.Req.Scheme);                                                    // Value of the x-forwarded-proto header, usually http or https
            Context.Log(Context.Req.Method);                                                    // Request method, such as GET, POST, PUT, DELETE, PATCH, etc.
            Context.Log(Context.Req.Url);                                                       // Full URL, for example: http://awesome.appwrite.io:8000/v1/hooks?limit=12&offset=50
            Context.Log(Context.Req.Host);                                                      // Hostname from the host header, such as awesome.appwrite.io
            Context.Log(Context.Req.Port);                                                      // Port from the host header, for example 8000
            Context.Log(Context.Req.Path);                                                      // Path part of URL, for example /v1/hooks
            Context.Log(Context.Req.QueryString);                                               // Raw query params string. For example "limit=12&offset=50"
            Context.Log(JsonSerializer.Serialize<object>(Context.Req.Query));                   // Parsed query params. For example, req.query.limit
        }

        switch (Context.Req.Method) {   
            case "POST":
                return Context.Res.Json(await Post(Context));
            default:
                return Context.Res.Json(new Dictionary<string, object>()
                    {
                        { "message", "This is not the endpoint you were looking for!" },
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") }
                    });
        }
    }
    private async Task<Dictionary<string, object?>> Post(DotNetRuntime.RuntimeContext Context)
    {
        var body = JsonSerializer.Serialize<object>(Context.Req.Body);
        string fileId = ""; 
        DateTime current = DateTime.UtcNow;

        // Validate that body holds a json object
        if (!body.StartsWith("{")) {
            throw new Exception("Invalid JSON object");
        }

        // Parse the body into a dictionary
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

        Context.Log(data);

        if (data.ContainsKey("file")) {
            if (data.ContainsKey("current")) {
                try {
                    current = DateTime.Parse(data["current"].ToString());
                }
                catch (Exception e) {
                    throw new Exception("Invalid date format");
                }
            }
            else {
                throw new Exception("Missing date");
            }
            fileId = data["file"].ToString();
        }
        else {
            throw new Exception("Invalid file");
        }

        if (fileId == "") {
            throw new Exception("File was empty");
        }

        var storage = new Storage(client);
        var file = await storage.GetFile("pub", fileId);

        if (current > DateTime.Parse(file.UpdatedAt))
        { 
            result = "true";
        }

            return new Dictionary<string, object>()
                    {
                        { "update", result }
                    };
    }
}    

        
