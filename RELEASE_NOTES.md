#### 1.0.0-beta1 August 15 2020 ####

- Repository structure overhaul. Alpakka now uses a single solution structure, consolidating and formalizing the folder structure of the repository.
- Add Github CD/CI, backed by Azure Pipelines, to run automated unit tests for each pull request.
- Add Docker container images to run automated unit test.
- Update all projects to use the latest dependancies.
- Refactor `Akka.Streams.Amqp` to `Akka.Streams.Amqp.RabbitMq` to avoid confusion. `Akka.Streams.Amqp` uses `RabbitMQ.Client` under the hood and uses features specific to RabbitMQ. Naming the package `Akka.Streams.Amqp` will confuse users because the name suggests that the package is a generic AMQP compliant adapter. Users looking for a generic AMQP adapter should use the `Akka.Streams.Amqp.V1` package instead.
