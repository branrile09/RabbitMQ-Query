namespace Query
{
    using System.Text;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;


    internal class Program
    {
        static void Main()
        {

            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            ConnectionSetup("Query", channel);

            while (true)
            {

                Console.WriteLine("\n\tEnter Person Identifier (Username): ");
                string username = Console.ReadLine()!;
                Console.Clear();
                Console.WriteLine($"\n\t{username} has made contact with:");
                SendMessage(username, channel, "Query");
                Thread.Sleep(200);

                //let stuff do stuff            

            }

        }


        static void ConnectionSetup(string room_code, IModel channel)
        {
            channel.ExchangeDeclare(exchange: room_code, type: ExchangeType.Fanout);

            // declare a server-named queue
            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                exchange: room_code,
                routingKey: string.Empty);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (ea.RoutingKey == "Response")
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);                   
                    string[] contacts = message.Split(' ');
                    //Console.WriteLine("People who have made contact from last to first");

                    foreach (string i in contacts)
                    {
                        Console.WriteLine(i);
                    }
                }

            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        static void SendMessage(string username, IModel channel, string room_code)
        {
            var adjusted_message = $"{username}";

            var encoded_message = Encoding.UTF8.GetBytes(adjusted_message);

            channel.BasicPublish(exchange: room_code,
                routingKey: "Request",
                basicProperties: null,
                body: encoded_message);
        }


    }
}