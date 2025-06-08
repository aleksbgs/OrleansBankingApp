using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Grains.Filters;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
        {
            siloBuilder.UseAzureStorageClustering(configureOptions: options =>
            {
                options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true");
            });
            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "devCluster";
                options.ServiceId = "devService";
            });
            siloBuilder.AddAzureTableGrainStorage("tableStorage",
                configureOptions: options =>
                {
                    options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true");
                });
            siloBuilder.AddAzureBlobGrainStorage("blobStorage",
                configureOptions: options =>
                {
                    options.BlobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true");
                });

            siloBuilder.UseAzureTableReminderService(configureOptions: options =>
            {
                options.Configure(o =>
                    o.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true"));
            });

            siloBuilder.AddAzureTableTransactionalStateStorageAsDefault(configureOptions: options =>
            {
                options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true");
            });
            siloBuilder.UseTransactions();

            siloBuilder.AddIncomingGrainCallFilter<LoggingIncomingGrain>();
            
            siloBuilder.AddAzureQueueStreams("StreamProvider",
                optionBuilder =>
                {
                    optionBuilder.Configure(options =>
                    {
                        options.QueueServiceClient = new QueueServiceClient("UseDevelopmentStorage=true");
                    });
                }).AddAzureTableGrainStorage("PubSubStore",
                configureOptions: options =>
                {
                    options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true");
                });
            
                // siloBuilder.Configure<GrainCollectionOptions>(options =>
                // {
                //     options.CollectionQuantum = TimeSpan.FromSeconds(20);
                //     options.CollectionAge = TimeSpan.FromSeconds(20);
                // });

        })
        .RunConsoleAsync();