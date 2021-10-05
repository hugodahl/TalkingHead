using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MrBigHead.Services;

namespace Magic8HeadService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IServiceProvider service;
        public readonly IConfiguration config;

        int buttonPin = 7;
        GpioController controller;
        List<string> sayings;
        Random random;
        ISayingResponse scopedSayingResponse;

        public Worker(IServiceProvider service, IConfiguration config, ILogger<Worker> logger)
        {
            this.logger = logger;
            this.service = service;
            this.config = config;

            var defaultLogLevel = config["Logging:LogLevel:Default"];
            logger.LogInformation($"defaultLogLevel = {defaultLogLevel}");

            var userName = config["TwitchBotConfiguration:UserName"];
            var accessToken = config["TwitchBotConfiguration:AccessToken"];

            using var scope = service.CreateScope();

            //var scopedSayingService =
            //    scope.ServiceProvider
            //        .GetRequiredService<ISayingService>();

            scopedSayingResponse =
                scope.ServiceProvider
                    .GetRequiredService<ISayingResponse>();

            var twitchBot = new TwitchBot(userName, accessToken, scopedSayingResponse, logger);

            SetupGPIO();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ExecuteAsync fired...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var status = controller?.Read(buttonPin);

                if (status == PinValue.Low)
                {
                    logger.LogInformation("saying words...");
                    var message = scopedSayingResponse.PickSaying();
                    logger.LogInformation($"ExecuteAsync: picked saying {message}");
                    await scopedSayingResponse.SaySomethingNice(message);
                }

                await Task.Delay(100, stoppingToken);
            }
        }

        public void SetupGPIO()
        {
            logger.LogInformation("Setiing up GPIO...");

            try
            {
                controller = new GpioController(0, new RaspberryPi3Driver());
                controller.OpenPin(buttonPin, PinMode.InputPullUp);
            }
            catch (Exception e)
            {
                logger.LogInformation("GPIO not available");
            }
        }
    }
}
