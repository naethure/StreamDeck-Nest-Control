﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tech.Aerove.StreamDeck.Client.Actions;
using Tech.Aerove.StreamDeck.NestControl.Tech.Aerove.Tools.Nest;
using Tech.Aerove.Tools.Nest;
using Tech.Aerove.Tools.Nest.Models;

namespace Tech.Aerove.StreamDeck.NestControl.Actions
{
    [PluginAction("tech.aerove.streamdeck.nestcontrol.thermostatinfo")]
    public class ThermostatInfo : ActionBase
    {
        private string DeviceName => $"{Context.Settings["device"]}";

        private int TemperatureStep => int.Parse($"{Context.Settings["temperatureStep"]}");
        private decimal CurrentSetPoint = 0;

        private readonly ILogger<TemperatureDown> _logger;
        private readonly ExampleService _handler;
        public ThermostatInfo(ILogger<TemperatureDown> logger, ExampleService handler)
        {
            _logger = logger;
            _handler = handler;
            _ = Ticker();
        }
        private ThermostatDevice Thermostat => GetThermostat();
        private ThermostatDevice _thermostat { get; set; }
        private ThermostatDevice GetThermostat()
        {
            var lookupThermostat = _handler.GetDevice(DeviceName);
            if (_thermostat == null || lookupThermostat.Name != _thermostat.Name)
            {
                if (_thermostat != null)
                {
                    _thermostat.OnUpdate -= OnUpdate;
                }
                _thermostat = lookupThermostat;
                _thermostat.OnUpdate += OnUpdate;
            }
            return _thermostat;
        }
        public void OnUpdate()
        {
            CurrentSetPoint = Thermostat.SetPoint;
        }
        public async Task Ticker()
        {
            while (true)
            {
                await Task.Delay(5000);
                if (string.IsNullOrWhiteSpace(DeviceName)) { continue; }
                try
                {
                    if (CurrentSetPoint != Thermostat.SetPoint)
                    {
                        CurrentSetPoint = Thermostat.SetPoint;
                    }
                    await Dispatcher.SetTitleAsync($"{Thermostat.Mode}\n{Thermostat.SetPoint}");
                    await Task.Delay(5000);
                    await Dispatcher.SetTitleAsync($"{Thermostat.CurrentTemperature}");
                }
                catch (Exception)
                {

                }
                await Task.Delay(60000);
            }
        }
       
    }
}
