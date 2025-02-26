package org.example;

import org.apache.ignite.catalog.ColumnType;
import org.apache.ignite.catalog.definitions.ColumnDefinition;
import org.apache.ignite.catalog.definitions.TableDefinition;
import org.apache.ignite.client.IgniteClient;
import org.apache.ignite.table.KeyValueView;
import org.apache.ignite.table.RecordView;
import org.apache.ignite.table.Table;
import org.apache.ignite.table.Tuple;

public class Main {
    public static void main(String[] args) {
        IgniteClient client = IgniteClient.builder()
                .addresses("localhost:10800", "localhost:10801", "localhost:10802")
                .build();

        System.out.println("Connected to the cluster: " + client.connections());

        // SQL API (use table created in CLI).
        client.sql().execute(null, "SELECT * FROM Person")
                .forEachRemaining(x -> System.out.println(x.stringValue("name")));

        // Java APIs
        Table table = client.catalog().createTable(TableDefinition.builder("Person2")
                .ifNotExists()
                .columns(
                        ColumnDefinition.column("ID", ColumnType.INT32),
                        ColumnDefinition.column("NAME", ColumnType.VARCHAR))
                .primaryKey("ID")
                .build());

        // All table view types provide identical capabilities.
        RecordView<Tuple> recordView = table.recordView();
        recordView.upsert(null, Tuple.create().set("id", 2).set("name", "Jane"));

        RecordView<Person> pojoView = table.recordView(Person.class);
        pojoView.upsert(null, new Person(3, "Jack"));

        KeyValueView<Tuple, Tuple> keyValueView = table.keyValueView();
        keyValueView.put(null, Tuple.create().set("id", 4), Tuple.create().set("name", "Jill"));

        KeyValueView<Integer, String> keyValuePojoView = table.keyValueView(Integer.class, String.class);
        keyValuePojoView.put(null, 5, "Joe");

        // Mix and match APIs.
        client.sql().execute(null, "SELECT * FROM Person2")
                .forEachRemaining(x -> System.out.println(x.stringValue("name")));
    }

    public static class Person {
        public Person() { }

        public Person(Integer id, String name) {
            this.id = id;
            this.name = name;
        }

        Integer id;
        String name;
    }
}
