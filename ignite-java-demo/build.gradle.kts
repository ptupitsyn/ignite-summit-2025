plugins {
    id("java")
}

group = "org.example"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    implementation("org.apache.ignite:ignite-client:3.0.0")
}

tasks.test {
    useJUnitPlatform()
}