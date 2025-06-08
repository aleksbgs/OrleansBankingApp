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
     client.UseTransactions();
 });
 
 var app = builder.Build();

 app.MapGet("/checkingaccount/{checkingAccountId}/balance", async (Guid checkingAccountId, IClusterClient client, ITransactionClient transactionClient) =>
 {
     decimal balance = 0;
     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
         Console.WriteLine($"Checking Account Balance {checkingAccountId}");
         var checkingAccountGrain = client.GetGrain<ICheckingAccountGrain>(checkingAccountId);
         balance = await checkingAccountGrain.GetBalance();
     });
  
     return TypedResults.Ok(balance);

 });

 app.MapPost("/checkingaccount", async (IClusterClient clusterClient,CreateAccount createAccount ,ITransactionClient transactionClient) =>
 {
     var checkingAccountId = Guid.NewGuid();
     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
         Console.WriteLine("Initialize Account");
         var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
         await checkingAccountGrain.Initialize(createAccount.OpeningBalance);
     });
     return TypedResults.Created($"checkingaccount/{checkingAccountId}");
 });

 app.MapPost("checkingaccount/{checkingAccountId}/debit", async (Guid checkingAccountId, Debit debit,
         IClusterClient clusterClient, ITransactionClient transactionClient)  =>
 {

     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
         var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
         Console.WriteLine("CheckingAccountGrain", checkingAccountId);
         await checkingAccountGrain.Debit(debit.amount);
     });

 return TypedResults.NoContent();
 }); 

 app.MapPost("checkingaccount/{checkingAccountId}/credit", async (IClusterClient clusterClient,Credit credit,Guid checkingAccountId,ITransactionClient transactionClient) =>
 { 
     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
         var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
         await checkingAccountGrain.Credit(credit.amount);
     });
     
     return TypedResults.NoContent();
 }); 
 
 app.MapPost("atm", async (IClusterClient clusterClient,CreateAtm createAtm,ITransactionClient transactionClient) =>
 {
     var atmId = Guid.NewGuid();
     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
         var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
         await atmGrain.Initialize(createAtm.InitialAtmCashBalance);
     });
     
     return TypedResults.Created($"atm/{atmId}");
 });

 app.MapPost("atm/{atmId}/withdrawl", async (IClusterClient clusterClient, AtmWithdrawl atmWithdrawl, Guid atmId,ITransactionClient transactionClient) =>
 {
     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
         var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
         var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(atmWithdrawl.CheckingAccountId);
         await atmGrain.Withdraw(atmWithdrawl.CheckingAccountId, atmWithdrawl.Amount);
         await checkingAccountGrain.Debit(atmWithdrawl.Amount);
     });
     return TypedResults.NoContent();
 });
 app.MapPost("checkingaccount/{checkingAccountId}/recurringPayment", async (IClusterClient clusterClient,
     CreateRecurringPayment createRecurringPayment, Guid checkingAccountId) =>
 {
     var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
     await checkingAccountGrain.AddRecurringPayment(createRecurringPayment.PaymentId,
         createRecurringPayment.PaymentAmount, createRecurringPayment.PaymentRecurrsEveryMinutes);
     return TypedResults.NoContent();
 });
 app.MapGet("atm/{atmId}/balance", async (Guid atmId, IClusterClient client, ITransactionClient transactionClient) =>
 {
     decimal balance = 0;
     transactionClient.RunTransaction(TransactionOption.Create, async () =>
     {
        var atmGrain = client.GetGrain<IAtmGrain>(atmId);
        balance = await atmGrain.GetBalance();
     });
  
     return TypedResults.Ok(balance);

 });

 app.MapGet("customer/{customerId}/networth", async (Guid customerId, IClusterClient client) =>
 {
     var customerGrain = client.GetGrain<ICustomerGrain>(customerId);
     var netWorth = await customerGrain.GetNetWorth();
     return TypedResults.Ok(netWorth);

 });
 app.MapPost("customer/{checkingAccountId}/recurringPayment", async (IClusterClient clusterClient,
     CustomerCheckingAccount customerCheckingAccount, Guid customerId) =>
 {
     var customerGrain = clusterClient.GetGrain<ICustomerGrain>(customerId);
     await customerGrain.AddCheckingAccount(customerCheckingAccount.AccountId);
     return TypedResults.NoContent();
 }); 
 
 app.MapPost("transfer", async (IClusterClient clusterClient,
     Transfer transfer) =>
 {
     var statlessTransferProcessingGrain = clusterClient.GetGrain<ITransferProcessingGrain>(0);
     await statlessTransferProcessingGrain.ProcessTransfer(transfer.FromAccountId, transfer.ToAccountId, transfer.Amount);
     return TypedResults.NoContent();
     
 });
     
     
 
 app.Run();
     