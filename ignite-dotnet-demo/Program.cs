using Apache.Ignite;
using Apache.Ignite.Sql;
using Apache.Ignite.Table;

var cfg = new IgniteClientConfiguration("localhost");
using IIgniteClient client = await IgniteClient.StartAsync(cfg);

Console.WriteLine("Connected: " + client);

// SQL API.
await client.Sql.ExecuteAsync(null, "DROP TABLE IF EXISTS Person");
await client.Sql.ExecuteAsync(null, "CREATE TABLE Person (id INT PRIMARY KEY, name VARCHAR)");
await client.Sql.ExecuteAsync(null, "UPSERT INTO Person VALUES (1, 'John')");

await foreach (IIgniteTuple row in await client.Sql.ExecuteAsync(null, "SELECT * FROM Person"))
{
    Console.WriteLine(row);
}

// .NET APIs.
ITable? table = await client.Tables.GetTableAsync("Person");

// All table view types provide identical capabilities.
IRecordView<IIgniteTuple> recordView = table!.RecordBinaryView;
await recordView.UpsertAsync(null, new IgniteTuple { ["id"] = 2, ["name"] = "Jane" });

IRecordView<Person> pocoView = table.GetRecordView<Person>();
await pocoView.UpsertAsync(null, new Person(3, "Jack"));

IKeyValueView<IIgniteTuple, IIgniteTuple> kvView = table.KeyValueBinaryView;
await kvView.PutAsync(null, new IgniteTuple { ["id"] = 4 }, new IgniteTuple { ["name"] = "Jill" });

IKeyValueView<int, string> kvPocoView = table.GetKeyValueView<int, string>();
await kvPocoView.PutAsync(null, 5, "Joe");

// SQL with mapping
IResultSet<Person> query = await client.Sql.ExecuteAsync<Person>(null, "select * from Person");
await foreach (Person person in query)
{
    Console.WriteLine(person);
}

// LINQ
IQueryable<Person> queryable = pocoView.AsQueryable();

var query2 = queryable
    .Where(p => p.Id > 2)
    .OrderBy(x => x.Id)
    .Select(p => p.Name);

Console.WriteLine("Generated SQL: " + query2);

foreach (string name in query2)
{
    Console.WriteLine(name);
}

// Model
public record Person(int Id, string Name);
