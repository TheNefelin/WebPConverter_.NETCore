var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebAppMVC_WebPConverter>("webappmvc-webpconverter");

builder.Build().Run();
