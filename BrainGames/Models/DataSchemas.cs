using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BrainGames.Models
{
    public abstract class DataSchemas
    {

        public class UserSchema
        {
            [PrimaryKey, JsonProperty("id")]
            public string Id { get; set; }
            public string UserId { get; set; }
            public string SubscriptionId { get; set; }
            public bool ActiveSubscription { get; set; }
            public string LastSubscriptionVerificationDate { get; set; }
            public string SignupDevice { get; set; }
            public string SignupModel { get; set; }
            public string SignupManufacturer { get; set; }
            public string SignupVersion { get; set; }
            public string SignupOS { get; set; }
            public double SignupScreenHeight;
            public double SignupScreenWidth;
            public double SignupScreenDensity;
        }

        public class BrainGameSessionSchema
        {
            [PrimaryKey, JsonProperty("id")]
            public string Id { get; set; }
            public string UserId { get; set; }
            public string SessionId { get; set; }
            public string datetime { get; set; }
            public string GameType { get; set; }
            public long SessionDuration { get; set; }
            public string Device { get; set; }
            public string Model { get; set; }
            public string Manufacturer { get; set; }
            public string DeviceVersion { get; set; }
            public string OS { get; set; }
            public double ScreenHeight;
            public double ScreenWidth;
            public double ScreenDensity;
        }

        public class ITGameRecordSchema
        {
            [PrimaryKey, JsonProperty("id")]
            public string Id { get; set; }
            public string UserId { get; set; }
            public string SessionId { get; set; }
            public string datetime { get; set; }
            public int trialnum { get; set; }
            public int reversalctr { get; set; }
            public double stimtime { get; set; }
            public double empstimtime { get; set; }
            public int stimtype { get; set; }
            public bool cor { get; set; }
        }
    }
}
