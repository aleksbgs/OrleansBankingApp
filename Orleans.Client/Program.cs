 using Orleans.Client.Contracts;
 using Orleans.Configuration;
 using Orleans.Grains.Abstractions;

 var builder = WebApplication.CreateBuilder(args);
 builder.Host.UseOrleansClient((context, client) =>
 {
     client.UseAzureStorageClustering(configureOptions: options =>
     {
          options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true");
     });
     client.Configure<ClusterOptions>(options =>
     {
         options.ClusterId = "devCluster";
         options.ServiceId = "devService";
     });
 });
 
 var app = builder.Build();

 app.MapGet("/checkingaccount/{checkingAccountId}/balance", async (Guid checkingAccountId, IClusterClient client) =>
 {
     var checkingAccountGrain = client.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     var balance = await checkingAccountGrain.GetBalance();
     return TypedResults.Ok(balance);

 });

 app.MapPost("/checkingaccount", async (IClusterClient clusterClient,CreateAccount createAccount) =>
 {
     var checkingAccountId = Guid.NewGuid();
     var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     checkingAccountGrain.Initialize(createAccount.OpeningBalance);
     
     return TypedResults.Created($"checkingaccount/{checkingAccountId}");
 });
 
 app.Run();
     