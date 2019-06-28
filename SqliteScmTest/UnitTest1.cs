using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite;
using Xunit;
using WidgetScmDataAccess;
using Xunit.Abstractions;

namespace SqliteScmTest
{
    public class UnitTest1 : IClassFixture<SampleScmDataFixture>
    {
        private SampleScmDataFixture fixture;
        private ScmContext context;
        private ITestOutputHelper output;

        public UnitTest1(SampleScmDataFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.context = new ScmContext(fixture.Connection);
            this.output = output;
        }

        [Fact]
        public void Test1()
        {
            var parts = context.Parts;
            Assert.Equal(1, parts.Count());
            var part = parts.First();
            Assert.Equal("8289 L-shaped plate", part.Name);

            var inventory = context.Inventory;
            Assert.Equal(1, inventory.Count());
            var item = inventory.First();
            Assert.Equal(part.Id, item.PartTypeId);
            Assert.Equal(100, item.Count);
            Assert.Equal(10, item.OrderThreshold);
        }

        [Fact]
        public void TestPartComands()
        {
            var item = context.Inventory.First();
            var startCount = item.Count;
            context.CreatePartCommand(new PartCommand()
            {
                PartTypeId = item.PartTypeId,
                PartCount = 10,
                Command = PartCountOperation.Add
            });
            context.CreatePartCommand(new PartCommand()
            {
                PartTypeId = item.PartTypeId,
                PartCount = 5,
                Command = PartCountOperation.Remove
            });
            var inventory = new Inventory(context);
            inventory.UpdateInventory();
            Assert.Equal(startCount + 5, item.Count);
        }

        [Fact]
        public void TestCreateOrderTransaction()
        {
            var placedDate = DateTime.Now;
            var supplier = context.Suppliers.First();
            var order = new Order()
            {
                PartTypeId = supplier.PartTypeId,
                SupplierId = supplier.Id,
                PartCount = 10,
                PlacedDate = placedDate
            };
            Assert.Throws<NullReferenceException>(() =>
                context.CreateOrder(order));

            var command = new SqliteCommand(
                @"
                SELECT Count(*)
                FROM [Order]
                WHERE SupplierId = @supplierId
                  AND PartTypeId = @partTypeId
                  AND PlacedDate = @placedDate
                  AND PartCount = 10
                  AND FulfilledDate IS NULL;
                ", fixture.Connection);
            AddParameter(command, "@supplierId", supplier.Id);
            AddParameter(command, "@partTypeId", supplier.PartTypeId);
            AddParameter(command, "@placedDate", placedDate);
            Assert.Equal(0, (long) command.ExecuteScalar());
        }

        [Fact]
        public void TestUpdateInventory()
        {
            var item = context.Inventory.First();
            var totalCount = item.Count;
            context.CreatePartCommand(new PartCommand()
            {
                PartTypeId = item.PartTypeId,
                PartCount = totalCount,
                Command = PartCountOperation.Remove
            });

            var inventory = new Inventory(context);
            inventory.UpdateInventory();
            var order = context.GetOrders().FirstOrDefault(
                o => o.PartTypeId == item.PartTypeId &&
                     !o.FulfilledDate.HasValue);
            Assert.NotNull(order);

            var mails = context.GetMails();

            Assert.Equal(1, mails.Count());

            context.CreatePartCommand(new PartCommand()
            {
                PartTypeId = item.PartTypeId,
                PartCount = totalCount,
                Command = PartCountOperation.Add
            });

            inventory.UpdateInventory();
            Assert.Equal(totalCount, item.Count);
        }

        private void AddParameter(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            if (value == null)
                throw new ArgumentNullException("value");
            Type t = value.GetType();
            if (t == typeof(int))
                p.DbType = DbType.Int32;
            else if (t == typeof(string))
                p.DbType = DbType.String;
            else if (t == typeof(DateTime))
                p.DbType = DbType.DateTime;
            p.Direction = ParameterDirection.Input;
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}