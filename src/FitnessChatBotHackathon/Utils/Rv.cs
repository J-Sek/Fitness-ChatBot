using Microsoft.Bot.Builder;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fitness.ChatBot.Utils
{
    public class Rv
    {
        private const string Database = "FitnessChatBot-2";
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
                        var data = change.Value as ConcurrentDictionary<string, object>;

                        if(change.Key.Contains("/users/"))
                            session.Store(new UserData {Id = change.Key, StateValue = data});
                        else if(change.Key.Contains("/conversations/"))
                            session.Store(new Conversation {Id = change.Key, StateValue = data});
                        else
                            session.Store(new OtherContextData {Id = change.Key, StateValue = data});
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

        public class UserData : EntityProxy
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