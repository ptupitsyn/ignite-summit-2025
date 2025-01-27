plugins {
    id("java")
}

group = "org.example"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
    maven("https://repository.apache.org/content/repositories/orgapacheignite-1569/")
}

dependencies {
    implementation("org.apache.ignite:ignite-client:3.0.0")
}

tasks.test {
    useJUnitPlatform()
}