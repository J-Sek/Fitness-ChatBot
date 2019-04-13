using System;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Fitness.ChatBot.Utils
{
    public class Rv
    {
        private const string Database = "FitnessChatBot";
        private static readonly string ServerUrl = Environment.GetEnvironmentVariable("FITNESSBOT_RAVENDB_ADDRESS");
        private static IDocumentStore NewConnection => new DocumentStore {Urls = new[] {ServerUrl}, Database = Database}.Initialize();

        public static void WithRaven(Action<IDocumentSession> action)
        {
            WithRaven(session =>
            {
                action(session);
                return true;
            });
        }

        public static T WithRaven<T>(Func<IDocumentSession, T> action)
        {
            using (var store = NewConnection)
            {
                var session = store.OpenSession();
                var result = action(session);
                return result;
            }
        }
    }
}