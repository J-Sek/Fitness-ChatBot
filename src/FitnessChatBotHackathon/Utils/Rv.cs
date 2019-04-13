using Microsoft.Bot.Builder;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fitness.ChatBot.Dialogs.Greeting;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Utils
{
    public class Rv
    {
        private const string Database = "FitnessChatBot";
        private static readonly string ServerUrl = Environment.GetEnvironmentVariable("FITNESSBOT_RAVENDB_ADDRESS");
        internal static IDocumentStore NewConnection => new DocumentStore { Urls = new[] { ServerUrl }, Database = Database }.Initialize();

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

    public class RavenDbBotStorage : IStorage
    {
        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() =>
            {
                return Rv.WithRaven(session =>
                {
                    return session
                        .Load<EntityProxy>(keys)
                        .ToDictionary(x => x.Key, x => (object) x.Value?.StateValue) as IDictionary<string, object>;
                });
            }, cancellationToken);
        }

        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() =>
            {
                Rv.WithRaven(session =>
                {
                    foreach (var change in changes)
                    {
                        if (change.Value is ConcurrentDictionary<string, object> stateValue)
                        {
                            if (stateValue.Keys.All(key => key == nameof(GreetingState)))
                                session.Store(new User {Id = change.Key, StateValue = stateValue});
                            else if (stateValue.Keys.All(key => key == nameof(DialogState)))
                                session.Store(new Conversation {Id = change.Key, StateValue = stateValue});
                            else
                                session.Store(new OtherContextData {Id = change.Key, StateValue = stateValue});
                        }
                    }

                    session.SaveChanges();
                });
            }, cancellationToken);
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() =>
            {
                Rv.WithRaven(session =>
                {
                    foreach (var key in keys)
                        session.Delete(key);

                    session.SaveChanges();
                });

            }, cancellationToken);
        }

        public abstract class EntityProxy
        {
            public string Id { get; set; }

            public ConcurrentDictionary<string, object> StateValue { get; set; }
        }

        public class User : EntityProxy
        {
        }
        public class Conversation : EntityProxy
        {
        }
        public class OtherContextData : EntityProxy
        {
        }
    }
}