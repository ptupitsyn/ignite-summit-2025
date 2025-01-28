using Apache.Ignite;
using Apache.Ignite.Sql;
using Apache.Ignite.Table;
using Apache.Ignite.Transactions;

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

// Read-only TX: snapshot of data at the time of TX start.
await using ITransaction roTx = await client.Transactions.BeginAsync(new TransactionOptions(ReadOnly: true));

Option<Person> roTxRes1 = await pocoView.GetAsync(roTx, new Person(3, null!));

await pocoView.UpsertAsync(transaction:null, new Person(3, "JACK-2")); // Update after RO TX.

Option<Person> roTxRes2 = await pocoView.GetAsync(roTx, new Person(3, null!));
Option<Person> actualRes = await pocoView.GetAsync(null, new Person(3, null!));

Console.WriteLine($"RO TX result 1: {roTxRes1.Value}, 2: {roTxRes2.Value}, no tx result: {actualRes.Value}");

// Model
public record Person(int Id, string Name);
