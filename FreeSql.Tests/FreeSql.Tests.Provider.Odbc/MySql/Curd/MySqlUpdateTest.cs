using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.MySql
{
    public class MySqlUpdateTest
    {
        IUpdate<Topic> update => g.mysql.Update<Topic>();

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        class TestEnumUpdateTb
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public TestEnumUpdateTbType type { get; set; }
            public DateTime time { get; set; } = new DateTime();
        }
        enum TestEnumUpdateTbType { str1, biggit, sum211 }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.mysql.Update<Topic>().ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test1' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = NULL, `Title` = 'newtitle', `CreateTime` = '0001-01-01 00:00:00.000' WHERE (`Id` = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = CASE `Id` WHEN 1 THEN NULL WHEN 2 THEN 100 WHEN 3 THEN 200 WHEN 4 THEN 300 WHEN 5 THEN 400 WHEN 6 THEN 500 WHEN 7 THEN 600 WHEN 8 THEN 700 WHEN 9 THEN 800 WHEN 10 THEN 900 END, `Title` = CASE `Id` WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END, `CreateTime` = CASE `Id` WHEN 1 THEN '0001-01-01 00:00:00.000' WHEN 2 THEN '0001-01-01 00:00:00.000' WHEN 3 THEN '0001-01-01 00:00:00.000' WHEN 4 THEN '0001-01-01 00:00:00.000' WHEN 5 THEN '0001-01-01 00:00:00.000' WHEN 6 THEN '0001-01-01 00:00:00.000' WHEN 7 THEN '0001-01-01 00:00:00.000' WHEN 8 THEN '0001-01-01 00:00:00.000' WHEN 9 THEN '0001-01-01 00:00:00.000' WHEN 10 THEN '0001-01-01 00:00:00.000' END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = CASE `Id` WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `CreateTime` = '2020-01-01 00:00:00.000' WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.mysql.Insert<TestEnumUpdateTb>().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("INSERT INTO `TestEnumUpdateTb`(`type`, `time`) VALUES('sum211', '0001-01-01 00:00:00.000')", sql);
            var id = g.mysql.Insert<TestEnumUpdateTb>().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ExecuteIdentity();
            Assert.True(id > 0);
            Assert.Equal(TestEnumUpdateTbType.sum211, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211', `time` = '0001-01-01 00:00:00.000' WHERE (`id` = 0)", sql);
            g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { id = (int)id, type = TestEnumUpdateTbType.biggit }).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.biggit, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Insert<TestEnumUpdateTb>().NoneParameter().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("INSERT INTO `TestEnumUpdateTb`(`type`, `time`) VALUES('sum211', '0001-01-01 00:00:00.000')", sql);
            id = g.mysql.Insert<TestEnumUpdateTb>().NoneParameter().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ExecuteIdentity();
            Assert.True(id > 0);
            Assert.Equal(TestEnumUpdateTbType.sum211, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211', `time` = '0001-01-01 00:00:00.000' WHERE (`id` = 0)", sql);
            g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { id = (int)id, type = TestEnumUpdateTbType.biggit }).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.biggit, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle' WHERE (`Id` = 1)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).IgnoreColumns(a => a.time).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).IgnoreColumns(a => a.time).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle' WHERE (`Id` = 1)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).UpdateColumns(a => a.type).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).UpdateColumns(a => a.type).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle' WHERE (`Id` = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle', `CreateTime` = '2020-01-01 00:00:00.000' WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = ifnull(`Clicks`, 0) * 10 div 1 WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Id` = (`Id` - 10) WHERE (`Id` = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = ifnull(`Clicks`, 0) * 10 div 1 WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Id` = (`Id` - 10) WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = `Clicks` * 10 div 1 WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Id` = 10 WHERE (`Id` = 1)", sql);

            var id = g.mysql.Insert<TestEnumUpdateTb>().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ExecuteIdentity();
            Assert.True(id > 0);
            sql = g.mysql.Update<TestEnumUpdateTb>().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.biggit).ToSql().Replace("\r\n", "");
            Assert.Equal($"UPDATE `TestEnumUpdateTb` SET `type` = 'biggit' WHERE (`id` = {id})", sql);
            g.mysql.Update<TestEnumUpdateTb>().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.biggit).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.biggit, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.str1).ToSql().Replace("\r\n", "");
            Assert.Equal($"UPDATE `TestEnumUpdateTb` SET `type` = 'str1' WHERE (`id` = {id})", sql);
            g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.str1).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.str1, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + ?", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET clicks = clicks + ? WHERE (`Id` = 1)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == 0).SetRaw("`type` = {0}".FormatOdbcMySql(TestEnumUpdateTbType.sum211)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (`Id` = 1)", sql);

            sql = update.Where("id = ?", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (id = ?)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (`Id` = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == 0 && a.type == TestEnumUpdateTbType.str1)
                .Set(a => a.type, TestEnumUpdateTbType.sum211).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0 AND `type` = 'str1')", sql);
        }
        [Fact]
        public void WhereExists()
        {

        }
        [Fact]
        public void ExecuteAffrows()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var time = DateTime.Now;
            var items222 = g.mysql.Select<Topic>().Where(a => a.CreateTime > time).Limit(10).ToList();

            update.SetSource(items.First()).NoneParameter().ExecuteAffrows();
            update.SetSource(items).NoneParameter().ExecuteAffrows();
        }
        [Fact]
        public void ExecuteUpdated()
        {

        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.mysql.Update<Topic>().ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test1' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
