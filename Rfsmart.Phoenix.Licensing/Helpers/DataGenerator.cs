using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Helpers
{
    public static class DataGenerator
    {
        public static List<FeatureTrackingRecord> Generate(int totalRecords = 5000, int elapsedDays = 30)
        {
            var startTime = DateTime.UtcNow.AddDays(elapsedDays * -1);
            var endTime = DateTime.UtcNow;
            var timeSpan = endTime - startTime;

            string[] actions = ["Add", "Remove"];
            string[] allUsers = ["Bob", "Gary", "Jeremy", "Steve", "Jenny", "Greg", "Heather", "Emma", "Kris", "Sharon", "Kristan", "Katie", "Vicky"];
            string[] features = ["Receiving", "Advanced Receiving", "Picking"];

            var receivingStartRecord = new FeatureTrackingRecord
            {
                FeatureName = "Receiving",
                Users = [],
                Created = startTime,
            };

            var advancedReceivingStartRecord = new FeatureTrackingRecord
            {
                FeatureName = "Advanced Receiving",
                Users = [],
                Created = startTime,
            };

            var pickingStartRecord = new FeatureTrackingRecord
            {
                FeatureName = "Picking",
                Users = [],
                Created = startTime,
            };

            var trackedRecords = new List<FeatureTrackingRecord>([receivingStartRecord, advancedReceivingStartRecord, pickingStartRecord]);

            var timeStamps = Enumerable.Range(0, totalRecords).Select(x =>
            {
                var randomDate = new Random();

                TimeSpan newSpan = new TimeSpan(0, randomDate.Next(0, (int)timeSpan.TotalMinutes), 0);
                return startTime + newSpan;
            }).OrderBy(x => x).ToList();

            foreach (var time in timeStamps)
            {
                var randomAction = actions[new Random().Next(0, actions.Length)];
                var randomFeature = features[new Random().Next(0, features.Length)];

                var lastRecord = trackedRecords.Last(x => x.FeatureName == randomFeature);
                var unassignedUsers = allUsers.Where(x => !lastRecord.Users.Contains(x)).ToArray();

                if (unassignedUsers.Length == 0)
                {
                    randomAction = "Remove";
                }

                if (lastRecord.UserCount == 0 && randomAction == "Remove")
                {
                    randomAction = "Add";
                }

                string[] newUsers = [];

                switch (randomAction)
                {
                    case "Add":
                        var randomUser = unassignedUsers[new Random().Next(0, unassignedUsers.Length)];
                        newUsers = [.. lastRecord.Users, randomUser];
                        break;
                    case "Remove":
                        var ind = new Random().Next(0, lastRecord.UserCount - 1);
                        newUsers = lastRecord.Users.Where((x, i) => i >= ind).ToArray();
                        break;
                    default:
                        break;
                }

                var newRecord = new FeatureTrackingRecord
                {
                    Users = newUsers,
                    FeatureName = randomFeature,
                    Created = time,
                };

                trackedRecords.Add(newRecord);
            }

            return trackedRecords;
        }
    }
}
