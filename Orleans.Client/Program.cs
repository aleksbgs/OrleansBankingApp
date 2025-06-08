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
     Console.WriteLine($"Checking Account Balance {checkingAccountId}");
     var checkingAccountGrain = client.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     var balance = await checkingAccountGrain.GetBalance();
     return TypedResults.Ok(balance);

 });

 app.MapPost("/checkingaccount", async (IClusterClient clusterClient,CreateAccount createAccount) =>
 {
     Console.WriteLine("Initialize Account");
     var checkingAccountId = Guid.NewGuid();
     var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     await checkingAccountGrain.Initialize(createAccount.OpeningBalance);
     
     return TypedResults.Created($"checkingaccount/{checkingAccountId}");
 }); 

 app.MapPost("checkingaccount/{checkingAccountId}/debit", async (Guid checkingAccountId,Debit debit,IClusterClient clusterClient) =>
 {
     Console.WriteLine("route Debit");
     var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     Console.WriteLine("CheckingAccountGrain",checkingAccountId);
     await checkingAccountGrain.Debit(debit.amount);
     return TypedResults.NoContent();
 }); 

 app.MapPost("checkingaccount/{checkingAccountId}/credit", async (IClusterClient clusterClient,Credit credit,Guid checkingAccountId) =>
 {
     var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     await checkingAccountGrain.Credit(credit.amount);
     return TypedResults.NoContent();
 });
 
 app.Run();
     