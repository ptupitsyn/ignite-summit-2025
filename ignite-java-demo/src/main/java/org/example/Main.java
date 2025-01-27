package org.example;

import org.apache.ignite.client.IgniteClient;

public class Main {
    public static void main(String[] args) {
        IgniteClient client = IgniteClient.builder()
                .addresses("localhost")
                .build();
    }
}