﻿
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Application.Interfaces;
using Application.DTOS;
using System;
using Infrastructure.RabbitMQ;
using Newtonsoft.Json;

namespace RabbitMQConsumers.UserDocumentEmailConsumer1
{
    public class UserDocumentEmailConsumer1 : BackgroundService
    {
        private IModel _channel;
        private IConnection _connection;
        private readonly ISendEmailServiceA _sendEmailService;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly string _password;
        public UserDocumentEmailConsumer1(ISendEmailServiceA sendEmailService, IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {
            _hostname = rabbitMqOptions.Value.Hostname;
            _queueName = rabbitMqOptions.Value.QueueName;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;
            _sendEmailService = sendEmailService;
            InitializeRabbitMqListener();
        }

        private void InitializeRabbitMqListener()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };

                _connection = factory.CreateConnection();
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error Creating Rabbit Mq Connection {ex.Message}");
            }
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (ConnectionExists())
            {
                stoppingToken.ThrowIfCancellationRequested();

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (ch, ea) =>
                {
                    
                    var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var emailsenderModel = JsonConvert.DeserializeObject<MailRequest>(content);

                    HandleMessage(emailsenderModel);

                    _channel.BasicAck(ea.DeliveryTag, false);
                };
                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerCancelled;

                _channel.BasicConsume(_queueName, false, consumer);

            }
            return Task.CompletedTask;
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            return _connection != null;
        }
        private void HandleMessage(MailRequest request)
        {
            _sendEmailService.SendEmailAsync(request);
        }

        private void OnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}

