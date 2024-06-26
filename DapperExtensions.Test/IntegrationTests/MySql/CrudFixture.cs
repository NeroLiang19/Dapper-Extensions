﻿using DapperExtensions.Predicate;
using DapperExtensions.Test.Data.Common;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Test.Data.TestEntity;
using MySql.Data.MySqlClient;

namespace DapperExtensions.Test.IntegrationTests.MySql
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class CrudFixture
    {
        [TestFixture]
        public class InsertMethod : MySqlBaseFixture
        {
            [Test]
            public void AddsEntityToDatabase_ReturnsKey()
            {
                Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                var id = Db.Insert(p);
                Assert.AreEqual(1, id);
                Assert.AreEqual(1, p.Id);
                Dispose();
            }
            
            [Test]
            public async Task GetListTest()
            {
                DapperAsyncExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                DapperExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                using var conn = new MySqlConnection("server=192.168.0.234;port=3366;database=xy_boss;uid=test;pwd=test;charset=utf8;pooling=true;DefaultCommandTimeout=60;");
                conn.Open();
                try
                {
                    var transaction = await conn.BeginTransactionAsync();
                    var predicate = Predicates.Field<stock_task>(f => f.serial_number, Operator.Eq, "ac4eea");
                    var stockTaskInfoList = await conn.GetListAsync<stock_task>(predicate, transaction: transaction);
                    var stockTaskInfo = stockTaskInfoList.ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            [Test]
            public async Task UpdatePartialTest()
            {
                DapperAsyncExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                DapperExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                using var conn = new MySqlConnection("server=192.168.0.234;port=3366;database=xy_boss;uid=test;pwd=test;charset=utf8;pooling=true;DefaultCommandTimeout=60;");
                conn.Open();
                var transaction = await conn.BeginTransactionAsync();

                try
                {
                    var predicate = Predicates.Field<produce>(x => x.produce_id, Operator.Eq,
                        new List<int> { 35463, 35462, 35461});
                    var produceList = (await conn.GetListAsync<produce>(predicate)).ToList();

                    // updateExpression
                    Expression<Func<produce, UpdateProduceForPlan>> updateExpression = p =>
                        new UpdateProduceForPlan
                        {
                            plan_produce_begin_date = p.plan_produce_begin_date,
                            plan_produce_end_date = p.plan_produce_end_date,
                            update_time = p.update_time,
                            updater = p.updater
                        };
                    
                    var updateList = new List<produce>();
                    foreach (var produce in produceList)
                    {
                        produce.plan_produce_begin_date = DateTime.Now.AddDays(-20);
                        produce.plan_produce_end_date = DateTime.Now.AddDays(20);
                        produce.update_time = DateTime.Now;
                        produce.updater = 1;
                        updateList.Add(produce);
                    }
                    
                    conn.UpdatePartial(updateList, updateExpression, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
                        
            [Test]
            public async Task InsertSnowIdTest()
            {
                DapperAsyncExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                DapperExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                using var conn = new MySqlConnection("server=192.168.0.234;port=3366;database=xy_boss;uid=test;pwd=test;charset=utf8;pooling=true;DefaultCommandTimeout=60;");
                conn.Open();
                var transaction = await conn.BeginTransactionAsync();

                try
                {
                    var maintenanceOrder = new maintenance_order
                    {
                        maintenance_order_id = 1767921778245636097,
                        serial_number = "serialNumber",
                        order_number = "orderNumber",
                        source_order_type = 1,
                        basis_type = "2",
                        produce_id = 1,
                        produce_order = "test",
                        item_id = 2,
                        product_guid = "productGuid",
                        status = 1,
                        remark = "remark",
                        problem_cause = "remark",
                        creator = 1,
                        create_time = DateTime.Now,
                        update_user = 2,
                        update_time = DateTime.Now,
                        question_feedback_approval_number = 123
                    };

                    var id = conn.Insert(maintenanceOrder, transaction);
                    Console.WriteLine(id);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            [Test]
            public void QueryItemList()
            {
                DapperAsyncExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
                DapperExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect()); 
                using var conn = new MySqlConnection("server=192.168.0.234;port=3366;database=xy_boss;uid=test;pwd=test;charset=utf8;pooling=true;DefaultCommandTimeout=60;");
                conn.Open();
                

                try
                {
                    var attrList = new List<string> { "3" };
                    var predicate = Predicates.Field<item>(f => f.attr, Operator.Eq, attrList);
                    var itemIdsByAttr = conn.GetList<item>(predicate).Select(a => a.item_id).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }


            [Test]
            public void AddsEntityToDatabase_ReturnsCompositeKey()
            {
                Multikey m = new Multikey { Key2 = "key", Value = "foo" };
                var key = Db.Insert(m);
                Assert.AreEqual(1, key.Key1);
                Assert.AreEqual("key", key.Key2);
                Dispose();
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id);
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(a1.Id, a2.Id);
                Dispose();
            }

            [Test]
            public void AddsEntityToDatabase_WithPassedInGuid()
            {
                var guid = Guid.NewGuid();
                Animal a1 = new Animal { Id = guid, Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id);
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(guid, a2.Id);
                Dispose();
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Animal a2 = new Animal { Name = "Bar" };
                Animal a3 = new Animal { Name = "Baz" };

                Db.Insert<Animal>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Animal>().ToList();
                Assert.AreEqual(3, animals.Count);
                Dispose();
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase_WithPassedInGuid()
            {
                var guid1 = Guid.NewGuid();
                Animal a1 = new Animal { Id = guid1, Name = "Foo" };
                var guid2 = Guid.NewGuid();
                Animal a2 = new Animal { Id = guid2, Name = "Bar" };
                var guid3 = Guid.NewGuid();
                Animal a3 = new Animal { Id = guid3, Name = "Baz" };

                Db.Insert<Animal>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Animal>().ToList();
                Assert.AreEqual(3, animals.Count);
                Assert.IsNotNull(animals.Find(x => x.Id == guid1));
                Assert.IsNotNull(animals.Find(x => x.Id == guid2));
                Assert.IsNotNull(animals.Find(x => x.Id == guid3));
                Dispose();
            }
        }

        [TestFixture]
        public class GetMethod : MySqlBaseFixture
        {
            [Test]
            public void UsingKey_ReturnsEntity()
            {
                Person p1 = new Person
                {
                    Active = true,
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1);

                Person p2 = Db.Get<Person>(id);
                Assert.AreEqual(id, p2.Id);
                Assert.AreEqual("Foo", p2.FirstName);
                Assert.AreEqual("Bar", p2.LastName);
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_ReturnsEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                Assert.AreEqual(1, m2.Key1);
                Assert.AreEqual("key", m2.Key2);
                Assert.AreEqual("bar", m2.Value);
                Dispose();
            }
        }

        [TestFixture]
        public class DeleteMethod : MySqlBaseFixture
        {
            private void PersonArrange()
            {
                Db.Insert(new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow });
            }

            [Test]
            public void UsingKey_DeletesFromDatabase()
            {
                Person p1 = new Person
                {
                    Active = true,
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1);

                Person p2 = Db.Get<Person>(id);
                Db.Delete(p2);
                Assert.IsNull(Db.Get<Person>(id));
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                Db.Delete(m2);
                Assert.IsNull(Db.Get<Multikey>(new { key.Key1, key.Key2 }));
                Dispose();
            }

            [Test]
            public void UsingPredicate_DeletesRows()
            {
                PersonArrange();

                var list = Db.GetList<Person>();
                Assert.AreEqual(3, list.Count());

                IPredicate pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
                var result = Db.Delete<Person>(pred);
                Assert.IsTrue(result);

                list = Db.GetList<Person>();
                Assert.AreEqual(1, list.Count());
                Dispose();
            }

            [Test]
            public void UsingObject_DeletesRows()
            {
                PersonArrange();

                var list = Db.GetList<Person>();
                Assert.AreEqual(3, list.Count());

                var result = Db.Delete<Person>(new { LastName = "Bar" });
                Assert.IsTrue(result);

                list = Db.GetList<Person>();
                Assert.AreEqual(1, list.Count());
                Dispose();
            }
        }

        [TestFixture]
        public class UpdateMethod : MySqlBaseFixture
        {
            [Test]
            public void UsingKey_UpdatesEntity()
            {
                Person p1 = new Person
                {
                    Active = true,
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1);

                var p2 = Db.Get<Person>(id);
                p2.FirstName = "Baz";
                p2.Active = false;

                Db.Update(p2);

                var p3 = Db.Get<Person>(id);
                Assert.AreEqual("Baz", p3.FirstName);
                Assert.AreEqual("Bar", p3.LastName);
                Assert.AreEqual(false, p3.Active);
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_UpdatesEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                m2.Key2 = "key";
                m2.Value = "barz";
                Db.Update(m2);

                Multikey m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
                Assert.AreEqual(1, m3.Key1);
                Assert.AreEqual("key", m3.Key2);
                Assert.AreEqual("barz", m3.Value);
                Dispose();
            }
        }

        [TestFixture]
        public class GetListMethod : MySqlBaseFixture
        {
            private void Arrange()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });
            }

            [Test]
            public void UsingNullPredicate_ReturnsAll()
            {
                Arrange();

                IEnumerable<Person> list = Db.GetList<Person>();
                Assert.AreEqual(4, list.Count());
                Dispose();
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Arrange();

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
                Dispose();
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Arrange();

                var predicate = new { Active = true, FirstName = "c" };
                IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
                Assert.AreEqual(1, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "c"));
                Dispose();
            }
        }

        [TestFixture]
        public class GetPageMethod : MySqlBaseFixture
        {
            private void Arrange(out long id1, out long id2, out long id3, out long id4)
            {
                id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });
            }

            [Test]
            public void UsingNullPredicate_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 0, 2);
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id2, list.First().Id);
                Assert.AreEqual(id1, list.Skip(1).First().Id);
                Dispose();
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 2);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
                Dispose();
            }

            [Test]
            public void NotFirstPage_Returns_NextResults()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 2, 2);
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id4, list.First().Id);
                Assert.AreEqual(id3, list.Skip(1).First().Id);
                Dispose();
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var predicate = new { Active = true };
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 2);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
                Dispose();
            }
        }

        [TestFixture]
        public class CountMethod : MySqlBaseFixture
        {
            private void Arrange()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });
            }

            [Test]
            public void UsingNullPredicate_Returns_Count()
            {
                Arrange();

                int count = Db.Count<Person>(null);
                Assert.AreEqual(4, count);
                Dispose();
            }

            [Test]
            public void UsingPredicate_Returns_Count()
            {
                Arrange();

                var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
                int count = Db.Count<Person>(predicate);
                Assert.AreEqual(2, count);
                Dispose();
            }

            [Test]
            public void UsingObject_Returns_Count()
            {
                Arrange();

                var predicate = new { FirstName = new[] { "b", "d" } };
                int count = Db.Count<Person>(predicate);
                Assert.AreEqual(2, count);
                Dispose();
            }
        }

        [TestFixture]
        public class GetMultipleMethod : MySqlBaseFixture
        {
            [Test]
            public void ReturnsItems()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                Db.Insert(new Animal { Name = "Foo" });
                Db.Insert(new Animal { Name = "Bar" });
                Db.Insert(new Animal { Name = "Baz" });

                var predicate = new GetMultiplePredicate();
                predicate.Add<Person>(null);
                predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
                predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

                var result = Db.GetMultiple(predicate);
                var people = result.Read<Person>().ToList();
                var animals = result.Read<Animal>().ToList();
                var people2 = result.Read<Person>().ToList();

                people.Should().HaveCount(4);
                animals.Should().HaveCount(2);
                people2.Should().HaveCount(1);
                Dispose();
            }
        }
    }
    
}